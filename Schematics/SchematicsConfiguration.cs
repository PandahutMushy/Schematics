using Rocket.API;

namespace Pandahut.Schematics
{
    public class SchematicsConfiguration : IRocketPluginConfiguration
    {
        public DatabaseInfo SchematicsDatabaseInfo;

        public bool UseDatabase;
        public float MaxDistanceToLoadSchematic;

        public void LoadDefaults()
        {
            UseDatabase = false;
            MaxDistanceToLoadSchematic = 400;
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