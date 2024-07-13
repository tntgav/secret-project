using AdminToys;
using InventorySystem.Items;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace secret_project
{
    public static class CustomItems
    {
        public static Dictionary<ushort, CustomItemType> LiveCustoms = new Dictionary<ushort, CustomItemType>();
        public static Dictionary<ushort, LightSourceToy> LiveLights = new Dictionary<ushort, LightSourceToy>();
        public static void UseCustomItem(Player plr, ItemBase item, CustomItemType type, Vector3 hit = new Vector3())
        {
            if (item == null) return; //weirdly common...
            if (hit.magnitude != 0) //there is a hit
            {
                if (type == CustomItemType.GrenadeLauncher) { Handlers.GrenadePosition(hit, plr); }
                if (type == CustomItemType.MiniNukeLauncher) { AlphaWarheadController.Singleton.Detonate(); }
            } else //non gun custom item
            {
                if (type == CustomItemType.Freaky500) { HintHandlers.text(plr, 300, "You feel an overwhelming sense of freakiness...", 3); plr.Health = float.MinValue; }
                if (type == CustomItemType.Heroin) { plr.Kill("You overdosed..."); }
            }
        }

        public static Dictionary<CustomItemType, string> descriptions = new Dictionary<CustomItemType, string>
        {
            { CustomItemType.GrenadeLauncher, "<color=#ffff00><b>Grenade Launcher</b></color><br><br>Shoots a grenade, simple as that.<br>Regenerates 1 ammo every 30 seconds, up to 30."},
            { CustomItemType.MiniNukeLauncher, "<color=#ffff00><b>Miniature Nuke Launcher</b></color><br><br>Please don't try this.<br>Regenerates 5 ammo every second up to 5."},
            { CustomItemType.Freaky500, "<color=#ff00ff><b>Freaky 500</b></color><br><br>Makes you feel... freaky..."},
            { CustomItemType.Heroin, "<color=#00ff00><b>Heroin</b></color><br><br>You might OD on this."},
        };

        public static Dictionary<CustomItemType, ItemType> items = new Dictionary<CustomItemType, ItemType>
        {
            { CustomItemType.GrenadeLauncher, ItemType.GunRevolver },
            { CustomItemType.MiniNukeLauncher, ItemType.GunCOM18 },
            { CustomItemType.Freaky500, ItemType.SCP500 },
            { CustomItemType.Heroin, ItemType.Adrenaline },
        };

        public static Dictionary<CustomItemType, Color> colors = new Dictionary<CustomItemType, Color>
        {
            { CustomItemType.GrenadeLauncher, new Color(1, 1, 0) },
            { CustomItemType.MiniNukeLauncher, new Color(1, 1, 0) },
            { CustomItemType.Freaky500, new Color(1, 0, 1) },
            { CustomItemType.Heroin, new Color(0, 1, 0) }
        };
    }

    public enum CustomItemType
    {
        GrenadeLauncher,
        MiniNukeLauncher,
        Freaky500,
        Heroin,
    }
}
