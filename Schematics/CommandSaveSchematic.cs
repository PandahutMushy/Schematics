using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Pandahut.Schematics
{
    internal class CommandSaveSchematics : IRocketCommand
    {
        public string Help => "Saves Schematic";

        public string Name => "SaveSchematic";

        public string Syntax => "<Range>";

        public List<string> Aliases => new List<string> {"SS", "SaveS"};

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public List<string> Permissions => new List<string> {"schematic.save"};

        public void Execute(IRocketPlayer caller, string[] command)
        {
            // Command: /saveSchematic
            var player = (UnturnedPlayer) caller;

            if (command == null || command.Length == 0 || command.Length == 1 || string.IsNullOrWhiteSpace(command[0]) || !int.TryParse(command[1], out var radius))
            {
                UnturnedChat.Say(player, "Invalid Syntax, use /SaveSchematic <name> <distance> [Optional Parameters: -Owner (Only gets structures placed by you) -Group (only gets structures placed by your current group), Input any Steamid64 to only get results from it");
                return;
            }

            var fullcommand = string.Join(" ", command).ToLower();
            ulong SpecificSteamid64 = 0;
            var GroupOnly = false;
            if (fullcommand.Contains("-Owner"))
                SpecificSteamid64 = player.CSteamID.m_SteamID;
            if (fullcommand.Contains("-Group"))
                GroupOnly = true;
            var match = Schematics.steamid64Regex.Match(fullcommand);
            if (match.Success && ulong.TryParse(match.Value, out var result))
                SpecificSteamid64 = result;
            radius += 92;
            var name = command[0].Replace(" ", "");
            var Barricades = GetBarricadeTransforms(player, radius, SpecificSteamid64, GroupOnly );
            var Structures = GetStructureTransforms(player, radius, SpecificSteamid64, GroupOnly);
            Logger.Log($"We have found Structures: {Structures.Count}  and Barricades: {Barricades.Count}");
            var river = ServerSavedata.openRiver($"/Rocket/Plugins/Schematics/Saved/{name}.dat", false);
            river.writeByte(Schematics.PluginVerison);
            river.writeUInt32(Provider.time);
            river.writeSingleVector3(player.Position);
            river.writeInt32(Barricades.Count);
            river.writeInt32(Structures.Count);
            foreach (var bdata in Barricades)
            {
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
            foreach (var sdata in Structures)
            {
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

            river.closeRiver();
            bool saveLocally = false;
            if (Schematics.Instance.Configuration.Instance.UseDatabase)
                try
                {
                    var file = new FileInfo(ReadWrite.PATH + ServerSavedata.directory + "/" + Provider.serverID + $"/Rocket/Plugins/Schematics/Saved/{name}.dat");
                    var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                    var binaryReader = new BinaryReader(fileStream);
                    Schematics.Instance.SchematicsDatabaseManager.AddSchematic(name, player.CharacterName, Provider.serverName, binaryReader.ReadBytes((int) fileStream.Length), (int) fileStream.Length, Structures.Count + Barricades.Count);
                    binaryReader.Close();
                    fileStream.Close();
                }
                catch (Exception e)
                {
                    saveLocally = true;
                    Logger.Log("Issue uploading file to your database, it has been saved locally instead.");
                }

            SendMessageAndLog(player, $"Done, we have saved Structures: {Structures.Count} and Barricades: {Barricades.Count} to {(Schematics.Instance.Configuration.Instance.UseDatabase ? "Database and Files" : "Files")} called {name}.", $"Saved {Structures.Count + Barricades.Count} elements for {player.CharacterName} to {(Schematics.Instance.Configuration.Instance.UseDatabase ? "Database and Files" : "Files")} called {name}.");
        }

        public List<StructureData> GetStructureTransforms(UnturnedPlayer player, int radius, ulong SpecificSteamid64, bool GroupOnly)
        {
            var position = player.Position;
            var error = 0;
            var Structures = new List<StructureData>();
            var transforms = 0;
            StructureData structure;
            var regionsfound = 0;
            var regionsused = 0;
            Vector3 pointVector3;
            StructureRegion structureRegion = null;
            Transform transform = null;
            for (var x = 0; x < StructureManager.regions.GetLength(0); x++)
            for (var y = 0; y < StructureManager.regions.GetLength(1); y++)
            {
                regionsfound++;
                Regions.tryGetPoint((byte) x, (byte) y, out pointVector3);
                if (Vector3.Distance(pointVector3 += new Vector3(64, 0, 64), new Vector3(position.x, 0f, position.z)) > radius)
                    continue;
                regionsused++;
                structureRegion = StructureManager.regions[x, y];
                transforms = structureRegion.drops.Count;
                for (var i = 0; i < transforms; i++)
                {
                    transform = structureRegion.drops[i].model;
                    if (structureRegion.structures[i] == null) { error++;  continue;}
                    structure = structureRegion.structures[i];
                    if (GroupOnly)
                        if (structure.group != player.SteamGroupID.m_SteamID)
                            continue;
                    if (SpecificSteamid64 != 0)
                        if (structure.owner != SpecificSteamid64)
                            continue;
                    if (Vector3.Distance(position, transform.position) < radius - 92)
                        Structures.Add(structureRegion.structures[i]);
                }
            }

            Logger.Log($"We have found {regionsfound} regions and used {regionsused} of them.");
            if (error != 0)
                SendMessageAndLog(player, "It seems your structure regions are a bit of sync, if you have issues, gotta restart server. This issue may be caused by one of your plugins.", $"Error on executing SaveSchematic command for {player.CharacterName},it seems structure regions are out of sync, gotta restart if this causes issues. Sorry! This could be caused by a server plugin, or just getting unlucky.");
            return Structures;
        }

        public List<BarricadeData> GetBarricadeTransforms(UnturnedPlayer player, int radius, ulong SpecificSteamid64, bool GroupOnly)
        {
            var position = player.Position;
            var error = 0;
            var Barricades = new List<BarricadeData>();
            var transforms = 0;
            var regionsfound = 0;
            var regionsused = 0;
            Vector3 pointVector3;
            BarricadeData barricade;
            BarricadeRegion barricadeRegion = null;
            Transform transform = null;
            for (var x = 0; x < BarricadeManager.regions.GetLength(0); x++)
            for (var y = 0; y < BarricadeManager.regions.GetLength(1); y++)
            {
                regionsfound++;
                Regions.tryGetPoint((byte) x, (byte) y, out pointVector3);

                if (Vector3.Distance(pointVector3 += new Vector3(64, 0, 64), new Vector3(position.x, 0f, position.z)) > radius)
                    continue;
                regionsused++;
                barricadeRegion = BarricadeManager.regions[x, y];
                transforms = barricadeRegion.drops.Count;
                for (var i = 0; i < transforms; i++)
                {
                    transform = barricadeRegion.drops[i].model;
                    if (barricadeRegion.barricades[i] == null) { error++; continue; }
                    barricade = barricadeRegion.barricades[i];
                    if (GroupOnly)
                        if (barricade.group != player.SteamGroupID.m_SteamID)
                            continue;
                    if (SpecificSteamid64 != 0)
                        if (barricade.owner != SpecificSteamid64)
                            continue;

                    if (Vector3.Distance(position, transform.position) < radius - 92)
                        Barricades.Add(barricade);
                }
            }

            Logger.Log($"We have found {regionsfound} regions and used {regionsused} of them.");
            if (error != 0)
                SendMessageAndLog(player, "It seems your barricade regions are a bit of sync, if you have issues, gotta restart server. This issue may be caused by one of your plugins.", $"Error on executing SaveSchematic command for {player.CharacterName},it seems barricade regions are out of sync, gotta restart if this causes issues. Sorry! This could be caused by a server plugin, or just getting unlucky.");
            return Barricades;
        }

        public void SendMessageAndLog(UnturnedPlayer player, string playermsg, string consolemsg)
        {
            UnturnedChat.Say(player, playermsg);
            Logger.Log(consolemsg);
        }
    }
}