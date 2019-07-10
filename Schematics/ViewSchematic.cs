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

    internal class CommandViewSchematic : IRocketCommand
    {
        public string Help => "Loads Schematic Info";

        public string Name => "ViewSchematic";

        public string Syntax => "<Name>";

        public List<string> Aliases => new List<string> {"VS", "ViewS"};

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public List<string> Permissions => new List<string> {"schematic.view"};

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command == null || command.Length == 0 || string.IsNullOrWhiteSpace(command[0]))
            {
                UnturnedChat.Say(caller, $"Invalid Syntax, use /ViewSchematic <Name>");
                return;
            }

            string name = command[0].Replace(" ", "");
            if (Schematics.Instance.Configuration.Instance.UseDatabase)
            {
                var Schematic = Schematics.Instance.SchematicsDatabaseManager.GetSchematicByName(name);
                if (Schematic == null)
                {
                    UnturnedChat.Say($"Cannot find {name} in Database");
                    return;
                }

                var fs = new FileStream(ReadWrite.PATH + ServerSavedata.directory + "/" + Provider.serverID + $"/Rocket/Plugins/Schematics/Saved/{name}.dat", FileMode.OpenOrCreate, FileAccess.Write);
                fs.Write(Schematic.SchmeticBytes, 0, (int) Schematic.Length);
                fs.Close();
            }

            var river = ServerSavedata.openRiver($"/Rocket/Plugins/Schematics/Saved/{name}.dat", isReading: false);
            var verison = river.readByte();
            var Time = river.readUInt32();
            var playerposition = river.readSingleVector3();
            var barricadecountInt32 = river.readInt32();
            var structurecountInt32 = river.readInt32();
            if (caller is ConsolePlayer)
                Logger.Log($"{name} was saved at {DateTimeOffset.FromUnixTimeSeconds(Time).ToLocalTime().ToString()} with Plugin Verison {verison}, it has {barricadecountInt32} barricades and {structurecountInt32} structures, total {barricadecountInt32 + structurecountInt32} elements.");
            else
                UnturnedChat.Say(caller, $"{name} was saved at {DateTimeOffset.FromUnixTimeSeconds(Time).ToLocalTime().ToString()} with Plugin Verison {verison}, it has {barricadecountInt32} barricades and {structurecountInt32} structures, total {barricadecountInt32 + structurecountInt32} elements.");
        }
    }
}