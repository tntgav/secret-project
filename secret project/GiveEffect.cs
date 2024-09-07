using CommandSystem;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms;
using InventorySystem.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginAPI.Core;
using InventorySystem;

namespace secret_project
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class GiveCustomEffect : ICommand
    {
        public bool SanitizeResponse => false;
        public string Command => "GiveCustomEffect";

        public string[] Aliases => new string[] { "aa" };

        public string Description => "custom effroct comadn";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            List<string> args = arguments.ToList();
            Log.Info(string.Join(", ", args));
            Player plr = Player.Get(sender);
            if (!int.TryParse(args[1], out int intensity)) { response = "Oopsie-woopsie, somesing went wrong! Couwdn't get intensity 3:"; return false; }
            if (!float.TryParse(args[2], out float dur)) { response = "Oopsie-woopsie, somesing went wrong! Couwdn't get duwation 3:"; return false; }
            if (dur == 0f) { dur = float.MaxValue; }
            if (Enum.TryParse(args[0], true, out CustomEffect.EffectType type))
            {
                CustomEffect.GiveEffect(plr, new CustomEffect(type, dur, intensity));
                response = "Given effect!";
                return true;

            } else
            {
                response = "Oopsie-woopsie, somesing went wrong! Couwdn't get effect type 3:";
                return false;
            }
        }
    }
}
