using System;
using CrimeGame.Core.Models;

namespace CrimeGame.Core.Commands
{
    public class PerformRoutineCommand : ICommand
    {
        private readonly NPC _npc;
        private readonly Room _room;
        private readonly string _actionType;
        private readonly int _time;

        public PerformRoutineCommand(NPC npc, Room room, string actionType, int time)
        {
            _npc = npc;
            _room = room;
            _actionType = actionType;
            _time = time;
        }

        public void Execute(WorldState state)
        {
            var schedule = _npc.GetComponent<ScheduleTrait>() ?? new ScheduleTrait();

            schedule.History.Add(new NpcAction {
                StartTime = _time,
                ActionType = _actionType,
                LocationId = _room.Id
            });

            _npc.AddComponent(new PositionTrait { CurrentRoomId = _room.Id });

            if (!_npc.HasComponent<ScheduleTrait>())
                _npc.AddComponent(schedule);
        }
    }
}
