using System;
using System.Collections.Generic;
using System.Text;

namespace AetherTouch
{
    public class Pattern(List<Step> steps)
    {
        public List<Step> Steps { get; private set; } = steps;
    }
}
