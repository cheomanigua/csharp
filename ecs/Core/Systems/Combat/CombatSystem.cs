using Core.Commands;
using System.Collections.Generic;
using Core;

namespace Core.Systems.Combat;

public class CombatSystem
{
    // Inject the action mappings loaded from JSON
    private readonly Dictionary<string, string> _actionMappings;

    public CombatSystem(Dictionary<string, string> actionMappings)
    {
        _actionMappings = actionMappings;
    }

    public void ExecuteAction(string actionName, EntityRegistry registry, int attackerId, int targetId)
    {
        // 1. Get the formula group name from action_mappings.json
        if (!_actionMappings.TryGetValue(actionName, out string? formulaName) || formulaName == null) return;

        // 2. Fetch data (this is pseudo-code for your registry lookup)
        var attackerStats = registry.GetStatsForEntity(attackerId);
        // ... get target stats, items, etc ...

        // 3. Create context for the FormulaProcessor
        var context = new FormulaContext(attackerStats); 

        // 4. Run the calculation
        // The FormulaProcessor uses formulas.json to compute the result
        float result = FormulaProcessor.Execute(formulaName, attackerStats, 0);

        // 5. Apply result (e.g., deal damage)
        // registry.ApplyDamage(targetId, (int)result);
    }
}
