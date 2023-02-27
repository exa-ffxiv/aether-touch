using Dalamud.Game.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AetherTouch.App.Triggers
{
    public class RegexTrigger: Trigger
    {
        public string regexPattern;
        public XivChatType chatType;

        public RegexTrigger(
            string name,
            string regexPattern = "",
            XivChatType chatType = XivChatType.None): base(name)
        {
            this.regexPattern = regexPattern;
            this.chatType = chatType;
        }
    }
}
