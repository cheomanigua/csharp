using Core;

class Program
{
    static void Main(string[] args)
    {
        // 0. Initialize the Processor FIRST
        FormulaProcessor.Initialize("Data/System/formulas.json");

        var registry = new EntityRegistry();
        var controller = new Controller(registry);

        // 1. Load data
        controller.LoadNPCFromJson("Data/npc_blueprint.json");

        // 2. Iterate only over active entities using the new high-performance list
        // This replaces the old 'allStats' loop which scanned empty slots
        var activeIds = registry.GetActiveEntities();
        
        foreach (int entityId in activeIds)
        {
            // O(1) access: Fetch stats directly using the ID
            ref readonly var stats = ref registry.GetStatsForEntity(entityId);
            
            // Print the found EntityId (restored functionality)
            Console.WriteLine($"Found EntityId: {stats.EntityId}");

            // 3. Resolve metadata using the dynamic ID
            string name = controller.GetName(entityId);
            string weaponName = controller.GetWeaponName(entityId);

            // 4. Render (restored functionality)
            View.DisplayCharacter(in stats, name, weaponName);
        }
    }
}
