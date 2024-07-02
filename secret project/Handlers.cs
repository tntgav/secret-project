using CustomPlayerEffects;
using MapGeneration;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace secret_project
{
    public class Handlers
    {
        public static void AddEffect<T>(Player player, byte intensity, int addedDuration = 0) where T : StatusEffectBase
        {
            foreach (StatusEffectBase effect in player.ReferenceHub.playerEffectsController.AllEffects)
            {
                if (effect.GetType() == typeof(T))
                {
                    byte inten = effect.Intensity;
                    float duration = effect.Duration;
                    byte newIntensity = Math.Min(System.Convert.ToByte(intensity + inten), System.Convert.ToByte(200));
                    player.ReferenceHub.playerEffectsController.ChangeState<T>(newIntensity, duration + addedDuration);
                    ServerConsole.AddLog($"{player.Nickname} has been given/added {effect.name} of intensity {newIntensity} for {duration + addedDuration} seconds");
                }
            }
        }

        public static void SetEffect<T>(Player player, byte intensity, int addedDuration = 0) where T : StatusEffectBase
        {
            foreach (StatusEffectBase effect in player.ReferenceHub.playerEffectsController.AllEffects)
            {
                if (effect.GetType() == typeof(T))
                {
                    byte inten = effect.Intensity;
                    float duration = effect.Duration;

                    player.ReferenceHub.playerEffectsController.ChangeState<T>(intensity, duration + addedDuration);
                }
            }
        }

        public static void RemoveEffect<T>(Player player) where T : StatusEffectBase
        {
            foreach (StatusEffectBase effect in player.ReferenceHub.playerEffectsController.AllEffects)
            {
                if (effect.GetType() == typeof(T))
                {
                    player.ReferenceHub.playerEffectsController.DisableEffect<T>();
                }
            }
        }

        public static RoomIdentifier RandomRoom()
        {
            List<RoomIdentifier> rooms = new List<RoomIdentifier>();
            foreach (RoomIdentifier room in RoomIdentifier.AllRoomIdentifiers)
            {
                rooms.Add(room);
            }
            return rooms[new Random().Next(rooms.Count)];
        }
    }
}
