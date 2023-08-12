using AetherTouch.App.Common;
using Dalamud.Game.Text;
using System;

namespace AetherTouch.App.Triggers
{
    public class Trigger: IComparable<Trigger>
    {
        public Guid Id { get; init; }

        public string Name;
        public bool enabled = true;
        public string messageRegex;
        public string senderRegex;
        public ChatTypes chatType;
        public Guid patternId = Guid.Empty;
        public int priority = 0;
        public bool ignoreOwn = false;

        public Trigger()
        {
            this.Name = string.Empty;
            this.Id = Guid.Empty;
            this.messageRegex = string.Empty;
            this.senderRegex = string.Empty;
            this.chatType = ChatTypes.Any;
        }

        public Trigger(string name)
        { 
            this.Name = name;
            this.Id = Guid.NewGuid();
            this.messageRegex = string.Empty;
            this.senderRegex = string.Empty;
            this.chatType = ChatTypes.Any;
        }

        public int CompareTo(Trigger? other)
        {
            if (other == null) return 1;
            return -this.priority.CompareTo(other.priority);
        }
    }
}
