using CustomPlayerEffects;
using HarmonyLib;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Jailbird;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using MapGeneration;
using MapGeneration.Distributors;
using MEC;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp173;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Windows.Speech;
using Random = System.Random;

namespace secret_project
{
    internal class EventHandlers
    {
        [PluginEvent(ServerEventType.RoundStart)]
        public async void RoundStart(RoundStartEvent ev)
        {
            ItemPickupBase.OnPickupAdded += Handlers.DroppedItem;
            ItemPickupBase.OnPickupDestroyed += Handlers.DeletedItem;
            await Task.Delay(50); //ummm this prevents some issues
            Round.IsLocked = true;
            
            foreach (KeyValuePair<ElevatorManager.ElevatorGroup, List<ElevatorDoor>> kvp in ElevatorDoor.AllElevatorDoors)
            {
                foreach (ElevatorDoor d in kvp.Value)
                {
                    d.ServerChangeLock(DoorLockReason.AdminCommand, true); //lock every door NOW!
                }
            }

            RunCoroutine();
            //state = RoundState.Buy;
            //counter = 30;
            foreach (Player plr in Player.GetPlayers()) 
            { 
                plr.SetRole(RoleTypeId.Tutorial);
                plr.AddItem(ItemType.Radio);
                FpcNoclip.PermitPlayer(plr.ReferenceHub);
                plr.Position = new Vector3(0, 1001, -40);
            }
            //Timing.CallPeriodically(30, 1, () => { counter--; });
            Timing.CallDelayed(30, () => { state = RoundState.Fight; });
            
        }

        public RoundState state = RoundState.PreRound;
        public int counter = 30;

        public int interval = 0;

        public void RunCoroutine() { Timing.CallPeriodically(float.MaxValue / 2, 1, update); }
        public void update()
        {
            interval++;
            foreach (Player plr in Player.GetPlayers())
            {
                if (!Storage.PlayerMoney.ContainsKey(plr)) Storage.PlayerMoney.Add(plr, 800);
                if (!Storage.selection.ContainsKey(plr)) Storage.selection.Add(plr, 0);
                if (!Storage.bsr.ContainsKey(plr)) Storage.bsr.Add(plr, 0);
                if (Storage.bsr[plr] > 0) { Storage.bsr[plr] -= 1; Storage.bsr[plr] /= 2; }
                if (Storage.bsr[plr] < 0) { Storage.bsr[plr] = 0; }
                foreach (ItemBase item in plr.Items) { Handlers.RegenerateGun(item, interval); Log.Debug("regenerating guns"); }

                if (plr.Role == RoleTypeId.Spectator) return;
                string built = "";
                if (state == RoundState.Buy)
                {
                    HintHandlers.text(plr, 400, $"<b><size=40>Buy phase ends in {counter} seconds<br></size><size=35>Toggle radio to buy, change range to swap item.</size></b>", 1);
                }
                //HintHandlers.text(plr, 125, $"<size=30><align=left><color=#00ff00>${Storage.PlayerMoney[plr]}</color><br><align=left><color=#ff0000>Spread: {Handlers.SpreadCalculation(plr)}</color></size></align>", 1);
                //HintHandlers.text(plr, 125, $"<size=30><align=left><color=#00ff00>${Storage.PlayerMoney[plr]}</color><br><align=left><color=#ff0000>in void? {Handlers.InVoid(plr)}</color></size></align>", 1);
            }
        }

        //[PluginEvent(ServerEventType.PlayerRadioToggle)]
        //public bool PlayerRadioToggle(PlayerRadioToggleEvent ev)
        //{
        //    Log.Info("dropped item. buying");
        //    int val = Storage.selection[ev.Player];
        //    GunInfo selected = Storage.guncosts[(Storage.guncosts.Keys.ToArray()[val])];
        //    if (Storage.PlayerMoney[ev.Player] >= selected.cost)
        //    {
        //        Storage.PlayerMoney[ev.Player] -= selected.cost;
        //        Handlers.GiveFullGun(ev.Player.ReferenceHub, selected.item);
        //        HintHandlers.text(ev.Player, 300, $"<b>Bought {selected.item} for ${selected.cost}</b>", 1f);
        //    }
        //    return false;
        //}
        //
        //public static string lines(int x) { return string.Concat(Enumerable.Repeat("<br>", x)); }
        //
        //[PluginEvent(ServerEventType.PlayerChangeRadioRange)]
        //public bool PlayerChangeRadioRange(PlayerChangeRadioRangeEvent ev)
        //{
        //    if (state == RoundState.Buy)
        //    {
        //        Log.Info("changed item. swapping");
        //        Storage.selection[ev.Player]++;
        //        if (Storage.selection[ev.Player] > Storage.guncosts.Count - 1) { Storage.selection[ev.Player] = 0; }
        //        int val = Storage.selection[ev.Player];
        //        GunInfo selected = Storage.guncosts[(Storage.guncosts.Keys.ToArray()[val])];
        //        HintHandlers.text(ev.Player, 300, $"</align><b>{selected.item}<br><color=#00ff00>Costs ${selected.cost}. Earns ${selected.killReward} on kill.</b>", 1f);
        //    }
        //    return state != RoundState.Buy;
        //}

        [PluginEvent(ServerEventType.PlayerDeath)]
        public void PlayerDeath(PlayerDeathEvent ev)
        {
            if (ev.Attacker != null)
            {
                if (Storage.guncosts.ContainsKey(ev.Attacker.CurrentItem.ItemTypeId))
                {
                    Storage.PlayerMoney[ev.Attacker] += Storage.guncosts[ev.Attacker.CurrentItem.ItemTypeId].killReward;
                } else
                {
                    Storage.PlayerMoney[ev.Attacker] += 250;
                }
            }
        }

        [PluginEvent(ServerEventType.PlayerChangeItem)]
        public void PlayerChangeItem(PlayerChangeItemEvent ev)
        {
            if (ev.Player.CurrentItem == null) { return; }
            if (CustomItems.LiveCustoms.ContainsKey(ev.Player.CurrentItem.ItemSerial)) { HintHandlers.text(ev.Player, 300, CustomItems.descriptions[CustomItems.LiveCustoms[ev.Player.CurrentItem.ItemSerial]], 2f); }
        }

        public static bool AccuracyPatch = true;

        [PluginEvent(ServerEventType.PlayerShotWeapon)]
        public void PlayerShotWeapon(PlayerShotWeaponEvent ev)
        {
            Storage.bsr[ev.Player]++;
        }

        [PluginEvent(ServerEventType.PlayerDamage)]
        public void PlayerDamage(PlayerDamageEvent ev)
        {
            if (((StandardDamageHandler)ev.DamageHandler).Hitbox == HitboxType.Headshot)
            {
                ((StandardDamageHandler)ev.DamageHandler).Damage *= 2.2f;
            } else
            {
                ((StandardDamageHandler)ev.DamageHandler).Damage *= 0.6f;
            }
        }

        [PluginEvent(ServerEventType.PlayerUsedItem)]
        public void PlayerUsedItem(PlayerUsedItemEvent ev)
        {
            ItemBase cur = ev.Player.CurrentItem;
            if (CustomItems.LiveCustoms.ContainsKey(cur.ItemSerial))
            {
                CustomItems.UseCustomItem(ev.Player, cur, CustomItems.LiveCustoms[cur.ItemSerial]);
            }
        }
    }

    public enum RoundState
    {
        PreRound,
        Buy,
        Fight,
        BombPlanted,
        PostRound
    }
}
