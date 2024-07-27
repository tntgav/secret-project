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
using UnityEngine;

namespace secret_project
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class DropCustomItem : ICommand
    {
        public bool SanitizeResponse => false;
        public string Command => "DropCustom";

        public string[] Aliases => new string[] { "drc" };

        public string Description => "drops a custom item";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            List<string> args = arguments.ToList();
            Log.Info(string.Join(", ", args));
            Player plr = Player.Get(sender);
            if (Enum.TryParse(args[0], true, out CustomItemType type))
            {
                if (args.ElementAtOrDefault(1) != null) { for (int i = 0; i < int.Parse(args[1]); i++) {
                        Handlers.DropCustom(plr.Position, type, Vector3.zero);
                    } } else {
                    Handlers.DropCustom(plr.Position, type, Vector3.zero);
                }
                response = "successfully dropped";
                return true;
            }
            else
            {
                response = "Failed to generate custom item type.";
                return false;
            }
        }
    }
}
