using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skirmish
{
    public class BattleSkill : Skill
    {
        //0: Self
        //1: Allies
        //2: Enemy
        public int Target;
        public int MinRange { get; }
        public int MaxRange { get; }
        public int Power { get; }
        public int Accuracy { get; }
        public int CriticalRate { get; }
        public (int, int) Hits { get; }
        public bool IsMagic { get; }
        /*
        0: Physical
        1: Pierce
        2: Fire
        3: Water
        4: Earth
        5: Wind
        6: Aether
        7: Healing
        8: Support
        */
        public int Type { get; }
        public int Cost { get; }

        public BattleSkill(string name, string description, int type, int cost, int target, int min_range, int max_range, int power, int acc, int crit, (int, int) hits, bool is_magic, string effect)
        {
            Name = name;
            Description = description;
            Cost = cost;
            Target = target;
            Type = type;
            MinRange = min_range;
            MaxRange = max_range;
            Power = power;
            Accuracy = acc;
            CriticalRate = crit;
            Hits = hits;
            IsMagic = is_magic;
            Effect = effect;
        }
    }
}