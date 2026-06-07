namespace Core.Commands;

public enum CommandType { Move, Attack, InitStats }

public struct GameCommand
{
    public CommandType Type;
    public int EntityId;
    public float Value;
    public string? Source; // For stats initialization or attribute references
}
