using AdminToys;
using InventorySystem.Items;
using MEC;
using Mirror;
using PlayerRoles;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
                if (type == CustomItemType.MitzeyScream) { for (int i = 0; i < 6; i++) { Handlers.GrenadePosition(hit, plr); } }
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
                        await Task.Delay(30000);
                        first.Damage(600000, "Expired");
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
                            if (plr.Team == near.Team && plr != near) continue; //cant move teammates but can move self
                            near.Position = Vector3.Lerp(hit, near.Position, 0.935f);
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
                    LightSourceToy light = Handlers.AddLight(hit, new Color(0.7f, 0.2f, 0.45f), 0.5f, 0.5f);
                    int i = 0;
                    Timing.CallPeriodically(5, 0.05f, () =>
                    {
                        i++;
                        light.NetworkLightIntensity += 1f;
                        light.NetworkLightRange += 1f;
                        foreach (Player near in Player.GetPlayers())
                        {
                            float dist = Vector3.Distance(hit, near.Position);
                            float maxdist = 0.5f + (i * 0.1f);
                            if (dist > maxdist) continue;
                            if (plr.Team == near.Team && plr != near) continue; //cant move teammates but can move self
                            near.Position = (((near.Position - hit).normalized) * ((maxdist - dist) / 6)) + near.Position;
                        }
                    });
                    Timing.CallDelayed(5.2f, () => { Handlers.GrenadePosition(hit, plr); NetworkServer.Destroy(light.gameObject); });
                }

            } else //non gun custom item
            {
                if (type == CustomItemType.Freaky500) { HintHandlers.text(plr, 300, "You feel an overwhelming sense of freakiness...", 3); plr.Health = float.MinValue; }
                if (type == CustomItemType.Heroin) { plr.Damage(6000, "Heroin hurts..."); }
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
            { CustomItemType.Arcer, "<color=#ff00ff><b>Pusher</b></color><br><br>Pushes targets away from where you shot.<br>Regenerates 10 ammo every 3 seconds up to 50."},
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
        };
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
    }
}
