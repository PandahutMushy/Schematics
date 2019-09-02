using System;
using System.Data;
using I18N.West;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;

namespace Pandahut.Schematics
{
    public class DatabaseManager
    {
        internal DatabaseManager()
        {
            new CP1250();
            var connection = CreateConnection();
            try
            {
                connection.Open();
                connection.Close();

                CreateSchematicsDatabaseScheme();
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1042)
                    Logger.LogError("Schematics failed to connect to MySQL database host.");
                else
                    Logger.LogException(ex);
                Logger.Log("Cannot connect to Database! Check your MySQL Information defined in Configuration.");
                Schematics.Instance.UnloadPlugin();
            }
        }

        private MySqlConnection CreateConnection()
        {
            MySqlConnection connection = null;
            try
            {
                if (Schematics.Instance.Configuration.Instance.SchematicsDatabaseInfo.DatabasePort == 0) Schematics.Instance.Configuration.Instance.SchematicsDatabaseInfo.DatabasePort = 3306;
                connection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", Schematics.Instance.Configuration.Instance.SchematicsDatabaseInfo.DatabaseAddress, Schematics.Instance.Configuration.Instance.SchematicsDatabaseInfo.DatabaseName, Schematics.Instance.Configuration.Instance.SchematicsDatabaseInfo.DatabaseUsername, Schematics.Instance.Configuration.Instance.SchematicsDatabaseInfo.DatabasePassword, Schematics.Instance.Configuration.Instance.SchematicsDatabaseInfo.DatabasePort.ToString()));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            return connection;
        }

        internal void CreateSchematicsDatabaseScheme()
        {
            try
            {
                var connection = CreateConnection();
                var command = connection.CreateCommand();
                connection.Open();
                command.CommandText = "SHOW TABLES LIKE '" + Schematics.Instance.Configuration.Instance.SchematicsDatabaseInfo.DatabaseTableName + "';";
                var checkDB = command.ExecuteScalar();
                if (checkDB == null)
                {
                    //CREATE TABLE `unturned_rptest`.`SchematicsTableName` ( `id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT , `Name` VARCHAR(32) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL , `Schematic` MEDIUMBLOB NOT NULL , `Madeby` VARCHAR(32) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL , `MadeAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP , `MadeIn` VARCHAR(32) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL , PRIMARY KEY (`id`)) ENGINE = InnoDB CHARSET=latin1 COLLATE latin1_swedish_ci;
                    command.CommandText = "CREATE TABLE `" + Schematics.Instance.Configuration.Instance.SchematicsDatabaseInfo.DatabaseTableName + "` ( `id` INT(11) UNSIGNED NOT NULL AUTO_INCREMENT , `Name` VARCHAR(32) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL , `Schematic` MEDIUMBLOB NOT NULL ,`Length` INT(11)  NOT NULL ,`TotalElements` INT(11)  NOT NULL , `Madeby` VARCHAR(32) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL , `MadeAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP , `MadeIn` VARCHAR(32) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL , PRIMARY KEY (`id`)) ENGINE = InnoDB CHARSET=latin1 COLLATE latin1_swedish_ci;";
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                Logger.Log("Commands Table Creation Error!");
                Logger.LogException(ex);
            }
        }

        public Schematics.Schematic GetSchematicByName(string Name)
        {
            try
            {
                var connection = CreateConnection();
                var command = new MySqlCommand("SELECT * FROM `" + Schematics.Instance.Configuration.Instance.SchematicsDatabaseInfo.DatabaseTableName + "` WHERE `Name` = @Name", connection);
                command.Parameters.AddWithValue("@Name", Name);
                connection.Open();
                var dataReader = command.ExecuteReader(CommandBehavior.SingleRow);
                var schematic = new Schematics.Schematic();
                while (dataReader.Read())
                {
                    schematic.id = Convert.ToInt32(dataReader["id"]);
                    schematic.SchematicName = Convert.ToString(dataReader["Name"]);
                    schematic.SchmeticBytes = (byte[]) dataReader["Schematic"];
                    schematic.Length = Convert.ToInt32(dataReader["Length"]);
                    schematic.Madeby = Convert.ToString(dataReader["Madeby"]);
                    schematic.MadeAt = Convert.ToDateTime(dataReader["MadeAt"]);
                    schematic.MadeIn = Convert.ToString(dataReader["MadeIn"]);
                }

                dataReader.Close();
                connection.Close();

                if (schematic.SchmeticBytes != null)
                    return schematic;
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return null;
            }
        }

        public void AddSchematic(string Name, string Madeby, string MadeIn, byte[] blob, int length, int TotalElementCount)
        {
            try
            {
                var connection = CreateConnection();
                var command = connection.CreateCommand();
                command.CommandText = "REPLACE INTO `" + Schematics.Instance.Configuration.Instance.SchematicsDatabaseInfo.DatabaseTableName + "` (`Name`, `Madeby`, `MadeAt`, `MadeIn`, `Schematic`, `Length`, `TotalElements`) VALUES (@Name,@MadeBy,@MadeAt,@MadeIn,@Schematic,@Length, @TotalElements);";
                command.Parameters.AddWithValue("@Name", Name);
                command.Parameters.AddWithValue("@Madeby", Madeby);
                command.Parameters.AddWithValue("@MadeAt", DateTime.Now);
                command.Parameters.AddWithValue("@MadeIn", MadeIn);
                command.Parameters.AddWithValue("@Schematic", blob);
                command.Parameters.AddWithValue("@Length", length);
                command.Parameters.AddWithValue("@TotalElements", TotalElementCount);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public bool DeleteSchematic(int id)
        {
            try
            {
                var connection = CreateConnection();
                var command = connection.CreateCommand();
                command.CommandText = $"DELETE FROM `{Schematics.Instance.Configuration.Instance.SchematicsDatabaseInfo.DatabaseTableName}` WHERE `{Schematics.Instance.Configuration.Instance.SchematicsDatabaseInfo.DatabaseTableName}`.`id` = {id}; ";
                connection.Open();
                command.ExecuteNonQuery();
                connection.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return false;
            }
        }
    }
}