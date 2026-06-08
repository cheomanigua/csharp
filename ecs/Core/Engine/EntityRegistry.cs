using System;
using System.Collections.Generic;
using Core.Engine;

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

    /// <summary>
    /// Updates the equipment component and marks the character stats as dirty.
    /// </summary>
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

            var equipment = _equipmentSieve.Get(entityId);
            
            // Recalculate stats based on equipment
            foreach (var itemId in (equipment.EquippedItemIds ?? Array.Empty<int>()))
            {
                if (_accessoryDatabase.TryGetValue(itemId, out var item))
                {
                    foreach (var comp in item.GrantedComponents)
                    {
                        switch (comp.Tag)
                        {
                            case "AttributeModifierComponent":
                                if (comp.Modifiers != null)
                                {
                                    foreach (var mod in comp.Modifiers)
                                    {
                                        if (Enum.TryParse<StatType>(mod.Target, out var type))
                                            stat.Values[(int)type] += (int)mod.Value;
                                    }
                                }
                                break;

                            case "MagicActionsComponent":
                                // Logic for processing magic effects goes here
                                DebugLog.Log($"Processing magic action for {item.Name} on entity {entityId}");
                                break;
                        }
                    }
                }
            }
            stat.IsDirty = false;
        }
    }
}
