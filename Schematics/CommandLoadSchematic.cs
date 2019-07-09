using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Schematics
{
    internal class CommandCheckDriver : IRocketCommand
    {
        public string Help => "Loads Schematic";

        public string Name => "LoadSchematic";

        public string Syntax => "<Name>";

        public List<string> Aliases => new List<string> { "LS" };

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public List<string> Permissions => new List<string> { "schematic.load" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = (UnturnedPlayer)caller;

            if (command == null || command.Length == 0 || string.IsNullOrWhiteSpace(command[0]))
            {
                UnturnedChat.Say($"Invalid Syntax, use /Loadschematic <Name> [-CurrentPos] [-Hitmarker] [-NoState]");
                return;
            }
            var keepHealth = true;
            string name = command[0].Replace(" ", "");
            River river = ServerSavedata.openRiver($"/Rocket/Plugins/Schematics/Saved/{name}.dat", isReading: false);
            byte verison = river.readByte();
            var Time = river.readUInt32();
            var barricadecountInt32 = river.readInt32();
            Logger.Log($"Time {Time}");
            int error = 0;
            for (int i = 0; i < barricadecountInt32; i++)
            {
               var barricadeid = river.readUInt16();
               var barricadehealth =  river.readUInt16();
               var barricadestate = river.readBytes();
               var point =  river.readSingleVector3();
               var angleX = river.readByte();
               var angleY = river.readByte();
               var angleZ = river.readByte();
               var owner = river.readUInt64();
               var group =  river.readUInt64();
               var objActiveDate = river.readUInt32();
               Barricade barricade = new Barricade(barricadeid);
               if (keepHealth)
                   barricade.health = barricadehealth;
               barricade.state = barricadestate;

               // For when nelson adds proper way to add barricades
               BarricadeData barricadeData = new BarricadeData(barricade, point, angleX, angleY, angleZ, owner, group, objActiveDate);
               if (!BarricadeManager.dropBarricade(barricade, null, point, angleX, angleY, angleZ, owner, group))
                   error++;
            }
            if (error != 0)
                Logger.Log($"Unexpected Barricade Error occured {error} times");
            error = 0;
            var structurecountInt32 = river.readInt32();
            for (int i = 0; i < structurecountInt32; i++)
            {
               var structureid = river.readUInt16();
               var structurehealth =  river.readUInt16();
               var point = river.readSingleVector3();
               var angleX = river.readByte();
               var angleY = river.readByte();
               var angleZ = river.readByte();
               var owner = river.readUInt64();
               var group = river.readUInt64();
               var objActiveDate = river.readUInt32();
               Structure structure = new Structure(structureid);
               if (keepHealth)
                   structure.health = structureid;
               // For when nelson adds proper way to add structures
               StructureData structureData = new StructureData(structure, point, angleX, angleY, angleZ, owner, group, objActiveDate);
               if (!StructureManager.dropStructure(structure, point, angleX, angleY, angleZ, owner, group))
                   error++;
            }
            if (error != 0)
                Logger.Log($"Unexpected Barricade Error occured {error} times");

            UnturnedChat.Say(player, $"Done, we have loaded Structures: {structurecountInt32} and Barricades: {barricadecountInt32} from {name}");
        }
    }
}