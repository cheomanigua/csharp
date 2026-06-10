namespace Core;

public class GameViewAdapter
{
    private readonly EntityRegistry _registry;
    private readonly MetadataRegistry _meta;

    public GameViewAdapter(EntityRegistry registry, MetadataRegistry meta)
    {
        _registry = registry;
        _meta = meta;
    }

    public unsafe void UpdateView(int entityId, IGameView view)
    {
        ref var hotData = ref _registry.GetHotData(entityId);
        ref readonly var meta = ref _meta.Get(entityId);
        
        var dto = new CharacterSheetDto(
            meta.Name,
            meta.WeaponName,
            meta.SkillName,
            hotData.Stats[(int)StatType.Health],
            hotData.Stats[(int)StatType.Mana],
            hotData.Stats[(int)StatType.Strength],
            hotData.Stats[(int)StatType.Intelligence]
        );
        
        view.Render(in dto);
    }
}
