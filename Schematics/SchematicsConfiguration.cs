using System.Collections.Generic;
using Rocket.API;
using Steamworks;

namespace Schematics
{
    public class SchematicsConfiguration : IRocketPluginConfiguration
    {
        public bool Features13;
        public string DatabaseAddress;
        public string DatabaseName;
        public string DatabaseUsername;
        public string DatabasePassword;
        public int DatabasePort;
        public string OrganizationsDatabaseTableName;
        public string EmployeesDatabaseTableName;
        public string OrganizationBankLogDatabaseTableName;
        public decimal MinCompanyPay;
        public decimal MinCompanyDeposit;
        public decimal MinCompanyWithdraw;
        public bool GroupBlacklisting;
        public int MaxAmountOfFoundedBusinesses;
        public List<string> BankRegions;

        public void LoadDefaults()
        {
            Features13 = false;
            DatabaseAddress = "localhost";
            DatabaseUsername = "unturned";
            DatabasePassword = "password";
            DatabaseName = "unturned";
            DatabasePort = 3306;
            OrganizationsDatabaseTableName = "rpcore_organizations";
            EmployeesDatabaseTableName = "rpcore_members";
            OrganizationBankLogDatabaseTableName = "rpcore_banklog";
            MinCompanyPay = 100;
            MinCompanyDeposit = 1000;
            MinCompanyWithdraw = 1000;
            MaxAmountOfFoundedBusinesses = 2;
            BankRegions = new List<string>
            {
                "Vault", "Vault2"
            };
        }
    }
}