using MEC;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace secret_project.protocols
{
    public class CustomProtocol
    {
        public string InternalName; public string Description; public int duration;

        private Dictionary<int, Action> Events = new Dictionary<int, Action>();

        private List<Action> EverySecond = new List<Action>();

        private int totalDuration;

        public CustomProtocol(string Name, string Description, int duration)
        {
            this.InternalName = Name;
            this.Description = Description;
            this.totalDuration = duration;
        }

        public void StartProtocol()
        {
            duration = totalDuration;
            Timing.CallPeriodically(totalDuration, 1, PerSecondCoroutine);
        }

        public void AddEvent(int time, Action action)
        {
            Events.Add(time, action);
        }

        public void AddEvent(Action action)
        {
            EverySecond.Add(action);
        }

        private void PerSecondCoroutine()
        {
            if (duration <= 0) { return; }
            duration -= 1;
            foreach (Player p in Player.GetPlayers())
            {
                HintHandlers.text(p, 200, $"{Description} Activated<br>{duration} seconds remain...", 1);
            }

            if (Events.ContainsKey(duration))
            {
                Events[duration].Invoke();
            }

            foreach (Action action in EverySecond) { action(); }
        }
    }
}
