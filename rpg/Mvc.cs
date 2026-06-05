using System;
using DataDrivenRPG.Core;
using DataDrivenRPG.Systems;

namespace DataDrivenRPG.Mvc
{
    // CONTROLLER
    public class CharacterController
    {
        private readonly RpgEntityFactory _registry;

        public CharacterController(RpgEntityFactory registry) => _registry = registry;

        // Uses ref/in parameters to pass data without struct copying overhead
        public void IssueCommand<T>(in Entity entity, in T command) where T : struct, IGameCommand
        {
            command.Execute(in entity);
        }
    }

    // VIEW
    public class CharacterConsoleView
    {
        // Demonstrating ReadOnlySpan<T> utilization for hyper-fast, zero-allocation data reads
        public void RenderCharacterSheet(in Entity entity, ReadOnlySpan<IdentityComponent> identities, ReadOnlySpan<StatsComponent> stats, ReadOnlySpan<SkillsComponent> skills)
        {
            ref readonly var identity = ref identities[entity.Id];
            ref readonly var stat = ref stats[entity.Id];
            ref readonly var skill = ref skills[entity.Id];

            Console.WriteLine("=================================");
            Console.WriteLine($" CHARACTER SHEET (ID: {entity.Id})");
            Console.WriteLine("=================================");
            Console.WriteLine($" Race:       {identity.Race}");
            Console.WriteLine($" Class:      {identity.Class}");
            Console.WriteLine($" HP / MP:    {stat.Health} / {stat.Mana}");
            Console.WriteLine($" Skills:     {skill.Skills}");
            Console.WriteLine("=================================\n");
        }
    }
}
