using System;
using System.Collections.Generic;

namespace CrimeGame.Core.Models
{
    // 1. Structural Composition Layer (Your untouched base code)
    public abstract class Entity 
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        
        private readonly Dictionary<Type, object> _components = new();

        public void AddComponent<T>(T component) where T : notnull => _components[typeof(T)] = component;
        public T? GetComponent<T>() => _components.TryGetValue(typeof(T), out var c) ? (T)c : default;
        public bool HasComponent<T>() => _components.ContainsKey(typeof(T));
    }

    // Lightweight concrete types
    public class NPC : Entity { }
    public class Room : Entity { }
    public class Item : Entity { }


    // 2. Creational Abstraction Layer (The Interface Contract)
    public interface IEntityFactory
    {
        NPC CreateNPC(string name);
        Room CreateRoom(string name);
        Item CreateItem(string name);
    }


    // 3. The Headless C# Implementation
    public class CoreEntityFactory : IEntityFactory
    {
        public NPC CreateNPC(string name) => new NPC { Name = name };
        public Room CreateRoom(string name) => new Room { Name = name };
        public Item CreateItem(string name) => new Item { Name = name };
    }
}
