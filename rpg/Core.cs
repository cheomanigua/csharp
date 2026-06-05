using System;

namespace DataDrivenRPG.Core
{
    // High-performance, ultra-lightweight structural handle to reference indices
    public readonly struct Entity
    {
        public readonly int Id;
        public Entity(int id) => Id = id;
    }

    // Dense sequential struct arrays mapped by Entity ID inside memory pools
    public enum Race { Human, Orc, Elf, Dwarf }
    public enum CharacterClass { Warrior, Wizard, Rogue }

    public struct IdentityComponent
    {
        public Race Race;
        public CharacterClass Class;
    }

    public struct StatsComponent
    {
        public int Health;
        public int Mana;
    }

    public struct SkillsComponent
    {
        public int Skills; // Changed from enum to a raw int bit field to support dynamic loading
    }
}
