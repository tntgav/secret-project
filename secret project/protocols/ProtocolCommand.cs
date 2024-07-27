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
using secret_project.protocols;
using CustomPlayerEffects;
using CustomRendering;
using MapGeneration;
using UnityEngine;

namespace secret_project
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class EnableProtocol : ICommand
    {
        public bool SanitizeResponse => false;
        public string Command => "startprotocol";

        public string[] Aliases => new string[] { "spc" };

        public string Description => "starts a custom protocol";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            List<string> args = arguments.ToList();
            Log.Info(string.Join(", ", args));
            Player plr = Player.Get(sender);
            if (args[0] == "cryo")
            {
                CustomProtocol cryoprotocol = new CustomProtocol("cryo", "<color=#ddddff>C.R.Y.O. Protocol</color>", 100);
                cryoprotocol.AddEvent(() =>
                {
                    foreach (Player p in Player.GetPlayers())
                    {
                        Handlers.AddEffect<Slowness>(p, 1);
                        Handlers.AddEffect<DamageReduction>(p, 2);
                        p.Damage(1, "The freezing cold. youch");
                    }

                    foreach (RoomIdentifier room in RoomIdentifier.AllRoomIdentifiers)
                    {
                        RoomLightController controller = room.GetComponentInChildren<RoomLightController>();
                        Color c = controller.NetworkOverrideColor;
                        controller.NetworkOverrideColor = new Color(c.r, c.g, c.b + 0.01f); // makes the facility turn more blue over time
                    }
                });

                cryoprotocol.AddEvent(99, () => { foreach (Player p in Player.GetPlayers()) { Handlers.AddEffect<FogControl>(p, 7); } });
                cryoprotocol.AddEvent(1, () => { foreach (Player p in Player.GetPlayers()) { p.Kill("You couldn't bear the cold."); } });

                cryoprotocol.StartProtocol();
            }

            response = "i tried my best.";
            return true;
        }
    }
}
