namespace Core
{
    // --- 1. Character Data Records ---
    public record RaceData(int RaceStr, int RaceInt);
    public record ClassData(int ClassHealth, int ClassMana, int ClassStr, int ClassInt, int PrimarySkillIndex);
    public record SkillData(string Name, string AttributeScale);

    // --- 2. Attribute/Modifier Records ---
    public record ModifierDto(string Target, float Value);

    // --- 3. Component Data Structures ---
    public record GrantedComponentDto(
        string Tag, 
        List<ModifierDto>? Modifiers,
				Dictionary<string, string>? Properties
    );

    // --- 4. Main Item Record ---
    public record ItemData(
        string Name, 
        string? Slot, 
        List<GrantedComponentDto> GrantedComponents
    );

		// --- 5. Constants
		public static class EngineConfig
		{
			public const int MaxItemCapacity = 700;
		}
}
