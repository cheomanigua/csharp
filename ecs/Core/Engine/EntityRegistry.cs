using System;
using System.Collections.Generic;

namespace Core;

public class EntityRegistry
{
    private const int MaxEntities = 1024;
    private readonly EntitySieve<CharacterStats> _statsSieve = new(MaxEntities);
    private readonly EntitySieve<EquipmentComponent> _equipmentSieve = new(MaxEntities);
    private readonly TagGrid _tagGrid = new(MaxEntities);
    private readonly int[] _activeEntities = new int[MaxEntities];
    private int _activeCount = 0;
    private readonly Dictionary<int, AccessoryData> _accessoryDatabase;

    public EntityRegistry(Dictionary<int, AccessoryData> accessoryDatabase)
    {
        _accessoryDatabase = accessoryDatabase;
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
        if (!_accessoryDatabase.ContainsKey(itemId))
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
                if (_accessoryDatabase.TryGetValue(itemId, out var item))
                {
                    foreach (var comp in item.GrantedComponents)
                    {
                        switch (comp.Tag)
                        {
                            case "AttributeComponent":
                                // If your DTO record doesn't have Properties, ensure it is added to Model.cs
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
