using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Timers;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Assets;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Extensions;
using Rocket.Unturned.Player;
using Pandahut.Schematics;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using Logger = Rocket.Core.Logging.Logger;

namespace Pandahut.Schematics
{
    public class Schematics : RocketPlugin<SchematicsConfiguration>
    {
        public class Schematic
        {
            public int id;
            public string SchematicName;
            public byte[] SchmeticBytes;
            public string Madeby;
            public DateTime MadeAt;
            public string MadeIn;
            public int Length;
        }
        public static Regex steamid64Regex = new Regex(@"/[0-9]{17}/", RegexOptions.Compiled);
        public static Schematics Instance;
        public static byte PluginVerison = 1;
        public DatabaseManager SchematicsDatabaseManager;
        protected override void Load()
        {
            ConfigCheck();
            Instance = this;
            if (Configuration.Instance.UseDatabase)
            {
                SchematicsDatabaseManager = new DatabaseManager();
                System.IO.DirectoryInfo di = new DirectoryInfo(ReadWrite.PATH + ServerSavedata.directory + "/" + Provider.serverID + $"/Rocket/Plugins/Schematics/Saved/");
                foreach (FileInfo file in di.GetFiles())
                {
                    var river = ServerSavedata.openRiver($"/Rocket/Plugins/Schematics/Saved/{file.Name}", isReading: true);
                    try
                    {
                        var verison = river.readByte();
                        var useDatabase = river.readBoolean();
                        if (!useDatabase)
                            file.Delete();
                        river.closeRiver();
                    }
                    catch (Exception _)
                    {
                        river.closeRiver();
                    }

                }
            }

            Logger.Log($"Welcome to Schematics!");
            
        }

        protected override void Unload()
        {
            Logger.Log($"Unloading Schematics!");
        }

        public void ConfigCheck()
        {
            if (Configuration.Instance.UseDatabase == null)
            {
                Logger.Log($"You're missing UseDB in Config, defaulting to N");
                Configuration.Instance.UseDatabase = false;
                Configuration.Save();
            }
            if (Configuration.Instance.SchematicsDatabaseInfo == null)
            {
                Logger.Log($"You're missing Schematics Database Info in Config, defaulting it and setting UseDatabase to false");
                Configuration.Instance.SchematicsDatabaseInfo = new DatabaseInfo
                {
                    DatabaseAddress = "localhost",
                    DatabaseUsername = "unturned",
                    DatabasePassword = "password",
                    DatabaseName = "unturned",
                    DatabaseTableName = "Schematics",
                    DatabasePort = 3306
                };
                Configuration.Instance.UseDatabase = false;
                Configuration.Save();
            }
            if (Configuration.Instance.MaxSpawnDistance == null)
            {
                Logger.Log($"You're missing MaxSpawnDistance in Config, defaulting to 300");
                Configuration.Instance.MaxSpawnDistance = 300;
                Configuration.Save();
            }
        }
    }
}
