using Core;
using Core.Commands;
using Core.Engine;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        // Enable debugging globally across the entire project
        DebugLog.Enabled = true;

        // 0. Initialize the Processor
        FormulaProcessor.Initialize("Data/System/formulas.json");
        
        // 1. Prepare dependencies
        var view = new ConsoleGameView();
        var accessoryDb = LoadAccessoryDatabase("Data/Items/accessories.json");

        // 2. Initialize the Engine 
        var engine = new EngineDriver(view, accessoryDb);

        // 3. Load data
        engine.LoadGameData("Data/npc_blueprint.json");

        // 4. Queue initialization commands
        engine.AddCommand(new GameCommand { Type = CommandType.InitStats, EntityId = 1 });
        engine.AddCommand(new GameCommand { Type = CommandType.InitStats, EntityId = 2 });

        // 5. Queue equip command
        // This will now find 502 in the dictionary and succeed!
        engine.AddCommand(new GameCommand { Type = CommandType.EquipItem, EntityId = 1, TargetId = 501 });
        engine.AddCommand(new GameCommand { Type = CommandType.EquipItem, EntityId = 2, TargetId = 502 });
        engine.AddCommand(new GameCommand { Type = CommandType.EquipItem, EntityId = 2, TargetId = 703 });

        // 6. Game Loop
        bool running = true;
        while (running)
        {
            engine.Tick(1.0f / 60.0f);
            running = false; // Exit after one tick for testing
        }
    }

    private static Dictionary<int, AccessoryData> LoadAccessoryDatabase(string path)
    {
        if (!File.Exists(path))
        {
            DebugLog.Log($"[ERROR] Accessory database file not found at: {path}");
            return new Dictionary<int, AccessoryData>();
        }

        try
        {
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            var db = JsonSerializer.Deserialize<Dictionary<int, AccessoryData>>(json, options);
            
            DebugLog.Log($"[SUCCESS] Loaded {db?.Count ?? 0} accessories from {path}");
            return db ?? new Dictionary<int, AccessoryData>();
        }
        catch (System.Exception ex)
        {
            DebugLog.Log($"[ERROR] Failed to parse accessory database: {ex.Message}");
            return new Dictionary<int, AccessoryData>();
        }
    }
}
