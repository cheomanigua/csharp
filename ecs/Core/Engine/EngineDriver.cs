using Core.Systems;
using Core.Systems.Inventory;
using Core;
using Core.Commands;
using System.Collections.Generic;
using System.Linq;

namespace Core;

public class EngineDriver
{
    private readonly EntityRegistry _registry; 
    private readonly MetadataRegistry _metaRegistry = new();
    private readonly CommandQueue _queue = new();
    private readonly Controller _controller;
    private readonly RenderSystem _renderSystem;
    private readonly StatInitializationSystem _initSystem = new();
    private readonly StatRegistry _statRegistry; // injected
    
    // Systems
    private readonly EquipmentSystem _equipmentSystem;

    public EngineDriver(IGameView view, Dictionary<int, AccessoryData> accessoryDatabase, StatRegistry statRegistry)
    {
        _statRegistry = statRegistry; 
        _registry = new EntityRegistry(accessoryDatabase, _statRegistry);
        _equipmentSystem = new EquipmentSystem(_statRegistry);
        _controller = new Controller(_registry, _metaRegistry);
        
        // FIX: Inject _statRegistry here to match the new GameViewAdapter constructor
        var adapter = new GameViewAdapter(_registry, _metaRegistry, _statRegistry);
        _renderSystem = new RenderSystem(adapter, view);
    }

    public void LoadGameData(string npcJsonPath) => _controller.LoadNPCFromJson(npcJsonPath);
    public void AddCommand(GameCommand cmd) => _queue.Enqueue(cmd);

    public void Tick(float deltaTime)
    {
        while (_queue.HasCommands)
        {
            var cmd = _queue.Dequeue();
            if (cmd.Type == CommandType.InitStats)
            {
                var bp = _controller.Blueprints.FirstOrDefault(b => b.EntityId == cmd.EntityId);
                if (bp != null)
                    _initSystem.Update(_registry, _statRegistry, _queue, bp, _controller.Classes, _controller.Races);
            }
            if (cmd.Type == CommandType.EquipItem)
                _equipmentSystem.Execute(cmd, _registry);
        }

        _registry.ProcessCombat();
        _renderSystem.Update(_registry);
        _queue.Clear();
    }
}
