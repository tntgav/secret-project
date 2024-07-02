using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace secret_project
{
    internal class EventHandlers
    {
        [PluginEvent(ServerEventType.RoundStart)]
        public async void RoundStart(RoundStartEvent ev)
        {
            await Task.Delay(50); //ummm this prevents some issues
            Server.FriendlyFire = true; // this is gun game from cs, everyone needs to fight each other
            nowinner = true;
            foreach (Player plr in Player.GetPlayers())
            {
                
                Storage.currentGuns.Add(plr, 0);
                spawnproperly(plr, true);
                
            }
        }
        bool nowinner;

        public void spawnproperly(Player plr, bool respawn = false)
        {
            if (respawn)
            {
                plr.SetRole(RoleTypeId.ChaosConscript);
                plr.Position = Handlers.RandomRoom().transform.position;
                plr.Position = new Vector3(plr.Position.x, plr.Position.y + 0.7f, plr.Position.z);
            }
            plr.ClearInventory();
            if (Storage.currentGuns[plr] < 0) { Storage.currentGuns[plr] = 0; }
            plr.AddItem(Storage.gungamelist[Storage.currentGuns[plr]]);
            plr.AddAmmo(ItemType.Ammo12gauge, ushort.MaxValue);
            plr.AddAmmo(ItemType.Ammo44cal, ushort.MaxValue);
            plr.AddAmmo(ItemType.Ammo556x45, ushort.MaxValue);
            plr.AddAmmo(ItemType.Ammo762x39, ushort.MaxValue);
            plr.AddAmmo(ItemType.Ammo9x19, ushort.MaxValue);
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        public void PlayerDeath(PlayerDeathEvent ev)
        {
            Storage.currentGuns[ev.Attacker] += 1;
            if (Storage.currentGuns[ev.Attacker] > Storage.gungamelist.Count-1 && nowinner)
            {
                nowinner = false; // there is in fact a winner
                Round.End();
                foreach (Player plr in Player.GetPlayers())
                {
                    HintHandlers.InitPlayer(plr);
                    HintHandlers.AddFadingText(plr, 900, $"<b>{ev.Attacker.Nickname} wins!</b>", 10f);
                }
            }
            spawnproperly(ev.Attacker);
            Storage.currentGuns[ev.Player] -= 1;
            spawnproperly(ev.Player, true);
        }
    }
}
