using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AetherTouch.App.Common
{
    public struct MessageMatchResult
    {
        public bool isMatch { get; set; }
        public string intensity { get; set; }
        public string duration { get; set; }

        public MessageMatchResult(bool isMatch, string intensity = "", string duration = "")
        { 
            this.isMatch = isMatch;
            this.intensity = intensity;
            this.duration = duration;
        }
    }
}
