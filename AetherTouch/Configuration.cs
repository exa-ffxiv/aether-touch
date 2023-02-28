using AetherTouch.App.Patterns;
using AetherTouch.App.Triggers;
using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AetherTouch
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public string ButtplugIOAddress { get; set; } = "localhost";
        public int ButtplugIOPort { get; set; } = 12345;

        private List<Trigger> sortedActiveTriggers = new();
        private Dictionary<Guid, Trigger> _triggers = new();
        public Dictionary<Guid, Trigger> Triggers { 
            get
            {
                return _triggers;
            }
            set
            {
                _triggers = value;
                // TODO: Figure out why sortedActiveTriggers has 0 members
                if (_triggers != null)
                {
                    sortedActiveTriggers = Triggers.Values.Where(x => x.enabled).ToList();
                    sortedActiveTriggers.Sort();
                }
                else
                {
                    sortedActiveTriggers.Clear();
                }
            }
        }

        public List<Trigger> GetSortedActiveTriggers()
        {
            var t = Triggers.Values.Where(x => x.enabled).ToList();
            t.Sort();
            return t;
            //return sortedActiveTriggers;
        }
        public Dictionary<Guid, Pattern> Patterns { get; set; } = new();

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
