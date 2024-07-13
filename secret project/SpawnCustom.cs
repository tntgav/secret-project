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
    public class Class : ICommand
    {
        public bool SanitizeResponse => false;
        public string Command => "SpawnCustom";

        public string[] Aliases => new string[] { "a" };

        public string Description => "gives you a custom item";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            List<string> args = arguments.ToList();
            Log.Info(string.Join(", ", args));
            Player plr = Player.Get(sender);
            if (Enum.TryParse(args[0], true, out CustomItemType type))
            {
                ItemBase itemBase = plr.ReferenceHub.inventory.ServerAddItem(CustomItems.items[type]);
                if (itemBase == null)
                {
                    response = "Generating item failed.";
                    return false;
                }
                CustomItems.LiveCustoms.Add(itemBase.ItemSerial, type);
                response = "Successfully given custom item!";
                return true;

            } else
            {
                response = "Failed to generate custom item type.";
                return false;
            }
        }
    }
}
