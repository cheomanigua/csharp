using System;
using CrimeGame.Core.Models;

namespace CrimeGame.Core.Commands
{
    /// <summary>
    /// Encapsulates the specific operational rules for a murder event,
    /// updating both the killer and the victim simultaneously within the data model.
    /// </summary>
    public class ExecuteMurderCommand : ICommand
    {
        private readonly NPC _killer;
        private readonly NPC _victim;
        private readonly Room _scene;
        private readonly int _time;

        public ExecuteMurderCommand(NPC killer, NPC victim, Room scene, int time)
        {
            _killer = killer;
            _victim = victim;
            _scene = scene;
            _time = time;
        }

        public void Execute(WorldState state)
        {
            // 1. Process and update the Killer's timeline and spatial location
            var killerSchedule = _killer.GetComponent<ScheduleTrait>() ?? new ScheduleTrait();
            
            killerSchedule.History.Add(new NpcAction 
            { 
                StartTime = _time, 
                ActionType = "MURDER", 
                LocationId = _scene.Id 
            });
            
            _killer.AddComponent(new PositionTrait { CurrentRoomId = _scene.Id });
            
            if (!_killer.HasComponent<ScheduleTrait>()) 
                _killer.AddComponent(killerSchedule);


            // 2. Process and update the Victim's timeline and spatial location
            var victimSchedule = _victim.GetComponent<ScheduleTrait>() ?? new ScheduleTrait();
            
            victimSchedule.History.Add(new NpcAction 
            { 
                StartTime = _time, 
                ActionType = "BEING_MURDERED", 
                LocationId = _scene.Id 
            });
            
            _victim.AddComponent(new PositionTrait { CurrentRoomId = _scene.Id });
            
            if (!_victim.HasComponent<ScheduleTrait>()) 
                _victim.AddComponent(victimSchedule);
        }
    }
}
