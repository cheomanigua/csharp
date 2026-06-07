namespace Core;

public class GameViewAdapter
{
    private readonly EntityRegistry _registry;
    private readonly MetadataRegistry _meta;
    private readonly StatRegistry _statRegistry; // Added for index lookups

    public GameViewAdapter(EntityRegistry registry, MetadataRegistry meta, StatRegistry statRegistry)
    {
        _registry = registry;
        _meta = meta;
        _statRegistry = statRegistry;
    }

    public void UpdateView(int entityId, IGameView view)
    {
        ref readonly var stats = ref _registry.GetStatsForEntity(entityId);
        ref readonly var meta = ref _meta.Get(entityId);
        
        // Transform the array data into a DTO using the StatRegistry indices
        var dto = new CharacterSheetDto(
            meta.Name,
            meta.WeaponName,
            meta.SkillName,
            stats.Values[_statRegistry.GetIndexOrThrow("Health")],
            stats.Values[_statRegistry.GetIndexOrThrow("Mana")],
            stats.Values[_statRegistry.GetIndexOrThrow("Strength")],
            stats.Values[_statRegistry.GetIndexOrThrow("Intelligence")]
        );
        
        view.Render(in dto);
    }
}
