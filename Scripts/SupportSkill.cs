using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Main
{
    public class SupportSkill: ActiveSkill
    {
        //0: Self
        //1: Single Ally
        //2: Single Enemy
        //3: All Allies in Range
        //4: All Enemies in Range
        //5: All Allies including User in Range
        public int Target;

        /*
        0-1 : HP or MP Healing
        2 : HP and MP Healing
        3-8 : Stat Change
        9 : Change All Stats
        */
        public int Modifier;

        public SupportSkill(string name, string description, int type, int cost, int target, int min_range, int max_range, int power, int acc, bool is_magic, string effect, int modifier)
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
            IsMagic = is_magic;
            Effect = effect;
            Modifier = modifier;
        }
    }
}