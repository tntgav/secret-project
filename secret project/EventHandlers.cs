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
            foreach (Player plr in Player.GetPlayers())
            {
                
                Storage.currentGuns.Add(plr, 0);
                spawnproperly(plr);
                
            }
        }

        public void spawnproperly(Player plr, bool respawn = false)
        {
            if (respawn)
            {
                plr.SetRole(RoleTypeId.ChaosConscript);
                plr.Position = Handlers.RandomRoom().transform.position;
                plr.Position = new Vector3(plr.Position.x, plr.Position.y + 0.7f, plr.Position.z);
            }
            plr.ClearInventory();
            plr.AddItem(Storage.gungamelist[Storage.currentGuns[plr]]);
        }
    }
}
