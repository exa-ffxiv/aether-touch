using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AetherTouch.App.Patterns
{
    public class Pattern
    {
        public Guid Id { get; set; }
        public string Name;
        // Series of intensity (0-100) and ms duration pairs.
        // example "int:dur,int:dur,int:dur" or "50:500,100:1000,25:500"
        public string PatternText;

        public Pattern()
        {
            Id = Guid.NewGuid();
            Name = "Default Pattern Name";
            PatternText = "50:1000,100:1000";
        }

        public Pattern(string name, string patternText = "50:1000,100:1000")
        {
            Id = Guid.NewGuid();
            Name = name;
            PatternText = patternText;
        }
    }
}
