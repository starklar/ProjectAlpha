using Godot;
using System;

namespace Skirmish
{
    public class Tile
    {
        public string TileType { get; }
        public int DefenceBonus { get; }
        public int EvasionBonus { get; }
        public int AllowedTypes { get; }
        public int MovementPenalty { get; }
        public UnitScene CurrUnit { get; set; }

        public Tile(string tile_type, int defence_bonus, int evasion_bonus, int allowed_types, int movement_penalty)
        {
            TileType = tile_type;
            DefenceBonus = defence_bonus;
            EvasionBonus = evasion_bonus;
            AllowedTypes = allowed_types;
            MovementPenalty = movement_penalty;
            CurrUnit = null;
        }

        public Tile Clone()
        {
            return new Tile(TileType, DefenceBonus, EvasionBonus, AllowedTypes, MovementPenalty);
        }
    }
}