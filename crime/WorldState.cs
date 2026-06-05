using System;
using System.Collections.Generic;

namespace CrimeGame.Core.Models
{
    // --- Top-Level State Model ---
    public class WorldState 
    {
        public List<NPC> Characters { get; set; } = new();
        public List<Room> Locations { get; set; } = new();
        public List<Item> Items { get; set; } = new();
        public Guid KillerId { get; set; }
        public Guid VictimId { get; set; }
        public int CrimeTime { get; set; } = 1320;
    }

    // --- JSON Configuration Mapping Models ---
    public class RoomTypeDefinition 
    {
        public string Type { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    public class ItemTypeDefinition 
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    public class GrievanceTemplate 
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int BaseBondModifier { get; set; }
    }

    public class TradeDefinition 
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, int> RoomWeights { get; set; } = new();
    }

    public class GameDefinitions 
    {
        public List<TradeDefinition> Trades { get; set; } = new();
        public List<string> StandardActions { get; set; } = new();
        public List<GrievanceTemplate> GrievanceTemplates { get; set; } = new();
        public List<RoomTypeDefinition> RoomTypes { get; set; } = new();
        public List<ItemTypeDefinition> ItemTypes { get; set; } = new();
    }
}
