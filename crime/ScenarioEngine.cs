using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CrimeGame.Core.Models;
using CrimeGame.Core.Commands; // Injects your clean command boundaries

namespace CrimeGame.Core.Controllers
{
    // Specialized Sub-Controller for handling historical timeline ticking
    public class SimulationController 
    {
        private readonly Random _rng;

        public SimulationController(Random rng) 
        {
            _rng = rng;
        }

        public void RunPreSimulation(WorldState model, GameDefinitions defs, int totalMinutes, int tickSize) 
        {
            var commandQueue = new List<ICommand>();

            for (int time = 0; time < totalMinutes; time += tickSize) 
            {
                DetermineTicksForTime(model, defs, time, commandQueue);
            }

            // Execute all requested operations batch-wise against the model state.
            foreach (var command in commandQueue)
            {
                command.Execute(model);
            }
        }

        private void DetermineTicksForTime(WorldState model, GameDefinitions defs, int currentTime, List<ICommand> queue)
        {
            var killer = model.Characters.First(c => c.Id == model.KillerId);
            var victim = model.Characters.First(c => c.Id == model.VictimId);
            var scene = model.Locations.First();

            if (currentTime == model.CrimeTime)
            {
                // Encapsulate specific "Murder Encounter" operations into a standalone Command capsule
                queue.Add(new ExecuteMurderCommand(killer, victim, scene, currentTime));

                foreach (var npc in model.Characters.Where(c => c.Id != model.KillerId && c.Id != model.VictimId))
                {
                    var targetRoom = GetWeightedRoom(npc, model.Locations, defs);
                    queue.Add(new PerformRoutineCommand(npc, targetRoom, "SLEEP", currentTime));
                }
            }
            else if (currentTime > model.CrimeTime)
            {
                queue.Add(new PerformRoutineCommand(victim, scene, "DEAD", currentTime));

                foreach (var npc in model.Characters.Where(c => c.Id != model.VictimId))
                {
                    string action = (currentTime > 1320) ? "Sleep" : "Work";
                    var targetRoom = GetWeightedRoom(npc, model.Locations, defs);
                    queue.Add(new PerformRoutineCommand(npc, targetRoom, action, currentTime));
                }
            }
            else
            {
                // Standard pre-crime timeline orchestration
                foreach (var npc in model.Characters)
                {
                    string action = (currentTime < 480) ? "Sleep" : "Work";
                    var targetRoom = GetWeightedRoom(npc, model.Locations, defs);
                    
                    // --- FIXED THEFT MECHANIC ---
                    // Bumped to a 25% rolling chance per work tick for testing visibility
                    if (action == "Work" && npc.Id != model.VictimId && model.Items.Any() && _rng.Next(100) < 5)
                    {
                        var targetItem = model.Items[_rng.Next(model.Items.Count)];
                        
                        // Check if it hasn't already been marked as stolen property
                        var evidence = targetItem.GetComponent<EvidenceTrait>();
                        if (evidence == null || !evidence.Description.Contains("Stolen property"))
                        {
                            // Queue up individual theft capsule instead of executing logic inline
                            queue.Add(new ExecuteTheftCommand(npc, targetItem, targetRoom, currentTime));
                            action = "STEAL"; 
                        }
                    }
                    // ----------------------------

                    queue.Add(new PerformRoutineCommand(npc, targetRoom, action, currentTime));
                }
            }
        }

        private Room GetWeightedRoom(NPC npc, List<Room> locations, GameDefinitions defs) 
        {
            var tradeName = npc.GetComponent<TradeTrait>()?.TradeName;
            var tradeDef = defs.Trades.FirstOrDefault(t => t.Name == tradeName);

            if (tradeDef == null || tradeDef.RoomWeights.Count == 0)
                return locations[_rng.Next(locations.Count)];

            int totalWeight = 0;
            var options = new List<(Room Room, int Weight)>();

            foreach (var loc in locations) 
            {
                int weight = tradeDef.RoomWeights.TryGetValue(loc.Name, out int w) ? w : 5;
                totalWeight += weight;
                options.Add((loc, weight));
            }

            int roll = _rng.Next(totalWeight);
            int cursor = 0;

            foreach (var option in options) 
            {
                cursor += option.Weight;
                if (roll < cursor) return option.Room;
            }
            return locations[0];
        }
    }

    // Orchestrator Master Controller (Engine-Agnostic via Factory & Command Interfaces)
    public class ScenarioEngine 
    {
        private readonly Random _rng = new Random();
        private readonly SimulationController _simController;
        private readonly IEntityFactory _factory; // Creational abstraction boundary
        private GameDefinitions _defs = new();
        private List<string> _namePool = new();

        public ScenarioEngine(IEntityFactory factory) 
        {
            _factory = factory;
            _simController = new SimulationController(_rng);
        }

        public WorldState Generate(string theme) 
        {
            LoadAllData();
            var state = new WorldState();

            // 1. Build Room Maps indirectly using the injected Factory contract
            foreach (var rt in _defs.RoomTypes) 
            {
                state.Locations.Add(_factory.CreateRoom(rt.Type));
            }

            // 2. Build Entities indirectly using the injected Factory contract
            int npcCount = Math.Min(4, _namePool.Count);
            for (int i = 0; i < npcCount; i++) 
            {
                state.Characters.Add(CreateNpcFromPool());
            }

            state.KillerId = state.Characters[0].Id;
            state.VictimId = state.Characters[1].Id;

            // 3. Inject Social Ledger Framework
            if (_defs.GrievanceTemplates.Any()) 
            {
                var template = _defs.GrievanceTemplates[_rng.Next(_defs.GrievanceTemplates.Count)];
                var social = new SocialLedger();
                var killer = state.Characters[0];
                var victim = state.Characters[1];

                string motive = $"{template.Description} {victim.Name}";
                social.Grievances[victim.Id] = new List<string> { motive };
                social.Bonds[victim.Id] = template.BaseBondModifier;
                
                killer.AddComponent(social);
            }

            // --- FIXED ORDER OF OPERATIONS ---
            // 4. Generate Clue Items BEFORE running the timeline so they exist to be stolen!
            if (_defs.ItemTypes.Any())
            {
                var itemDef = _defs.ItemTypes[_rng.Next(_defs.ItemTypes.Count)];
                var clue = _factory.CreateItem(itemDef.Name);
                clue.AddComponent(new EvidenceTrait { 
                    FactId = "Weapon", 
                    OwnerId = state.KillerId,
                    Description = $"Found hidden in the {state.Locations.First().Name}." 
                });
                state.Items.Add(clue);
            }

            // 5. Run Timeline Simulation (Now properly detects items created above)
            _simController.RunPreSimulation(state, _defs, 1440, 15);

            return state;
        }

        private void LoadAllData() 
        {
            if (File.Exists("Names.txt")) 
            {
                _namePool = File.ReadAllLines("Names.txt")
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                                .OrderBy(x => _rng.Next())
                                .ToList();
            }

            if (File.Exists("definitions.json")) 
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                string json = File.ReadAllText("definitions.json");
                _defs = JsonSerializer.Deserialize<GameDefinitions>(json, options) ?? new GameDefinitions();
            }
        }

        private NPC CreateNpcFromPool() 
        {
            string name = _namePool.Any() ? _namePool[0] : "Unknown";
            if (_namePool.Any()) _namePool.RemoveAt(0);

            var npc = _factory.CreateNPC(name);
            
            if (_defs.Trades.Any()) 
            {
                var trade = _defs.Trades[_rng.Next(_defs.Trades.Count)];
                npc.AddComponent(new TradeTrait { TradeName = trade.Name });
            }
            return npc;
        }
    }
}
