using Core.Commands;

namespace Core.Systems.Inventory;

public class EquipmentSystem 
{
    private readonly StatRegistry _registry;
    public EquipmentSystem(StatRegistry registry) => _registry = registry;

    public void Execute(GameCommand cmd, EntityRegistry registry) 
    {
        if (cmd.Type == CommandType.EquipItem)
        {
            registry.EquipItem(cmd.EntityId, cmd.TargetId);
        }
    }
}
