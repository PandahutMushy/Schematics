using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
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
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using Logger = Rocket.Core.Logging.Logger;

namespace Schematics
{
    public class Schematics : RocketPlugin<SchematicsConfiguration>
    {
        public static byte PluginVerison = 1;
        protected override void Load()
        {

            Logger.Log($"Welcome to Schematics!");
        }

        protected override void Unload()
        {
            Logger.Log($"Unloading Schematics!");
        }
    }
}
