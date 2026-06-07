using Core;

namespace Core.Systems;

public class RenderSystem
{
    private readonly GameViewAdapter _adapter;
    private readonly IGameView _view;

    public RenderSystem(GameViewAdapter adapter, IGameView view)
    {
        _adapter = adapter;
        _view = view;
    }

    public void Update(EntityRegistry registry)
    {
        var entities = registry.GetActiveEntities();
        foreach (int entityId in entities)
        {
            _adapter.UpdateView(entityId, _view);
        }
    }
}
