using System.Text.Json;
using System.IO;
using System.Collections.Generic;

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
        
        // Initialize arrays with default values to ensure safe access
        for (int i = 0; i < MaxEntities; i++)
        {
            _names[i] = "Unknown";
            _weaponNames[i] = "Unarmed";
        }
        
        _races = JsonSerializer.Deserialize<Dictionary<string, RaceData>>(
            File.ReadAllText("Data/Character/races.json"))!;
        
        _classes = JsonSerializer.Deserialize<Dictionary<string, ClassData>>(
            File.ReadAllText("Data/Character/classes.json"))!;
        
        _weaponLookup = JsonSerializer.Deserialize<Dictionary<int, WeaponData>>(
            File.ReadAllText("Data/Items/weapons.json"))!;
    }

    public void LoadNPCFromJson(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var dto = JsonSerializer.Deserialize<NPCBlueprintDto>(json);

        if (dto != null && _races.TryGetValue(dto.Race, out var race) && 
            _classes.TryGetValue(dto.Class, out var charClass))
        {
            var stats = new CharacterStats
            {
                EntityId = dto.EntityId,
                Strength = charClass.BaseStr + race.BonusStr,
                Intelligence = charClass.BaseInt + race.BonusInt,
                Health = charClass.BaseHealth,
                Mana = charClass.BaseMana,
                IsDirty = true
            };

            _registry.RegisterStats(dto.EntityId, in stats);

            // DOD optimization: Direct array indexing by ID
            if (dto.EntityId < MaxEntities)
            {
                _names[dto.EntityId] = dto.Name;

								// RESOLVE: Look up weapon name by ID and store in the fast array
                if (_weaponLookup.TryGetValue(dto.EquippedWeaponId, out var weapon))
                {
                    _weaponNames[dto.EntityId] = weapon.Name;
                }
            }
        }
    }

    // High-performance direct access: 
    // No hashing, no allocation, no potential dictionary resizing.
    public string GetName(int entityId) => 
        (entityId >= 0 && entityId < MaxEntities) ? _names[entityId] : "Unknown";

    public string GetWeaponName(int entityId) => 
        (entityId >= 0 && entityId < MaxEntities) ? _weaponNames[entityId] : "Unarmed";
}

// Data models remain clean
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
