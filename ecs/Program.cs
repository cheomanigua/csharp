using Core;

class Program
{
    static void Main(string[] args)
    {
        // 0. Initialize the Processor
        FormulaProcessor.Initialize("Data/System/formulas.json");

        // 1. Setup Registries
        var registry = new EntityRegistry();
        var metaRegistry = new MetadataRegistry();
        
        // 2. Initialize Controller with required dependencies
        var controller = new Controller(registry, metaRegistry);

        // 3. Load data
        controller.LoadNPCFromJson("Data/npc_blueprint.json");
        
        // Use the Adapter
        var adapter = new GameViewAdapter(registry, metaRegistry);
        var view = new ConsoleGameView();
        var activeIds = registry.GetActiveEntities();
        
        foreach (int entityId in activeIds)
        {
            adapter.UpdateView(entityId, view);
        }

    }
}
