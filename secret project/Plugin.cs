using HarmonyLib;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using RueI;
using RueI.Events;
using System;

namespace secret_project
{
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }

        private static EventHandlers EventHandlers { get; set; }

        [PluginEntryPoint("pluginName", "1.0.0", "description", "author")]
        void LoadPluginAPI()
        {
            Instance = this;

            EventHandlers = new EventHandlers();
            EventManager.RegisterEvents(EventHandlers);
            EventManager.RegisterAllEvents(EventHandlers);
            RueIMain.EnsureInit();
            Harmony _harmony;
            _harmony = new Harmony("com.tpd.patches");
            _harmony.PatchAll();
            ServerConsole.AddLog("project 2", ConsoleColor.Magenta);

        }
        [PluginUnload]
        void UnloadPluginAPI()
        {
            EventManager.UnregisterEvents(EventHandlers);
        }
    }


}