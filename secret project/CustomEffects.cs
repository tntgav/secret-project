using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace secret_project
{
    public class CustomEffect
    {

        public EffectType type;
        public float duration;
        public int intensity;
        public CustomEffect(EffectType type, float duration, int intensity)
        {
            this.type = type;
            this.duration = duration;   
            this.intensity = intensity;
        }

        private static Dictionary<Player, List<CustomEffect>> effects = new Dictionary<Player, List<CustomEffect>>();

        public static void HandleEffects(Player plr = null)
        {
            //runs every frame on every player. very server taxing lmao
            if (plr == null) { foreach (Player p in Player.GetPlayers())
                {
                    if (!effects.ContainsKey(plr)) continue;
                    foreach (CustomEffect effect in effects[plr])
                    {
                        effect.duration -= EventHandlers.Rate;
                        if (effect.duration <= 0) { RemoveEffect(plr, effect); continue; }
                        HandleEffect(plr, effect);
                    }
                }
                return;
            }

            if (!effects.ContainsKey(plr)) return;
            foreach (CustomEffect effect in effects[plr])
            {
                effect.duration -= EventHandlers.Rate;
                if (effect.duration <= 0) { RemoveEffect(plr, effect); continue; }
                HandleEffect(plr, effect);
            }
        }

        private static void HandleEffect(Player plr, CustomEffect effect)
        {
            if (effect.type == EffectType.Wungus) { plr.Heal(((plr.Velocity.magnitude > 5 ? 1 : 0) * (plr.Velocity.magnitude / 2)) * EventHandlers.Rate); }
        }

        public static void RemoveEffect(Player plr, CustomEffect eff)
        {
            if (!effects.ContainsKey(plr)) effects.Add(plr, new List<CustomEffect>());
            if (effects[plr].Contains(eff)) { effects[plr].Remove(eff); }
        }

        public static void GiveEffect(Player plr, CustomEffect eff)
        {
            if (!effects.ContainsKey(plr)) effects.Add(plr, new List<CustomEffect>());
            List<EffectType> types = effects[plr].Select(e => e.type).ToList();
            if (types.Contains(eff.type)) //long-winded way to ask if the player has the effect
            {
                effects[plr][types.IndexOf(eff.type)] = eff;
            } else
            {
                effects[plr].Add(eff);
            }

        }

        public enum EffectType
        {
            Wungus,
        }
    }
}
