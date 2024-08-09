using CommandSystem.Commands.RemoteAdmin;
using CustomPlayerEffects;
using MEC;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Visibility;
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
        public DateTime startTime;
        public CustomEffect(EffectType type, float duration, int intensity)
        {
            this.type = type;
            this.duration = duration;   
            this.intensity = intensity;
            startTime = DateTime.Now;
        }

        private static Dictionary<Player, List<CustomEffect>> effects = new Dictionary<Player, List<CustomEffect>>();

        public static void HandleEffects(Player plr)
        {
            //runs every frame on every player. very server taxing lmao
            if (!effects.ContainsKey(plr)) return;
            if (plr.Role == RoleTypeId.Spectator) { ClearEffects(plr); }
            List<string> plreffects = effects[plr].Select(x => $"{x.type} {x.intensity} for {x.duration}s").ToList();
            Log.Info($"{plr.Nickname} has {string.Join(", ", plreffects)}");
            foreach (CustomEffect effect in effects[plr])
            {
                effect.duration -= EventHandlers.Rate;
                if (effect.duration <= 0 || effect.intensity <= 0) { RemoveEffect(plr, effect); continue; }
                HandleEffect(plr, effect);
            }
        }

        private static void HandleEffect(Player plr, CustomEffect effect)
        {
            //Log.Info($"Handling effect {effect.type} for {plr.Nickname}");
            if (effect.type == EffectType.Wungus) { plr.Health += (((plr.Velocity.magnitude > 5 ? 1 : 0) * (plr.Velocity.magnitude / 2)) * EventHandlers.Rate * effect.intensity); if (plr.Health > 100 + Math.Pow(2, effect.intensity)) plr.Health = 100 + (float)Math.Pow(2, effect.intensity); }
            if (effect.type == EffectType.Shattering) { plr.Damage(effect.intensity, "Shattered into a million pieces"); effect.intensity += 1; }
            if (effect.type == EffectType.Godly && plr.Health <= 25)
            {
                plr.ReferenceHub.characterClassManager.GodMode = true;
                Handlers.CreateThrowable(ItemType.GrenadeHE).SpawnActive(plr.Position, 0.05f, plr);
                Timing.CallDelayed(0.2f, () => { plr.ReferenceHub.characterClassManager.GodMode = false; });
                plr.Heal(100);
                RemoveEffect(plr, effect);
            }
        }

        public static void RemoveEffect(Player plr, CustomEffect eff)
        {
            if (!effects.ContainsKey(plr)) effects.Add(plr, new List<CustomEffect>());
            if (effects[plr].Contains(eff)) { effects[plr].Remove(eff); }
        }

        public static void ClearEffects(Player p)
        {
            foreach (CustomEffect effect in effects[p]) { RemoveEffect(p, effect); }
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

        public static void AddEffect(Player plr, CustomEffect eff)
        {
            if (!effects.ContainsKey(plr)) effects.Add(plr, new List<CustomEffect>());
            List<EffectType> types = effects[plr].Select(e => e.type).ToList();
            if (types.Contains(eff.type)) //long-winded way to ask if the player has the effect
            {
                CustomEffect old = effects[plr][types.IndexOf(eff.type)];
                effects[plr][types.IndexOf(eff.type)] = new CustomEffect(eff.type, old.duration + eff.duration, old.intensity + eff.intensity);
                Log.Info($"Added {eff.intensity} to {plr.Nickname}'s intensity of {eff.type}, cur intensity: {eff.intensity+old.intensity}");
            }
            else
            {
                effects[plr].Add(eff);
            }
            
        }

        public enum EffectType
        {
            Wungus,
            Shattering,
            Godly,
        }
    }
}
