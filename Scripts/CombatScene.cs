using Godot;
using System;
using System.Collections.Generic;
using Main;

namespace Skirmish
{
    public class CombatScene : Node2D
    {
        private static string UnitAChar = "A";
        private static string UnitBChar = "B";

        private Label UnitAName;
        private Label UnitAHP;
        private Label UnitAMP;
        private List<Label> UnitAElementRes = new List<Label>(6);

        private MarginContainer UnitADamageBox;
        private Label UnitADamage;
        private Label UnitAHits;
        private Label UnitAHitRate;
        private Label UnitACritRate;

        private Label UnitBName;
        private Label UnitBHP;
        private Label UnitBMP;
        private List<Label> UnitBElementRes = new List<Label>(6);

        private MarginContainer UnitBDamageBox;
        private Label UnitBDamage;
        private Label UnitBHits;
        private Label UnitBHitRate;
        private Label UnitBCritRate;

        private MarginContainer ActionSelectionBox;
        private Label SkillDescription;
        private TextureRect AttackElement;
        private Label AttackName;
        private Label AttackAffinity;
        private List<BattleSkill> UsableSkills = new List<BattleSkill>(4);
        private List<Label> SkillNames = new List<Label>(4);
        private List<TextureRect> SkillElements = new List<TextureRect>(4);
        private List<Label> SkillAffinity = new List<Label>(4);

        private Label CurrentSpeedBonus;

        [Signal]
        delegate void EndCombatSignal(UnitScene A, UnitScene B);

        private UnitScene UnitA;
        private UnitScene UnitB;
        private int TerrainDefenceBonusA;
        private int TerrainEvasionBonusA;
        private int TerrainDefenceBonusB;
        private int TerrainEvasionBonusB;
        private int Phase;
        private string CurrentUnit;
        private int SpeedBonus;

        private bool GuardingA;
        private bool GuardingB;

        private AnimationPlayer AnimationA;
        private AnimationPlayer AnimationB;
        private AnimationPlayer SkillAnimation;

        private float MiddleXPos = 700.0f;
        private float[] XPos = { 278.0f, 508.0f, 735.0f, 963.0f };
        private float[] YPos = { 508.0f, 560.0f, 612.0f };

        private int CursorX;
        private int CursorY;
        private KinematicBody2D PointerNode;

        private int Distance;
        
        public override void _Ready()
        {
            this.GetParent().Connect("StartCombatSignal", this, "StartCombat");
            
            //UnitA vvv
            UnitAName = (Label) GetNode("TopHUD/HBoxContainer/UnitAHUD/MarginContainer/NinePatchRect/Name");
            UnitAHP = (Label) GetNode("TopHUD/HBoxContainer/UnitAHUD/HBoxContainer/MarginContainer/NinePatchRect/HP");
            UnitAMP = (Label) GetNode("TopHUD/HBoxContainer/UnitAHUD/HBoxContainer/MarginContainer/NinePatchRect/MP");

            UnitADamageBox = (MarginContainer) GetNode("UnitACalculationBox");
            UnitADamage = (Label) GetNode("UnitACalculationBox/VBoxContainer/DamageBox/NinePatchRect/DamageValue");
            UnitAHits = (Label) GetNode("UnitACalculationBox/VBoxContainer/DamageBox/NinePatchRect/Hits");
            UnitAHitRate = (Label) GetNode("UnitACalculationBox/VBoxContainer/HitRateBox/NinePatchRect/HitRate");
            UnitACritRate = (Label) GetNode("UnitACalculationBox/VBoxContainer/CriticalHitBox/NinePatchRect/CriticalHitRate");

            UnitAElementRes.Add((Label) GetNode("TopHUD/HBoxContainer/UnitAHUD/NinePatchRect/HBoxContainer/PhysicalRes"));
            UnitAElementRes.Add((Label) GetNode("TopHUD/HBoxContainer/UnitAHUD/NinePatchRect/HBoxContainer/PierceRes"));
            UnitAElementRes.Add((Label) GetNode("TopHUD/HBoxContainer/UnitAHUD/NinePatchRect/HBoxContainer/FireRes"));
            UnitAElementRes.Add((Label) GetNode("TopHUD/HBoxContainer/UnitAHUD/NinePatchRect/HBoxContainer/WaterRes"));
            UnitAElementRes.Add((Label) GetNode("TopHUD/HBoxContainer/UnitAHUD/NinePatchRect/HBoxContainer/WindRes"));
            UnitAElementRes.Add((Label) GetNode("TopHUD/HBoxContainer/UnitAHUD/NinePatchRect/HBoxContainer/EarthRes"));
            //UnitA ^^^

            //UnitB vvv
            UnitBName = (Label) GetNode("TopHUD/HBoxContainer/UnitBHUD/MarginContainer/NinePatchRect/Name");
            UnitBHP = (Label) GetNode("TopHUD/HBoxContainer/UnitBHUD/HBoxContainer/MarginContainer/NinePatchRect/HP");
            UnitBMP = (Label) GetNode("TopHUD/HBoxContainer/UnitBHUD/HBoxContainer/MarginContainer/NinePatchRect/MP");

            UnitBDamageBox = (MarginContainer) GetNode("UnitBCalculationBox");
            UnitBDamage = (Label) GetNode("UnitBCalculationBox/VBoxContainer/DamageBox/NinePatchRect/DamageValue");
            UnitBHits = (Label) GetNode("UnitBCalculationBox/VBoxContainer/DamageBox/NinePatchRect/Hits");
            UnitBHitRate = (Label) GetNode("UnitBCalculationBox/VBoxContainer/HitRateBox/NinePatchRect/HitRate");
            UnitBCritRate = (Label) GetNode("UnitBCalculationBox/VBoxContainer/CriticalHitBox/NinePatchRect/CriticalHitRate");

            UnitBElementRes.Add((Label) GetNode("TopHUD/HBoxContainer/UnitBHUD/NinePatchRect/HBoxContainer/PhysicalRes"));
            UnitBElementRes.Add((Label) GetNode("TopHUD/HBoxContainer/UnitBHUD/NinePatchRect/HBoxContainer/PierceRes"));
            UnitBElementRes.Add((Label) GetNode("TopHUD/HBoxContainer/UnitBHUD/NinePatchRect/HBoxContainer/FireRes"));
            UnitBElementRes.Add((Label) GetNode("TopHUD/HBoxContainer/UnitBHUD/NinePatchRect/HBoxContainer/WaterRes"));
            UnitBElementRes.Add((Label) GetNode("TopHUD/HBoxContainer/UnitBHUD/NinePatchRect/HBoxContainer/WindRes"));
            UnitBElementRes.Add((Label) GetNode("TopHUD/HBoxContainer/UnitBHUD/NinePatchRect/HBoxContainer/EarthRes"));
            //UnitB ^^^

            ActionSelectionBox = (MarginContainer) GetNode("ActionSelection");
            AttackElement = (TextureRect) GetNode("ActionSelection/VBoxContainer/AttackCommand/NinePatchRect/ElementIcon");
            AttackName = (Label) GetNode("ActionSelection/VBoxContainer/AttackCommand/NinePatchRect/Name");
            AttackAffinity = (Label) GetNode("ActionSelection/VBoxContainer/AttackCommand/NinePatchRect/ElementIcon/Affinity");

            for (int i = 1; i <= 4; i++)
            {
                SkillNames.Add((Label) GetNode("ActionSelection/VBoxContainer/MoveCommands/Move" + i + "/NinePatchRect/Name"));
                SkillElements.Add((TextureRect) GetNode("ActionSelection/VBoxContainer/MoveCommands/Move" + i + "/NinePatchRect/ElementIcon"));
                SkillAffinity.Add((Label) GetNode("ActionSelection/VBoxContainer/MoveCommands/Move" + i + "/NinePatchRect/ElementIcon/Affinity"));
            }

            SkillDescription = (Label) GetNode("ActionSelection/VBoxContainer/SkillDescription/NinePatchRect/Description");

            CurrentSpeedBonus = (Label) GetNode("TopHUD/HBoxContainer/SpeedHUD/VBoxContainer/MarginContainer2/SpeedBonusValue");

            PointerNode = (KinematicBody2D) GetNode("Pointer");
            PointerNode.Position = new Vector2(MiddleXPos, YPos[0]);
            
            var animatedSprite = GetNode<AnimationPlayer>("Pointer/AnimationPlayer");
            animatedSprite.Play("Left");

            Visible = false;
            SetProcessInput(false);
        }

        private void StartCombat(int distance, UnitScene unitA, int tileADef, int tileAEv, UnitScene unitB, int tileBDef, int tileBEv)
        {
            UsableSkills = new List<BattleSkill>(4);

            Distance = distance;
            
            UnitA = unitA;
            TerrainDefenceBonusA = tileADef;
            TerrainEvasionBonusA = tileAEv;
            UnitB = unitB;
            TerrainDefenceBonusB = tileBDef;
            TerrainEvasionBonusB = tileBEv;

            //Labels vvv

            UnitAName.Text = UnitA.UnitName;
            UnitBName.Text = UnitB.UnitName;

            UpdateHPMP();

            for (int i = 0; i < UnitA.ElementRes.Count - 1; i++)
            {
                UnitAElementRes[i].Text = UnitA.ElementRes[i];
                UnitBElementRes[i].Text = UnitB.ElementRes[i];
            }

            //Labels ^^^

            string unitASceneName = unitA.UnitName + "CombatScene";
            PackedScene aScene = (PackedScene) ResourceLoader.Load("res://Units/" + unitASceneName + ".tscn");
            var unitAScene = aScene.Instance();
            AddChild(unitAScene);
            AnimationA = GetNode<AnimationPlayer>(unitASceneName + "/AnimationPlayer");

            string unitBSceneName = unitB.UnitName + "CombatScene";
            PackedScene bScene = (PackedScene) ResourceLoader.Load("res://Units/" + unitBSceneName + ".tscn");
            var unitBScene = bScene.Instance();

            AddChild(unitBScene);
            AnimationB = GetNode<AnimationPlayer>(unitBSceneName + "/AnimationPlayer");

            KinematicBody2D UnitABody = GetNode<KinematicBody2D>(unitASceneName);
            KinematicBody2D UnitBBody = GetNode<KinematicBody2D>(unitBSceneName);

            UnitABody.Position = new Vector2(Global.WINDOW_WIDTH / 4 - 8, Global.WINDOW_HEIGHT / 2 - 8);
            UnitBBody.Position = new Vector2(Global.WINDOW_WIDTH * 3 / 4 - 8, Global.WINDOW_HEIGHT / 2 - 8);

            AnimationA.Play("Idle");
            AnimationB.Play("Idle");

            SkillAnimation = GetNode<AnimationPlayer>("SkillAnimations/SkillAnimationPlayer");

            GuardingA = false;
            GuardingB = false;

            CursorX = 0;
            CursorY = 0;

            Phase = 0;
            CurrentUnit = UnitAChar;
            SpeedBonus = unitA.Stats[5] + unitA.StatMods[3] - unitB.Stats[5] - unitB.StatMods[3];

            if (SpeedBonus > Global.MAX_SPEED_BONUS)
            {
                SpeedBonus = Global.MAX_SPEED_BONUS;
            }
            else if(SpeedBonus < -Global.MAX_SPEED_BONUS)
            {
                SpeedBonus = -Global.MAX_SPEED_BONUS;
            }

            CurrentSpeedBonus.Text = SpeedBonus.ToString();

            HideAll();

            PointerNode.Position = new Vector2(MiddleXPos, YPos[0]);

            if(UnitA.Team == 0)
            {
                SetSkillBoxes(UnitA);
                SkillDescription.Text = UnitA.StandardAttack.Description;
                FillDamageBox(UnitAChar, UnitA.StandardAttack);
            }
            else if(UnitB.Team == 0)
            {
                SetSkillBoxes(UnitB);
                SkillDescription.Text = UnitB.StandardAttack.Description;
                FillDamageBox(UnitBChar, UnitB.StandardAttack);
            }

            Visible = true;
            this.GetNode<Camera2D>("Camera2D").Current = true;

            ChangePhase();
        }

        private void EndCombat()
        {
            Visible = false;
            SetProcessInput(false);
            EmitSignal("EndCombatSignal", UnitA, UnitB);
        }

        private void SetSkillBoxes(UnitScene unit)
        {
            AttackElement.Texture = (Texture) ResourceLoader.Load("res://HUDImages/" + Global.ELEMENTS[unit.StandardAttack.Type] + "Icon.png");
            int attackAffValue = unit.ElementAff[unit.StandardAttack.Type];

            if(attackAffValue > 0)
            {
                AttackAffinity.Text = "+";
            }
            else if(attackAffValue < 0)
            {
                AttackAffinity.Text = "-";
            }
            else
            {
                AttackAffinity.Text = "";
            }

            if(SkillInRange(unit.StandardAttack))
            {
                AttackName.Text = "Attack";
            }
            else
            {
                AttackName.Text = "-- O F B --";
            }

            AttackAffinity.Text += unit.ElementAff[unit.StandardAttack.Type];

            int skillIndex = 0;
            foreach(ActiveSkill skill in unit.ActiveSkills)
            {
                if(skill.Type >= 7)
                {
                    continue;
                }

                UsableSkills.Add((BattleSkill) skill);

                if(SkillInRange((BattleSkill) skill))
                {
                    SkillNames[skillIndex].Text = skill.Name;
                }
                else
                {
                    SkillNames[skillIndex].Text = "-- O F B --";
                }

                int skillAffValue = unit.ElementAff[skill.Type];

                if(skillAffValue > 0)
                {
                    SkillAffinity[skillIndex].Text = "+";
                }
                else if(skillAffValue < 0)
                {
                    SkillAffinity[skillIndex].Text = "-";
                }
                else
                {
                    SkillAffinity[skillIndex].Text = "";
                }

                SkillAffinity[skillIndex].Text += unit.ElementAff[skill.Type];

                SkillElements[skillIndex].Texture = (Texture) ResourceLoader.Load("res://HUDImages/" + Global.ELEMENTS[skill.Type] + "Icon.png");

                skillIndex++;
            }

            for(int i = skillIndex; i < 4; i++)
            {
                SkillNames[i].Text = "--";
                SkillAffinity[i].Text = "";
                SkillElements[i].Texture = null;
            }
        }

        private void FillDamageBox(string unit_char, BattleSkill skill)
        {
            if(unit_char == UnitAChar)
            {
                if(SkillInRange(skill))
                {
                    UnitBDamageBox.Visible = true;
                    UnitBDamage.Text = Global.CalculateDamage(UnitA, UnitB, skill, TerrainDefenceBonusB, GuardingB).ToString();
                    UnitBHitRate.Text = Global.CalculateAccuracy(UnitA, UnitB, skill, TerrainEvasionBonusB).ToString();
                    UnitBCritRate.Text = Global.CalculateCritical(UnitA, UnitB, skill).ToString();
                    UnitBHits.Text = "x" + skill.Hits.Item1;
                    if(skill.Hits.Item1 != skill.Hits.Item2)
                    {
                        UnitBHits.Text += "-" + skill.Hits.Item2;
                    }
                }
            }
            else
            {
                if(SkillInRange(skill))
                {
                    UnitADamageBox.Visible = true;
                    UnitADamage.Text = Global.CalculateDamage(UnitB, UnitA, skill, TerrainDefenceBonusA, GuardingA).ToString();
                    UnitAHitRate.Text = Global.CalculateAccuracy(UnitB, UnitA, skill, TerrainEvasionBonusA).ToString();
                    UnitACritRate.Text = Global.CalculateCritical(UnitB, UnitA, skill).ToString();
                    UnitAHits.Text = "x" + skill.Hits.Item1;
                    if(skill.Hits.Item1 != skill.Hits.Item2)
                    {
                        UnitAHits.Text += "-" + skill.Hits.Item2;
                    }
                }
            }
        }

        private bool SkillInRange(BattleSkill skill)
        {
            return Distance <= skill.MaxRange && Distance >= skill.MinRange;
        }

        private void UpdateHPMP()
        {
            UnitAHP.Text = UnitA.CurrHP.ToString() + " /" + UnitA.Stats[0].ToString();
            UnitAMP.Text = UnitA.CurrMP.ToString() + " /" + UnitA.Stats[1].ToString();

            UnitBHP.Text = UnitB.CurrHP.ToString() + " /" + UnitB.Stats[0].ToString();
            UnitBMP.Text = UnitB.CurrMP.ToString() + " /" + UnitB.Stats[1].ToString();
        }

        private void ChangePhase()
        {
            Phase++;

            CursorX = 0;
            CursorY = 0;

            PointerNode.Position = new Vector2(MiddleXPos, YPos[CursorY]);

            if(UnitA.CurrHP == 0 || UnitB.CurrHP == 0)
            {
                EndCombat();
                return;
            }

            if(Phase == 1)
            {
                CurrentUnit = UnitAChar;
            }
            else if(Phase == 3)
            {
                if (SpeedBonus > Global.FOLLOW_UP_THREASHOLD)
                {
                    CurrentUnit = UnitAChar;
                }
                else if(SpeedBonus < -Global.FOLLOW_UP_THREASHOLD)
                {
                    CurrentUnit = UnitBChar;
                }
                else
                {
                    EndCombat();
                    return;
                }
            }
            else if(Phase >= 4)
            {
                EndCombat();
                return;
            }
            else
            {
                if(CurrentUnit == UnitAChar)
                {
                    CurrentUnit = UnitBChar;
                }
                else if(CurrentUnit == UnitBChar)
                {
                    CurrentUnit = UnitAChar;
                }
            }
            
            if(CurrentUnit == UnitAChar)
            {
                if(UnitA.Team == 0)
                {
                    UnitBDamageBox.Visible = true;
                    ActionSelectionBox.Visible = true;
                    PointerNode.Visible = true;
                    SetProcessInput(true);
                }
                else
                {
                    ActionSelectionBox.Visible = false;
                    HideAll();
                    //SIGNAL FOR AI TO MAKE DECISIONS
                    AIMoveDecision((AIUnitScene) UnitA, UnitB, UnitAChar);
                }
            }
            else if(CurrentUnit == UnitBChar)
            {
                if(UnitB.Team == 0)
                {
                    UnitADamageBox.Visible = true;
                    ActionSelectionBox.Visible = true;
                    PointerNode.Visible = true;
                    SetProcessInput(true);
                }
                else
                {
                    ActionSelectionBox.Visible = false;
                    HideAll();
                    //SIGNAL FOR AI TO MAKE DECISIONS
                    AIMoveDecision((AIUnitScene) UnitB, UnitA, UnitBChar);
                }
            }
        }

        private void AIMoveDecision(AIUnitScene AIUnit, UnitScene Target, string userChar)
        {
            string battleAIType = AIUnit.GetBattleAIPattern();
            BattleSkill skillToUse = null;
            
            if(battleAIType == "Basic")
            {
                skillToUse = BasicAI(AIUnit, Target, userChar);
            }
            else if(battleAIType == "Random")
            {
                skillToUse = RandomAI(AIUnit);
            }
            else if(battleAIType == "Defensive")
            {
                skillToUse = DefensiveAI(AIUnit, Target, userChar);
            }
            else if(battleAIType == "Survivor")
            {
                //Is supposed to be blank due to this AI type only choosing to guard
            }
            else
            {
                //Failcase state
                skillToUse = BasicAI(AIUnit, Target, userChar);
            }

            if(skillToUse == null)
            {
                Guard(userChar);
            }
            else
            {
                UseMove(skillToUse, userChar);
            }
        }

        private BattleSkill BasicAI(UnitScene AIUnit, UnitScene Target, string userChar)
        {
            List<BattleSkill> availableSkills = new List<BattleSkill>(){ AIUnit.StandardAttack };

            foreach(BattleSkill skill in AIUnit.ActiveSkills)
            {
                if(skill.Type < 7 && AIUnit.CurrMP >= skill.Cost && SkillInRange(skill))
                {
                    availableSkills.Add(skill);
                }
            }

            int bestDamage = int.MinValue;
            int bestAccuracy = int.MinValue;
            int bestCritical = int.MinValue;
            bool canKO = false;

            int targetTerrainDef = 0;
            int targetTerrainEv = 0;
            bool targetIsGuarding = false;

            BattleSkill bestSkill = null;

            if(userChar == UnitAChar)
            {
                targetTerrainDef = TerrainDefenceBonusB;
                targetTerrainEv = TerrainEvasionBonusB;
                targetIsGuarding = GuardingB;
            }
            else
            {
                targetTerrainDef = TerrainDefenceBonusA;
                targetTerrainEv = TerrainEvasionBonusA;
                targetIsGuarding = GuardingA;
            }

            foreach(BattleSkill skill in availableSkills)
            {
                int tempDamage = Global.CalculateDamage(AIUnit, Target, skill, targetTerrainDef, targetIsGuarding);
                int tempAccuracy = Global.CalculateAccuracy(AIUnit, Target, skill, targetTerrainEv);
                int tempCritical = Global.CalculateCritical(AIUnit, Target, skill);

                if(tempAccuracy >= 15)
                {
                    if(Target.CurrHP <= tempDamage)
                    {
                        if(canKO)
                        {
                            if(bestAccuracy < tempAccuracy)
                            {
                                bestAccuracy = tempAccuracy;
                                bestCritical = tempCritical;
                                bestSkill = skill;
                                canKO = true;
                            }
                            else if(bestAccuracy == tempAccuracy)
                            {
                                if(bestCritical < tempCritical)
                                {
                                    bestAccuracy = tempAccuracy;
                                    bestCritical = tempCritical;
                                    bestSkill = skill;
                                    canKO = true;
                                }
                            }
                        }
                        else
                        {
                            bestAccuracy = tempAccuracy;
                            bestCritical = tempCritical;
                            bestSkill = skill;
                            canKO = true;
                        }
                    }
                    else if(Target.CurrHP <= tempDamage * 2 && tempCritical >= 50)
                    {
                        if(canKO)
                        {
                            if(bestAccuracy < tempAccuracy)
                            {
                                bestAccuracy = tempAccuracy;
                                bestCritical = tempCritical;
                                bestSkill = skill;
                                canKO = true;
                            }
                            else if(bestAccuracy == tempAccuracy)
                            {
                                if(bestCritical < tempCritical)
                                {
                                    bestAccuracy = tempAccuracy;
                                    bestCritical = tempCritical;
                                    bestSkill = skill;
                                    canKO = true;
                                }
                            }
                        }
                        else
                        {
                            bestAccuracy = tempAccuracy;
                            bestCritical = tempCritical;
                            bestSkill = skill;
                            canKO = true;
                        }
                    }
                    else if(!canKO)
                    {
                        if(bestDamage < tempDamage)
                        {
                            bestAccuracy = tempAccuracy;
                            bestCritical = tempCritical;
                            bestSkill = skill;
                            canKO = false;
                        }
                        else if(bestDamage == tempDamage)
                        {
                            if(bestAccuracy < tempAccuracy)
                            {
                                bestAccuracy = tempAccuracy;
                                bestCritical = tempCritical;
                                bestSkill = skill;
                                canKO = false;
                            }
                            else if(bestAccuracy == tempAccuracy)
                            {
                                if(bestCritical < tempCritical)
                                {
                                    bestAccuracy = tempAccuracy;
                                    bestCritical = tempCritical;
                                    bestSkill = skill;
                                    canKO = false;
                                }
                            }
                        }
                    }
                }
            }

            return bestSkill;
        }

        private BattleSkill RandomAI(UnitScene AIUnit)
        {
            List<BattleSkill> availableSkills = new List<BattleSkill>(){ AIUnit.StandardAttack };

            foreach(BattleSkill skill in AIUnit.ActiveSkills)
            {
                if(skill.Type < 7 && AIUnit.CurrMP >= skill.Cost && SkillInRange(skill))
                {
                    availableSkills.Add(skill);
                }
            }

            var rand = new Random();

            int randomMove = rand.Next(0, availableSkills.Count);

            return availableSkills[randomMove];
        }

        private BattleSkill DefensiveAI(UnitScene AIUnit, UnitScene Target, string userChar)
        {
            List<BattleSkill> availableSkills = new List<BattleSkill>(){ AIUnit.StandardAttack };

            foreach(BattleSkill skill in AIUnit.ActiveSkills)
            {
                if(skill.Type < 7 && AIUnit.CurrMP >= skill.Cost && SkillInRange(skill))
                {
                    availableSkills.Add(skill);
                }
            }

            int bestAccuracy = int.MinValue;
            int bestCritical = int.MinValue;
            bool canKO = false;

            int targetTerrainDef = 0;
            int targetTerrainEv = 0;
            bool targetIsGuarding = false;

            BattleSkill bestSkill = null;

            if(userChar == UnitAChar)
            {
                targetTerrainDef = TerrainDefenceBonusB;
                targetTerrainEv = TerrainEvasionBonusB;
                targetIsGuarding = GuardingB;
            }
            else
            {
                targetTerrainDef = TerrainDefenceBonusA;
                targetTerrainEv = TerrainEvasionBonusA;
                targetIsGuarding = GuardingA;
            }

            foreach(BattleSkill skill in availableSkills)
            {
                int tempDamage = Global.CalculateDamage(AIUnit, Target, skill, targetTerrainDef, targetIsGuarding);
                int tempAccuracy = Global.CalculateAccuracy(AIUnit, Target, skill, targetTerrainEv);
                int tempCritical = Global.CalculateCritical(AIUnit, Target, skill);

                if(tempAccuracy >= 15)
                {
                    if(Target.CurrHP <= tempDamage)
                    {
                        if(canKO)
                        {
                            if(bestAccuracy < tempAccuracy)
                            {
                                bestAccuracy = tempAccuracy;
                                bestCritical = tempCritical;
                                bestSkill = skill;
                                canKO = true;
                            }
                            else if(bestAccuracy == tempAccuracy)
                            {
                                if(bestCritical < tempCritical)
                                {
                                    bestAccuracy = tempAccuracy;
                                    bestCritical = tempCritical;
                                    bestSkill = skill;
                                    canKO = true;
                                }
                            }
                        }
                        else
                        {
                            bestAccuracy = tempAccuracy;
                            bestCritical = tempCritical;
                            bestSkill = skill;
                            canKO = true;
                        }
                    }
                }
            }

            return bestSkill;
        }

        private void HideAll()
        {
            UnitADamageBox.Visible = false;
            UnitBDamageBox.Visible = false;
            ActionSelectionBox.Visible = false;
            PointerNode.Visible = false;
            SetProcessInput(false);
        }

        private void Guard(string unit_char)
        {
            if(unit_char == UnitAChar)
            {
                GuardingA = true;
            }
            else
            {
                GuardingB = true;
            }

            bool heal = false;

            if(Phase == 2 && Math.Abs(SpeedBonus) < Global.FOLLOW_UP_THREASHOLD)
            {
                heal = true;
            }
            else if(Phase == 3)
            {
                heal = true;
            }

            if(heal)
            {
                if(unit_char == UnitAChar)
                {
                    int mpHeal = UnitA.Stats[1] / 10;

                    if(UnitA.CurrMP + mpHeal > UnitA.Stats[1])
                    {
                        UnitA.CurrMP = UnitA.Stats[1];
                    }
                    else
                    {
                        UnitA.CurrMP = UnitA.CurrMP + mpHeal;
                    }
                }
                else
                {
                    int mpHeal = UnitB.Stats[1] / 10;

                    if(UnitB.CurrMP + mpHeal > UnitB.Stats[1])
                    {
                        UnitB.CurrMP = UnitB.Stats[1];
                    }
                    else
                    {
                        UnitB.CurrMP = UnitB.CurrMP + mpHeal;
                    }
                }
            }

            ChangePhase();
        }

        private void UseMove(BattleSkill skill, string userChar)
        {
            UnitScene user;
            if (userChar == UnitAChar){
                user = UnitA;
            }
            else{
                user = UnitB;
            }

            if(user.CurrMP < skill.Cost)
            {
                return;
            }

            UnitScene target;
            int targetTileDefenceBonus;
            int targetTileEvasionBonus;
            bool targetIsGuarding;
            int direction = 1;
            if (userChar == UnitAChar){
                target = UnitB;
                targetTileDefenceBonus = TerrainDefenceBonusB;
                targetTileEvasionBonus = TerrainEvasionBonusB;
                targetIsGuarding = GuardingB;
                direction = 1;
            }
            else{
                target = UnitA;
                targetTileDefenceBonus = TerrainDefenceBonusA;
                targetTileEvasionBonus = TerrainEvasionBonusA;
                targetIsGuarding = GuardingA;
                direction = -1;
            }

            int total_damage = Global.CalculateDamage(user, target, skill, targetTileDefenceBonus, targetIsGuarding);
            
            user.CurrMP -= skill.Cost;

            var rand = new Random();

            int total_accuracy = Global.CalculateAccuracy(user, target, skill, targetTileEvasionBonus);

            int max_hits = rand.Next(skill.Hits.Item2);
            
            if(max_hits < skill.Hits.Item1)
            {
                max_hits = skill.Hits.Item1;
            }

            for(int i = 0; i < max_hits; i++)
            {
                if (total_accuracy > rand.Next(100))
                {
                    int total_crit = Global.CalculateCritical(user, target, skill);

                    if(target.ElementRes[skill.Type] == Global.RESISTANCE_LEVELS[2])
                    {
                        total_crit = total_crit / 2;
                    }

                    if(total_crit > rand.Next(100))
                    {
                        total_damage *= 2;
                        SpeedBonus += 2 * direction;
                    }

                    target.CurrHP -= total_damage;

                    if(target.ElementRes[skill.Type] == Global.RESISTANCE_LEVELS[2])
                    {
                        SpeedBonus -= 1 * direction;
                    }
                    else if(target.ElementRes[skill.Type] == Global.RESISTANCE_LEVELS[0])
                    {
                        SpeedBonus += 2 * direction;
                    }
                    else if(target.ElementRes[skill.Type] == Global.RESISTANCE_LEVELS[3])
                    {
                        SpeedBonus += 2 * direction;
                    }
                }
                else
                {
                    SpeedBonus -= 2 * direction;
                }
            }

            if(target.CurrHP < 0)
            {
                target.CurrHP = 0;
            }
            if (userChar == UnitAChar){
                UnitB = target;
            }
            else
            {
                UnitA = target;
            }

            if (userChar == UnitAChar)
            {
                UnitA = user;
            }
            else
            {
                UnitB = user;
            }

            UpdateHPMP();

            CurrentSpeedBonus.Text = SpeedBonus.ToString();

            ChangePhase();
        }

        public override void _Input(InputEvent inputEvent)
        {
            if (inputEvent.IsActionPressed("move_cursor_up"))
            {
                if (CursorY > 0)
                {
                    CursorY -= 1;
                }

                float tempX = 0;

                if(CursorY == 1)
                {
                    tempX = XPos[CursorX];
                    if(CurrentUnit == UnitAChar && CursorX < UsableSkills.Count)
                    {
                        FillDamageBox(UnitAChar, UsableSkills[CursorX]);
                    }
                    else if(CurrentUnit == UnitBChar  && CursorX < UsableSkills.Count)
                    {
                        FillDamageBox(UnitBChar, UsableSkills[CursorX]);
                    }
                    else
                    {
                        UnitADamageBox.Visible = false;
                        UnitBDamageBox.Visible = false;
                    }
                }
                else
                {
                    tempX = MiddleXPos;
                    if(CurrentUnit == UnitAChar)
                    {
                        FillDamageBox(UnitAChar, UnitA.StandardAttack);
                    }
                    else
                    {
                        FillDamageBox(UnitBChar, UnitB.StandardAttack);
                    }
                }

                PointerNode.Position = new Vector2(tempX, YPos[CursorY]);
            }
            else if(inputEvent.IsActionPressed("move_cursor_down"))
            {
                if (CursorY < 2)
                {
                    CursorY += 1;
                }

                float tempX = 0;

                if(CursorY == 1)
                {
                    tempX = XPos[CursorX];
                    if(CurrentUnit == UnitAChar && CursorX < UsableSkills.Count)
                    {
                        FillDamageBox(UnitAChar, UsableSkills[CursorX]);
                    }
                    else if(CurrentUnit == UnitBChar && CursorX < UsableSkills.Count)
                    {
                        FillDamageBox(UnitBChar, UsableSkills[CursorX]);
                    }
                    else
                    {
                        UnitADamageBox.Visible = false;
                        UnitBDamageBox.Visible = false;
                    }
                }
                else
                {
                    tempX = MiddleXPos;
                    UnitADamageBox.Visible = false;
                    UnitBDamageBox.Visible = false;
                }

                PointerNode.Position = new Vector2(tempX, YPos[CursorY]);
            }
            else if (inputEvent.IsActionPressed("move_cursor_left"))
            {
                if (CursorY == 1 && CursorX > 0)
                {
                    CursorX -= 1;
                    PointerNode.Position = new Vector2(XPos[CursorX], YPos[CursorY]);

                    if(CursorX < UsableSkills.Count && SkillInRange(UsableSkills[CursorX]))
                    {
                        if(CurrentUnit == UnitAChar)
                        {
                            FillDamageBox(UnitAChar, UsableSkills[CursorX]);
                        }
                        else if(CurrentUnit == UnitBChar)
                        {
                            FillDamageBox(UnitBChar, UsableSkills[CursorX]);
                        }
                    }
                    else
                    {
                        UnitADamageBox.Visible = false;
                        UnitBDamageBox.Visible = false;
                    }
                }
            }
            else if(inputEvent.IsActionPressed("move_cursor_right"))
            {
                if (CursorY == 1 && CursorX < 3)
                {
                    CursorX += 1;
                    PointerNode.Position = new Vector2(XPos[CursorX], YPos[CursorY]);

                    if(CursorX < UsableSkills.Count && SkillInRange(UsableSkills[CursorX]))
                    {
                        if(CurrentUnit == UnitAChar)
                        {
                            FillDamageBox(UnitAChar, UsableSkills[CursorX]);
                        }
                        else if(CurrentUnit == UnitBChar)
                        {
                            FillDamageBox(UnitBChar, UsableSkills[CursorX]);
                        }
                    }
                    else
                    {
                        UnitADamageBox.Visible = false;
                        UnitBDamageBox.Visible = false;
                    }
                }
            }
            else if (inputEvent.IsActionPressed("grid_select"))
            {
                BattleSkill attack = null;
                if(CursorY == 0)
                {
                    if(CurrentUnit == UnitAChar)
                    {
                        attack = UnitA.StandardAttack;
                    }
                    else
                    {
                        attack = UnitB.StandardAttack;
                    }
                    if(SkillInRange(attack))
                    {
                        UseMove(attack, CurrentUnit);
                    }
                }
                else if(CursorY == 2)
                {
                    Guard(CurrentUnit);
                }
                else
                {
                    if(CurrentUnit == UnitAChar)
                    {
                        if(CursorX < UsableSkills.Count && SkillInRange(UsableSkills[CursorX]))
                        {
                            UseMove(UsableSkills[CursorX], UnitAChar);
                        }
                        else
                        {
                            UnitADamageBox.Hide();
                            //UseMove(UsableSkills[CursorX], UserBChar);
                        }
                    }
                    else
                    {
                        if(CursorX < UsableSkills.Count && SkillInRange(UsableSkills[CursorX]))
                        {
                            UseMove(UsableSkills[CursorX], UnitBChar);
                        }
                        else
                        {
                            UnitBDamageBox.Hide();
                            //UseMove(UsableSkills, UserBChar);
                        }
                    }
                }
            }
            else if (inputEvent.IsActionPressed("grid_deselect"))
            {

            }

            if(inputEvent.IsActionPressed("move_cursor_up") || inputEvent.IsActionPressed("move_cursor_down") || inputEvent.IsActionPressed("move_cursor_right") || inputEvent.IsActionPressed("move_cursor_left"))
            {
                UnitScene unit;

                if(CurrentUnit == UnitAChar)
                {
                    unit = UnitA;
                }
                else
                {
                    unit = UnitB;
                }
                if(CursorY == 0)
                {
                    SkillDescription.Text = unit.StandardAttack.Description;
                }
                else if(CursorY == 2)
                {
                    UnitADamageBox.Visible = false;
                    UnitBDamageBox.Visible = false;
                    SkillDescription.Text = "Halve next enemy attacks, heals 10% MP if final turn";
                }
                else
                {
                    if(CursorX < UsableSkills.Count && SkillInRange(UsableSkills[CursorX]))
                    {
                        SkillDescription.Text = UsableSkills[CursorX].Description;
                    }
                    else
                    {
                        SkillDescription.Text = "--";
                    }
                }
            }
        }
    }
}