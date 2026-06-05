using System.Runtime.InteropServices;

namespace Core;

[StructLayout(LayoutKind.Explicit, Size = 24)]
public struct CharacterStats
{
    [FieldOffset(0)] public int EntityId;
    [FieldOffset(4)] public int Strength;
    [FieldOffset(8)] public int Intelligence;
    [FieldOffset(12)] public int Health;
    [FieldOffset(16)] public int Mana;
    [FieldOffset(20)] public bool IsDirty;
}

[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct WeaponComponent
{
    [FieldOffset(0)] public int EntityId;
    [FieldOffset(4)] public int WeaponId;
    [FieldOffset(8)] public int Damage;
}
