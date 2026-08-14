using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AetherTouch
{
    public class Pattern
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("steps")]
        public List<Step> Steps { get; init; }

        public Pattern()
        {
            Steps = [];
            Id = Guid.Empty;
            Name = "Default";
        }

        public Pattern(List<Step> steps, string? Name = null, Guid? id = null)
        {
            Steps = steps;
            this.Id = id ?? Guid.NewGuid();
            this.Name = Name ?? Id.ToString();
        }
    }
}
