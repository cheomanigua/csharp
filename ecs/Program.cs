using Core;
using Core.Commands;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
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
        engine.AddCommand(new GameCommand { Type = CommandType.EquipItem, EntityId = 2, TargetId = 501 });

        // 6. Game Loop
        bool running = true;
        while (running)
        {
            engine.Tick(1.0f / 60.0f);
            running = false; // Add exit condition here
        }
    }

    private static Dictionary<int, AccessoryData> LoadAccessoryDatabase(string path)
    {
        return new Dictionary<int, AccessoryData>();
    }
}
