using System;

namespace Core;

[Flags]
public enum ComponentMask : ulong
{
    None            = 0,
    Stats           = 1 << 0, // 1
    Weapon          = 1 << 1, // 2
    StatusEffect    = 1 << 2, // 4
    Position        = 1 << 3, // 8
    Inventory       = 1 << 4  // 16
}

public class TagGrid
{
    // High-performance alternative to Dictionary
    // Direct array access eliminates hashing overhead and minimizes cache misses.
    private readonly ComponentMask[] _entityMasks;

    public TagGrid(int capacity)
    {
        _entityMasks = new ComponentMask[capacity];
    }

    public void AddComponent(int entityId, ComponentMask mask) 
    {
        _entityMasks[entityId] |= mask; 
    }

    public void RemoveComponent(int entityId, ComponentMask mask) 
    {
        _entityMasks[entityId] &= ~mask;
    }

    public bool Has(int entityId, ComponentMask mask) 
    {
        // Direct array access: O(1) time complexity with zero allocation
        return (_entityMasks[entityId] & mask) == mask;
    }
}
