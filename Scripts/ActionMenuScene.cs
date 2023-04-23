using System;
using Godot;
using System.Linq;
using System.Collections.Generic;
using Main;

namespace Skirmish
{
    public class ActionMenuScene : CanvasLayer
    {
        [Signal]
        delegate void EnableMapMovementSignal(int new_mode);

        [Signal]
        delegate void UnitUndoMoveSignal();

        [Signal]
        delegate void UnitWaitSignal();

        [Signal]
        delegate void SpawnAttackTilesSignal();

        [Signal]
        delegate void SpawnSupportTilesSignal(string skill_name);

        private Vector2 MenuLeftPos = new Vector2(0.0f, 200.0f);
        private Vector2 MenuRightPos = new Vector2(0.0f, 832.0f);
        private float LeftCursorPos = 214.0f;
        private float RightCursorPos = 810.0f;
        private int MainMenuSlots = 4;
        private float[] YPos = { 233.0f, 303.0f, 373.0f, 443.0f };

        private Node2D MainMenuNode;
        private Node2D SupportMenuNode;
        private KinematicBody2D PointerNode;

        private List<TextureRect> SkillNodes = new List<TextureRect>();
        private List<Label> SkillLabels = new List<Label>();
        private List<SupportSkill> SupportSkills = new List<SupportSkill>();
        private List<bool> SupportSkillSelectable = new List<bool>();

        private bool IsMainMenu;
        private bool CanInteract;
        private float PointerSide = 800.0f;
        private int PointerSlot = 1;

        public override void _Ready()
        {
            MainMenuNode = (Node2D) GetNode("MainMenu");
            SupportMenuNode = (Node2D) GetNode("SupportMenu");
            PointerNode = (KinematicBody2D) GetNode("Pointer");

            for(int i = 0; i < 4; i++)
            {
                SkillNodes.Add((TextureRect) GetNode("SupportMenu/Support" + i));
                SkillLabels.Add((Label) GetNode("SupportMenu/Support" + i + "/Support" + i + "Label"));
            }

            HideAll();

            IsMainMenu = true;
            CanInteract = false;
            PointerNode.Position = new Vector2(PointerSide, YPos[PointerSlot]);
            
            var animatedSprite = GetNode<AnimationPlayer>("Pointer/AnimationPlayer");
            animatedSprite.Play("Right");

            this.GetParent().GetParent().Connect("ShowActionMenuSignal", this, "ShowMenu");
            this.GetParent().GetParent().Connect("GetSupportSkillsSignal", this, "GetSupportSkills");
        }

        private void ShowMenu(string type)
        {
            if (type == "hide")
            {
                HideAll();
            }
            else
            {
                SetProcessInput(true);
                if (type == "main")
                {
                    ShowMainMenu(false);
                }
                else if (type == "interact")
                {
                    ShowMainMenu(true);
                }
                else if (type == "support")
                {
                    ShowSupportMenu();
                }
            }
        }

        private void HideAll()
        {
            MainMenuNode.Visible = false;
            SupportMenuNode.Visible = false;
            PointerNode.Visible = false;
            SetProcessInput(false);
        }

        private void GetSupportSkills(UnitScene unit)
        {
            SupportSkills.Clear();
            SupportSkillSelectable.Clear();
            foreach(ActiveSkill s in unit.ActiveSkills)
            {
                if(s.Type >= 7)
                {
                    SupportSkills.Add((SupportSkill) s);
                    if(unit.CurrMP >= s.Cost)
                    {
                        SupportSkillSelectable.Add(true);
                    }
                    else
                    {
                         SupportSkillSelectable.Add(false);
                    }
                }
            }
        }

        private void ShowMainMenu(bool interactable)
        {
            IsMainMenu = true;
            CanInteract = interactable;

            PointerSlot = 1;
            PointerNode.Visible = true;
            PointerNode.Position = new Vector2(PointerSide, YPos[PointerSlot]);
            MainMenuNode.Visible = true;
            SupportMenuNode.Visible = false;

            int size = SupportSkills.Count;
            Label SupportLableNode = (Label) GetNode("MainMenu/Support/SupportLabel");
            if(size > 0)
            {
                SupportLableNode.Text = "Support";
            }
            else
            {
                SupportLableNode.Text = "---";
            }

            TextureRect InteractTabNode = (TextureRect) GetNode("MainMenu/Interact");

            if (interactable)
            {
                MainMenuSlots = 4;
                InteractTabNode.Visible = true;
            }
            else
            {
                MainMenuSlots = 3;
                InteractTabNode.Visible = false;
            }
        }

        private void ShowSupportMenu()
        {
            IsMainMenu = false;

            PointerSlot = 0;
            PointerNode.Visible = true;
            PointerNode.Position = new Vector2(PointerSide, YPos[PointerSlot]);
            MainMenuNode.Visible = false;
            SupportMenuNode.Visible = true;

            int size = SupportSkills.Count;

            for(int i = 0; i < size; i++)
            {
                SkillLabels[i].Text = SupportSkills[i].Name;
                SkillNodes[i].Visible = true;
            }

            for(int i = size; i < 4; i++)
            {
                SkillNodes[i].Visible = false;
            }
        }

        //Set param to true to say move all to the left, false to move to the right
        private void SwitchSides(bool move_left)
        {
            if (move_left)
            {
                PointerSide = LeftCursorPos;
                PointerNode.Position = new Vector2(PointerSide, YPos[PointerSlot]);
                MainMenuNode.Position = MenuLeftPos;
                SupportMenuNode.Position = MenuLeftPos;
                var animatedSprite = GetNode<AnimationPlayer>("Pointer/AnimationPlayer");
                animatedSprite.Play("Left");
            }
            else
            {
                PointerSide = RightCursorPos;
                PointerNode.Position = new Vector2(PointerSide, YPos[PointerSlot]);
                MainMenuNode.Position = MenuRightPos;
                SupportMenuNode.Position = MenuRightPos;
                var animatedSprite = GetNode<AnimationPlayer>("Pointer/AnimationPlayer");
                animatedSprite.Play("Right");
            }

        }

        private void SelectOption()
        {
            if(IsMainMenu)
            {
                if(PointerSlot == 0)
                {
                    EmitSignal("UnitWaitSignal");
                    EmitSignal("EnableMapMovementSignal", 0);
                    HideAll();
                }
                else if(PointerSlot == 1)
                {
                    EmitSignal("SpawnAttackTilesSignal");
                    EmitSignal("EnableMapMovementSignal", 2);
                    HideAll();
                }
                else if(PointerSlot == 2)
                {
                    if(SupportSkills.Count > 0)
                    {
                        ShowSupportMenu();
                    }
                }
                else if(PointerSlot == 3)
                {
                    EmitSignal("EnableMapMovementSignal", 4);
                    HideAll();
                }
            }
            else
            {
                if(PointerSlot < SupportSkills.Count)
                {
                    if(SupportSkillSelectable[PointerSlot])
                    {
                        EmitSignal("SpawnSupportTilesSignal", SupportSkills[PointerSlot].Name);
                        EmitSignal("EnableMapMovementSignal", 4);
                        HideAll();
                    }
                }
            }
        }

        public override void _Input(InputEvent inputEvent)
        {
            if (inputEvent.IsActionPressed("move_cursor_up"))
            {
                if (PointerSlot == 0)
                {
                    if (IsMainMenu)
                    {
                        PointerSlot = MainMenuSlots - 1;
                    }
                    else
                    {
                        PointerSlot = SupportSkills.Count - 1;
                    }
                }
                else
                {
                    PointerSlot -= 1;
                }
                PointerNode.Position = new Vector2(PointerSide, YPos[PointerSlot]);
            }
            else if(inputEvent.IsActionPressed("move_cursor_down"))
            {
                if (IsMainMenu && PointerSlot == MainMenuSlots - 1)
                {
                    PointerSlot = 0;
                }
                else if(!IsMainMenu && PointerSlot == SupportSkills.Count - 1)
                {
                    PointerSlot = 0;
                }
                else
                {
                    PointerSlot += 1;
                }

                PointerNode.Position = new Vector2(PointerSide, YPos[PointerSlot]);
            }
            else if (inputEvent.IsActionPressed("grid_select"))
            {
                SelectOption();
            }
            else if (inputEvent.IsActionPressed("grid_deselect"))
            {
                if(IsMainMenu)
                {
                    EmitSignal("UnitUndoMoveSignal");
                    EmitSignal("EnableMapMovementSignal", 1);
                    HideAll();
                }
                else if(IsMainMenu == false)
                {
                    ShowMainMenu(CanInteract);
                }
            }
        }
    }
}