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

    public void UpdateView(int entityId, IGameView view)
    {
        ref readonly var stats = ref _registry.GetStatsForEntity(entityId);
        ref readonly var meta = ref _meta.Get(entityId);
        
        var dto = new CharacterSheetDto(
            meta.Name,
            meta.WeaponName,
            meta.SkillName,
            stats.Values[(int)StatType.Health],
            stats.Values[(int)StatType.Mana],
            stats.Values[(int)StatType.Strength],
            stats.Values[(int)StatType.Intelligence]
        );
        
        view.Render(in dto);
    }
}
