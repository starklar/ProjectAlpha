using System;
using System.Collections.Generic;
using Godot;
using Skirmish;
using Main;

namespace Data
{
    public class UnitFullHUDScene: Node2D
    {
        [Signal]
        delegate void CloseUnitHUDSignal();

        private static float StartX = 0.0f;
        private static float StartY = -640.0f;

        private TextureRect TeamColor;
        private TextureRect UnitPortrait;
        private TextureRect MovementType;
        private TextureRect TopTextBar;
        private TextureRect BottomTextBar;
        private Label TopText;
        private Label BottomText;

        private Label UnitName;
        private Label Level;
        private Label EXP;
        private Label HP;
        private Label MP;
        private List<Label> ElementalResistances = new List<Label>(Global.ELEMENTS.Length);
        private List<Label> ElementalAffinities = new List<Label>(Global.ELEMENTS.Length);
        private List<Label> Stats = new List<Label>(6);
        private List<Label> PassiveSkills = new List<Label>(4);
        private List<Label> ActiveSkills = new List<Label>(5);

        private Vector2[][] PointerPos = {
            new Vector2[]{
                new Vector2(230.0f + StartX, 178.0f + StartY),
                new Vector2(210.0f + StartX, 230.0f + StartY),
                new Vector2(210.0f + StartX, 302.0f + StartY),
                new Vector2(210.0f + StartX, 374.0f + StartY),
                new Vector2(210.0f + StartX, 446.0f + StartY),
                new Vector2(210.0f + StartX, 518.0f + StartY),
                new Vector2(210.0f + StartX, 590.0f + StartY),
            },
            new Vector2[]{
                new Vector2(390.0f + StartX, 58.0f + StartY),
                new Vector2(390.0f + StartX, 134.0f + StartY),
                new Vector2(390.0f + StartX, 190.0f + StartY),
                new Vector2(600.0f + StartX, 246.0f + StartY),
                new Vector2(600.0f + StartX, 302.0f + StartY),
                new Vector2(600.0f + StartX, 372.0f + StartY),
                new Vector2(600.0f + StartX, 444.0f + StartY),
                new Vector2(600.0f + StartX, 516.0f + StartY),
                new Vector2(600.0f + StartX, 588.0f + StartY),
            },
            new Vector2[]{
                new Vector2(1020.0f + StartX, 130.0f + StartY),
                new Vector2(1020.0f + StartX, 188.0f + StartY),
                new Vector2(990.0f + StartX, 272.0f + StartY),
                new Vector2(990.0f + StartX, 344.0f + StartY),
                new Vector2(990.0f + StartX, 416.0f + StartY),
                new Vector2(990.0f + StartX, 488.0f + StartY),
                new Vector2(990.0f + StartX, 560.0f + StartY),
            },
        };

        private int PointerX;
        private int PointerY;
        private KinematicBody2D Pointer;

        private string ShowDetails;

        private static string Top = "TOP";
        private static string Bottom = "BOTTOM";
        private static string Off = "OFF";
        private static string PlayerTeamColor = "res://HUDImages/PlayerBackground.png";
        private static string EnemyTeamColor = "res://HUDImages/EnemyBackground.png";
        private static string OtherTeamColor = "res://HUDImages/OtherBackground.png";

        private UnitScene Unit;

        public override void _Ready()
        {
            TeamColor = (TextureRect) GetNode("TeamColor");
            UnitPortrait = (TextureRect) GetNode("UnitPortrait");
            MovementType = (TextureRect) GetNode("MovementType");

            TopTextBar = (TextureRect) GetNode("TopTextBar");
            BottomTextBar = (TextureRect) GetNode("BottomTextBar");

            TopText = (Label) GetNode("TopTextBar/Label");
            BottomText = (Label) GetNode("BottomTextBar/Label");

            Pointer = (KinematicBody2D) GetNode("Pointer");

            UnitName = (Label) GetNode("UnitName");
            Level = (Label) GetNode("UnitLevel");
            EXP = (Label) GetNode("UnitEXP");
            HP = (Label) GetNode("UnitHP");
            MP = (Label) GetNode("UnitMP");

            ElementalResistances.Add((Label) GetNode("Resistances/HBoxContainer/PhysicalResistance/NinePatchRect/Label"));
            ElementalResistances.Add((Label) GetNode("Resistances/HBoxContainer/PierceResistance/NinePatchRect/Label"));
            ElementalResistances.Add((Label) GetNode("Resistances/HBoxContainer/FireResistance/NinePatchRect/Label"));
            ElementalResistances.Add((Label) GetNode("Resistances/HBoxContainer/WaterResistance/NinePatchRect/Label"));
            ElementalResistances.Add((Label) GetNode("Resistances/HBoxContainer/WindResistance/NinePatchRect/Label"));
            ElementalResistances.Add((Label) GetNode("Resistances/HBoxContainer/EarthResistance/NinePatchRect/Label"));
            ElementalResistances.Add((Label) GetNode("Resistances/HBoxContainer/AetherResistance/NinePatchRect/Label"));

            ElementalAffinities.Add((Label) GetNode("Affinities/HBoxContainer/PhysicalAffinity/NinePatchRect/Label"));
            ElementalAffinities.Add((Label) GetNode("Affinities/HBoxContainer/PierceAffinity/NinePatchRect/Label"));
            ElementalAffinities.Add((Label) GetNode("Affinities/HBoxContainer/FireAffinity/NinePatchRect/Label"));
            ElementalAffinities.Add((Label) GetNode("Affinities/HBoxContainer/WaterAffinity/NinePatchRect/Label"));
            ElementalAffinities.Add((Label) GetNode("Affinities/HBoxContainer/WindAffinity/NinePatchRect/Label"));
            ElementalAffinities.Add((Label) GetNode("Affinities/HBoxContainer/EarthAffinity/NinePatchRect/Label"));
            ElementalAffinities.Add((Label) GetNode("Affinities/HBoxContainer/AetherAffinity/NinePatchRect/Label"));

            Stats.Add((Label) GetNode("Stats/VBoxContainer/Move/NinePatchRect/Label"));
            Stats.Add((Label) GetNode("Stats/VBoxContainer/Strength/NinePatchRect/Label"));
            Stats.Add((Label) GetNode("Stats/VBoxContainer/Focus/NinePatchRect/Label"));
            Stats.Add((Label) GetNode("Stats/VBoxContainer/Speed/NinePatchRect/Label"));
            Stats.Add((Label) GetNode("Stats/VBoxContainer/Vitality/NinePatchRect/Label"));
            Stats.Add((Label) GetNode("Stats/VBoxContainer/Spirit/NinePatchRect/Label"));

            ActiveSkills.Add((Label) GetNode("BattleSkills/VBoxContainer/Attack/NinePatchRect/Label"));

            for(int i = 0; i < 4; i++)
            {
                ActiveSkills.Add((Label) GetNode("BattleSkills/VBoxContainer/Skill" + i + "/NinePatchRect/Label"));
                PassiveSkills.Add((Label) GetNode("PassiveSkills/VBoxContainer/Skill" + i + "/NinePatchRect/Label"));
            }

            this.GetParent().Connect("UpdateUnitFullHUDSignal", this, "UpdateLabels");
            this.GetParent().Connect("ShowUnitFullHUDSignal", this, "Show");

            ShowDetails = Off;

            PointerX = 0;
            PointerY = 0;

            Pointer.Position = PointerPos[PointerX][PointerY];

            var animatedSprite = GetNode<AnimationPlayer>("Pointer/AnimationPlayer");
            animatedSprite.Play("Left");

            ShowInfoBox();
            Show(false);
        }

        private void UpdateLabels(UnitScene unit)
        {
            Unit = unit;

            if(unit.Team == 0)
            {
                TeamColor.Texture = (Texture) GD.Load(PlayerTeamColor);
            }
            else if(unit.Team == 1)
            {
                TeamColor.Texture = (Texture) GD.Load(EnemyTeamColor);
            }
            else if(unit.Team == 2)
            {
                TeamColor.Texture = (Texture) GD.Load(OtherTeamColor);
            }

            MovementType.Texture = (Texture) GD.Load("res://HUDImages/" + Global.MOVEMENT_TYPE[unit.MovementType] + "Unit.png");

            UnitName.Text = unit.UnitName;
            //TODO: Finalize this
            //UnitPortrait = (Texture) GD.Load(unit.UnitName + "Portrait.jpg");
            Level.Text = unit.Level.ToString();
            EXP.Text = unit.EXP.ToString();
            HP.Text = unit.CurrHP.ToString() + " /" + unit.Stats[0].ToString();
            MP.Text = unit.CurrMP.ToString() + " /" + unit.Stats[1].ToString();

            for(int i = 0; i < unit.ElementRes.Count - 1; i++)
            {
                ElementalResistances[i].Text = unit.ElementRes[i];

                int attackAffValue = unit.ElementAff[unit.StandardAttack.Type];

                if(attackAffValue > 0)
                {
                    ElementalAffinities[i].Text = "+";
                }
                else if(attackAffValue < 0)
                {
                    ElementalAffinities[i].Text = "-";
                }
                else
                {
                    ElementalAffinities[i].Text = "";
                }

                ElementalAffinities[i].Text += unit.ElementAff[i];
            }

            for(int k = 0; k < unit.StatMods.Count; k++)
            {
                Stats[k].Text = (unit.Stats[k + 2] + unit.StatMods[k]).ToString();

                /*TODO: May use this section for coloring text to show stat mod is in effect instead
                if(unit.StatMods[k] < 0)
                {
                    Stats[k].Text += "-" + unit.StatMods[k];
                }
                else if(unit.StatMods[k] > 0)
                {
                    Stats[k].Text += "+" + unit.StatMods[k];
                }*/
            }

            ActiveSkills[0].Text = Unit.StandardAttack.Name;

            for(int j = 0; j < 4; j++)
            {
                if(j < Unit.ActiveSkills.Count)
                {
                    ActiveSkills[j + 1].Text = Unit.ActiveSkills[j].Name;
                }
                else
                {
                    ActiveSkills[j + 1].Text = "--";
                }
                if(j < Unit.PassiveSkills.Count)
                {
                    PassiveSkills[j].Text = Unit.PassiveSkills[j].Name;
                }
                else
                {
                    PassiveSkills[j].Text = "--";
                }
            }

            UpdateInfoBox();
        }

        private void Show(bool show)
        {
            this.SetProcessInput(show);
            this.Visible = show;
            this.GetNode<Camera2D>("Camera2D").Current = show;
            ShowCursor(false);
        }

        private void ShowCursor(bool show)
        {
            Pointer.Visible = show;
            if(show)
            {
                PointerX = 0;
                PointerY = 0;
                Pointer.Position = PointerPos[PointerX][PointerY];

                ShowDetails = Bottom;
            }
            else
            {
                ShowDetails = Off;
            }
        }

        private void ShowInfoBox()
        {
            if(ShowDetails == Top)
            {
                TopTextBar.Visible = true;
                BottomTextBar.Visible = false;
            }
            else if(ShowDetails == Bottom)
            {
                TopTextBar.Visible = false;
                BottomTextBar.Visible = true;
            }
            else
            {
                TopTextBar.Visible = false;
                BottomTextBar.Visible = false;
            }
        }

        private void UpdateInfoBox()
        {
            string newText = "";
            if(PointerX == 0)
            {
                if(PointerY == 0)
                {
                    newText += "Unit's movement type\n";
                    newText += "This unit type is: " + Global.MOVEMENT_TYPE[Unit.MovementType];
                }
                else
                {
                    newText += Unit.Stats[PointerY + 1];
                    
                    if(Unit.StatMods[PointerY - 1] > 0)
                    {
                        newText += " + " + Unit.StatMods[PointerY - 1];
                    }
                    else if(Unit.StatMods[PointerY - 1] < 0)
                    {
                        newText += Unit.StatMods[PointerY - 1];
                    }

                    newText += "\n";

                    if(PointerY == 1)
                    {
                        newText += "Max number of spaces the unit can move.";
                    }
                    else if(PointerY == 2)
                    {
                        newText += "Unit's Strength. Increases both Physical Damage and Critical Hit rate.";
                    }
                    else if(PointerY == 3)
                    {
                        newText += "Unit's Focus. Increases both Magical Damage and Accuracy.";
                    }
                    else if(PointerY == 4)
                    {
                        newText += "Unit's reaction Speed. If Unit has >= " + Global.FOLLOW_UP_THREASHOLD + " Speed" +
                                    "than opponent, enables Unit to perform a followup attack during combat. Also " + 
                                    "increases unit evasion rate.";
                    }
                    else if(PointerY == 5)
                    {
                        newText += "Unit's Vitality. Reduces Physical Damage taken and chance to be hit Critically";
                    }
                    else if(PointerY == 6)
                    {
                        newText += "Unit's Spirit. Reduces Magical Damage taken. Also increases MP recovered when " +
                                    "defending during combat.";
                    }
                }
            }
            else if(PointerX == 1)
            {
                if(PointerY == 0)
                {
                    newText += "Unit Name.";
                }
                else if(PointerY == 1)
                {
                    newText += "Unit's current Level.";
                }
                else if(PointerY == 2)
                {
                    newText += "Unit's current EXP. Get 100 EXP to gain a Level";
                }
                else if(PointerY == 3)
                {
                    newText += "Unit's current Health / Unit's Max Health.\n";
                    newText += "If current Health reaches 0, unit is defeated and is removed from the Skirmish";
                }
                else if(PointerY == 4)
                {
                    newText += "Unit's current Mana / Unit's Max Mana.\n";
                    newText += "Used to perform special skills in and out of combat.";
                }
                else if(PointerY - 4 < Unit.PassiveSkills.Count)
                {
                    newText += Unit.PassiveSkills[PointerY - 4].Description;
                }
                else
                {
                    newText += "--";
                }
            }
            else if(PointerX == 2)
            {
                if(PointerY == 0)
                {
                    newText += "Unit's Resistance to specific elements. Increases or decreases " + 
                                "damage unit takes when hit by them.\n";
                    newText += "Wk = Weak, -- = Neutral, Rs = Resist, Wd = Warded";
                }
                else if(PointerY == 1)
                {
                    newText += "Unit's Affinity to using specific elements.\n";
                    newText += "Affects power and accuracy of skills of associated element.\n";
                    newText += "+ = Good Affinity, - = Bad Affinity";
                }
                else if(PointerY == 2)
                {
                    newText += Unit.StandardAttack.Name + " " + Unit.StandardAttack.Cost + "MP\nElement: " + Global.ELEMENTS[Unit.StandardAttack.Type];
                    newText += "\nPower: " + Unit.StandardAttack.Power + " Acc: " + Unit.StandardAttack.Accuracy + " Crit: " + Unit.StandardAttack.CriticalRate;
                    newText += "\nMin Range: " + Unit.StandardAttack.MinRange + " Max Range: "+ Unit.StandardAttack.MaxRange + "\n";
                    newText += Unit.StandardAttack.Description;
                }
                else if(PointerY - 3 < Unit.ActiveSkills.Count)
                {
                    if(Unit.ActiveSkills[PointerY - 3].Type < 7)
                    {
                        BattleSkill bSkill = (BattleSkill) Unit.ActiveSkills[PointerY - 3];
                        newText += bSkill.Name + " Cost: " + bSkill.Cost + "MP\nElement: " + Global.ELEMENTS[bSkill.Type];
                        newText += "\nPower: " + bSkill.Power + " Acc: " + bSkill.Accuracy + " Crit: " + bSkill.CriticalRate;
                        newText += "\nMin Range: " + Unit.ActiveSkills[PointerY - 3].MinRange + " Max Range: "+ Unit.ActiveSkills[PointerY - 3].MaxRange + "\n";
                        newText += bSkill.Description;
                    }
                    else
                    {
                        SupportSkill sSkill = (SupportSkill) Unit.ActiveSkills[PointerY - 3];
                        newText += sSkill.Name + " Cost: " + sSkill.Cost + "MP" + "\nElement: ";
                        if(sSkill.Modifier <= 2)
                        {
                            newText += "Healing";
                        }
                        else
                        {
                            newText += "Stat Modifier";
                        }
                        newText += "\nPower: " + sSkill.Power + " Acc: " + sSkill.Accuracy;
                        newText += "\nMin Range: " + Unit.ActiveSkills[PointerY - 3].MinRange + " Max Range: "+ Unit.ActiveSkills[PointerY - 3].MaxRange + "\n";
                        newText += sSkill.Description;
                    }
                }
            }
            TopText.Text = newText;
            BottomText.Text = newText;
        }

        private void MoveCursor(string direction)
        {
            if(ShowDetails != Off)
            {
                if(direction == "Up")
                {
                    if(PointerY > 0)
                    {
                        PointerY--;
                    }
                }
                else if(direction == "Down")
                {
                    if(PointerY + 1 < PointerPos[PointerX].Length)
                    {
                        PointerY++;
                    }
                }
                else if(direction == "Right")
                {
                    if(PointerX + 1 < PointerPos.Length)
                    {
                        PointerX++;
                    }
                }
                else if(direction == "Left")
                {
                    if(PointerX > 0)
                    {
                        PointerX--;
                    }
                }

                if(PointerY >= PointerPos[PointerX].Length)
                {
                    PointerY = PointerPos[PointerX].Length - 1;
                }

                Pointer.Position = PointerPos[PointerX][PointerY];

                if(Pointer.Position.y < 280 + StartY)
                {
                    ShowDetails = Bottom;
                }
            

                if(Pointer.Position.y > 360 + StartY)
                {
                    ShowDetails = Top;
                }
                

                ShowInfoBox();
                UpdateInfoBox();
            }
        }

        public override void _Input(InputEvent inputEvent)
        {
            if(ShowDetails == Off)
            {
                if(inputEvent.IsActionPressed("grid_select"))
                {
                    ShowCursor(true);
                    ShowInfoBox();
                    UpdateInfoBox();
                }
                else if(inputEvent.IsActionPressed("grid_deselect"))
                {
                    ShowDetails = Off;
                    Show(false);
                    ShowInfoBox();
                    EmitSignal("CloseUnitHUDSignal");
                }
            }
            else if(ShowDetails != Off)
            {
                string dir = "";
                if(inputEvent.IsActionPressed("move_cursor_up"))
                {
                    dir = "Up";
                }
                else if(inputEvent.IsActionPressed("move_cursor_down"))
                {
                    dir = "Down";
                }
                else if(inputEvent.IsActionPressed("move_cursor_left"))
                {
                    dir = "Left";
                }
                else if(inputEvent.IsActionPressed("move_cursor_right"))
                {
                    dir = "Right";
                }

                MoveCursor(dir);

                if(inputEvent.IsActionPressed("grid_deselect"))
                {
                    ShowDetails = Off;
                    ShowCursor(false);
                    ShowInfoBox();
                }
            }
        }
    }
}