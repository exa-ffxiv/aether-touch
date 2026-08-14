using System;
using System.Collections.Generic;
using System.Text;
using Dalamud.Interface.Windowing;

namespace AetherTouch.Windows
{
    public class PatternEditWindow : Window, IDisposable
    {
        private readonly Plugin plugin;
        private readonly DataManager dataManager;

        public PatternEditWindow(Plugin plugin, DataManager dataManager)
            : base("Pattern Editor##AetherTouch")
        {
            this.plugin = plugin;
            this.dataManager = dataManager;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void Draw()
        {
            throw new NotImplementedException();
        }
    }
}
