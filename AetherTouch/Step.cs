using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AetherTouch
{
    public class Step
    {
        private int duration;
        [JsonPropertyName("d")]
        public int Duration
        {
            get => duration;
            private set => duration = Math.Max(0, value);
        }

        private int intensity;
        [JsonPropertyName("i")]
        public int Intensity 
        {
            get => intensity;
            private set => intensity = Math.Clamp(value, 0, 100);
        }

        public Step(int duration, int intensity)
        {
            Duration = duration;
            Intensity = intensity;
        }
    }
}
