using System;
using System.Collections.Generic;

namespace Core;

public class EntityRegistry
{
    private const int MaxEntities = 1024;
    private readonly ItemData[] _itemDatabase = new ItemData[EngineConfig.MaxItemCapacity];
    private readonly EntitySieve<CharacterStats> _statsSieve = new(MaxEntities);
    private readonly EntitySieve<EquipmentComponent> _equipmentSieve = new(MaxEntities);
    private readonly TagGrid _tagGrid = new(MaxEntities);
    private readonly int[] _activeEntities = new int[MaxEntities];
    private int _activeCount = 0;

    public EntityRegistry(Dictionary<int, ItemData> itemDatabase)
    {
        // Copy the loaded dictionary into the array
        foreach (var kvp in itemDatabase)
        {
            if (kvp.Key < EngineConfig.MaxItemCapacity) _itemDatabase[kvp.Key] = kvp.Value;
        }
    }

    public ref CharacterStats GetStatsForEntity(int entityId) => ref _statsSieve.Get(entityId);

    public void RegisterStats(int entityId, in CharacterStats stats)
    {
        _statsSieve.Set(entityId, in stats);
        _tagGrid.AddComponent(entityId, ComponentMask.Stats);
        _activeEntities[_activeCount++] = entityId;
    }

    public void EquipItem(int entityId, int itemId)
    {
        // Direct array access is now O(1) and cache-friendly
        if (itemId >= EngineConfig.MaxItemCapacity || _itemDatabase[itemId] == null)
        {
            DebugLog.Log($"EquipItem: FAILED. Item {itemId} does not exist in database.");
            return; 
        }
        ref var equipment = ref _equipmentSieve.Get(entityId);
        var items = new List<int>(equipment.EquippedItemIds ?? Array.Empty<int>()) { itemId };
        equipment.EquippedItemIds = items.ToArray();
        
        ref var stats = ref _statsSieve.Get(entityId);
        stats.IsDirty = true;
    }

    public void ProcessCombat()
    {
        for (int i = 0; i < _activeCount; i++)
        {
            int entityId = _activeEntities[i];
            ref var stat = ref _statsSieve.Get(entityId);
            if (!stat.IsDirty) continue;
    
            // RECALCULATE: Apply equipment modifiers directly to existing values
            var equipment = _equipmentSieve.Get(entityId);
            
            foreach (var itemId in (equipment.EquippedItemIds ?? Array.Empty<int>()))
            {
                // Direct array access check
                if (itemId >= 0 && itemId < EngineConfig.MaxItemCapacity)
                {
                    var item = _itemDatabase[itemId];
                    
                    // Ensure the slot is not null before processing
                    if (item != null)
                    {
                        foreach (var comp in item.GrantedComponents)
                        {
                            switch (comp.Tag)
                            {
                                case "AttributeComponent":
                                    if (comp.Properties != null && comp.Properties.TryGetValue("Target", out var target))
                                    {
                                        if (Enum.TryParse<StatType>(target, out var type) && 
                                            comp.Properties.TryGetValue("Value", out var valStr))
                                        {
                                            stat.Values[(int)type] += (int)float.Parse(valStr);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            stat.IsDirty = false;
        }
    }


    public int[] GetActiveEntities()
    {
        int[] active = new int[_activeCount];
        Array.Copy(_activeEntities, active, _activeCount);
        return active;
    }
}
