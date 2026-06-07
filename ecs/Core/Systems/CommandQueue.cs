using System.Collections.Generic;
using Core.Commands;

namespace Core.Systems;

public class CommandQueue
{
    private readonly List<GameCommand> _commands = new();
    public void Enqueue(GameCommand cmd) => _commands.Add(cmd);
    public IEnumerable<GameCommand> GetCommands() => _commands;
    public void Clear() => _commands.Clear();
}
