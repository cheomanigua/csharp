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

    /// <summary>
    /// Fetches the necessary data from the registries, transforms it into 
    /// a UI-agnostic DTO, and updates the provided view.
    /// </summary>
    public void UpdateView(int entityId, IGameView view)
    {
        // 1. O(1) Fetch from registries
        // Using 'ref readonly' to avoid copying registry data unnecessarily
        ref readonly var stats = ref _registry.GetStatsForEntity(entityId);
        ref readonly var meta = ref _meta.Get(entityId);
        
        // 2. Transform raw registry data into a simple, immutable DTO
        var dto = new CharacterSheetDto(
            meta.Name,
            meta.WeaponName,
            meta.SkillName,
            stats.Health,
            stats.Mana,
            stats.Strength,
            stats.Intelligence
        );
        
        // 3. Send the DTO to the agnostic view
        view.Render(in dto);
    }
}
