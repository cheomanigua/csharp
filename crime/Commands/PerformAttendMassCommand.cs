using System;
using CrimeGame.Core.Models;

namespace CrimeGame.Core.Commands
{
    public class PerformAttendMassCommand : ICommand
    {
        private readonly NPC _monk;
        private readonly Room _chapel;
        private readonly int _time;
        private readonly string _liturgyType; // e.g., "Vespers", "Matins"

        public PerformAttendMassCommand(NPC monk, Room chapel, int time, string liturgyType)
        {
            _monk = monk;
            _chapel = chapel;
            _time = time;
            _liturgyType = liturgyType;
        }

        public void Execute(WorldState state)
        {
            var schedule = _monk.GetComponent<ScheduleTrait>() ?? new ScheduleTrait();
            
            // Log the specific monastic action
            schedule.History.Add(new NpcAction {
                StartTime = _time,
                ActionType = $"MASS:{_liturgyType}",
                LocationId = _chapel.Id
            });

            // Physically move them there
            _monk.AddComponent(new PositionTrait { CurrentRoomId = _chapel.Id });

            // Gain a tiny devotion trait or sanity buff? Easy to add here!
            if (!_monk.HasComponent<ScheduleTrait>())
                _monk.AddComponent(schedule);
        }
    }
}
