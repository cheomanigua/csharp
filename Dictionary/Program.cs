using System;
using System.Collections.Generic;
using System.Text.Json;

public class NpcStats
{
    public required string RaceName { get; set; }
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Endurance { get; set; }
    public int Intelligence { get; set; }
    public int Health { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        // Read JSON file
        string jsonString = File.ReadAllText("creaturesclass.json");

        // Deserialize (No extra options needed because the keys match exactly!)
        var creatures = JsonSerializer.Deserialize<Dictionary<string, NpcStats>>(jsonString) ?? throw new Exception("Failed to deserialize creatures JSON.");

        // Print the dictionary formatted as JSON (serialization)
        string jsonOutput = JsonSerializer.Serialize(creatures, new JsonSerializerOptions { WriteIndented = true });
        Console.Write(jsonOutput);

        // Setup lookup variables for the documentation samples below
        var ckey = "goblin";

        // Cache the properties once via reflection to keep loop execution performant
        var properties = typeof(NpcStats).GetProperties();


        // ====================================================================
        // PART 1: ITERATION VARIATIONS
        // ====================================================================
        Console.WriteLine("\n===== 1. ITERATION EXAMPLES =====");

        // A. Print Primary Keys (The high-level dictionary keys)
        Console.WriteLine("--- PRIMARY KEYS ---");
        foreach (var race in creatures)
        {
            Console.WriteLine(race.Key);
        }

        // B. Print attributes of a SINGLE creature (VERSION 1: Dynamic Reflection)
        Console.WriteLine("\n--- SINGLE CREATURE: VERSION 1 (Reflection) ---");
        foreach (var property in properties)
        {
            var value = property.GetValue(creatures[ckey]);
            Console.WriteLine($"{property.Name}: {value}");
        }

        // C. Print attributes of a SINGLE creature (VERSION 2: Static / Hardcoded)
        Console.WriteLine("\n--- SINGLE CREATURE: VERSION 2 (Direct Property Access) ---");
        var gob = creatures[ckey];
        Console.WriteLine($"RaceName: {gob.RaceName}");
        Console.WriteLine($"Strength: {gob.Strength}");
        Console.WriteLine($"Dexterity: {gob.Dexterity}");
        Console.WriteLine($"Endurance: {gob.Endurance}");
        Console.WriteLine($"Intelligence: {gob.Intelligence}");
        Console.WriteLine($"Health: {gob.Health}");


        // --- PRINT ALL CREATURES VARIATIONS ---

        // Loop Version A: Direct Property Access
        // Best for production gameplay logic. Incredibly fast, type-safe, and
        // benefits from IDE autocomplete.
        Console.WriteLine("\n--- ALL CREATURES: VERSION A (Direct) ---");
        foreach (var creature in creatures)
        {
            Console.WriteLine($"Creature: {creature.Key}");
            Console.WriteLine($"  RaceName: {creature.Value.RaceName}");
            Console.WriteLine($"  Strength: {creature.Value.Strength}");
            Console.WriteLine($"  Dexterity: {creature.Value.Dexterity}");
            Console.WriteLine($"  Endurance: {creature.Value.Endurance}");
            Console.WriteLine($"  Intelligence: {creature.Value.Intelligence}");
            Console.WriteLine($"  Health: {creature.Value.Health}");
        }

        // Loop Version B: Reflection
        // Perfect for telemetry, generalized debugging, or log-dumps. If you add
        // new stats to the blueprint class later, this loop auto-updates itself.
        Console.WriteLine("\n--- ALL CREATURES: VERSION B (Reflection) ---");
        foreach (var creature in creatures)
        {
            Console.WriteLine($"Creature: {creature.Key}");
            foreach (var property in properties)
            {
                var value = property.GetValue(creature.Value);
                Console.WriteLine($"  {property.Name}: {value}");
            }
        }

        // Loop Version C: LINQ / Inline String Join
        // Highly compact variation using a LINQ projection to smash all
        // data names and values into a single inline print statement.
        Console.WriteLine("\n--- ALL CREATURES: VERSION C (LINQ / Inline) ---");
        foreach (var creature in creatures)
        {
            var valuesList = properties.Select(p => $"{p.Name}: {p.GetValue(creature.Value)}");
            Console.WriteLine($"Creature: {creature.Key} -> {string.Join(", ", valuesList)}");
        }


        // ====================================================================
        // PART 2: ACCESSING PARTICULAR KEYS AND VALUES
        // ====================================================================
        Console.WriteLine("\n===== 2. ACCESS PARTICULAR KEYS AND VALUES =====");

        // Accessing list of primary keys
        Console.WriteLine(string.Join(", ", creatures.Keys));                // agoiru, orc, adivia, human, goblin

        // Accessing list of primary values (Outputs the loaded C# object types)
        Console.WriteLine(string.Join(", ", creatures.Values));

        // Accessing list of secondary keys (Property names from the class definition via LINQ)
        var secondaryKeys = properties.Select(p => p.Name);
        Console.WriteLine(string.Join(", ", secondaryKeys));                 // RaceName, Strength, Dexterity, Endurance, Intelligence, Health

        // Accessing list of secondary values for a specific key
        var secondaryValues = properties.Select(p => p.GetValue(creatures[ckey]));
        Console.WriteLine(string.Join(", ", secondaryValues));                // goblin, 5, 7, 7, 5, 10

        // Accessing goblin strength directly
        // Notice how clean, type-safe, and compile-checked this is compared to string dictionary lookups!
        Console.WriteLine(creatures["goblin"].Strength);                     // 5
        Console.WriteLine(creatures[ckey].Strength);
    }
}
