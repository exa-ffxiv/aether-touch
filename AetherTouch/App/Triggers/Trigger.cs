using AetherTouch.App.Common;
using Dalamud.Game.Text;
using System;

namespace AetherTouch.App.Triggers
{
    public class Trigger
    {
        public Guid Id { get; init; }

        public string Name;
        public bool enabled = true;
        public string regexPattern;
        public ChatTypes chatType;
        public string patternId = Guid.Empty.ToString();

        public Trigger()
        {
            this.Name = string.Empty;
            this.Id = Guid.Empty;
            this.regexPattern = string.Empty;
            this.chatType = ChatTypes.Any;
        }

        public Trigger(string name)
        { 
            this.Name = name;
            this.Id = Guid.NewGuid();
            this.regexPattern = string.Empty;
            this.chatType = ChatTypes.Any;
        }
    }
}
