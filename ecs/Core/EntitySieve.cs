using System;

namespace Core;

public class EntitySieve<T> where T : struct
{
    private T[] _data;
    private bool[] _exists; 

    public EntitySieve(int capacity) 
    {
        _data = new T[capacity];
        _exists = new bool[capacity];
    }

    // Direct access for O(1) performance
    public ref T Get(int entityId) => ref _data[entityId];
    
    public void Set(int entityId, in T item)
    {
        _data[entityId] = item;
        _exists[entityId] = true;
    }

    // Return the full span so we can check all potential slots
    public Span<T> AsSpan() => _data.AsSpan();
    
    // Helper to check if an entity index is actually used
    public bool Has(int entityId) => _exists[entityId];
}
