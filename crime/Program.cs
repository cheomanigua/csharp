using System;
using System.Linq;
using CrimeGame.Core.Models;
using CrimeGame.Core.Controllers;

class Program 
{
    static void Main() 
    {
        IEntityFactory coreFactory = new CoreEntityFactory();
        var engine = new ScenarioEngine(coreFactory);
        var world = engine.Generate("Abbey");

        RenderConsoleView(world);
    }

    static void RenderConsoleView(WorldState world)
    {
        Console.WriteLine("--- [MVC VIEW LAYER - MYSTERY SIMULATION RUN] ---");
        
        var killer = world.Characters.FirstOrDefault(c => c.Id == world.KillerId);
        var victim = world.Characters.FirstOrDefault(c => c.Id == world.VictimId);

        if (killer == null || victim == null) return;

        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"[CRIME INCIDENT] {killer.Name} murdered {victim.Name}.\n");
        Console.ResetColor();

        // --- THEFT EVENT VIEW RENDERING ---
        Console.WriteLine("--- Incidental Crimes Blotter (Theft Events) ---");
        bool theftOccurred = false;

        foreach (var npc in world.Characters)
        {
            var schedule = npc.GetComponent<ScheduleTrait>();
            if (schedule == null) continue;

            // Isolate and scan history arrays exclusively for our customized theft tokens
            var thefts = schedule.History.Where(h => h.ActionType.StartsWith("THEFT:"));
            foreach (var theft in thefts)
            {
                theftOccurred = true;
                string itemName = theft.ActionType.Replace("THEFT:", "");
                string roomName = world.Locations.FirstOrDefault(l => l.Id == theft.LocationId)?.Name ?? "Unknown Area";
                string timeDisplay = TimeSpan.FromMinutes(theft.StartTime).ToString(@"hh\:mm");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[THEFT EVENT] At {timeDisplay}, {npc.Name} stole the [{itemName}] from the {roomName}!");
                Console.ResetColor();
            }
        }

        if (!theftOccurred)
        {
            Console.WriteLine("No operational items were reported stolen during this shift.");
        }
        // ----------------------------------

        Console.WriteLine("\n--- World Initialization State (Spawning Coordinates) ---");
        foreach (var npc in world.Characters) 
        {
            var pos = npc.GetComponent<PositionTrait>();
            var trade = npc.GetComponent<TradeTrait>()?.TradeName ?? "None";
            var roomName = world.Locations.FirstOrDefault(l => l.Id == pos?.CurrentRoomId)?.Name ?? "Unknown";
            string status = (npc.Id == world.VictimId) ? "[DEAD]" : "[ALIVE]";
            
            Console.WriteLine($"[Spawn Node] {npc.Name} ({trade}) {status} -> Current Location: {roomName}");
        }
    }
}
