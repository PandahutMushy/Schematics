using System;
using System.Collections.Generic;
using System.Globalization;
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

    internal class CommandDeleteSchematic : IRocketCommand
    {
        public string Help => "Deletes Schematic Info";

        public string Name => "DeleteSchematic";

        public string Syntax => "<Name>";

        public List<string> Aliases => new List<string> { };

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public List<string> Permissions => new List<string> { "schematic.delete" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            bool Console = caller is ConsolePlayer;
            if (command == null || command.Length == 0 || string.IsNullOrWhiteSpace(command[0]))
            {
                SendMessage(caller, $"Invalid Syntax, use /DeleteSchematic <Name>", Console);
                return;
            }
            string name = command[0].Replace(" ", "");
            if (Schematics.Instance.Configuration.Instance.UseDatabase)
            {
                var Schematic = Schematics.Instance.SchematicsDatabaseManager.GetSchematicByName(name);
                if (Schematic == null)
                {
                    SendMessage(caller, $"Cannot find {name} in Database", Console);
                    return;
                }
                var success =  Schematics.Instance.SchematicsDatabaseManager.DeleteSchematic(Schematic.id);
                 if (success)
                     SendMessage(caller, $"Successfully deleted {Schematic.SchematicName} from Database.", Console);
                 else
                     SendMessage(caller, $"Failed deleting {Schematic.SchematicName} from Database, most likely not enough database permissions.", Console);
            }

            try
            {
                var file = new FileInfo(ReadWrite.PATH + ServerSavedata.directory + "/" + Provider.serverID + $"/Rocket/Plugins/Schematics/Saved/{name}.dat");
                if (file == null || !file.Exists)
                {
                    SendMessage(caller, $"Can't find file with name of {name} in files. May of never been on this server in first place.", Console);
                    return;
                }
                file.Delete();
                SendMessage(caller, $"Successfully deleted {file.Name} from Files.", Console);
            }
            catch (Exception e)
            {
                SendMessage(caller, $"File is in use, printing error to your logs.", Console);
                Logger.LogError(e.Message);
            }

        }

        public void SendMessage(IRocketPlayer caller, string msg, bool Console)
        {
            if (Console)
                Logger.Log(msg);
            else
                UnturnedChat.Say(caller, msg);
        }
    }
}