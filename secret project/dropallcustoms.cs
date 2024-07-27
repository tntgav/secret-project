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
    public class DropAllCustoms : ICommand
    {
        public bool SanitizeResponse => false;
        public string Command => "DropAllCustoms";

        public string[] Aliases => new string[] { "daa" };

        public string Description => "drops all custom item s";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            List<string> args = arguments.ToList();
            Log.Info(string.Join(", ", args));
            Player plr = Player.Get(sender);
            foreach (CustomItemType type in Enum.GetValues(typeof(CustomItemType)))
            {
                Handlers.DropCustom(plr.Position, type, Vector3.zero);
            }
            response = "ok did it ..";
            return true;
        }
    }
}
