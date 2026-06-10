using Core.Commands;
using System.Collections.Generic;
using Core;

namespace Core.Systems.Combat;

public class CombatSystem
{
    private readonly Dictionary<string, string> _actionMappings;

    public CombatSystem(Dictionary<string, string> actionMappings)
    {
        _actionMappings = actionMappings;
    }

    public void ExecuteAction(string actionName, EntityRegistry registry, int attackerId, int targetId)
    {
        if (!_actionMappings.TryGetValue(actionName, out string? formulaName) || formulaName == null) return;

        ref var attackerStats = ref registry.GetHotData(attackerId);

        var context = new FormulaContext(attackerStats); 

        float result = FormulaProcessor.Execute(formulaName, attackerStats, 0);

        // TODO: Apply damage to target
    }
}
