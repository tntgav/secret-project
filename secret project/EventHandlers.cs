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

        public void spawnproperly(Player plr, bool respawn = false, int change = 0)
        {
            if (plr == null) { return; }
            if (!Storage.currentGuns.ContainsKey(plr)) { Storage.currentGuns.Add(plr, 0); }
            Log.Debug($"{string.Join(", ", Storage.currentGuns)}");
            if (respawn)
            {
                plr.SetRole(RoleTypeId.ChaosConscript);
                plr.Position = Handlers.RandomRoom().transform.position;
                plr.Position = new Vector3(plr.Position.x, plr.Position.y + 0.7f, plr.Position.z);
            }
            plr.ClearInventory();
            Storage.currentGuns[plr] += change;
            if (Storage.currentGuns[plr] < 0) { Storage.currentGuns[plr] = 0; }
            
            plr.AddItem(ItemType.ArmorHeavy);
            plr.AddItem(ItemType.KeycardO5);
            try 
            { 
                plr.AddItem(Storage.gungamelist[Storage.currentGuns[plr]]);
            } catch (Exception e) 
            { 
                Log.Debug($"index of {Storage.currentGuns[plr]}. exception {e}");
            }
            ushort ammocount = 5000;
            plr.AddAmmo(ItemType.Ammo12gauge, ammocount);
            plr.AddAmmo(ItemType.Ammo44cal, ammocount);
            plr.AddAmmo(ItemType.Ammo556x45, ammocount);
            plr.AddAmmo(ItemType.Ammo762x39, ammocount);
            plr.AddAmmo(ItemType.Ammo9x19, ammocount);
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        public void PlayerDeath(PlayerDeathEvent ev)
        {
            if (ev.Attacker != null)
            {
                if (Storage.currentGuns[ev.Attacker] > Storage.gungamelist.Count - 1 && nowinner)
                {
                    nowinner = false; // there is in fact a winner
                    Round.End();
                    foreach (Player plr in Player.GetPlayers())
                    {
                        HintHandlers.InitPlayer(plr);
                        HintHandlers.AddFadingText(plr, 900, $"<b>{ev.Attacker.Nickname} wins!</b>", 10f);
                    }
                }
                spawnproperly(ev.Attacker, false, 1);
            }
            spawnproperly(ev.Player, true, -1);
        }
    }
}
