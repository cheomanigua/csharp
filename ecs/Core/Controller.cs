using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System;

namespace Core;

public class Controller
{
    private readonly EntityRegistry _registry;
    private readonly Dictionary<string, RaceData> _races;
    private readonly Dictionary<string, ClassData> _classes;
    private readonly Dictionary<int, WeaponData> _weaponLookup;

    // DOD optimization: Replace Dictionaries with fixed-size arrays.
    // Indexing directly by entityId gives O(1) performance with no hashing or collisions.
    private const int MaxEntities = 1024;
    private readonly string[] _names = new string[MaxEntities];
    private readonly string[] _weaponNames = new string[MaxEntities];

    public Controller(EntityRegistry registry)
    {
        _registry = registry;
        
        for (int i = 0; i < MaxEntities; i++)
        {
            _names[i] = "Unknown";
            _weaponNames[i] = "Unarmed";
        }
        
        // Load data with basic validation
        _races = LoadData<Dictionary<string, RaceData>>("Data/Character/races.json");
        _classes = LoadData<Dictionary<string, ClassData>>("Data/Character/classes.json");
        _weaponLookup = LoadData<Dictionary<int, WeaponData>>("Data/Items/weapons.json");
    }

    private T LoadData<T>(string path)
    {
        string json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<T>(json);
        if (data == null) throw new Exception($"Failed to load data from {path}");
        return data;
    }

    public void LoadNPCFromJson(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var npcs = JsonSerializer.Deserialize<List<NPCBlueprintDto>>(json);

        if (npcs == null) return;

        foreach (var dto in npcs)
        {
            if (!_races.TryGetValue(dto.Race, out var race)) continue;
            if (!_classes.TryGetValue(dto.Class, out var charClass)) continue;

            // DEBUG: Verify values before passing to FormulaProcessor
            Console.WriteLine($"DEBUG: Loading {dto.Name}. Class: {dto.Class}, Race: {dto.Race}");
            Console.WriteLine($"DEBUG: Class BaseStr: {charClass.BaseStr}, Race BonusStr: {race.BonusStr}");
            Console.WriteLine($"DEBUG: Class BaseInt: {charClass.BaseInt}, Race BonusInt: {race.BonusInt}");

            var stats = new CharacterStats { EntityId = dto.EntityId };
            
            // FormulaProcessor initialization
            FormulaProcessor.ExecuteInitialization("InitStats", ref stats, charClass, race);

            _registry.RegisterStats(dto.EntityId, in stats);

            if (dto.EntityId < MaxEntities)
            {
                _names[dto.EntityId] = dto.Name;
                if (_weaponLookup.TryGetValue(dto.EquippedWeaponId, out var weapon))
                {
                    _weaponNames[dto.EntityId] = weapon.Name;
                }
            }
        }
    }

    public string GetName(int entityId) => 
        (entityId >= 0 && entityId < MaxEntities) ? _names[entityId] : "Unknown";

    public string GetWeaponName(int entityId) => 
        (entityId >= 0 && entityId < MaxEntities) ? _weaponNames[entityId] : "Unarmed";
}

public record RaceData(int BonusStr, int BonusInt);
public record ClassData(int BaseHealth, int BaseMana, int BaseStr, int BaseInt, int PrimarySkillIndex);
public record WeaponData(string Name, int Damage);

public class NPCBlueprintDto
{
    public int EntityId { get; set; }
    public required string Name { get; set; }
    public required string Race { get; set; }
    public required string Class { get; set; }
    public int EquippedWeaponId { get; set; }
}
