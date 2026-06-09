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
        if (!classes.TryGetValue(bp.Class, out var classData)) return;
        if (!races.TryGetValue(bp.Race, out var race)) return;
    
        // 1. Initialize empty stats struct
        var stats = new CharacterStats((int)StatType.Count) { EntityId = bp.EntityId, IsDirty = true };
        
        // 2. Create the context containing all required data for the FormulaProcessor
        var context = new FormulaContext(stats, classData, race);

        // 3. Delegate to FormulaProcessor using the "InitStats" formula group
        // This replaces the manual assignments of Health, Mana, Strength, and Intelligence
        FormulaProcessor.ExecuteInit("InitStats", ref stats, context);
				DebugLog.Log($"DEBUG: After Init, Health is {stats.Values[(int)StatType.Health]}");
    
        registry.RegisterStats(bp.EntityId, in stats);
    }
}
