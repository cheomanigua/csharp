using System.Runtime.InteropServices;

namespace Core;

public struct CharacterStats
{
    public int EntityId;
    public bool IsDirty;
    public int[] Values; 

    public CharacterStats(int statCount)
    {
        EntityId = 0;
        IsDirty = true;
        Values = new int[statCount];
    }
}

[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct WeaponComponent
{
    [FieldOffset(0)] public int EntityId;
    [FieldOffset(4)] public int WeaponId;
    [FieldOffset(8)] public int Damage;
}

public struct MetadataComponent
{
    public string Name;
    public string WeaponName;
    public string SkillName;
}

public struct AttributeModifier
{
    public string Target; 
    public float Value;
}

public struct EquipmentComponent
{
    public int EntityId;
    public int[] EquippedItemIds;
}
