using System;
using System.Collections.Generic;

namespace CrimeGame.Core.Models
{
    public class TradeTrait { public string TradeName { get; set; } = string.Empty; }
    public class PositionTrait { public Guid CurrentRoomId { get; set; } }
    
    public class NpcAction 
    {
        public int StartTime { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public Guid LocationId { get; set; }
    }

    public class ScheduleTrait { public List<NpcAction> History { get; set; } = new(); }
    
    public class SocialLedger 
    {
        public Dictionary<Guid, int> Bonds { get; set; } = new();
        public Dictionary<Guid, List<string>> Grievances { get; set; } = new();
    }

    public class EvidenceTrait 
    {
        public string FactId { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
