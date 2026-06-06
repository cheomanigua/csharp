namespace Core;

/// <summary>
/// A simple, immutable Data Transfer Object used to pass character data 
/// from the backend to any UI implementation (Console, Godot, etc.)
/// </summary>
public readonly struct CharacterSheetDto
{
    public readonly string Name;
    public readonly string Weapon;
    public readonly string Skill;
    public readonly int Health;
    public readonly int Mana;
    public readonly int Strength;
    public readonly int Intelligence;

    public CharacterSheetDto(string name, string weapon, string skill, int health, int mana, int strength, int intelligence)
    {
        Name = name;
        Weapon = weapon;
        Skill = skill;
        Health = health;
        Mana = mana;
        Strength = strength;
        Intelligence = intelligence;
    }
}
