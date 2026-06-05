using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DataDrivenRPG.Core;
using DataDrivenRPG.Systems;
using DataDrivenRPG.Mvc;

namespace DataDrivenRPG
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            Console.WriteLine("=== INITIALIZING DATA ENGINE ===");

            // ==========================================
            // STEP 1: LOAD DYNAMIC SKILLS DATABASE
            // ==========================================
            string skillsPath = "skills.json";
            if (!File.Exists(skillsPath))
            {
                Console.WriteLine($"Critical Error: Missing mandatory asset file '{skillsPath}'");
                return;
            }

            string skillsJson = File.ReadAllText(skillsPath);
            List<SkillTemplate>? skillTemplates = JsonSerializer.Deserialize<List<SkillTemplate>>(skillsJson, jsonOptions);
            if (skillTemplates != null)
            {
                SkillDatabase.LoadDatabase(skillTemplates);
            }

            // ==========================================
            // STEP 2: LOAD DYNAMIC WEAPON DATABASE
            // ==========================================
            var weaponDamageDatabase = new Dictionary<int, int>();
            string weaponsPath = "weapons.json";

            if (!File.Exists(weaponsPath))
            {
                Console.WriteLine($"Critical Error: Missing mandatory asset file '{weaponsPath}'");
                return;
            }

            string weaponsJson = File.ReadAllText(weaponsPath);
            List<WeaponTemplate>? weaponTemplates = JsonSerializer.Deserialize<List<WeaponTemplate>>(weaponsJson, jsonOptions);

            if (weaponTemplates != null)
            {
                foreach (var weapon in weaponTemplates)
                {
                    int weaponHash = weapon.Id.Trim().ToLowerInvariant().GetHashCode();
                    weaponDamageDatabase[weaponHash] = weapon.Damage;
                }
            }

            // ==========================================
            // STEP 3: INITIALIZE ECS SUB-ARCHITECTURE
            // ==========================================
            RpgEntityFactory registry = new RpgEntityFactory();
            CombatSystem combatSystem = new CombatSystem(registry, weaponDamageDatabase);
            CharacterController controller = new CharacterController(registry);
            CharacterConsoleView view = new CharacterConsoleView();

            // ==========================================
            // STEP 4: LOAD CHARACTERS FROM DISK
            // ==========================================
            string charactersPath = "characters.json";
            if (!File.Exists(charactersPath))
            {
                Console.WriteLine($"Critical Error: Missing profile layout structure file '{charactersPath}'");
                return;
            }

            string charactersJson = File.ReadAllText(charactersPath);
            List<CharacterTemplate>? characterTemplates = JsonSerializer.Deserialize<List<CharacterTemplate>>(charactersJson, jsonOptions);

            if (characterTemplates == null || characterTemplates.Count < 2)
            {
                Console.WriteLine("Error: Dataset configuration file did not contain enough template records.");
                return;
            }

            // ==========================================
            // STEP 5: RUNTIME EXECUTION SIMULATION
            // ==========================================
            // Entity 0: human_warrior (Equipped with steel_claymore: 28 damage)
            // Entity 1: orc_berserker (Equipped with iron_sword: 15 damage)
            Entity attackerEntity = registry.CreateCharacter(characterTemplates[0]); 
            Entity targetEntity = registry.CreateCharacter(characterTemplates[1]);   

            ReadOnlySpan<IdentityComponent> identities = registry.IdentityPool;
            ReadOnlySpan<StatsComponent> stats = registry.StatsPool;
            ReadOnlySpan<SkillsComponent> skills = registry.SkillsPool;

            Console.WriteLine("\n--- INITIAL GAME STATE (LOADED VIA DATA FILES) ---");
            view.RenderCharacterSheet(in attackerEntity, identities, stats, skills);
            view.RenderCharacterSheet(in targetEntity, identities, stats, skills);

            // Execute an action using data keys
            // human_warrior attacks using "TwoHandedWeapons"
            AttackCommand attackCmd = new AttackCommand(targetEntity, combatSystem, "TwoHandedWeapons");
            
            Console.WriteLine("\n--- EXECUTING ACTION COMMAND ---");
            controller.IssueCommand(in attackerEntity, in attackCmd);

            Console.WriteLine("\n--- POST-ACTION GAME STATE ---");
            view.RenderCharacterSheet(in targetEntity, identities, stats, skills);

            Console.ReadLine();
        }
    }
}
