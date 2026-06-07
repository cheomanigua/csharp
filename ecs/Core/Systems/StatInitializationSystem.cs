using Core.Commands;

namespace Core.Systems;

public class StatInitializationSystem
{
    public void Update(EntityRegistry registry, CommandQueue queue, ClassData charClass, RaceData race)
    {
        foreach (var cmd in queue.GetCommands())
        {
            if (cmd.Type != CommandType.InitStats) continue;

            var stats = new CharacterStats { EntityId = cmd.EntityId };
            // Logic moved from Controller.cs
            FormulaProcessor.ExecuteInitialization("InitStats", ref stats, charClass, race);
            registry.RegisterStats(cmd.EntityId, in stats);
        }
    }
}
