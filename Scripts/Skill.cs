using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Main
{
    public abstract class Skill
    {
        public string Name { get; set; }
        public string Effect { get; set; }
        public string Description { get; set; }
    }
}