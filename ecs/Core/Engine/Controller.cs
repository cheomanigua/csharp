using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System;

namespace Core
{
    public class MetadataRegistry
    {
        private const int MaxEntities = 1024;
        private readonly MetadataComponent[] _metadata = new MetadataComponent[MaxEntities];

        public void Register(int entityId, string name, string weapon, string skill) =>
            _metadata[entityId] = new MetadataComponent { Name = name, WeaponName = weapon, SkillName = skill };

        public ref readonly MetadataComponent Get(int entityId) => ref _metadata[entityId];
    }

    public class Controller
    {
        private readonly EntityRegistry _registry;
        private readonly MetadataRegistry _metaRegistry;
        private readonly Dictionary<string, RaceData> _races;
        private readonly Dictionary<string, ClassData> _classes;
        private readonly Dictionary<string, SkillData> _skills;
        private readonly Dictionary<int, ItemData> _itemDatabase = new();
        private List<NPCBlueprintDto> _blueprints = new();

        public IReadOnlyDictionary<string, RaceData> Races => _races;
        public IReadOnlyDictionary<string, ClassData> Classes => _classes;
        public IReadOnlyList<NPCBlueprintDto> Blueprints => _blueprints;

        public Controller(EntityRegistry registry, MetadataRegistry metaRegistry)
        {
            _registry = registry;
            _metaRegistry = metaRegistry;
            
            _races = LoadData<Dictionary<string, RaceData>>("Data/Character/races.json");
            _classes = LoadData<Dictionary<string, ClassData>>("Data/Character/classes.json");
            _skills = LoadData<Dictionary<string, SkillData>>("Data/Character/skills.json");

            LoadAndMerge("Data/Items/weapons.json");
            LoadAndMerge("Data/Items/potions.json");
            LoadAndMerge("Data/Items/accessories.json");
        }

        private void LoadAndMerge(string path)
        {
            if (!File.Exists(path)) return;
            var data = LoadData<Dictionary<int, ItemData>>(path);
            foreach (var entry in data) _itemDatabase[entry.Key] = entry.Value;
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
            _blueprints = npcs;

            foreach (var dto in npcs)
            {
                if (!_races.TryGetValue(dto.Race, out var race)) continue;
                if (!_classes.TryGetValue(dto.Class, out var charClass)) continue;

                var stats = new CharacterStats { EntityId = dto.EntityId };
                _registry.RegisterStats(dto.EntityId, in stats);

                string skillName = _skills.TryGetValue(charClass.PrimarySkillIndex.ToString(), out var skill) 
                    ? skill.Name : "None";
                
                string itemName = _itemDatabase.TryGetValue(dto.EquippedItemId, out var item) 
                    ? item.Name : "Unarmed";

                _metaRegistry.Register(dto.EntityId, dto.Name, itemName, skillName);
            }
        }
    }

    public class NPCBlueprintDto
    {
        public int EntityId { get; set; }
        public required string Name { get; set; }
        public required string Race { get; set; }
        public required string Class { get; set; }
        public int EquippedItemId { get; set; }
    }
}
