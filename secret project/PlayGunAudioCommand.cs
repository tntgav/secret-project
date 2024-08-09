using CommandSystem;
using InventorySystem.Items;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace secret_project
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class PlayGunAudioCommand : ICommand
    {
        public bool SanitizeResponse => false;
        public string Command => "gunaudio";

        public string[] Aliases => new string[] { "ga" };

        public string Description => "plays gun audio";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            List<string> args = arguments.ToList();
            Log.Info(string.Join(", ", args));
            Player plr = Player.Get(sender);

            if (!Enum.TryParse(args[0], out ItemType guntype)) { response = "err item type"; return false; }
            if (!byte.TryParse(args[1], out byte audio)) { response = "err audio clip"; return false; }
            Handlers.PlayGunAudio(plr.Position, guntype, audio);
            response = "played";
            return true;
        }
    }
}
