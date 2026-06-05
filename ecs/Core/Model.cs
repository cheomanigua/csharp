namespace Core;

// Data used by the View to display a character
public class NPCModel
{
    public string Name { get; set; } = string.Empty; // Alternative: Initialize with empty string
    public CharacterStats Stats { get; set; }
    public string WeaponName { get; set; } = string.Empty;
}
