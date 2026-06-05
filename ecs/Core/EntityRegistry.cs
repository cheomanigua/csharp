using System;

namespace Core;

public class EntityRegistry
{
    private const int MaxEntities = 1024;
    
    // Data storage (Structure of Arrays approach)
    private readonly EntitySieve<CharacterStats> _statsSieve = new(MaxEntities);
    private readonly TagGrid _tagGrid = new(MaxEntities);
    
    // Performance Optimization: Active List
    // This allows us to iterate ONLY over registered entities
    private readonly int[] _activeEntities = new int[MaxEntities];
    private int _activeCount = 0;

    // Registers a new entity and tracks it in the active list
    public void RegisterStats(int entityId, in CharacterStats stats)
    {
        _statsSieve.Set(entityId, in stats);
        _tagGrid.AddComponent(entityId, ComponentMask.Stats);
        
        // Add to the dense active list for O(N) iteration where N = active entities
        _activeEntities[_activeCount++] = entityId;
    }

    // O(1) access: Direct memory lookup
    public ref CharacterStats GetStatsForEntity(int entityId)
    {
        if (!_statsSieve.Has(entityId))
            throw new Exception($"Entity {entityId} not found.");
            
        return ref _statsSieve.Get(entityId);
    }

    // Returns a span of ONLY the active entity IDs
    public ReadOnlySpan<int> GetActiveEntities() => _activeEntities.AsSpan(0, _activeCount);

    // Iterates only over existing entities, ignoring empty buffer slots
    public void ProcessCombat()
    {
        var activeIds = GetActiveEntities();
        for (int i = 0; i < activeIds.Length; i++)
        {
            int entityId = activeIds[i];
            ref var stat = ref _statsSieve.Get(entityId);
            
            if (stat.IsDirty) 
            {
                stat.IsDirty = false;
            }
        }
    }
}
