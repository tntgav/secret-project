using CommandSystem;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using Mirror;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UnityEngine;
using Utils.NonAllocLINQ;
using ICommand = CommandSystem.ICommand;

namespace secret_project
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class SpawnDoorCommand : ICommand
    {
        public string Command => "SpawnDoor";

        public string[] Aliases => new string[] { "cdr" };

        public string Description => "spawns a door at your position with a specific scale";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            List<string> args = arguments.ToList();
            Log.Info(string.Join(", ", args));
            Player plr = Player.Get(sender);
            float x = float.Parse(args[0]);
            if (x == -1)
            {
                DoorVariant.AllDoors.ForEach(d => Log.Info($"DOOR: {d.name}"));
                NetworkClient.prefabs.Values.ToList().ForEach(d => Log.Info($"PREFAB: {d.name}"));
                plr.ReferenceHub.playerEffectsController.AllEffects.ForEach(d => Log.Info($"EFFECTNAME: {d.name}"));
                response = "logged";
                return true;
            }
            float y = float.Parse(args[1]);
            if (y == -1)
            {
                int length = DoorVariant.AllDoors.Count;
                int i = 0;
                DoorVariant.AllDoors.ForEach(d => d.transform.localPosition = plr.Position);
                response = "hopefully brought every door to you";
                return true;
            }
            float z = float.Parse(args[2]);

            if (Enum.TryParse(args[3], true, out PrefabTypes type))
            {
                Handlers.SpawnDoor(plr.Position, Vector3.zero, new Vector3(x, y, z), type);
                response = "done";
                return true;
            } else
            {
                response = "failure, sadness, even";
                return false;
            }
        }
    }
}
