using Godot;
using System;
using System.Collections.Generic;
using Main;

namespace Skirmish
{
    public class CombatForecastHUDScene : CanvasLayer
    {
        private Label SpeedValue;

        private Label LeftUnitName;
        private Label LeftLevel;
        private Label LeftHP;
        private Label LeftMP;
        private List<Label> LeftElementalResistances = new List<Label>(6);
        private List<Label> LeftSkillName = new List<Label>(5);
        private List<Label> LeftDamage = new List<Label>(5);
        private List<Label> LeftAccuracy = new List<Label>(5);
        private List<Label> LeftCritical = new List<Label>(5);

        private Label RightUnitName;
        private Label RightLevel;
        private Label RightHP;
        private Label RightMP;
        private List<Label> RightElementalResistances = new List<Label>(6);
        private List<Label> RightSkillName = new List<Label>(5);
        private List<Label> RightDamage = new List<Label>(5);
        private List<Label> RightAccuracy = new List<Label>(5);
        private List<Label> RightCritical = new List<Label>(5);

        public override void _Ready()
        {
            SpeedValue = (Label) GetNode("SpeedHUD/VBoxContainer/SpeedBonusValue/Value");

            LeftUnitName = (Label) GetNode("HUD/HBoxContainer/LeftBox/MarginContainer/UnitHUD/UnitName/Name");
            LeftLevel = (Label) GetNode("HUD/HBoxContainer/LeftBox/MarginContainer/UnitHUD/UnitImage/Level");
            LeftHP = (Label) GetNode("HUD/HBoxContainer/LeftBox/MarginContainer/UnitHUD/HP/HPLabel");
            LeftMP = (Label) GetNode("HUD/HBoxContainer/LeftBox/MarginContainer/UnitHUD/MP/MPLabel");
            LeftElementalResistances.Add((Label) GetNode("HUD/HBoxContainer/LeftBox/MarginContainer/UnitHUD/ElementResistance/PhysicalResistance"));
            LeftElementalResistances.Add((Label) GetNode("HUD/HBoxContainer/LeftBox/MarginContainer/UnitHUD/ElementResistance/PierceResistance"));
            LeftElementalResistances.Add((Label) GetNode("HUD/HBoxContainer/LeftBox/MarginContainer/UnitHUD/ElementResistance/FireResistance"));
            LeftElementalResistances.Add((Label) GetNode("HUD/HBoxContainer/LeftBox/MarginContainer/UnitHUD/ElementResistance/WaterResistance"));
            LeftElementalResistances.Add((Label) GetNode("HUD/HBoxContainer/LeftBox/MarginContainer/UnitHUD/ElementResistance/WindResistance"));
            LeftElementalResistances.Add((Label) GetNode("HUD/HBoxContainer/LeftBox/MarginContainer/UnitHUD/ElementResistance/EarthResistance"));

            RightUnitName = (Label) GetNode("HUD/HBoxContainer/RightBox/MarginContainer/UnitHUD/UnitName/Name");
            RightLevel = (Label) GetNode("HUD/HBoxContainer/RightBox/MarginContainer/UnitHUD/UnitImage/Level");
            RightHP = (Label) GetNode("HUD/HBoxContainer/RightBox/MarginContainer/UnitHUD/HP/HPLabel");
            RightMP = (Label) GetNode("HUD/HBoxContainer/RightBox/MarginContainer/UnitHUD/MP/MPLabel");

            RightElementalResistances.Add((Label) GetNode("HUD/HBoxContainer/RightBox/MarginContainer/UnitHUD/ElementResistance/PhysicalResistance"));
            RightElementalResistances.Add((Label) GetNode("HUD/HBoxContainer/RightBox/MarginContainer/UnitHUD/ElementResistance/PierceResistance"));
            RightElementalResistances.Add((Label) GetNode("HUD/HBoxContainer/RightBox/MarginContainer/UnitHUD/ElementResistance/FireResistance"));
            RightElementalResistances.Add((Label) GetNode("HUD/HBoxContainer/RightBox/MarginContainer/UnitHUD/ElementResistance/WaterResistance"));
            RightElementalResistances.Add((Label) GetNode("HUD/HBoxContainer/RightBox/MarginContainer/UnitHUD/ElementResistance/WindResistance"));
            RightElementalResistances.Add((Label) GetNode("HUD/HBoxContainer/RightBox/MarginContainer/UnitHUD/ElementResistance/EarthResistance"));
            
            for(int i = 0; i < 5; i++)
            {
                LeftSkillName.Add((Label) GetNode("HUD/HBoxContainer/LeftBox/MarginContainer/Skills/Skill" + i + "/Name"));
                LeftDamage.Add((Label) GetNode("HUD/HBoxContainer/LeftBox/MarginContainer/Skills/Skill" + i + "/HBoxContainer/Damage/NinePatchRect/Value"));
                LeftAccuracy.Add((Label) GetNode("HUD/HBoxContainer/LeftBox/MarginContainer/Skills/Skill" + i + "/HBoxContainer/Accuracy/NinePatchRect/Value"));
                LeftCritical.Add((Label) GetNode("HUD/HBoxContainer/LeftBox/MarginContainer/Skills/Skill" + i + "/HBoxContainer/Critical/NinePatchRect/Value"));

                RightSkillName.Add((Label) GetNode("HUD/HBoxContainer/RightBox/MarginContainer/Skills/Skill" + i + "/Name"));
                RightDamage.Add((Label) GetNode("HUD/HBoxContainer/RightBox/MarginContainer/Skills/Skill" + i + "/HBoxContainer/Damage/NinePatchRect/Value"));
                RightAccuracy.Add((Label) GetNode("HUD/HBoxContainer/RightBox/MarginContainer/Skills/Skill" + i + "/HBoxContainer/Accuracy/NinePatchRect/Value"));
                RightCritical.Add((Label) GetNode("HUD/HBoxContainer/RightBox/MarginContainer/Skills/Skill" + i + "/HBoxContainer/Critical/NinePatchRect/Value"));
            }

            Show(false);

            SetProcessInput(false);

            this.GetParent().GetParent().Connect("UpdateCombatForecastSignal", this, "Update");
            this.GetParent().GetParent().Connect("ShowCombatForecastSignal", this, "Show");
        }

        private void Show(bool show)
        {
            MarginContainer SpeedHUD = (MarginContainer) GetNode("SpeedHUD");
            MarginContainer HUD = (MarginContainer) GetNode("HUD");
            
            SpeedHUD.Visible = show;
            HUD.Visible = show;

            SetProcessInput(show);
        }

        private void ClearSkillLabels()
        {
            for(int i = 0; i < 5; i++)
            {
                LeftSkillName[i].Text = "--";
                LeftDamage[i].Text = "--";
                LeftAccuracy[i].Text = "--";
                LeftCritical[i].Text = "--";

                RightSkillName[i].Text = "--";
                RightDamage[i].Text = "--";
                RightAccuracy[i].Text = "--";
                RightCritical[i].Text = "--";
            }
        }

        private void Update(int distance, UnitScene left_unit, int left_tile_def, int left_tile_ev, UnitScene right_unit, int right_tile_def, int right_tile_ev)
        {
            ClearSkillLabels();

            int speedBonus = left_unit.Stats[5] + left_unit.StatMods[3] - right_unit.Stats[5] - right_unit.StatMods[3];

            if (speedBonus > Global.MAX_SPEED_BONUS)
            {
                speedBonus = Global.MAX_SPEED_BONUS;
            }
            else if(speedBonus < -Global.MAX_SPEED_BONUS)
            {
                speedBonus = -Global.MAX_SPEED_BONUS;
            }

            SpeedValue.Text = speedBonus.ToString();

            LeftUnitName.Text = left_unit.UnitName;
            LeftLevel.Text = "Lv. " + left_unit.Level.ToString();
            LeftHP.Text = left_unit.CurrHP.ToString() + " /" + left_unit.Stats[0].ToString();
            LeftMP.Text = left_unit.CurrMP.ToString() + " /" + left_unit.Stats[1].ToString();

            RightUnitName.Text = right_unit.UnitName;
            RightLevel.Text = "Lv. " + right_unit.Level.ToString();
            RightHP.Text = right_unit.CurrHP.ToString() + " /" + right_unit.Stats[0].ToString();
            RightMP.Text = right_unit.CurrMP.ToString() + " /" + right_unit.Stats[1].ToString();

            for(int i = 0; i < 6; i++)
            {
                LeftElementalResistances[i].Text = left_unit.ElementRes[i];
                RightElementalResistances[i].Text = right_unit.ElementRes[i];
            }

            if(distance <= left_unit.StandardAttack.MaxRange && distance >= left_unit.StandardAttack.MinRange)
            {
                LeftSkillName[0].Text = left_unit.StandardAttack.Name + "\nMP: " + left_unit.StandardAttack.Cost;
                LeftDamage[0].Text = Global.CalculateDamage(left_unit, right_unit, left_unit.StandardAttack, right_tile_def, false).ToString();
                LeftDamage[0].Text += " x" + left_unit.StandardAttack.Hits.Item1;
                if(left_unit.StandardAttack.Hits.Item1 != left_unit.StandardAttack.Hits.Item2)
                {
                    LeftDamage[0].Text += "-" + left_unit.StandardAttack.Hits.Item2;
                }
                LeftAccuracy[0].Text = Global.CalculateAccuracy(left_unit, right_unit, left_unit.StandardAttack, right_tile_def).ToString();
                LeftCritical[0].Text = Global.CalculateCritical(left_unit, right_unit, left_unit.StandardAttack).ToString();
            }
            else
            {
                LeftSkillName[0].Text = left_unit.StandardAttack.Name + "\nOUT OF BOUND";
            }

            if(distance <= right_unit.StandardAttack.MaxRange && distance >= right_unit.StandardAttack.MinRange)
            {
                RightSkillName[0].Text = right_unit.StandardAttack.Name + "\nMP: " + right_unit.StandardAttack.Cost;
                RightDamage[0].Text = Global.CalculateDamage(right_unit, left_unit, right_unit.StandardAttack, left_tile_def, false).ToString();
                RightDamage[0].Text += " x" + right_unit.StandardAttack.Hits.Item1;
                if(right_unit.StandardAttack.Hits.Item1 != right_unit.StandardAttack.Hits.Item2)
                {
                    RightDamage[0].Text += "-" + right_unit.StandardAttack.Hits.Item2;
                }
                RightAccuracy[0].Text = Global.CalculateAccuracy(right_unit, left_unit, right_unit.StandardAttack, left_tile_def).ToString();
                RightCritical[0].Text = Global.CalculateCritical(right_unit, left_unit, right_unit.StandardAttack).ToString();
            }
            else
            {
                RightSkillName[0].Text = right_unit.StandardAttack.Name + "\nOUT OF BOUND";
            }

            int l = 0;
            int r = 0;

            for(int i = 0; i < 4; i++)
            {
                if(i < left_unit.ActiveSkills.Count && left_unit.ActiveSkills[i].Type < 7)
                {
                    l++;

                    BattleSkill skill = (BattleSkill) left_unit.ActiveSkills[i];

                    if(distance <= skill.MaxRange && distance >= skill.MinRange)
                    {
                        LeftSkillName[l].Text = skill.Name + "\nMP: " + skill.Cost;
                        LeftDamage[l].Text = Global.CalculateDamage(left_unit, right_unit, skill, right_tile_def, false).ToString();
                        LeftDamage[l].Text += " x" + skill.Hits.Item1;
                        if(skill.Hits.Item1 != skill.Hits.Item2)
                        {
                            LeftDamage[l].Text += "-" + skill.Hits.Item2;
                        }
                        LeftAccuracy[l].Text = Global.CalculateAccuracy(left_unit, right_unit, skill, right_tile_def).ToString();
                        LeftCritical[l].Text = Global.CalculateCritical(left_unit, right_unit, skill).ToString();
                    }
                    else
                    {
                        LeftSkillName[l].Text = skill.Name + "\nOUT OF BOUND";
                    }
                }
                if(i < right_unit.ActiveSkills.Count && right_unit.ActiveSkills[i].Type < 7)
                {
                    r++;

                    BattleSkill skill = (BattleSkill) right_unit.ActiveSkills[i];

                    if(distance <= skill.MaxRange && distance >= skill.MinRange)
                    {
                        RightSkillName[r].Text = skill.Name + "\nMP: " + skill.Cost;
                        RightDamage[r].Text = Global.CalculateDamage(right_unit, left_unit, skill, left_tile_def, false).ToString();
                        RightDamage[r].Text += " x" + skill.Hits.Item1;
                        if(skill.Hits.Item1 != skill.Hits.Item2)
                        {
                            RightDamage[r].Text += "-" + skill.Hits.Item2;
                        }
                        RightAccuracy[r].Text = Global.CalculateAccuracy(right_unit, left_unit, skill, left_tile_def).ToString();
                        RightCritical[r].Text = Global.CalculateCritical(right_unit, left_unit, skill).ToString();
                    }
                    else
                    {
                        RightSkillName[r].Text = skill.Name + "\nOUT OF BOUND";
                    }
                }
            }
        }
    }
}
