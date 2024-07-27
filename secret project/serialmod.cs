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
    public class SerialMod : ICommand
    {
        public bool SanitizeResponse => false;
        public string Command => "incserial";

        public string[] Aliases => new string[] { "sm" };

        public string Description => "artificially increases the current item serial, resulting in ocasionally strange results";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            List<string> args = arguments.ToList();
            Handlers.ModSerials(int.Parse(args[0]));
            response = $"Successfully changed. New item serial is {ItemSerialGenerator._ai}";
            return true;
        }
    }
}
