using System.Collections.Generic;
using System.IO;
using Rocket.API;
using Steamworks;

namespace Pandahut.Schematics
{
    public class SchematicsConfiguration : IRocketPluginConfiguration
    {

        public bool UseDatabase;
        public float MaxSpawnDistance;
        public DatabaseInfo SchematicsDatabaseInfo;

        public void LoadDefaults()
        {
            UseDatabase = false;
            MaxSpawnDistance = 300;
            SchematicsDatabaseInfo = new DatabaseInfo
            {
                DatabaseAddress = "localhost",
                DatabaseUsername = "unturned",
                DatabasePassword = "password",
                DatabaseName = "unturned",
                DatabaseTableName = "Schematics",
                DatabasePort = 3306
            };
        }
    }
    public class DatabaseInfo
    {
        public string DatabaseAddress;
        public string DatabaseName;
        public string DatabasePassword;
        public int DatabasePort;
        public string DatabaseTableName;
        public string DatabaseUsername;
    }
}