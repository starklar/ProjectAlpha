using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Main
{
    public class BattleSkill: ActiveSkill
    {
        public int CriticalRate { get; }
        public (int, int) Hits { get; }

        public BattleSkill(string name, string description, int type, int cost, int min_range, int max_range, int power, int acc, int crit, (int, int) hits, bool is_magic, string effect)
        {
            Name = name;
            Description = description;
            Cost = cost;
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