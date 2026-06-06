namespace Core;

/// <summary>
/// Defines the contract for any view capable of displaying character information.
/// This allows for UI-agnostic rendering (Console, GUI, Network, etc.)
/// </summary>
public interface ICharacterView
{
    void Render(in CharacterSheetDto data);
}
