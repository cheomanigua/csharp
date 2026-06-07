using Core.Systems;
using Core;
using Core.Commands;

namespace Core;

public class EngineDriver
{
    private readonly EntityRegistry _registry = new();
    private readonly MetadataRegistry _metaRegistry = new();
    private readonly CommandQueue _queue = new();
    private readonly Controller _controller;
    private readonly RenderSystem _renderSystem;

    public EngineDriver(IGameView view)
    {
        _controller = new Controller(_registry, _metaRegistry);
        _renderSystem = new RenderSystem(new GameViewAdapter(_registry, _metaRegistry), view);
    }

    // Restored this method so the game can initialize blueprints
    public void LoadGameData(string npcJsonPath)
    {
        _controller.LoadNPCFromJson(npcJsonPath);
    }

    public void AddCommand(GameCommand cmd) => _queue.Enqueue(cmd);

    public void Tick(float deltaTime)
    {
        // 1. Logic systems would go here
        // 2. Rendering
        _renderSystem.Update(_registry);
        
        // 3. Clear transient commands
        _queue.Clear();
    }
}
