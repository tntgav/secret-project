using PluginAPI.Core.Attributes;
using PluginAPI.Core;
using PluginAPI.Enums;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventorySystem.Items.ThrowableProjectiles;
using System.ComponentModel;
using Mirror;
using UnityEngine;
using Footprinting;
using InventorySystem.Items.Pickups;
using InventorySystem.Items;
using InventorySystem;
using AdminToys;
using InventorySystem.Items.Firearms;
using CustomPlayerEffects;
using secret_project;
using PlayerRoles.PlayableScps.Scp079;
using Waits;
using YamlDotNet.Serialization;
using UnityEngine.PlayerLoop;
using GameCore;
using RueI;
using RueI.Displays;
using RueI.Elements;
using RueI.Elements.Enums;
using RueI.Elements.Delegates;
using System.Xml.Linq;
using System.Collections;
using System.Runtime.CompilerServices;
using Log = PluginAPI.Core.Log;
using Display = RueI.Displays.Display;
using MEC;
using System.Xml.Schema;
namespace secret_project
{
    internal class HintHandlers
    {
        public static Dictionary<Player, RueI.Displays.Display> playerDisplays = new Dictionary<Player, RueI.Displays.Display>();

        /// <summary>
        /// sets up a player's data in the playerdisplays dictionary. required to use hinthandlers on a player, it also resets their current data if they have any, meaning it's safe to call at any time.
        /// </summary>
        /// <param name="plr"></param>
        public static void InitPlayer(Player plr)
        {
            ReferenceHub hub = plr.ReferenceHub;
            DisplayCore core = DisplayCore.Get(hub);
            RueI.Displays.Display display = new RueI.Displays.Display(core);
            if (playerDisplays.ContainsKey(plr))
            {
                playerDisplays.Remove(plr);
            }
            playerDisplays.Add(plr, display);


        }
        /// <summary>
        /// adds text to a player's screen. position ranges from 0-1000, and it affects the text's VERTICAL position
        /// </summary>
        /// <param name="plr"></param>
        /// <param name="position"></param>
        /// <param name="text"></param>
        public static SetElement AddText(Player plr, int position, string text)
        {
            SetElement element = new SetElement(position, text);
            element.Options |= ElementOptions.PreserveSpacing;
            playerDisplays[plr].Elements.Add(element);
            DisplayCore.Get(plr.ReferenceHub).Update();
            return element;
        }
        /// <summary>
        /// clears all of a player's current text hints.
        /// </summary>
        /// <param name="plr"></param>
        public static void ClearAll(Player plr)
        {
            playerDisplays[plr].Delete();
            DisplayCore.Get(plr.ReferenceHub).Update();
        }

        /// <summary>
        /// adds "fading" text to a players screen. functions identially to addtext, but the text is *suddenly* removed after the given time passes
        /// </summary>
        /// <param name="plr"></param>
        /// <param name="position"></param>
        /// <param name="text"></param>
        /// <param name="time"></param>
        public static async void AddFadingText(Player plr, int position, string text, float time)
        {
            SetElement element = AddText(plr, position, text);
            //List<Element> old = playerDisplays[plr].Elements;
            await Task.Delay(TimeSpan.FromSeconds(time));
            //Log.Debug($"{string.Join(", ", old)}\n{string.Join(", ", playerDisplays[plr].Elements)}");
            playerDisplays[plr].Elements.Remove(element);
            DisplayCore.Get(plr.ReferenceHub).Update();

        }

        public static void text(Player plr, int position, string content, float time)
        {
            InitPlayer(plr);
            ReferenceHub hub = plr.ReferenceHub;
            DisplayCore core = DisplayCore.Get(hub);
            IElemReference<SetElement> idk = DisplayCore.GetReference<SetElement>();
            SetElement element = core.GetElementOrNew(idk, () => new SetElement(position, content));
            Display disp = new Display(core);
            core.Update();

            Timing.CallDelayed(time, () =>
            {
                disp.Elements.Remove(element);
                element.Enabled = false;
                core.Update();
            });
        }
    }
}