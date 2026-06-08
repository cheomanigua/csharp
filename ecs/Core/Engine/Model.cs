namespace Core.Engine;

// 1. Used for AttributeModifierComponent
public record ModifierDto(string Target, float Value);

// 2. Used for AttributeStealComponent
public record StealPropertiesDto(float SiphonPercentage, string SourceTarget, string DestinationTarget);

// 3. The main DTO that mirrors your JSON structure
public record GrantedComponentDto(
    string Tag, 
    List<ModifierDto>? Modifiers,     // Matches "Modifiers": [...] in JSON
    StealPropertiesDto? Properties    // Matches "Properties": {...} in JSON
);

// 4. The main Item record
public record AccessoryData(
    string Name, 
    string? Slot, 
    List<GrantedComponentDto> GrantedComponents
);
