using System;
using System.Collections.Generic;
using DataDrivenRPG.Core;

namespace DataDrivenRPG.Systems
{
    // --- COMMAND PATTERN ---
    public interface IGameCommand
    {
        void Execute(in Entity executioner);
    }

    public readonly struct AttackCommand : IGameCommand
    {
        private readonly Entity _target;
        private readonly CombatSystem _combatSystem;
        private readonly string _skillId; // Pass the text identifier from game input / scripts

        public AttackCommand(Entity targetEntity, CombatSystem combatSystem, string skillId)
        {
            _target = targetEntity;
            _combatSystem = combatSystem;
            _skillId = skillId;
        }

        public void Execute(in Entity executioner)
        {
            _combatSystem.ProcessAttack(in executioner, in _target, _skillId);
        }
    }

    // --- DATA-DRIVEN STRUCT TEMPLATES (DTOs) ---
    public struct SkillTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public float DamageMultiplier { get; set; }
        public float Range { get; set; }
    }

    public struct WeaponTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Damage { get; set; }
        public float Weight { get; set; }
    }

    public struct CharacterTemplate
    {
        public string Id { get; set; }
        public string Race { get; set; }
        public string Class { get; set; }
        public int Health { get; set; }
        public int Mana { get; set; }
        public List<string> Skills { get; set; }
        public string EquippedWeapon { get; set; } 
    }

    // --- DYNAMIC SINGLE SOURCE OF TRUTH FOR SKILLS ---
    public static class SkillDatabase
    {
        // Case-insensitive mapping from "Archery" -> bitmask flag (1, 2, 4, 8...)
        private static readonly Dictionary<string, int> _skillToBitMask = new(StringComparer.InvariantCultureIgnoreCase);
        // Mapping from bitmask flag -> full configuration dataset
        private static readonly Dictionary<int, SkillTemplate> _bitMaskToStats = new();

        public static void LoadDatabase(List<SkillTemplate> templates)
        {
            _skillToBitMask.Clear();
            _bitMaskToStats.Clear();

            for (int i = 0; i < templates.Count; i++)
            {
                // Accumulate unique single-bit sequences cleanly up to 32 independent skills
                int bitMask = 1 << i; 
                
                _skillToBitMask[templates[i].Id] = bitMask;
                _bitMaskToStats[bitMask] = templates[i];
                
                Console.WriteLine($"[Skill Engine] Registered: {templates[i].Id} -> Binary Bitmask: {bitMask}");
            }
        }

        // Loops text arrays on initialization to return a single combined bitwise integer back
        public static int GetCombinedBitmask(List<string> skillIds)
        {
            int combined = 0;
            if (skillIds == null) return combined;

            foreach (var id in skillIds)
            {
                if (_skillToBitMask.TryGetValue(id, out int mask))
                {
                    combined |= mask; // Stack bit switches together
                }
            }
            return combined;
        }

        public static bool TryGetSkillByMask(int bitMask, out SkillTemplate skill)
        {
            return _bitMaskToStats.TryGetValue(bitMask, out skill);
        }

        public static int GetSingleBitmask(string skillId)
        {
            return _skillToBitMask.TryGetValue(skillId, out int mask) ? mask : 0;
        }
    }

    // --- THE COMBAT SYSTEM ---
    public class CombatSystem
    {
        private readonly RpgEntityFactory _registry;
        private readonly Dictionary<int, int> _weaponDamageDatabase;

        public CombatSystem(RpgEntityFactory registry, Dictionary<int, int> weaponDb)
        {
            _registry = registry;
            _weaponDamageDatabase = weaponDb;
        }

        public void ProcessAttack(in Entity attacker, in Entity target, string skillId)
        {
            // 1. Fetch runtime calculated lookup bit sequence
            int requiredBit = SkillDatabase.GetSingleBitmask(skillId);
            
            // 2. Hardware-level bitwise verification check (&) with zero heap garbage overhead
            ref readonly var skillsComponent = ref _registry.SkillsPool[attacker.Id];
            if ((skillsComponent.Skills & requiredBit) == 0)
            {
                Console.WriteLine($"\n[Combat System] Execution Denied: Entity {attacker.Id} does not know the '{skillId}' skill!");
                return;
            }

            // 3. Look up weapon base damage from string-hash array
            int weaponHash = _registry.EquipmentPool[attacker.Id];
            if (!_weaponDamageDatabase.TryGetValue(weaponHash, out int baseDamage))
            {
                baseDamage = 5; // Unarmed default fallback punch value
            }

            // 4. Look up skill metadata metrics via dynamic mask index
            SkillDatabase.TryGetSkillByMask(requiredBit, out var skillData);

            // 5. In-place struct reference data modifications
            ref var targetStats = ref _registry.StatsPool[target.Id];
            int finalDamage = (int)(baseDamage * skillData.DamageMultiplier);
            targetStats.Health -= finalDamage;

            Console.WriteLine($"\n[Combat System] Entity {attacker.Id} attacked using {skillData.Name}!");
            Console.WriteLine($"               Weapon Base Damage: {baseDamage} | Skill Multiplier: {skillData.DamageMultiplier}x");
            Console.WriteLine($"               Hit Entity {target.Id} for {finalDamage} DMG.");
            Console.WriteLine($"               Target HP remaining: {targetStats.Health}");
        }
    }

    // --- ABSTRACT ECS FACTORY ---
    public interface IEntityFactory
    {
        Entity CreateCharacter(in CharacterTemplate template);
    }

    public class RpgEntityFactory : IEntityFactory
    {
        private int _entityIdCounter = 0;

        public IdentityComponent[] IdentityPool = new IdentityComponent[100];
        public StatsComponent[] StatsPool = new StatsComponent[100];
        public SkillsComponent[] SkillsPool = new SkillsComponent[100];
        public int[] EquipmentPool = new int[100]; 

        public Entity CreateCharacter(in CharacterTemplate template)
        {
            int id = _entityIdCounter++;
            Entity entity = new Entity(id);

            IdentityPool[id] = new IdentityComponent
            {
                Race = Enum.Parse<Race>(template.Race),
                Class = Enum.Parse<CharacterClass>(template.Class)
            };

            StatsPool[id] = new StatsComponent
            {
                Health = template.Health,
                Mana = template.Mana
            };

            // Ingests string array tracking sets and encodes them into dense integer primitives
            SkillsPool[id] = new SkillsComponent 
            { 
                Skills = SkillDatabase.GetCombinedBitmask(template.Skills) 
            };

            // String normalization protection guard checks
            EquipmentPool[id] = string.IsNullOrEmpty(template.EquippedWeapon) 
                ? 0 
                : template.EquippedWeapon.Trim().ToLowerInvariant().GetHashCode();

            return entity;
        }
    }
}
