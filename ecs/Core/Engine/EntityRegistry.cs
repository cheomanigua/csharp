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
    private readonly StatRegistry _statRegistry;

    public EntityRegistry(Dictionary<int, AccessoryData> accessoryDatabase, StatRegistry statRegistry)
    {
        _accessoryDatabase = accessoryDatabase;
        _statRegistry = statRegistry;
    }

		// Add these to EntityRegistry.cs to satisfy the compiler
public ref CharacterStats GetStatsForEntity(int entityId) 
{
    return ref _statsSieve.Get(entityId);
}

public int[] GetActiveEntities()
{
    int[] active = new int[_activeCount];
    Array.Copy(_activeEntities, active, _activeCount);
    return active;
}

    public void RegisterStats(int entityId, in CharacterStats stats)
    {
        _statsSieve.Set(entityId, in stats);
        _tagGrid.AddComponent(entityId, ComponentMask.Stats);
        _activeEntities[_activeCount++] = entityId;
    }

    public void EquipItem(int entityId, int itemId)
    {
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
            if (stat.IsDirty) 
            {
                var equipment = _equipmentSieve.Get(entityId);
                foreach (var itemId in (equipment.EquippedItemIds ?? Array.Empty<int>()))
                {
                    if (_accessoryDatabase.TryGetValue(itemId, out var item))
                    {
                        foreach (var comp in item.GrantedComponents)
                        {
                            if (comp.Tag == "AttributeModifierComponent" && comp.Modifiers != null)
                            {
                                foreach (var mod in comp.Modifiers)
                                {
                                    int idx = _statRegistry.GetIndex(mod.Target);
                                    if (idx != -1) stat.Values[idx] += (int)mod.Value;
                                }
                            }
                        }
                    }
                }
                stat.IsDirty = false;
            }
        }
    }
}
