using Godot;
using System.Collections.Generic;
using UnitInformation;

namespace Skirmish
{
    public class UnitScene : KinematicBody2D
    {
        public string UnitName { get; set; }
        
        //0 - Player
        //1 - Enemy
        //2 - Other
        public int Team { get; set; }

        public int MovementType { get; set; }

        public int CurrHP { get; set; }
        public int CurrMP { get; set; }

        public int Level { get; set; }
        public int EXP { get; set; }
        
        public BattleSkill StandardAttack { get; set; }
        public List<BattleSkill> BattleSkills { get; set; }
        public List<PassiveSkill> PassiveSkills { get; set; }

        //Weakness / Wk
        //Neutral / --
        //Resist / Rs
        //Null / Nu

        /*
        0 - Physical
        1 - Pierce
        2 - Fire
        3 - Water
        4 - Earth
        5 - Wind
        6 - Aether
        */
        public List<string> ElementRes { get; set; }
        /*
        0 - Physical
        1 - Pierce
        2 - Fire
        3 - Water
        4 - Earth
        5 - Wind
        6 - Aether
        7 - Heal
        8 - Support
        */
        public List<int> ElementAff { get; set; }

        /*
        0 - Max HP: Maximum HP
        1 - Max MP: Maximum MP
        2 - Movement: Number of spaces unit can move on the map at once
        3 - Strength: Increase physical attack damage and critical hit percent
        4 - Focus: Increase magic attack damage and hit percent
        5 - Speed: Must be >= 4 to follow up attack and increase evasion percent
        6 - Vitality: Reduce physical damage taken and increase critical dodge percent
        7 - Spirit: Reduce magic damage taken and increase MP regen rate
        */
        public List<int> Stats { get; set; }
        /*
        0 - Movement: Number of spaces unit can move on the map at once
        1 - Strength: Increase physical attack damage and critical hit percent
        2 - Focus: Increase magic attack damage and hit percent
        3 - Speed: Must be >= 4 to follow up attack and increase evasion percent
        4 - Vitality: Reduce physical damage taken and increase critical dodge percent
        5 - Spirit: Reduce magic damage taken and increase MP regen rate
        */
        public List<int> StatMods { get; set; }

        public int CurrX { get; set; }
        public int CurrY { get; set; }
        public int PrevX { get; set; }
        public int PrevY { get; set; }

        public bool HasMoved { get; set; }

        protected AnimationPlayer AniPlayer;

        public override void _Ready()
        {
            
        }

        public void Create(UnitInfo unit, int start_x, int start_y, int team)
        {
            CurrX = start_x;
            CurrY = start_y;
            PrevX = start_x;
            PrevY = start_y;
            Position = new Vector2(CurrX * Global.MAP_SCALE + Global.MAP_SCALE / 2, CurrY * Global.MAP_SCALE + Global.MAP_SCALE / 2);

            Show();

            UnitName = unit.UnitName;
            Team = team;
            MovementType = unit.MovementType;

            Level = unit.CurrentLevel;
            EXP = unit.CurrentEXP;

            Stats = new List<int>(unit.Stats);
            int[] statModsList = { 0, 0, 0, 0, 0, 0 };
            StatMods = new List<int>(statModsList);
            
            //Temporary, non modified attack
            StandardAttack = new BattleSkill("Attack", "1 hit Physical attack.", 0, 0, 2, 1, 1, 3, 80, 5, (1, 1), false, null);

            BattleSkills = new List<BattleSkill>();

            PassiveSkills = new List<PassiveSkill>();

            ElementAff = new List<int>(unit.ElementAff);

            ElementRes = new List<string>();

            foreach(int idx in unit.BattleSkills)
            {
                BattleSkills.Add(Global.BATTLE_SKILLS[idx]);
            }

            foreach(int idx in unit.PassiveSkills)
            {
                PassiveSkills.Add(Global.PASSIVE_SKILLS[idx]);
            }

            foreach(int idx in unit.ElementRes)
            {
                ElementRes.Add(Global.RESISTANCE_LEVELS[idx]);
            }

            CurrHP = Stats[0];
            CurrMP = Stats[1];

            AniPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            AniPlayer.Play("Idle");

            HasMoved = false;
        }

        public void Move(int new_x, int new_y)
        {
            PrevX = CurrX;
            PrevY = CurrY;
            CurrX = new_x;
            CurrY = new_y;
            Position = new Vector2(CurrX * Global.MAP_SCALE + Global.MAP_SCALE / 2, CurrY * Global.MAP_SCALE + Global.MAP_SCALE / 2);
        }

        public void Refresh()
        {
            HasMoved = false;
            AniPlayer.Play("Idle");
        }

        public void StatModsRefresh()
        {
            for(int i = 0; i < 6; i++)
            {
                if(StatMods[i] < 0)
                {
                    StatMods[i] += 1;
                }
                else if(StatMods[i] > 0)
                {
                    StatMods[i] -= 1;
                }
            }
        }

        public void Place()
        {
            PrevX = CurrX;
            PrevY = CurrY;

            HasMoved = true;
            AniPlayer.Play("Wait");
        }

        public void UndoMove()
        {
            CurrX = PrevX;
            CurrY = PrevY;
            Position = new Vector2(PrevX * Global.MAP_SCALE + Global.MAP_SCALE / 2, PrevY * Global.MAP_SCALE + Global.MAP_SCALE / 2);
        }

        public void Dead()
        {
            Visible = false;
            AniPlayer.Stop();
        }

    }
}