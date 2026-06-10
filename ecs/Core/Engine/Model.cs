using System.Collections.Generic;

namespace Core
{
    // --- 1. Character Data Records ---
    public record RaceData(int RaceStr, int RaceInt);
    public record ClassData(int ClassHealth, int ClassMana, int ClassStr, int ClassInt, string PrimarySkill);
    public record SkillData(string AttributeScale);

    // --- 2. Context for FormulaProcessor ---
    public record FormulaContext(
        EntityHotData Stats, 
        ClassData? Class = null, 
        RaceData? Race = null, 
        int WeaponDamage = 0,
        Dictionary<string, float>? ExtraParams = null
    );

    // --- 3. Attribute/Modifier Records ---
    public record ModifierDto(string Target, float Value);

    // --- 4. Component Data Structures ---
    public record GrantedComponentDto(
        string Tag, 
        List<ModifierDto>? Modifiers,
        Dictionary<string, string>? Properties
    );

    // --- 5. Main Item Record ---
    public record ItemData(
        string Name, 
        string? Slot, 
        List<GrantedComponentDto> GrantedComponents
    );

    // --- 6. Constants ---
    public static class EngineConfig
    {
        public const int MaxItemCapacity = 1024;
        public const int MaxEntities = 1024;
    }
}
