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
using Utils;
using PlayerStatsSystem;

namespace secret_project
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class VaporizeCommand : ICommand
    {
        public bool SanitizeResponse => false;
        public string Command => "vaporize";

        public string[] Aliases => new string[] { "va" };

        public string Description => "removes someone from existance, cleanly.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            List<string> args = arguments.ToList();
            List<ReferenceHub> r = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out string[] useless);

            foreach (ReferenceHub player in r)
            {
                Player.Get(player).Damage(new DisruptorDamageHandler(new Footprinting.Footprint(ReferenceHub.HostHub), float.MaxValue));
            }
            response = "terminated";
            return true;
        }
    }
}
