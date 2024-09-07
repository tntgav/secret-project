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
    public class SpawnLootpile : ICommand
    {
        public bool SanitizeResponse => false;
        public string Command => "spawnlootpile";

        public string[] Aliases => new string[] { "slp" };

        public string Description => "spawns an item lootpile";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            List<string> args = arguments.ToList();
            Log.Info(string.Join(", ", args));
            Player plr = Player.Get(sender);
            if (!Enum.TryParse(args[0], true, out ItemType item)) { response = "failed"; return false; }
            Handlers.SpawnGrid3D(plr.Position, 15, 1, 0.4f, (Vector3 roompos) => { Handlers.spawnItem(roompos, item, Vector3.zero); });
            response = "done";
            return true;
        }
    }
}
