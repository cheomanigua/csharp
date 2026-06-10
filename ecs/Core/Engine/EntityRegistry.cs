using System;
using System.Collections.Generic;

namespace Core;

public class EntityRegistry
{
    private readonly ItemData[] _itemDatabase = new ItemData[EngineConfig.MaxItemCapacity];
    private unsafe readonly EntityHotData[] _hotData = new EntityHotData[EngineConfig.MaxEntities];
    private readonly TagGrid _tagGrid = new(EngineConfig.MaxEntities);
    private readonly int[] _activeEntities = new int[EngineConfig.MaxEntities];
    private int _activeCount = 0;

    public EntityRegistry(Dictionary<int, ItemData> itemDatabase)
    {
        foreach (var kvp in itemDatabase)
        {
            if (kvp.Key < EngineConfig.MaxItemCapacity) 
                _itemDatabase[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>
    /// Main accessor - Pure AoS for best cache locality with 5000+ NPCs
    /// </summary>
    public ref EntityHotData GetHotData(int entityId) => ref _hotData[entityId];

    public unsafe void RegisterStats(int entityId, in EntityHotData data)
    {
        _hotData[entityId] = data;
        _tagGrid.AddComponent(entityId, ComponentMask.Stats);
        _activeEntities[_activeCount++] = entityId;
    }

    public unsafe void EquipItem(int entityId, int itemId)
    {
        if (itemId >= EngineConfig.MaxItemCapacity || _itemDatabase[itemId] == null)
        {
            DebugLog.Log($"EquipItem: FAILED. Item {itemId} does not exist in database.");
            return; 
        }

        ref var data = ref _hotData[entityId];
        data.AddEquippedItem(itemId);
    }

    public unsafe void ProcessCombat()
    {
        for (int i = 0; i < _activeCount; i++)
        {
            int eid = _activeEntities[i];
            ref var data = ref _hotData[eid];

            if (!data.IsDirty) continue;

            for (int j = 0; j < data.EquippedItemCount; j++)
            {
                int itemId = data.EquippedItemIds[j];
                if (itemId < 0 || itemId >= EngineConfig.MaxItemCapacity) continue;

                var item = _itemDatabase[itemId];
                if (item == null) continue;

                foreach (var comp in item.GrantedComponents)
                {
                    if (comp.Tag == "AttributeComponent" &&
                        comp.Properties?.TryGetValue("Target", out var target) == true &&
                        comp.Properties.TryGetValue("Value", out var valStr))
                    {
                        if (Enum.TryParse<StatType>(target, out var type))
                        {
                            data.Stats[(int)type] += (int)float.Parse(valStr);
                        }
                    }
                }
            }

            data.IsDirty = false;
        }
    }

    public int[] GetActiveEntities()
    {
        int[] active = new int[_activeCount];
        Array.Copy(_activeEntities, active, _activeCount);
        return active;
    }
}
