using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Schematics
{
    internal class CommandSaveSchematics : IRocketCommand
    {
        public string Help => "Saves Schematic";

        public string Name => "SaveSchematic";

        public string Syntax => "<Range>";

        public List<string> Aliases => new List<string>();

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public List<string> Permissions => new List<string> { "schematic.save" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            // Command: /saveSchematic
            var player = (UnturnedPlayer)caller;
            
            if (command == null || command.Length == 0 || command.Length == 1 || string.IsNullOrWhiteSpace(command[0]) || !int.TryParse(command[1], out int radius))
            {
                UnturnedChat.Say($"Invalid Syntax, use /SaveSchematic <distance> [Optional Parameters: -Everyone -ID64 -Hitmarker");
                return;
            }
            string name = command[0].Replace(" ", "");
            List<Transform> Barricades = new List<Transform>();
            BarricadeManager.getBarricadesInRadius(player.Position, radius ^ 2, Barricades);
            List<Transform> Structures = new List<Transform>();
            List<RegionCoordinate> Coordinates = new List<RegionCoordinate>();
            for (byte b = 0; b < Regions.WORLD_SIZE; b = (byte) (b + 1))
            {
                for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2 = (byte) (b2 + 1))
                {
                    if (Vector3.Distance(new Vector3(b,player.Position.y, b2), player.Position) < radius)
                       Coordinates.Add(new RegionCoordinate(b, b2));
                   
                }
            }

           if(StructureManager.tryGetRegion((byte) player.Position.x, (byte) player.Position.y, out StructureRegion region) && !Coordinates.Any(coord => coord.x == (byte) player.Position.x && coord.y == (byte)player.Position.y))
           {
               Coordinates.Add(new RegionCoordinate((byte)player.Position.x, (byte)player.Position.y));
            }
            StructureManager.getStructuresInRadius(player.Position, radius ^ 2, Coordinates, Structures);
            Logger.Log($"We have found Structures: {Structures.Count} and Barricades: {Barricades.Count}");
            Logger.Log($"Loading up the .dat");
            River river = ServerSavedata.openRiver($"/Rocket/Plugins/Schematics/saved/{name}.dat", isReading: false);
            river.writeByte(Schematics.PluginVerison);
            river.writeUInt32(Provider.time);
            river.writeInt32(Barricades.Count);
            int error = 0;
            foreach (var barricade in Barricades)
            {
                if (BarricadeManager.tryGetInfo(barricade, out var x, out var y, out var plant, out var index, out var r))
                {
                    var bdata = r.barricades[index];
                    river.writeUInt16(bdata.barricade.id);
                    river.writeUInt16(bdata.barricade.health);
                    river.writeBytes(bdata.barricade.state);
                    river.writeSingleVector3(bdata.point);
                    river.writeByte(bdata.angle_x);
                    river.writeByte(bdata.angle_y);
                    river.writeByte(bdata.angle_z);
                    river.writeUInt64(bdata.owner);
                    river.writeUInt64(bdata.group);
                    river.writeUInt32(bdata.objActiveDate);
                }
                else
                {
                    error++;
                }
            }
            if (error != 0)
                Logger.Log($"Unexpected Barricade Error occured {error} times");
            error = 0;
            river.writeInt32(Structures.Count);
            foreach (var structure in Structures)
            {
                if (StructureManager.tryGetInfo(structure, out var x, out var y, out var index, out var r))
                {
                    var sdata = r.structures[index];
                    river.writeUInt16(sdata.structure.id);
                    river.writeUInt16(sdata.structure.health);
                    river.writeSingleVector3(sdata.point);
                    river.writeByte(sdata.angle_x);
                    river.writeByte(sdata.angle_y);
                    river.writeByte(sdata.angle_z);
                    river.writeUInt64(sdata.owner);
                    river.writeUInt64(sdata.group);
                    river.writeUInt32(sdata.objActiveDate);
                }
                else
                {
                    error++;
                }
            }
            if (error != 0)
                Logger.Log($"Unexpected Structure Error occured {error} times");
            river.closeRiver();
            UnturnedChat.Say($"Done, we have saved Structures: {Structures.Count} and Barricades: {Barricades.Count} to {name}");
        }
    }
}