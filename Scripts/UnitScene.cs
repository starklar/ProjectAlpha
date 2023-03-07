using System;
using Godot;
using System.Collections.Generic;
using UnitInformation;
using Main;

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
        public List<ActiveSkill> ActiveSkills { get; set; }
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
            StandardAttack = new BattleSkill("Attack", "1 hit Physical attack.", 0, 0, 1, 1, 3, 80, 0, (1, 1), false, null);

            ActiveSkills = new List<ActiveSkill>();

            PassiveSkills = new List<PassiveSkill>();

            ElementAff = new List<int>(unit.ElementAff);

            ElementRes = new List<string>();

            foreach(int idx in unit.BattleSkills)
            {
                ActiveSkills.Add(Global.BATTLE_SKILLS[idx]);
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


        //Returns True if effect can be applied and will apply effect properly
        //Else returns False
        public bool ApplyEffect(ref UnitScene casting_unit, SupportSkill skill)
        {
            bool applied = false;
            string effect = skill.Effect;

            if(effect == Global.SUPPORT_SKILL_EFFECTS[0])
            {
                int hp_heal_ammount = 0;
                int mp_heal_ammount = 0;

                if(skill.Modifier == 0 || skill.Modifier == 2)
                {
                    if(skill.IsMagic)
                    {
                        hp_heal_ammount = casting_unit.Stats[4] + casting_unit.StatMods[2];
                    }
                    else
                    {
                        hp_heal_ammount = casting_unit.Stats[4] + casting_unit.StatMods[2];
                    }

                    if(CurrHP - Stats[0] >= hp_heal_ammount)
                    {
                        CurrHP += hp_heal_ammount;
                        applied = true;
                    }
                    else if(CurrHP < Stats[0])
                    {
                        CurrHP = Stats[0];
                        applied = true;
                    }
                }
                if(skill.Modifier == 1 || skill.Modifier == 2)
                {
                    if(skill.IsMagic)
                    {
                        mp_heal_ammount = casting_unit.Stats[4] + casting_unit.StatMods[2];
                    }
                    else
                    {
                        mp_heal_ammount = casting_unit.Stats[4] + casting_unit.StatMods[2];
                    }

                    if(CurrMP - Stats[1] >= mp_heal_ammount)
                    {
                        CurrMP += mp_heal_ammount;
                        applied = true;
                    }
                    else if(CurrMP < Stats[1])
                    {
                        CurrMP = Stats[1];
                        applied = true;
                    }
                }
            }
            else if(effect == Global.SUPPORT_SKILL_EFFECTS[1])
            {
                if(skill.Modifier == 9)
                {
                    for(int i = 0; i < StatMods.Count; i++)
                    {
                        if(StatMods[i] < skill.Power)
                        {
                            StatMods[i] += skill.Power;
                            applied = true;
                        }
                    }
                }
                else
                {
                    int modIndex = skill.Modifier - 3;

                    if(StatMods[modIndex] < skill.Power)
                    {
                        StatMods[modIndex] += skill.Power;
                        applied = true;
                    }
                }
            }
            else if(effect == Global.SUPPORT_SKILL_EFFECTS[2])
            {
                if(skill.Modifier == 9)
                {
                    for(int i = 0; i < StatMods.Count; i++)
                    {
                        if(StatMods[i] > -skill.Power)
                        {
                            StatMods[i] -= skill.Power;
                            applied = true;
                        }
                    }
                }
                else
                {
                    int modIndex = skill.Modifier - 3;

                    if(StatMods[modIndex] > -skill.Power)
                    {
                        StatMods[modIndex] -= skill.Power;
                        applied = true;
                    }
                }
            }

            return applied;
        }
    }
}