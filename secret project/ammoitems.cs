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
    public class EveryItemAsAmmo : ICommand
    {
        public bool SanitizeResponse => false;
        public string Command => "ammoitems";

        public string[] Aliases => new string[] { "ami" };

        public string Description => "gives you 255 of every item in the game in ammo form";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            List<string> args = arguments.ToList();
            Log.Info(string.Join(", ", args));
            Player plr = Player.Get(sender);
            foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
            {
                if (item != ItemType.None)
                {
                    plr.AddAmmo(item, 255);
                }
            }
            response = "given";
            return true;
        }
    }
}
