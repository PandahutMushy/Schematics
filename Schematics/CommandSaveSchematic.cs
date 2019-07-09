using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;

namespace Schematics
{
    internal class CommandCheckDriver : IRocketCommand
    {
        public string Help => "Saves Schematic";

        public string Name => "SaveSchematic";

        public string Syntax => "<Range>";

        public List<string> Aliases => new List<string>();

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public List<string> Permissions => new List<string> { "driver.check" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var user = (UnturnedPlayer)caller;
            var driver = UnturnedPlayer.FromName(command[0]);

        }
    }
}