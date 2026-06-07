using System.Collections.Generic;

namespace Core.Commands;

public class CommandQueue
{
    private readonly Queue<GameCommand> _queue = new();
    public bool HasCommands => _queue.Count > 0;
    public void Enqueue(GameCommand cmd) => _queue.Enqueue(cmd);
    public GameCommand Dequeue() => _queue.Dequeue();
    public void Clear() => _queue.Clear();
    public IEnumerable<GameCommand> GetCommands() => _queue;
}
