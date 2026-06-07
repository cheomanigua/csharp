using System.Collections.Generic;

namespace Core;

// --- Presentation Models (View Layer) ---

// Data used by the View to display a character
public class NPCModel
{
    public string Name { get; set; } = string.Empty;
    public CharacterStats Stats { get; set; }
    public string WeaponName { get; set; } = string.Empty;
}

// --- Blueprint/Data Models (Initialization Layer) ---

// Represents the schema for accessories.json
public record AccessoryData(string Name, string Slot, List<GrantedComponentDto> GrantedComponents);

// Represents specific effects granted by items
public record GrantedComponentDto(string Tag, List<AttributeModifierDto>? Modifiers, Dictionary<string, float>? Properties);

// Used for stat modification logic
public record AttributeModifierDto(string Target, float Value);

