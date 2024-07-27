using AdminToys;
using CustomPlayerEffects;
using Footprinting;
using Hints;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using JetBrains.Annotations;
using MapGeneration;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Events;
using RueI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;

namespace secret_project
{
    public static class CustomItems
    {
        public static Dictionary<ushort, CustomItemType> LiveCustoms = new Dictionary<ushort, CustomItemType>();
        public static Dictionary<ushort, LightSourceToy> LiveLights = new Dictionary<ushort, LightSourceToy>();
        public static async void UseCustomItem(Player plr, ItemBase item, CustomItemType type, Vector3 hit = new Vector3())
        {
            if (item == null) return; //weirdly common...
            if (hit.magnitude != 0) //there is a hit
            {
                if (type == CustomItemType.GrenadeLauncher) { Handlers.GrenadePosition(hit, plr); }
                if (type == CustomItemType.MitzeyScream) { for (int i = 0; i < 5; i++) { Handlers.CreateThrowable(ItemType.GrenadeHE).SpawnActive(hit, 0.05f, plr); } }
                if (type == CustomItemType.MiniNukeLauncher) { AlphaWarheadController.Singleton.Detonate(); }
                if (type == CustomItemType.LightningTest)
                {
                    GameObject a = new GameObject();
                    a.transform.position = hit;
                    float thickness = 0.3f;
                    PrimitiveObjectToy prim = Handlers.SpawnPrim(hit, new Vector3(thickness, 100f, thickness), Vector3.zero, new Color(0.02f, 0.85f, 1), PrimitiveType.Cylinder, false);
                    LightSourceToy light = Handlers.AddLight(hit, new Color(0.02f, 0.85f, 1), 40, 40);
                    Handlers.GrenadePosition(hit, plr);
                    await Task.Delay(500);
                    NetworkServer.Destroy(prim.gameObject);
                    NetworkServer.Destroy(light.gameObject);
                }

                

                if (type == CustomItemType.MinionGun)
                {
                    int specs = 0;
                    List<Player> spectators = new List<Player>();
                    foreach (Player user in Player.GetPlayers()) { if (user.Role == RoleTypeId.Spectator) { specs++; spectators.Add(user); } }
                    if (specs > 0)
                    {
                        Player first = spectators.First();
                        first.Role = plr.Role;
                        first.Position = hit;
                        first.Position = new Vector3(first.Position.x, first.Position.y + 0.7f, first.Position.z);
                    }
                }

                if (type == CustomItemType.GravityGun)
                {
                    LightSourceToy light = Handlers.AddLight(hit, new Color(0.5f, 0, 0.65f), 0.5f, 0.5f);
                    int i = 0;
                    Timing.CallPeriodically(5, 0.05f, () =>
                    {
                        i++;
                        light.NetworkLightIntensity += 1f;
                        light.NetworkLightRange += 1f;
                        foreach (Player near in Player.GetPlayers())
                        {
                            float dist = Vector3.Distance(hit, near.Position);
                            if (dist > 0.5 + (i * 0.5)) continue;
                            if (plr.Team == near.Team) continue; //cant move teammates but can move self
                            near.Position = Vector3.Lerp(hit, near.Position, 0.935f);
                        }

                        foreach (ItemPickupBase cur in Handlers.items)
                        {
                            float dist = Vector3.Distance(hit, cur.Position);
                            if (dist > 0.5 + (i * 0.5)) continue;
                            cur.Position = Vector3.Lerp(hit, cur.Position, 0.935f);
                        }
                    });
                    Timing.CallDelayed(5.2f, () => { Handlers.GrenadePosition(hit, plr); NetworkServer.Destroy(light.gameObject); });
                }

                if (type == CustomItemType.Grappler)
                {
                    Timing.CallPeriodically(3, 0.02f, () =>
                    {
                        plr.Position = Vector3.Lerp(hit, plr.Position, 0.9f);
                    });
                }

                if (type == CustomItemType.Arcer)
                {
                    Log.Info("attempting primitive spawn");
                    PrimitiveObjectToy prim = Handlers.SpawnPrim(hit, Vector3.zero, Vector3.zero, new Color(0, 0, 0, 0.99f), PrimitiveType.Sphere, false);
                    Log.Info("spawned primitive");
                    float fintime = 3;
                    Timing.CallPeriodically(fintime, 0.02f, () =>
                    {
                        //Log.Info("attempting size increase");
                        float curscale = prim.transform.localScale.x;
                        float lerped = Mathf.Lerp(curscale, 5.2f, 0.035f);
                        Vector3 newscale = new Vector3(lerped, lerped, lerped);
                        prim.transform.localScale = newscale;
                        //prim.NetworkMovementSmoothing = 20;
                    });
                    Timing.CallDelayed(fintime, () =>
                    {
                        Log.Info("handling explosion");
                        NetworkServer.Destroy(prim.gameObject);
                        foreach (Player p in Player.GetPlayers())
                        {
                            float dist = Vector3.Distance(p.Position, hit);
                            bool valid = (dist <= 5.2f && p.Team != plr.Team);
                            if (valid && p.Team != Team.SCPs) { p.Damage(float.MaxValue, "Crushed by the immense weight of the void."); }
                            else if (valid && p.Team == Team.SCPs) { p.Damage((p.Health / 10) + 100, "Crushed by the immense weight of the void"); }
                            //if (dist <= 5.2f && p == plr) { p.Damage(100, "You have been detained. Await your sentance at the end of time."); }
                        }
                    });
                    
                }

                if (type == CustomItemType.GlowGun)
                {
                    Handlers.AddLight(hit, new Color(
                        Handlers.RangeInt(0, 255) / 255f,
                        Handlers.RangeInt(0, 255) / 255f,
                        Handlers.RangeInt(0, 255) / 255f
                        ), 25, 25);
                }

                if (type == CustomItemType.BacconsFunGun)
                {
                    List<Player> nearby = Player.GetPlayers().FindAll(p => Vector3.Distance(p.Position, hit) < 2);
                    foreach (Player p in nearby)
                    {
                        Handlers.SetEffect<Strangled>(p, 1, 60);
                    }
                }

            } else //non gun custom item
            {
                if (type == CustomItemType.Freaky500) { HintHandlers.text(plr, 300, "You feel an overwhelming sense of freakiness...", 3); plr.Health = float.MinValue; }
                if (type == CustomItemType.Heroin) { plr.Damage(6000, "Heroin hurts..."); }
                if (type == CustomItemType.Hyperion) 
                { 
                    plr.ReferenceHub.characterClassManager.GodMode = true;
                    Handlers.GrenadePosition(plr.Position, plr);
                    Timing.CallDelayed(0.6f, () => {
                        plr.ReferenceHub.characterClassManager.GodMode = false;
                    });
                    
                }
                if (type == CustomItemType.AcesBrew)
                {
                    for (int i = 0; i < 17; i++)
                    {
                        Handlers.CreateThrowable(ItemType.GrenadeHE).SpawnActive(plr.Position, 5, plr);
                    }
                }

                if (type == CustomItemType.StatBuffer)
                {
                    int choice = Handlers.RangeInt(1, 2); // 1 = speed, 2 = health, 3 = random positive effect
                    if (choice == 1)
                    {
                        plr.ReferenceHub.playerEffectsController.AllEffects.ForEach((e) =>
                        {
                            if (e.GetType() == typeof(MovementBoost))
                            {
                                e.Intensity += 15;
                            }
                        });
                    } else if (choice == 2)
                    {
                        plr.GetStatModule<AhpStat>().ServerAddProcess(60, float.MaxValue, 0, 1, 1, true);
                    }
                }

                if (type == CustomItemType.PortalOpener)
                {
                    RoomIdentifier room = Handlers.RandomRoom();
                    float size = 4;
                    Vector3 pos = room.transform.position + new Vector3(0, 1, 0);
                    //LightSourceToy entryL = Handlers.AddLight(plr.Position, new Color(1, 0, 1), 2, float.MaxValue);
                    PrimitiveObjectToy entryP = Handlers.SpawnPrim(plr.Position, new Vector3(size, size, size), Vector3.zero, new Color(0, 0, 0, 0.99f), PrimitiveType.Sphere, false);

                    //LightSourceToy exitL = Handlers.AddLight(pos, new Color(1, 0, 1), 2, float.MaxValue);
                    PrimitiveObjectToy exitP = Handlers.SpawnPrim(pos, new Vector3(size, size, size), Vector3.zero, new Color(0, 0, 0, 0.99f), PrimitiveType.Sphere, false);
                    plr.ReferenceHub.playerEffectsController.EnableEffect<Flashed>(4);
                    Timing.CallDelayed(1, () => { plr.Position = pos; });
                    

                    Timing.CallDelayed(3, () =>
                    {
                        //NetworkServer.Destroy(entryL.gameObject);
                        NetworkServer.Destroy(entryP.gameObject);
                        //NetworkServer.Destroy(exitL.gameObject);
                        NetworkServer.Destroy(exitP.gameObject);
                    });
                }
            }
        }

        public static void UseCustomItem(Player plr, CustomItemType type, Vector3 hit = new Vector3())
        {
            if (type == CustomItemType.TsarBomb)
            {
                List<Vector3> pos = new List<Vector3>();
                int x = 4;
                int y = 4;

                float combined = x * x * y;
                Handlers.SpawnGrid3D(hit, x, y, 0.1f, (p) => { pos.Add(p); });
                pos.ShuffleList();
                int index = 0;
                Timing.CallPeriodically(2, 2/combined, () =>
                {
                    ThrowableItem item = Handlers.CreateThrowable(ItemType.GrenadeHE);
                    LiveCustoms.Add(item.ItemSerial, CustomItemType.NullGrenade);
                    item.SpawnActive(pos[index], 0.5f, plr);
                    index++;
                });
            }

            if (type == CustomItemType.NullGrenade)
            {
                List<Vector3> pos = new List<Vector3>();
                int x = 4;
                int y = 4;

                float combined = x * x * y;
                Handlers.SpawnGrid3D(hit, x, y, 0.1f, (p) => { pos.Add(p); });
                pos.ShuffleList();
                int index = 0;
                Timing.CallPeriodically(2, 2 / combined, () =>
                {
                    ThrowableItem item = Handlers.CreateThrowable(ItemType.GrenadeHE);
                    item.SpawnActive(pos[index], 0.5f, plr);
                    index++;
                });
            }

            if (type == CustomItemType.Infininade)
            {
                ThrowableItem ipb = Handlers.CreateThrowable(ItemType.GrenadeHE);
                LiveCustoms.Add(ipb.ItemSerial, CustomItemType.Infininade);
                Vector3 velocity = UnityEngine.Random.insideUnitSphere * 15;
                ipb.SpawnActive(hit, 2, plr, velocity);
            }
        }

        public static bool ProcessDamage(Player Attacker, Player Target, CustomItemType type, ushort serial = 0)
        {
            if (type == CustomItemType.BlindingBarrage && Attacker.Team != Target.Team) { Target.ReferenceHub.playerEffectsController.EnableEffect<Flashed>(3, false); }
            if (type == CustomItemType.GlowGun) 
            {
                Handlers.AddLight(Attacker.ReferenceHub.transform, new Color(
                        Handlers.RangeInt(0, 255) / 255f,
                        Handlers.RangeInt(0, 255) / 255f,
                        Handlers.RangeInt(0, 255) / 255f
                        ), 25, 25);
            }

            if (type == CustomItemType.Ego)
            {
                if (!customitemdata.ContainsKey(serial)) return true;
                List<ItemType> consumed = customitemdata[serial];
                Log.Info($"running ego actives {string.Join(", ", consumed)}");

                if (consumed.Contains(ItemType.GrenadeHE) && Handlers.RangeInt(1, 100) <= 5) { Handlers.GrenadePosition(Target.Position, Attacker); }
                if (consumed.Contains(ItemType.GrenadeFlash) && Handlers.RangeInt(1, 100) <= 5) { Handlers.CreateThrowable(ItemType.GrenadeFlash).SpawnActive(Target.Position, 0.05f, Attacker); }
                if (consumed.Contains(ItemType.SCP018) && Handlers.RangeInt(1, 100) <= 1) { Handlers.CreateThrowable(ItemType.SCP018).SpawnActive(Target.Position, 90, Attacker); }
                if (consumed.Contains(ItemType.SCP207)) { Handlers.AddEffect<MovementBoost>(Attacker, 3, 5); }
            }

            

            if (type == CustomItemType.SiphoningSyringe && Attacker.Team != Target.Team) 
            {
                float damageDealt = DamageValues[CustomItemType.SiphoningSyringe];
                float perheal = damageDealt / 2;
                Attacker.GetStatModule<AhpStat>().ServerAddProcess(perheal, 250, 0, 1, float.MaxValue, true);
                List<Player> teammates = Player.GetPlayers().Where((p) => p.Team == Attacker.Team).ToList();
                foreach (Player p in teammates) 
                {
                    p.GetStatModule<AhpStat>().ServerAddProcess(perheal / teammates.Count, 250, 0, 1, float.MaxValue, true);
                }
            }

            if (type == CustomItemType.Invisgun)
            {
                Target.ReferenceHub.playerEffectsController.EnableEffect<Invisible>(3, true);
            }

            if (type == CustomItemType.LaserCannon)
            {
                Target.ReferenceHub.characterClassManager.GodMode = false;
                Target.Health = 1;
                Target.GetStatModule<AhpStat>().CurValue = 0;
                float thickness = 1.5f;
                PrimitiveObjectToy prim = Handlers.SpawnPrim(Target.Position, new Vector3(thickness, 100f, thickness), Vector3.zero, new Color(0.02f, 0.85f, 1, 0.99f), PrimitiveType.Cylinder, false);
                LightSourceToy light = Handlers.AddLight(Target.Position, new Color(0.02f, 0.85f, 1), 100, 100);
                Timing.CallDelayed(1, () =>
                {
                    NetworkServer.Destroy(prim.gameObject);
                    NetworkServer.Destroy(light.gameObject);
                });
                try { Target.Damage(new DisruptorDamageHandler(new Footprint(ReferenceHub.HostHub), float.MaxValue)); } catch (Exception e) { Log.Info($"IGNORE: {e}"); }
            }

            if (type == CustomItemType.TougherTimes)
            {
                if (Handlers.RangeInt(0, 100) <= 60) { HintHandlers.text(Target, 300, "Damage blocked!", 1f); return false; }
            }

            return true;
        }

        public static void ProcessFrame(Player plr, CustomItemType type, ushort serial = 0)
        {
            float rate = EventHandlers.Rate;
            if (type == CustomItemType.PersonalShieldGenerator)
            {
                float ahp = plr.GetStatModule<AhpStat>().CurValue;
                plr.GetStatModule<AhpStat>().ServerAddProcess((float)(5 * rate / Math.Pow(ahp/10, 2)), float.MaxValue, 0, 1, 0, true);
            }

            if (type == CustomItemType.Ego)
            {
                if (!customitemdata.ContainsKey(serial)) return;
                List<ItemType> consumed = customitemdata[serial];
                Log.Info($"running ego passives {string.Join(", ", consumed)}");
                
                if (consumed.Contains(ItemType.Painkillers)) { plr.Heal(4 * rate); }
                if (consumed.Contains(ItemType.Medkit)) { plr.Heal(8 * rate); }
                if (consumed.Contains(ItemType.SCP500)) { plr.Heal(16 * rate); }
                if (consumed.Contains(ItemType.Adrenaline)) { plr.GetStatModule<AhpStat>().ServerAddProcess(1, 75, 0, 0.7f, 1, true); }
                if (consumed.Contains(ItemType.KeycardO5) || consumed.Contains(ItemType.KeycardFacilityManager) || consumed.Contains(ItemType.KeycardMTFCaptain)) { plr.ReferenceHub.serverRoles.BypassMode = true; }
                if (consumed.Contains(ItemType.SCP268) && (DateTime.Now - Handlers.LastShot[plr]).TotalSeconds > 5) { Handlers.SetEffect<Invisible>(plr, 1, 1); } else { Handlers.RemoveEffect<Invisible>(plr); }
            }
        }


        public static void OnDeath(Player p, CustomItemType type, ItemBase item)
        {
            if (type == CustomItemType.DiosBestFriend) 
            {
                //p.ReferenceHub.inventory.ServerRemoveItem(item.ItemSerial, item.PickupDropModel);
                Log.Info("saving role");
                
                RoleTypeId r = p.Role;
                Log.Info("role saved , changing role");
                p.ReferenceHub.roleManager.ServerSetRole(r, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);
                Log.Info("role changed, sending hint");
                HintHandlers.text(p, 300, "<color=#ff0000><b>Dio's best friend has saved your life!</b></color>", 2);
            }
        }

        public static Dictionary<CustomItemType, string> descriptions = new Dictionary<CustomItemType, string>
        {
            { CustomItemType.GrenadeLauncher, "<color=#ffff00><b>Grenade Launcher</b></color><br><br>Shoots a grenade, simple as that.<br>Regenerates 1 ammo every 30 seconds, up to 30."},
            { CustomItemType.MiniNukeLauncher, "<color=#ff00ff><b>Miniature Nuke Launcher</b></color><br><br>Please don't try this.<br>Regenerates 5 ammo every second up to 5."},
            { CustomItemType.Freaky500, "<color=#ff00ff><b>Freaky 500</b></color><br><br>Makes you feel... freaky..."},
            { CustomItemType.Heroin, "<color=#00ff00><b>Heroin</b></color><br><br>You might OD on this."},
            { CustomItemType.MitzeyScream, "<color=#ff00ff><b>Mitzey Scream</b></color><br><br>This is a bad idea.<br>Regenerates 200 ammo every second up to 200."},
            { CustomItemType.LightningTest, "<color=#ff00ff><b>Lightning blaster</b></color><br><br>im sorry this is so bad pers0n<br>Regenerates 200 ammo every second up to 200."},
            { CustomItemType.MinionGun, "<color=#ffff00><b>Minion gun</b></color><br><br>Revives someone to be your minion for 30 seconds.<br>Regenerates 1 ammo every 90 seconds up to 5." },
            { CustomItemType.GravityGun, "<color=#8833bb><b>Gravity gun</b></color><br><br>Slowly pulls targets in, before exploding<br>Regenerates 1 ammo every 3 seconds up to 5." },
            { CustomItemType.Grappler, "<color=#ff00ff><b>Grappler</b></color><br><br>Slides you towards the place you shoot.<br>Regenerates 1 ammo every 3 seconds up to 5."},
            { CustomItemType.Arcer, "<color=#ff00ff><b>Void's Implosion</b></color><br><br>Creates a devestating void implosion.<br>Regenerates 1 ammo every 25 seconds up to 5."},
            { CustomItemType.ShatteringJustice, "<color=#ff0000><b>Shattering Justice</b></color><br><br>\"Crush them... and then some...\"<br>Regenerates 200 ammo every second up to 200." },
            { CustomItemType.SmokeBomb, "<color=#888888><b>Super flashbang</b></color><br><br>Don't even gotta throw it, it's just that bright!" },
            { CustomItemType.BlindingBarrage, "<color=#5539cc><b>Blinding Barrage</b></color><br><br>Blinds hit enemies, but deals extremely low damage<br>Regenerates 20 ammo every second up to 60." },
            { CustomItemType.Hyperion, "<color=#ff00ff><b>Hyperion</b></color><br><br>haha funny boomstick" },
            { CustomItemType.AcesBrew, "<color=#79AD52><b>Ace's Brew</b></color><br><br>slop slop slop" },
            { CustomItemType.GlowGun, "<color=#ADD8E6><b>Glow Gun</b></color><br><br>Shoots random glow.<br>Regenerates 200 ammo every second up to 200." },
            { CustomItemType.DiosBestFriend, "<color=#ADD8E6><b>Dio's best friend</b></color><br><br>Prevents death once." },
            { CustomItemType.Invisgun, "<color=#0000bb><b>Invis-gun</b></color><br><br>Deals heavy damage, but makes the target invisible.<br>Regenerates 1 ammo every second up to 3." },
            { CustomItemType.StatBuffer, "<color=#33ff22><b>Stat-Buffer 9000</b></color><br><br>Permanently increases one of your stats a small amount." },
            { CustomItemType.LaserCannon, "<color=#ff00ff><b>Laser Cannon</b></color><br><br>If you've got your hands on this, you know damn well what it does." },
            { CustomItemType.AimbotGun, "<color=#ffcc00><b>Aimbot Gun</b></color><br><br>Every shot hits, if there's a valid target to hit<br>Regenerates 200 ammo every second up to 200." },
            { CustomItemType.SiphoningSyringe, "<color=#cc11cc><b>Siphoning Syringe</b></color><br><br>Heals you for half damage dealt, heals all live teammates the rest.<br>Regenerates 10 ammo every 3 seconds up to 100." },
            { CustomItemType.PortalOpener, "<color=#ff00ff><b>Portal machine</b></color><br><br>Rips a hole in the universe to bring you to another place." },
            { CustomItemType.PersonalShieldGenerator, "<color=#ff00ff><b>Personal Shield Generator</b></color><br><br>Gives you a quick-regenerating but low barrier." },
            { CustomItemType.TougherTimes, "<color=#ff00ff><b>Tougher Times</b></color><br><br>Gives you a 60% chance to block incoming damage." },
            { CustomItemType.TsarBomb, "<color=#ff0000><b>Tsar Bomb</b></color><br><br>City-Levelling demolition." },
            { CustomItemType.Infininade, "<color=#00ffff><b>Infininade</b></color><br><br>Explodes into itself recursively. Has no end." },
            { CustomItemType.BacconsFunGun, "<color=#cc11cc><b>Baccon's fun gun</b></color><br><br>Injects the target with heroin.<br>Regenerates 1 ammo every 20 seconds up to 3." },
            { CustomItemType.Ego, "<color=#ddddff><b>True Narcissism</b></color><br><br>Consumes all of your items. Grows stronger with every consumed item." },
            { CustomItemType.NullGrenade, "contact normalcat if you find this, somehow." },

        };

        public static Dictionary<CustomItemType, ItemType> items = new Dictionary<CustomItemType, ItemType>
        {
            { CustomItemType.GrenadeLauncher, ItemType.GunRevolver },
            { CustomItemType.MiniNukeLauncher, ItemType.GunAK },
            { CustomItemType.Freaky500, ItemType.SCP500 },
            { CustomItemType.Heroin, ItemType.Adrenaline },
            { CustomItemType.MitzeyScream, ItemType.GunCom45 },
            { CustomItemType.LightningTest, ItemType.GunCrossvec },
            { CustomItemType.MinionGun, ItemType.GunRevolver },
            { CustomItemType.GravityGun, ItemType.GunCOM15 },
            { CustomItemType.Grappler, ItemType.GunCOM18 },
            { CustomItemType.Arcer, ItemType.GunCOM15 },
            { CustomItemType.ShatteringJustice, ItemType.GunFRMG0 },
            { CustomItemType.SmokeBomb, ItemType.GrenadeFlash },
            { CustomItemType.BlindingBarrage, ItemType.GunE11SR },
            { CustomItemType.Hyperion, ItemType.Jailbird },
            { CustomItemType.AcesBrew, ItemType.SCP1853 },
            { CustomItemType.GlowGun, ItemType.GunCom45 },
            { CustomItemType.DiosBestFriend, ItemType.Lantern},
            { CustomItemType.Invisgun, ItemType.GunRevolver},
            { CustomItemType.StatBuffer, ItemType.Painkillers},
            { CustomItemType.LaserCannon, ItemType.GunRevolver},
            { CustomItemType.AimbotGun, ItemType.GunFSP9},
            { CustomItemType.SiphoningSyringe, ItemType.GunCrossvec},
            { CustomItemType.TougherTimes, ItemType.ArmorHeavy},
            { CustomItemType.PersonalShieldGenerator, ItemType.ArmorCombat},
            { CustomItemType.PortalOpener, ItemType.SCP1576},
            { CustomItemType.TsarBomb, ItemType.GrenadeHE},
            { CustomItemType.Infininade, ItemType.GrenadeHE},
            { CustomItemType.BacconsFunGun, ItemType.GunCOM18},
            { CustomItemType.Ego, ItemType.GunCrossvec},
            { CustomItemType.NullGrenade, ItemType.GrenadeHE},
        };

        public static Dictionary<CustomItemType, Color> colors = new Dictionary<CustomItemType, Color>
        {
            { CustomItemType.GrenadeLauncher, new Color(1, 1, 0) },
            { CustomItemType.MiniNukeLauncher, new Color(1, 0, 1) },
            { CustomItemType.Freaky500, new Color(1, 0, 1) },
            { CustomItemType.Heroin, new Color(0, 1, 0) },
            { CustomItemType.MitzeyScream, new Color(1, 0, 1) },
            { CustomItemType.LightningTest, new Color(0.02f, 0.85f, 1) },
            { CustomItemType.MinionGun, new Color(1, 1, 0) },
            { CustomItemType.GravityGun, new Color(0.3f, 0, 0.45f) },
            { CustomItemType.Grappler, new Color(1, 1, 1) },
            { CustomItemType.Arcer, new Color(1, 0.46f, 0) },
            { CustomItemType.ShatteringJustice, new Color(1, 0, 0) },
            { CustomItemType.SmokeBomb, new Color(1, 1, 1) },
            { CustomItemType.BlindingBarrage, new Color(0.33f, 0.22f, 0.8f) },
            { CustomItemType.Hyperion, new Color(1, 0, 1) },
            { CustomItemType.AcesBrew, new Color(0.47f, 0.68f, 0.32f) },
            { CustomItemType.GlowGun, new Color(0.678f, 0.847f, 0.902f) },
            { CustomItemType.DiosBestFriend, new Color(1, 0, 0) },
            { CustomItemType.Invisgun, new Color(0, 0, 0.7f) },
            { CustomItemType.StatBuffer, new Color(0.2f, 1, 0.2f) },
            { CustomItemType.LaserCannon, new Color(1, 0, 1) },
            { CustomItemType.AimbotGun, new Color(1, 0.67f, 0) },
            { CustomItemType.SiphoningSyringe, new Color(0.7f, 0.1f, 0.7f) },
            { CustomItemType.TougherTimes, new Color(0.6f, 0.6f, 1) },
            { CustomItemType.PersonalShieldGenerator, new Color(0.3f, 0.3f, 1f) },
            { CustomItemType.PortalOpener, new Color(1, 0, 1) },
            { CustomItemType.TsarBomb, new Color(1, 0, 0) },
            { CustomItemType.Infininade, new Color(0, 1, 1) },
            { CustomItemType.BacconsFunGun, new Color(1, 1, 1) },
            { CustomItemType.Ego, new Color(0.5f, 0.5f, 1) },
            { CustomItemType.NullGrenade, new Color(1, 0, 0) },
        };

        public static Dictionary<CustomItemType, Tuple<float, float>> GlowPowers = new Dictionary<CustomItemType, Tuple<float, float>>
        {
            { CustomItemType.SmokeBomb, Tuple.Create(2f, 200f) },
            { CustomItemType.TsarBomb, Tuple.Create(5f, 6f) },
            { CustomItemType.NullGrenade, Tuple.Create(5f, 6f) },
            { CustomItemType.Infininade, Tuple.Create(5f, 25f) },
            { CustomItemType.Ego, Tuple.Create(25f, 25f) },
        };

        public static Dictionary<CustomItemType, float> DamageValues = new Dictionary<CustomItemType, float>
        {
            { CustomItemType.BlindingBarrage, 5 },
            { CustomItemType.Invisgun, 97 },
            { CustomItemType.ShatteringJustice, 75 },
            { CustomItemType.SiphoningSyringe, 17.5f }
        };

        public static Dictionary<CustomItemType, float> SpawnWeights = new Dictionary<CustomItemType, float>
        {
            { CustomItemType.GrenadeLauncher, 200 },
            { CustomItemType.MiniNukeLauncher, 0.1f },
            { CustomItemType.Freaky500, 100 },
            { CustomItemType.Heroin, 60 },
            { CustomItemType.MitzeyScream, 0.1f },
            { CustomItemType.LightningTest, 5 },
            { CustomItemType.MinionGun, 250 },
            { CustomItemType.Grappler, 1 },
            { CustomItemType.Arcer, 80 },
            { CustomItemType.ShatteringJustice, 70 },
            { CustomItemType.SmokeBomb, 2 },
            { CustomItemType.BlindingBarrage, 125 },
            { CustomItemType.Hyperion, 40 },
            { CustomItemType.AcesBrew, 300 },
            { CustomItemType.DiosBestFriend, 150 },
            { CustomItemType.Invisgun, 225 },
            { CustomItemType.StatBuffer, 400 },
            { CustomItemType.SiphoningSyringe, 100 },
            { CustomItemType.TougherTimes, 200 },
            { CustomItemType.PersonalShieldGenerator, 200 },
            { CustomItemType.TsarBomb, 1 },
            { CustomItemType.Infininade, 100 },
            { CustomItemType.BacconsFunGun, 80 },
            { CustomItemType.Ego, 10 },
        };

        public static List<CustomItemType> WeightedValues = new List<CustomItemType>();

        public static Dictionary<ushort, List<ItemType>> customitemdata = new Dictionary<ushort, List<ItemType>>();
    }


    public enum CustomItemType
    {
        GrenadeLauncher,
        MiniNukeLauncher,
        Freaky500,
        Heroin,
        MitzeyScream,
        LightningTest,
        MinionGun,
        GravityGun,
        Grappler,
        Arcer,
        ShatteringJustice,
        SmokeBomb,
        BlindingBarrage,
        Hyperion,
        AcesBrew,
        GlowGun,
        DiosBestFriend,
        Invisgun,
        StatBuffer,
        LaserCannon,
        AimbotGun,
        SiphoningSyringe,
        TougherTimes,
        PersonalShieldGenerator,
        PortalOpener,
        TsarBomb,
        Infininade,
        BacconsFunGun,
        Ego,
        NullGrenade,
    }
}
