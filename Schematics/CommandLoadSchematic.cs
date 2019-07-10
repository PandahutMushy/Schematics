using System;
using System.Collections.Generic;
using System.IO;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Pandahut.Schematics
{
    extern alias UnityEnginePhysics;
    internal class CommandLoadSchematic : IRocketCommand
    {
        public string Help => "Loads Schematic";

        public string Name => "LoadSchematic";

        public string Syntax => "<Name>";

        public List<string> Aliases => new List<string> { "LS", "LoadS" };

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public List<string> Permissions => new List<string> { "schematic.load" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = (UnturnedPlayer)caller;

            if (command == null || command.Length == 0 || string.IsNullOrWhiteSpace(command[0]))
            {
                UnturnedChat.Say(player, $"Invalid Syntax, use /Loadschematic <Name> [Optional: -KeepPos -NoState -KeepHealth -SetOwner -SetGroup, Input any Steamid64 to set owner to it]");
                return;
            }

            if (!UnityEnginePhysics::UnityEngine.Physics.Raycast(player.Player.look.aim.position, player.Player.look.aim.forward, out var hit))
            {
                UnturnedChat.Say(caller, $"Cannot get what you're aiming at to spawn the schematic.");
                return;
            }

            if (Schematics.Instance.Configuration.Instance.MaxSpawnDistance != 0 && Vector3.Distance(player.Position, hit.point) > Schematics.Instance.Configuration.Instance.MaxSpawnDistance)
            {
                UnturnedChat.Say(caller, $"You're looking at a position that is over your MaxSpawnDistance defined in Configuration.");
                return;
            }
            var fullcommand = string.Join(" ", command).ToLower();
            var keepLocation = false;
            var keepHealth = false;
            var keepState = true;
            ulong SpecificSteamid64 = 0;
            ulong specificgroup = 0;
            if (fullcommand.Contains("-keeppos"))
                keepLocation = true;
            if (fullcommand.Contains("-health"))
                keepHealth = true;
            if (fullcommand.Contains("-nostate"))
                keepState = false;
            if (fullcommand.Contains("-setowner"))
                SpecificSteamid64 = player.CSteamID.m_SteamID;
            if (fullcommand.Contains("-setgroup"))
                specificgroup = player.SteamGroupID.m_SteamID;
            var match = Schematics.steamid64Regex.Match(fullcommand);
            if (match.Success && ulong.TryParse(match.Value, out var result))
                SpecificSteamid64 = result;
            string name = command[0].Replace(" ", "");
            if (Schematics.Instance.Configuration.Instance.UseDatabase)
            {
               var Schematic =  Schematics.Instance.SchematicsDatabaseManager.GetSchematicByName(name);
               if (Schematic == null)
               {
                   UnturnedChat.Say($"Cannot find {name} in Database");
                   return;
               }
               var fs = new FileStream(ReadWrite.PATH + ServerSavedata.directory + "/" + Provider.serverID + $"/Rocket/Plugins/Schematics/Saved/{name}.dat", FileMode.OpenOrCreate, FileAccess.Write);
               fs.Write(Schematic.SchmeticBytes, 0, (int)Schematic.Length);
               fs.Close();
            }
            River river = ServerSavedata.openRiver($"/Rocket/Plugins/Schematics/Saved/{name}.dat", isReading: false);
            byte verison = river.readByte();
            var Time = river.readUInt32();
            var playerposition = river.readSingleVector3();
            UnturnedChat.Say(player, $"Loading {name} saved at {DateTimeOffset.FromUnixTimeSeconds(Time).ToLocalTime().ToString()}");
            var barricadecountInt32 = river.readInt32();
            var structurecountInt32 = river.readInt32();
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
               if (!keepState)
                   barricade.state = barricadestate;
               if (!keepLocation)
               {
                   point = (point - playerposition) + hit.point;
               }
               if (SpecificSteamid64 != 0)
                   owner = SpecificSteamid64;
               if (specificgroup != 0)
                   group = specificgroup;
               // For when nelson adds proper way to add barricades
               BarricadeData barricadeData = new BarricadeData(barricade, point, angleX, angleY, angleZ, owner, group, Provider.time);
               if (!BarricadeManager.dropBarricade(barricade, null, point, angleX, angleY, angleZ, owner, group))
                   error++;
            }
            if (error != 0)
                Logger.Log($"Unexpected Barricade Error occured {error} times");
            error = 0;
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
                if (!keepLocation)
                {
                    point = (point - playerposition) + hit.point;
                }
                if (SpecificSteamid64 != 0)
                    owner = SpecificSteamid64;
                if (specificgroup != 0)
                    group = specificgroup;
                StructureData structureData = new StructureData(structure, point, angleX, angleY, angleZ, owner, group, Provider.time);
               if (!StructureManager.dropStructure(structure, point, angleX, angleY, angleZ, owner, group))
                   error++;
            }
            if (error != 0)
                Logger.Log($"Unexpected Barricade Error occured {error} times");
            river.closeRiver();
            UnturnedChat.Say(player, $"Done, we have loaded Structures: {structurecountInt32} and Barricades: {barricadecountInt32} from {name}");
        }
        public void SendMessageAndLog(UnturnedPlayer player, string playermsg, string consolemsg)
        {
            UnturnedChat.Say(player, playermsg);
            Logger.Log(consolemsg);
        }
    }
}