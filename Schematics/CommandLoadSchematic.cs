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
    internal class CommandLoadSchematics : IRocketCommand
    {
        public string Help => "Saves Schematic";

        public string Name => "SaveSchematic";

        public string Syntax => "<Range>";

        public List<string> Aliases => new List<string>();

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public List<string> Permissions => new List<string> { "driver.check" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            // Command: /saveSchematic
            var player = (UnturnedPlayer)caller;
            if (command.Length == 1 || !int.TryParse(command[1], out int radius))
            {
                UnturnedChat.Say($"Invalid Syntax, use /SaveSchematic <distance> [Optional Parameters: -Everyone -ID64 -Hitmarker");
                return;
            }
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
            River river = ServerSavedata.openRiver("/Rocket/Plugins/Schematics", isReading: false);
            river.writeByte(Schematics.PluginVerison);
            river.writeUInt32(Provider.time);
            foreach (var barricade in Barricades)
            {
                if (BarricadeManager.tryGetInfo(barricade, out var x, out var y, out var plant, out var index, out var r))
                {
                    var bdata = r.barricades[index];
                }
            }
        }
    }
}