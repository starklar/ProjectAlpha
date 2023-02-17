using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Skirmish
{
    public static class Global
    {
        public const int WINDOW_WIDTH = 1024;
        public const int WINDOW_HEIGHT = 640;
        public const int MAX_MOVEMENT_TYPE = 100;
        public const int MAX_MOVEMENT_PENALTY = 100;
        public const float MAP_SCALE = 64f;
        public const int FOLLOW_UP_THREASHOLD = 3;
        public const int MAX_SPEED_BONUS = 6;
        
        //Weakness / Wk
        //Neutral / --
        //Resist / Rs
        //Guarded / Gd
        public static readonly string[] RESISTANCE_LEVELS = new string[]{
            "Wk",
            "--",
            "Rs",
            "Gd"
        };

        public static readonly string[] ELEMENTS = new String[]
        {
            "Physical",
            "Pierce",
            "Fire",
            "Water",
            "Wind",
            "Earth",
            "Aether"
        };

        public static readonly Tile[] TILE_TYPES = new Tile[]
        {
            new Tile("Grass", 0, 0, 0, 0),
            new Tile("Wall", 0, 0, MAX_MOVEMENT_TYPE, MAX_MOVEMENT_PENALTY),
            new Tile("Sigil", 2, 20, MAX_MOVEMENT_TYPE, 1)
        };

        public static readonly BattleSkill[] BATTLE_SKILLS = new BattleSkill[]
        {
            new BattleSkill("Ignis", "Magical Fire attack", 2, 8, 2, 1, 1, 2, 70, 0, (1, 1), true, null),
            new BattleSkill("Glacies", "Magical Ice attack", 3, 8, 2, 1, 1, 2, 70, 0, (1, 1), true, null),
            new BattleSkill("Terra", "Magical Earth attack", 4, 8, 2, 1, 1, 2, 70, 0, (1, 1), true, null),
            new BattleSkill("Ventus", "Magical Wind attack", 5, 8, 2, 1, 1, 2, 70, 0, (1, 1), true, null),
            new BattleSkill("Flare", "Long range Fire attack", 2, 15, 2, 4, 8, 10, 40, 0, (1, 1), true, null),
            new BattleSkill("Comet", "Long range Ice attack", 3, 15, 2, 4, 8, 10, 40, 0, (1, 1), true, null),
            new BattleSkill("Squall", "Long range Wind attack", 4, 15, 2, 4, 8, 10, 40, 0, (1, 1), true, null),
            new BattleSkill("Meteor", "Long range Earth attack", 5, 15, 2, 4, 8, 10, 40, 0, (1, 1), true, null),
            new BattleSkill("Star Drop", "Long range Aether attack", 6, 30, 2, 4, 8, 12, 50, 0, (1, 1), true, null),
        };

        public static readonly PassiveSkill[] PASSIVE_SKILLS = new PassiveSkill[]
        {
            
        };

        public static int CalculateDamage(UnitScene user, UnitScene target, BattleSkill skill, int targetTileDefenceBonus, bool targetIsGuarding)
        {
            int total_damage = 0;
            int total_power = 0;
            int total_defence = targetTileDefenceBonus;
            total_power = skill.Power;

            if(target.ElementRes[skill.Type] == RESISTANCE_LEVELS[2])
            {
                total_power /= 2;
            }
            else if(target.ElementRes[skill.Type] == RESISTANCE_LEVELS[0])
            {
                total_power = total_power * 2 ;
            }
            /*else if(target.ElementRes[skill.Type] == GlobalVars.RESISTANCE_LEVELS[3])
            {
                total_power = 0;
            }*/

            if (skill.IsMagic)
            {
                total_power += user.Stats[4] + user.StatMods[2];
                total_defence += target.Stats[7] + user.StatMods[5];
            }
            else
            {
                total_power += user.Stats[3] + user.StatMods[1];
                total_defence += target.Stats[6] + user.StatMods[4];
            }

            total_power += user.ElementAff[skill.Type];

            total_damage = total_power - total_defence;

            if(target.ElementRes[skill.Type] == RESISTANCE_LEVELS[3])
            {
                total_power = total_damage / 2;
            }

            if(targetIsGuarding)
            {
                total_damage = total_damage * 3 / 4;
            }

            if (total_damage < 0)
            {
                total_damage = 1;
            }
                
            return total_damage;
        }

        public static int CalculateAccuracy(UnitScene user, UnitScene target, BattleSkill skill, int targetTileEvasionBonus)
        {
            int hitrate = skill.Accuracy + (user.Stats[4] + user.StatMods[2]) * 3 + user.ElementAff[skill.Type] * 2;
            int dodgerate = (target.Stats[5] + target.StatMods[3]) * 3 + targetTileEvasionBonus;

            int total_accuracy = hitrate - dodgerate;

            if (total_accuracy < 0)
            {
                return 0;
            }
            if (total_accuracy > 100)
            {
                return 100;
            }
            return total_accuracy;
        }

        public static int CalculateCritical(UnitScene user, UnitScene target, BattleSkill skill)
        {
            if(target.ElementRes[skill.Type] != RESISTANCE_LEVELS[2])
            {
                int critrate = skill.CriticalRate + (user.Stats[3] + user.StatMods[1]) * 3;
                int luckrate = (target.Stats[6] + target.StatMods[4]) * 3;
                int total_crit = critrate - luckrate;

                if (total_crit < 0)
                {
                    return 0;
                }
                return total_crit;
            }

            return 0;
        }
    }
}