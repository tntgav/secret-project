using CustomPlayerEffects;
using HarmonyLib;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Jailbird;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using MapGeneration;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Pinging;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp173;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using RelativePositioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Windows.Speech;
using Random = System.Random;

namespace secret_project
{
    public static class Patches
    {

        [HarmonyPatch(typeof(SingleBulletHitreg), nameof(SingleBulletHitreg.ServerRandomizeRay))]
        public static class PerfectAccuracy
        {
            public static bool Prefix(SingleBulletHitreg __instance)
            {
                if (EventHandlers.AccuracyPatch)
                {
                    Log.Info("hitreg patch");
                    if (!EventManager.ExecuteEvent(new PlayerShotWeaponEvent(__instance.Hub, __instance.Firearm)))
                    {
                        return false;
                    }
                    Ray ray = new Ray(__instance.Hub.PlayerCameraReference.position, __instance.Hub.PlayerCameraReference.forward);

                    Vector3 randomspread = (new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) - Vector3.one / 2f).normalized;
                    float factors = Handlers.SpreadCalculation(Player.Get(__instance.Hub));
                    ray.direction = Quaternion.Euler(randomspread * factors) * ray.direction;
                    Player p = Player.Get(__instance.Hub);
                    ItemBase cur = p.CurrentItem;
                    if (CustomItems.LiveCustoms.ContainsKey(cur.ItemSerial))
                    {
                        if (CustomItems.LiveCustoms[cur.ItemSerial] == CustomItemType.AimbotGun)
                        {
                            Log.Info("aimbot gun shot ray alteration course magic bull shit");
                            Player near = Handlers.NearestPlayer(p);
                            ray.direction = Handlers.CalculateLookAtAngle(p.Camera.position, near.Camera.position);
                        }
                    } 
                    FirearmBaseStats baseStats = __instance.Firearm.BaseStats;
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, baseStats.MaxDistance(), StandardHitregBase.HitregMask))
                    {
                        __instance.ServerProcessRaycastHit(ray, hit);
                        Handlers.OnBulletShot(hit, Player.Get(__instance.Hub));
                        return false;
                    }
                    return false;
                } else 
                {
                    if (!EventManager.ExecuteEvent(new PlayerShotWeaponEvent(__instance.Hub, __instance.Firearm)))
                    {
                        return false;
                    }
                    Ray ray = new Ray(__instance.Hub.PlayerCameraReference.position, __instance.Hub.PlayerCameraReference.forward);

                    Vector3 randomspread = (new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) - Vector3.one / 2f).normalized;
                    FirearmBaseStats baseStats = __instance.Firearm.BaseStats;
                    IFpcRole fpc = (__instance.Hub.roleManager.CurrentRole as IFpcRole);
                    float factors = baseStats.GetInaccuracy(__instance.Firearm, __instance.Firearm.AdsModule.ServerAds, fpc.FpcModule.Motor.Velocity.magnitude, fpc.FpcModule.IsGrounded);
                    Player p = Player.Get(__instance.Hub);
                    if (!Handlers.HasEffect<Scp1853>(p)) { ray.direction = Quaternion.Euler(randomspread * factors) * ray.direction; };
                    
                    RaycastHit hit;
                    
                    ItemBase cur = __instance.Firearm as ItemBase;
                    if (CustomItems.LiveCustoms.ContainsKey(cur.ItemSerial))
                    {
                        if (CustomItems.LiveCustoms[cur.ItemSerial] == CustomItemType.AimbotGun)
                        {
                            Log.Info("aimbot gun shot ray alteration course magic bull shit");
                            Player near = Handlers.NearestPlayer(p);
                            ray.direction = Handlers.CalculateLookAtAngle(p.Camera.position, near.Camera.position);
                        }
                    }
                    if (Physics.Raycast(ray, out hit, baseStats.MaxDistance(), StandardHitregBase.HitregMask))
                    {
                        __instance.ServerProcessRaycastHit(ray, hit);
                        Handlers.OnBulletShot(hit, Player.Get(__instance.Hub));
                        return false;
                    }
                    return false;
                }
            }

            public static void Postfix(SingleBulletHitreg __instance)
            {
            }
        }

        [HarmonyPatch(typeof(JailbirdItem), nameof(JailbirdItem.ServerAttack))]
        public static class jcustom
        {
            public static bool Prefix(JailbirdItem __instance)
            {
                return true;
            }

            public static void Postfix(JailbirdItem __instance)
            {
                if (CustomItems.LiveCustoms.ContainsKey(__instance.ItemSerial))
                {
                    Player p = Player.Get(__instance.Owner);
                    CustomItems.UseCustomItem(p, p.CurrentItem, CustomItems.LiveCustoms[__instance.ItemSerial]);
                    if (CustomItems.LiveCustoms[__instance.ItemSerial] == CustomItemType.Hyperion)
                    {
                        __instance.TotalChargesPerformed = 0;
                        __instance._hitreg.TotalMeleeDamageDealt = 0;
                        __instance._chargeDuration = float.MaxValue;
                        __instance._chargeReadyTime = 0f;
                        //__instance._charging = true;
                    }
                }
            }
        }

        

        public static Dictionary<ushort, int> bounces = new Dictionary<ushort, int>();


        [HarmonyPatch(typeof(DisruptorHitreg), nameof(DisruptorHitreg.ServerPerformShot))]
        public static class DisruptorPatch
        {
            public static bool Prefix(Ray ray, DisruptorHitreg __instance)
            {
                FirearmStatus st = __instance.Firearm.Status;
                __instance.Firearm.Status = new FirearmStatus(byte.MaxValue, st.Flags, st.Attachments);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Handlers.OnBulletShot(hit, Player.Get(__instance.Hub));
                }
                return true;
            }

            public static void Postfix(SingleBulletHitreg __instance)
            {
            }
        }

        [HarmonyPatch(typeof(Scp1853), nameof(Scp1853.TryGetSpeed))]
        public static class maxspeed1853
        {
            public static bool Prefix(Scp1853 __instance)
            {
                __instance.ItemUsageSpeedMultiplier = float.MaxValue;
                __instance._searchSpeedMultiplier = float.MaxValue;
                return true;
            }

            public static void Postfix(Scp1853 __instance)
            {
            }
        }

        [HarmonyPatch(typeof(TimeGrenade), nameof(TimeGrenade.Update))]
        public static class impactnades
        {
            public static bool Prefix(TimeGrenade __instance)
            {
                return true;
            }

            public static void Postfix(TimeGrenade __instance)
            {
                if (Handlers.impactnades.Contains(__instance.Info.Serial) && (__instance.PhysicsModule as PickupStandardPhysics).Rb.velocity.magnitude < 3)
                {
                    __instance.ServerFuseEnd();
                    __instance._alreadyDetonated = true;
                }
            }
        }
    }
}
