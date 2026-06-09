using Core;
using Core.Commands;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System;

class Program
{
    static void Main(string[] args)
    {
        // Enable debugging globally
        DebugLog.Enabled = true;

        // 0. Initialize the Processor
        FormulaProcessor.Initialize("Data/System/formulas.json");
        
        // 1. Load the Manifest and Item Database
        var manifest = LoadManifest("Data/manifest.json");
        var itemDb = LoadItemDatabaseFromManifest(manifest);

        // 2. Prepare dependencies
        var view = new ConsoleGameView();
        
        // 3. Initialize the Engine 
        var engine = new EngineDriver(view, itemDb);

        // 4. Load game data
        engine.LoadGameData("Data/npc_blueprint.json");

        // 5. Queue initialization commands
        engine.AddCommand(new GameCommand { Type = CommandType.InitStats, EntityId = 1 });
        engine.AddCommand(new GameCommand { Type = CommandType.InitStats, EntityId = 2 });

        // 6. Queue equip commands
        engine.AddCommand(new GameCommand { Type = CommandType.EquipItem, EntityId = 1, TargetId = 500 });
        engine.AddCommand(new GameCommand { Type = CommandType.EquipItem, EntityId = 2, TargetId = 500 });
        engine.AddCommand(new GameCommand { Type = CommandType.EquipItem, EntityId = 2, TargetId = 800 });

        // 7. Game Loop
        bool running = true;
        while (running)
        {
            engine.Tick(1.0f / 60.0f);
            running = false; // Exit after one tick for testing
        }
    }

    private static Manifest LoadManifest(string path)
    {
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Manifest>(json) ?? new Manifest(new List<string>());
    }

    private static Dictionary<int, ItemData> LoadItemDatabaseFromManifest(Manifest manifest)
    {
        var masterDb = new Dictionary<int, ItemData>();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        foreach (var modulePath in manifest.ConfigModules)
        {
            // Only process files inside the "Items/" directory
            if (modulePath.StartsWith("Items/"))
            {
                string fullPath = Path.Combine("Data", modulePath);
                if (File.Exists(fullPath))
                {
                    string json = File.ReadAllText(fullPath);
                    var db = JsonSerializer.Deserialize<Dictionary<int, ItemData>>(json, options);
                    
                    if (db != null)
                    {
                        foreach (var kvp in db) masterDb[kvp.Key] = kvp.Value;
                        DebugLog.Log($"[SUCCESS] Loaded {db.Count} items from {modulePath}");
                    }
                }
            }
        }
        return masterDb;
    }
}

// Data structures for manifest loading
public record Manifest(List<string> ConfigModules);
