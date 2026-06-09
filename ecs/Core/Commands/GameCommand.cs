namespace Core.Commands;

public enum CommandType { Move, Attack, UpdateStats, EquipItem }


public struct GameCommand
{
    public CommandType Type;
    public int EntityId;
		public int TargetId;
    public float Value;
    public string? Source; // For stats initialization or attribute references
}
