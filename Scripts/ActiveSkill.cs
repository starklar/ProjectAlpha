using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Main
{
    public class ActiveSkill : Skill
    {
        public int MinRange { get; set; }
        public int MaxRange { get; set; }
        public int Power { get; set; }
        public int Accuracy { get; set; }
        public bool IsMagic { get; set; }
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
        public int Type { get; set; }
        public int Cost { get; set; }
    }
}