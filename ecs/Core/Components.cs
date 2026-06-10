using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Core;


[StructLayout(LayoutKind.Sequential, Pack = 4)]
public unsafe struct EntityHotData
{
    public int EntityId;
    public bool IsDirty;

    public fixed int Stats[(int)StatType.Count];

    public int EquippedItemCount;
    public fixed int EquippedItemIds[8];

    public EntityHotData(int entityId)
    {
        EntityId = entityId;
        IsDirty = true;
        EquippedItemCount = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddEquippedItem(int itemId)
    {
        if (EquippedItemCount < 8)
        {
            EquippedItemIds[EquippedItemCount++] = itemId;
            IsDirty = true;
        }
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
