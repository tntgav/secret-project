using CustomPlayerEffects;
using HarmonyLib;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
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
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
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
                    Log.Info("attempting patch of sbhr for perfect accuracy");
                    if (!EventManager.ExecuteEvent(new PlayerShotWeaponEvent(__instance.Hub, __instance.Firearm)))
                    {
                        return false;
                    }
                    Ray ray = new Ray(__instance.Hub.PlayerCameraReference.position, __instance.Hub.PlayerCameraReference.forward);

                    Vector3 randomspread = (new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) - Vector3.one / 2f).normalized;
                    float factors = Handlers.SpreadCalculation(Player.Get(__instance.Hub));
                    ray.direction = Quaternion.Euler(randomspread * factors) * ray.direction;
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

                    Vector3 a = (new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) - Vector3.one / 2f).normalized;
                    FirearmBaseStats baseStats = __instance.Firearm.BaseStats;
                    float num = baseStats.GetInaccuracy(__instance.Firearm, __instance.Firearm.AdsModule.ServerAds, __instance.Hub.GetVelocity().magnitude, __instance.Hub.IsGrounded());
                    ray.direction = Quaternion.Euler(num * a) * ray.direction;

                    RaycastHit hit;
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
    }
}
