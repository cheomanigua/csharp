using Core.Commands;
using System.Collections.Generic;
using Core;

namespace Core.Systems;

public class StatInitializationSystem
{
    public void Update(EntityRegistry registry, CommandQueue queue, NPCBlueprintDto bp, 
                   IReadOnlyDictionary<string, ClassData> classes, 
                   IReadOnlyDictionary<string, RaceData> races)
    {
        if (!classes.TryGetValue(bp.Class, out var charClass)) return;
        if (!races.TryGetValue(bp.Race, out var race)) return;
    
        var stats = new CharacterStats((int)StatType.Count) { EntityId = bp.EntityId, IsDirty = true };
    
        stats.Values[(int)StatType.Health]       = charClass.ClassHealth;
        stats.Values[(int)StatType.Mana]         = charClass.ClassMana;
        stats.Values[(int)StatType.Strength]     = charClass.ClassStr + race.RaceStr;
        stats.Values[(int)StatType.Intelligence] = charClass.ClassInt + race.RaceInt;
    
        registry.RegisterStats(bp.EntityId, in stats);
    }
}
