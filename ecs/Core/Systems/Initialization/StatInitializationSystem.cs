using Core.Commands;
using System.Collections.Generic;
using System;

namespace Core.Systems;

public class StatInitializationSystem
{
	public void Update(EntityRegistry registry, StatRegistry statRegistry, CommandQueue queue, NPCBlueprintDto bp, 
                   IReadOnlyDictionary<string, ClassData> classes, 
                   IReadOnlyDictionary<string, RaceData> races)
    {
        if (!classes.TryGetValue(bp.Class, out var charClass)) return;
        if (!races.TryGetValue(bp.Race, out var race)) return;
    
        var stats = new CharacterStats(statRegistry.IntStatCount) { EntityId = bp.EntityId, IsDirty = true };
    
        // Map the record properties to the flat array indices
        // We use the Registry to tell us WHERE to put the values
        stats.Values[statRegistry.GetIndexOrThrow("Health")] = charClass.ClassHealth + 0; // Races don't have health bonus?
        stats.Values[statRegistry.GetIndexOrThrow("Mana")]   = charClass.ClassMana + 0;
        stats.Values[statRegistry.GetIndexOrThrow("Strength")]     = charClass.ClassStr + race.RaceStr;
        stats.Values[statRegistry.GetIndexOrThrow("Intelligence")] = charClass.ClassInt + race.RaceInt;
    
        registry.RegisterStats(bp.EntityId, in stats);
    }
}
