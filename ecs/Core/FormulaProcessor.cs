namespace Core;

public static class FormulaProcessor
{
    // Pass by 'in' to guarantee zero-copy performance
    public static float Calculate(in CharacterStats stats, int weaponDamage)
    {
        // Direct, branchless-friendly math
        return (stats.Strength * 0.5f) + weaponDamage;
    }
}
