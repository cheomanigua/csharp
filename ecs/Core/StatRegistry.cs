using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace Core;

public class StatRegistry
{
    private readonly Dictionary<string, int> _intStatIndex = new();
    
    /// <summary>
    /// Returns the total number of stats registered. 
    /// Useful for initializing the CharacterStats array size.
    /// </summary>
    public int IntStatCount => _intStatIndex.Count;

    /// <summary>
    /// Initializes the registry from a JSON definition file.
    /// </summary>
    public void Initialize(string jsonPath)
    {
        string json = File.ReadAllText(jsonPath);
        using JsonDocument doc = JsonDocument.Parse(json);
        
        int idx = 0;
        foreach (var stat in doc.RootElement.GetProperty("Stats").EnumerateArray())
        {
            string name = stat.GetProperty("Name").GetString()!;
            _intStatIndex[name] = idx++;
        }
    }

    /// <summary>
    /// Returns the index for a given stat name. Returns -1 if not found.
    /// </summary>
    public int GetIndex(string name) => _intStatIndex.TryGetValue(name, out var idx) ? idx : -1;

    /// <summary>
    /// Returns the index for a given stat name. Throws an exception if not found,
    /// which is highly recommended for catching typos in JSON data during development.
    /// </summary>
    public int GetIndexOrThrow(string name) 
    {
        int idx = GetIndex(name);
        if (idx == -1) 
            throw new Exception($"Stat '{name}' not found in registry! Check your StatsDefinition.json.");
        return idx;
    }
}
