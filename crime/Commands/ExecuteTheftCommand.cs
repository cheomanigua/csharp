using System;
using CrimeGame.Core.Models;

namespace CrimeGame.Core.Commands
{
    public class ExecuteTheftCommand : ICommand
    {
        private readonly NPC _thief;
        private readonly Item _targetItem;
        private readonly Room _location;
        private readonly int _time;

        public ExecuteTheftCommand(NPC thief, Item targetItem, Room location, int time)
        {
            _thief = thief;
            _targetItem = targetItem;
            _location = location;
            _time = time;
        }

        public void Execute(WorldState state)
        {
            // 1. Mutate item components to assign new hidden data
            var evidence = _targetItem.GetComponent<EvidenceTrait>() ?? new EvidenceTrait();
            evidence.OwnerId = _thief.Id;
            evidence.Description = $"Stolen property! Taken by {_thief.Name} from the {_location.Name}.";
            _targetItem.AddComponent(evidence);

            // 2. Add an action flag to the thief's history log
            var schedule = _thief.GetComponent<ScheduleTrait>() ?? new ScheduleTrait();
            schedule.History.Add(new NpcAction 
            {
                StartTime = _time,
                ActionType = $"THEFT:{_targetItem.Name}",
                LocationId = _location.Id
            });

            if (!_thief.HasComponent<ScheduleTrait>())
                _thief.AddComponent(schedule);
        }
    }
}
