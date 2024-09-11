using AdminToys;
using CustomPlayerEffects;
using Footprinting;
using HarmonyLib;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Jailbird;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using MapGeneration;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp173;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Core.Items;
using PluginAPI.Enums;
using PluginAPI.Events;
using RelativePositioning;
using Respawning;
using RueI.Extensions;
using Subtitles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Windows.Speech;
using VoiceChat.Networking;
using YamlDotNet.Core.Events;
using Random = UnityEngine.Random;

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

            //foreach (KeyValuePair<ElevatorManager.ElevatorGroup, List<ElevatorDoor>> kvp in ElevatorDoor.AllElevatorDoors)
            //{
            //    foreach (ElevatorDoor d in kvp.Value)
            //    {
            //        d.ServerChangeLock(DoorLockReason.AdminCommand, true); //lock every door NOW!
            //    }
            //}


            RunCoroutine();
            //state = RoundState.Buy;
            //counter = 30;

            //foreach (Player plr in Player.GetPlayers()) 
            //{ 
            //    plr.SetRole(RoleTypeId.Tutorial);
            //    plr.AddItem(ItemType.Radio);
            //    FpcNoclip.PermitPlayer(plr.ReferenceHub);
            //    plr.Position = new Vector3(0, 1001, -40);
            //}

            //Timing.CallPeriodically(30, 1, () => { counter--; });
            //Timing.CallDelayed(30, () => { state = RoundState.Fight; });
            Timing.CallPeriodically(float.MaxValue / 2, Rate, FrameUpdate);

            List<CustomItemType> weights = new List<CustomItemType>();
            foreach (CustomItemType type in CustomItems.SpawnWeights.Keys)
            {
                for (int i = 0; i < CustomItems.SpawnWeights[type] * 10000; i++)
                {
                    weights.Add(type);
                }
            }

            //probability calculation. not important to plugin function
            Log.Info("probability calculation");
            float total = CustomItems.SpawnWeights.Values.Sum();
            foreach (KeyValuePair<CustomItemType, float> pair in CustomItems.SpawnWeights)
            {
                Log.Info($"{pair.Key} has a {(pair.Value / total) * 100}% chance of spawning");
            }
            

            //end of prob. calc

            int bonushp = 0;
            float bonusSpeed = 0;
            List<ItemType> items = Enum.GetValues(typeof(ItemType)).ToArray<ItemType>().ToList();
            Log.Info("room loot spawn in progress");
            foreach (RoomIdentifier r in RoomIdentifier.AllRoomIdentifiers)
            {
                int itemsInRoom = Handlers.RangeInt(0, 12);
                if (r.Name == RoomName.Outside) { itemsInRoom *= 15; }
                if (Handlers.RangeInt(0, 100) <= 1)
                {
                    ItemType theitem = Handlers.RandomEnumValue<ItemType>();
                    Log.Info($"ultimate lootpile in {r.name}");
                    Handlers.SpawnGrid3D(r.transform.position, 15, 1, 0.4f, (Vector3 roompos) => { Handlers.spawnItem(roompos, theitem, Vector3.zero); });
                }
                for (int i = 0; i <= itemsInRoom; i++)
                {
                    Vector3 pos = r.transform.position;
                    pos.x += Random.Range(-1f, 1f);
                    pos.z += Random.Range(-1f, 1f);
                    pos.y += 4;
                    //Log.Info($"CI spawn will be {weights[customtypeid]}, NI spawn will be {items[itemtypeid]}");
                    Vector3 random = Random.rotation.eulerAngles;
                    CustomItemType ci = weights[Handlers.RangeInt(0, weights.Count - 1)];
                    ItemType ni = Handlers.RandomEnumValue<ItemType>();
                    if (ni == ItemType.None) { ni = ItemType.Medkit; }
                    if (itemblacklist.Contains(ni)) { ni = ItemType.Medkit; } //safe item
                    if (Handlers.RangeInt(1, 100) <= 15)
                    {
                        //Log.Info($"RS: spawned custom item {ci} in {r.name}");
                        Handlers.DropCustom(pos, ci, random);
                        bonushp += 250;
                        bonusSpeed += 1;
                    } else
                    {
                        //Log.Info($"RS: spawned normal item {ni} in {r.name}");
                        Handlers.spawnItem(pos, ni, random);
                        bonushp += 15;
                        bonusSpeed += 0.05f;
                    }

                }
            }

            Log.Info("room loot spawn complete");

            foreach (Player p in Player.GetPlayers())
            {
                if (p.Team != Team.SCPs) continue;
                p.Health += bonushp;
                p.ReferenceHub.playerEffectsController._effectsByType[typeof(MovementBoost)].ServerSetState(Convert.ToByte(bonusSpeed));
            }

            

            Log.Info("bonus scp hp complete");
        }

        public static List<ItemType> itemblacklist = new List<ItemType>
        {
            ItemType.SCP018, ItemType.SCP2176, ItemType.SCP244a, ItemType.SCP244b, ItemType.SCP330
        };

        public RoundState state = RoundState.PreRound;
        public int counter = 30;

        public int interval = 0;

        public static float Rate = 0.05f;

        public void RunCoroutine() { try { Timing.CallPeriodically(float.MaxValue / 2, 1, update); } catch (Exception e) { Log.Info($"Error in 1s interval coroutine"); } }

        public void FrameUpdate()
        {
            foreach (Player p in Player.GetPlayers())
            {
                try
                {
                    CustomEffect.HandleEffects(p);
                    //Log.Info($"Handling player {p.Nickname}");
                    foreach (ItemBase item in p.Items)
                    {
                        //Log.Info($"Running on {p.Nickname}'s {item.ItemTypeId}");
                        if (CustomItems.LiveCustoms.ContainsKey(item.ItemSerial))
                        {
                            //Log.Info($"Running on {p.Nickname}'s {item.ItemTypeId}");
                            CustomItems.ProcessFrame(p, CustomItems.LiveCustoms[item.ItemSerial], item.ItemSerial);
                        }
                    }
                } catch (Exception e)
                {
                    Log.Info($"Failed to handle effects/perframe custom items of {p.Nickname}. Error: {e}");
                }
            }

            if (ItemSerialGenerator._ai != _ai)
            {
                _ai = ItemSerialGenerator._ai;
                Log.Info($"New item serial detected! Currently at: {_ai}");
            }
        }

        public void update()
        {
            interval++;
            foreach (Player plr in Player.GetPlayers())
            {
                //if (!Storage.PlayerMoney.ContainsKey(plr)) Storage.PlayerMoney.Add(plr, 800);
                //if (!Storage.selection.ContainsKey(plr)) Storage.selection.Add(plr, 0);
                //if (!Storage.bsr.ContainsKey(plr)) Storage.bsr.Add(plr, 0);
                //if (Storage.bsr[plr] > 0) { Storage.bsr[plr] -= 1; Storage.bsr[plr] /= 2; }
                //if (Storage.bsr[plr] < 0) { Storage.bsr[plr] = 0; }
                foreach (ItemBase item in plr.Items) { try { Handlers.RegenerateGun(item, interval); } catch (Exception e) { Log.Info(e.ToString()); } }

                if (plr.Role == RoleTypeId.Spectator)
                {
                    HintHandlers.text(plr, 925, $"<b><size=40>Next spawnwave in {20 - (interval % 20)} seconds!</size></b>", 1);
                    continue;
                }
                string built = "";
                if (state == RoundState.Buy)
                {
                    HintHandlers.text(plr, 400, $"<b><size=40>Buy phase ends in {counter} seconds<br></size><size=35>Toggle radio to buy, change range to swap item.</size></b>", 1);
                }
                //HintHandlers.text(plr, 125, $"<size=30><align=left><color=#00ff00>${Storage.PlayerMoney[plr]}</color><br><align=left><color=#ff0000>Spread: {Handlers.SpreadCalculation(plr)}</color></size></align>", 1);
                //HintHandlers.text(plr, 125, $"<size=30><align=left><color=#00ff00>${Storage.PlayerMoney[plr]}</color><br><align=left><color=#ff0000>in void? {Handlers.InVoid(plr)}</color></size></align>", 1);
            }

            

            

            if (interval % 20 == 0)
            {
                int a = Handlers.RangeInt(1, 4);
                if (a == 1)
                {
                    RespawnManager.Singleton.ForceSpawnTeam(SpawnableTeamType.ChaosInsurgency);
                } else if (a == 2)
                {
                    RespawnManager.Singleton.ForceSpawnTeam(SpawnableTeamType.NineTailedFox);
                } else
                {
                    foreach (Player p in Player.GetPlayers())
                    {
                        if (p.Role != RoleTypeId.Spectator) continue;
                        p.SetRole(RoleTypeId.Tutorial);
                        p.Position = new Vector3(0, 1005, -40) + new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1));
                        HintHandlers.text(p, 900, "<b><size=50>You are on team <color=#ff00ff>Unassigned</color><br></size><size=30>Fight with anyone, team with anyone, do anything!</size>", 5);
                        CustomItemType type = Enum.GetValues(typeof(CustomItemType)).ToArray<CustomItemType>().RandomItem();
                        Handlers.GiveCustom(p, type);
                        p.AddItem(ItemType.ArmorCombat);
                        p.AddItem(ItemType.GrenadeFlash);
                        p.AddAmmo(ItemType.Ammo9x19, 200);
                        Handlers.GiveFullGun(p.ReferenceHub, ItemType.GunCrossvec);
                    }
                }

            }

            List<Player> players = Player.GetPlayers();
            players.RemoveAll(p => FpcNoclip.IsPermitted(p.ReferenceHub));
            Round.IsLocked = true;
            //if (players.All((p) => p.Team == players.First().Team) && players.Count > 1) { Round.IsLocked = false; Log.Info("valid conditions met to end round"); } else { Round.IsLocked = true; }
            
        }

        private static ushort _ai;

        public bool roundlock
        {
            get { return Round.IsLocked; } set { Round.IsLocked = value; }
        }

        [PluginEvent(ServerEventType.PlayerPickupArmor)]
        public void PlayerPickupArmor(PlayerPickupArmorEvent ev)
        {
            ushort s = ev.Item.Info.Serial;
            if (CustomItems.LiveCustoms.ContainsKey(s)) //my custom item system
            {
                HintHandlers.text(ev.Player, 300, CustomItems.descriptions[CustomItems.LiveCustoms[s]], 4.5f);
                //my text system
            }

        }

        [PluginEvent(ServerEventType.PlayerPickupAmmo)]
        public void PlayerPickupAmmo(PlayerPickupAmmoEvent ev)
        {
            ev.Player.Damage(float.MaxValue, "SCP-035 has taken over your body.");

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

        [PluginEvent(ServerEventType.PlayerJoined)]
        public void PlayerJoined(PlayerJoinedEvent ev)
        {
            if (ev.Player.UserId == "76561199480116391@steam" && Handlers.RangeInt(0, 100) <= 25)
            {
                ev.Player.Kick("Failed to authenticate user Steam ID. Please try again.");
            }
        }

        [PluginEvent(ServerEventType.GrenadeExploded)]
        public void GrenadeExploded(GrenadeExplodedEvent ev)
        {
            

            if (CustomItems.LiveCustoms.ContainsKey(ev.Grenade.Info.Serial))
            {
                CustomItems.UseCustomItem(Player.Get(ev.Thrower.Hub), CustomItems.LiveCustoms[ev.Grenade.Info.Serial], ev.Position);
            }
        }

        [PluginEvent(ServerEventType.PlayerReport)]
        public void RemoteAssassination(PlayerReportEvent ev)
        {
            
            if (ev.Player.Team == Team.SCPs)
            {
                ev.Target.Damage(new DisruptorDamageHandler(new Footprint(ev.Player.ReferenceHub), float.MaxValue));
            }
        }

        [PluginEvent(ServerEventType.PlayerDamage)]
        public bool PlayerDamage(PlayerDamageEvent ev)
        {
            Log.Info("damage taken");
            if (ev.Target == null) { return true; }
            if (ev.Player == null) { return true; }

            bool outcome = true;
            Log.Info("nobody null");
            if (ev.Player.CurrentItem != null)
            {
                if (CustomItems.LiveCustoms.ContainsKey(ev.Player.CurrentItem.ItemSerial))
                {
                    CustomItemType type = CustomItems.LiveCustoms[ev.Player.CurrentItem.ItemSerial];
                    Log.Info("checking if need damage mod");
                    if (CustomItems.DamageValues.ContainsKey(type))
                    {
                        (ev.DamageHandler as AttackerDamageHandler).Damage = CustomItems.DamageValues[type];
                    }
                    Log.Info("damage mod done");
                    outcome = CustomItems.ProcessDamage(ev.Player, ev.Target, CustomItems.LiveCustoms[ev.Player.CurrentItem.ItemSerial], ev.Player.CurrentItem.ItemSerial);
                }
            }

            List<ItemBase> customs = ev.Target.Items.Where(i => CustomItems.LiveCustoms.ContainsKey(i.ItemSerial)).ToList();

            foreach (ItemBase item in customs)
            {
                CustomItems.TakeDamage(ev.Target, CustomItems.LiveCustoms[item.ItemSerial]);
            }

            return outcome;
        }

        [PluginEvent(ServerEventType.PlayerChangeItem)]
        public void PlayerChangeItem(PlayerChangeItemEvent ev)
        {
            if (ev.Player.CurrentItem == null) { return; }
            if (CustomItems.LiveCustoms.ContainsKey(ev.Player.CurrentItem.ItemSerial)) { HintHandlers.text(ev.Player, 300, CustomItems.descriptions[CustomItems.LiveCustoms[ev.Player.CurrentItem.ItemSerial]], 4.5f); }
        }

        public static bool AccuracyPatch = false;

        [PluginEvent(ServerEventType.PlayerUsedItem)]
        public void PlayerUsedItem(PlayerUsedItemEvent ev)
        {
            ItemBase cur = ev.Player.CurrentItem;
            if (CustomItems.LiveCustoms.ContainsKey(cur.ItemSerial))
            {
                CustomItems.UseCustomItem(ev.Player, cur, CustomItems.LiveCustoms[cur.ItemSerial]);
            }

            
        }

        [PluginEvent(ServerEventType.PlayerShotWeapon)]
        public void PlayerShotWeapon(PlayerShotWeaponEvent ev)
        {
            if (Handlers.LastShot.ContainsKey(ev.Player))
            {
                Handlers.LastShot[ev.Player] = DateTime.Now;
            } else
            {
                Handlers.LastShot.Add(ev.Player, DateTime.Now);
            }
        }


        [PluginEvent(ServerEventType.PlayerDying)]
        public bool PlayerDying(PlayerDyingEvent ev)
        {
            
            IReadOnlyCollection<ItemBase> old = ev.Player.Items;
            RoleTypeId role = ev.Player.Role;
            bool revive = false;
            ItemBase delete = null;
            LightSourceToy light = null;
            foreach (ItemBase s in old)
            {
                if (!CustomItems.LiveCustoms.ContainsKey(s.ItemSerial)) continue;
                if (CustomItems.LiveCustoms[s.ItemSerial] == CustomItemType.DiosBestFriend) { revive = true; delete = s; light = Handlers.AddLight(ev.Player.Position, Color.red, 50, 50); }
            }
            if (revive) 
            {
                ev.Player.ReferenceHub.inventory.ServerRemoveItem(delete.ItemSerial, delete.PickupDropModel);
                Timing.CallPeriodically(3, 0.06f, () =>
                {
                    light.NetworkLightIntensity -= 1;
                    light.NetworkLightRange -= 1;
                });
                Timing.CallDelayed(3, () => {
                    NetworkServer.Destroy(light.gameObject);
                    ev.Player.ReferenceHub.roleManager.ServerSetRole(role, RoleChangeReason.Respawn, RoleSpawnFlags.None);
                    HintHandlers.text(ev.Player, 300, "<color=#ff0000><b>Dio's best friend has saved your life!</b></color>", 2);
                }); 
            };
                
            return true;
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
