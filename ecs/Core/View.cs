namespace Core;

public static class View
{
    // Pass the strings as parameters, resolved from a registry/lookup outside the combat loop
    public static void DisplayCharacter(in CharacterStats stats, string name, string weaponName)
    {
        Console.WriteLine($"--- Character Sheet: {name} ---");
        Console.WriteLine($"Weapon: {weaponName}");
        Console.WriteLine($"Health: {stats.Health} | Mana: {stats.Mana}");
        Console.WriteLine($"Strength: {stats.Strength} | Intelligence: {stats.Intelligence}");
    }
}
