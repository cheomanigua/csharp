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
    private readonly Dictionary<string, SkillData> _skills;
    private readonly Dictionary<int, WeaponData> _weaponLookup;

    // DOD optimization: Replace Dictionaries with fixed-size arrays.
    // Indexing directly by entityId gives O(1) performance with no hashing or collisions.
    private const int MaxEntities = 1024;
    private readonly string[] _names = new string[MaxEntities];
    private readonly string[] _weaponNames = new string[MaxEntities];
    private readonly string[] _skillNames = new string[MaxEntities];

    public Controller(EntityRegistry registry)
    {
        _registry = registry;
        
        for (int i = 0; i < MaxEntities; i++)
        {
            _names[i] = "Unknown";
            _weaponNames[i] = "Unarmed";
            _skillNames[i] = "None";
        }
        
        // Load data with basic validation
        _races = LoadData<Dictionary<string, RaceData>>("Data/Character/races.json");
        _classes = LoadData<Dictionary<string, ClassData>>("Data/Character/classes.json");
        _skills = LoadData<Dictionary<string, SkillData>>("Data/Character/skills.json");
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
            Console.WriteLine($"DEBUG: Class Str: {charClass.ClassStr}, Race Str: {race.RaceStr}");
            Console.WriteLine($"DEBUG: Class Int: {charClass.ClassInt}, Race Int: {race.RaceInt}");

            var stats = new CharacterStats { EntityId = dto.EntityId };
            
            // FormulaProcessor initialization
            FormulaProcessor.ExecuteInitialization("InitStats", ref stats, charClass, race);

            _registry.RegisterStats(dto.EntityId, in stats);

            string skillName = _skills.TryGetValue(charClass.PrimarySkillIndex.ToString(), out var skill) ? skill.Name : "None";
            _skillNames[dto.EntityId] = skillName;

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
    public string GetSkillName(int entityId) =>
        (entityId >= 0 && entityId < MaxEntities) ? _skillNames[entityId] : "None";
}

public record RaceData(int RaceStr, int RaceInt);
public record ClassData(int ClassHealth, int ClassMana, int ClassStr, int ClassInt, int PrimarySkillIndex);
public record SkillData(string Name, string AttributeScale);
public record WeaponData(string Name, int Damage);

public class NPCBlueprintDto
{
    public int EntityId { get; set; }
    public required string Name { get; set; }
    public required string Race { get; set; }
    public required string Class { get; set; }
    public int EquippedWeaponId { get; set; }
}
