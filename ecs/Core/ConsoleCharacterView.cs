namespace Core;

public class ConsoleCharacterView : ICharacterView
{
    public void Render(in CharacterSheetDto data)
    {
        Console.WriteLine($"--- Character Sheet: {data.Name} ---");
        Console.WriteLine($"Weapon: {data.Weapon}");
        Console.WriteLine($"Skill: {data.Skill}");
        Console.WriteLine($"Health: {data.Health} | Mana: {data.Mana}");
        Console.WriteLine($"Strength: {data.Strength} | Intelligence: {data.Intelligence}");
    }
}
