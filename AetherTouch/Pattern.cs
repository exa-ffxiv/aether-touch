using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AetherTouch
{
    public class Pattern
    {
        [JsonPropertyName("id")]
        public Guid Id { get; private set; }

        [JsonPropertyName("steps")]
        public List<Step> Steps { get; private set; }

        public Pattern()
        {
            Steps = [];
            Id = Guid.Empty;
        }

        public Pattern(List<Step> steps, Guid? id = null)
        {
            Steps = steps;
            if (id == null)
            {
                this.Id = Guid.NewGuid();
            }
        }
    }
}
