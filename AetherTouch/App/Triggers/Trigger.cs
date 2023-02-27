using System;

namespace AetherTouch.App.Triggers
{
    public class Trigger
    {
        public Guid Id { get; init; }

        public string Name;
        public bool enabled { get; set; } = true;

        public Trigger()
        {
            this.Name = string.Empty;
            this.Id = Guid.Empty;
        }

        public Trigger(string name)
        { 
            this.Name = name;
            this.Id = Guid.NewGuid();
        }
    }
}
