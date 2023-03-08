using Godot;
using System;

namespace Skirmish
{
    public class TerrainHUDScene : CanvasLayer
    {
        private Label TerrainName;
        private Label DefenceBonus;
        private Label EvasionBonus;
        private Label HPBonus;
        private Label MPBonus;

        public override void _Ready()
        {
            TerrainName = (Label) GetNode("HUD/MarginContainer/VBoxContainer/NameBox/NinePatchRect/Name");
            DefenceBonus = (Label) GetNode("HUD/MarginContainer/VBoxContainer/DefenceBox/NinePatchRect/HBoxContainer/NinePatchRect/DefenceBonus");
            EvasionBonus = (Label) GetNode("HUD/MarginContainer/VBoxContainer/EvasionBox/NinePatchRect/HBoxContainer/NinePatchRect/EvasionBonus");
            HPBonus = (Label) GetNode("HUD/MarginContainer/VBoxContainer/HpBox/NinePatchRect/HBoxContainer/NinePatchRect/HPBonus");
            MPBonus = (Label) GetNode("HUD/MarginContainer/VBoxContainer/MpBox/NinePatchRect/HBoxContainer/NinePatchRect/MPBonus");
            Node2D HUD = (Node2D) GetNode("HUD");

            this.GetParent().GetParent().Connect("TerrainCheckSignal", this, "UpdateLabels");
            this.GetParent().GetParent().Connect("ShowTerrainHUDSignal", this, "Show");
            this.GetParent().GetParent().GetNode<Node2D>("Cursor").Connect("ChangeTerrainHUDSideSignal", this, "SwapSide");
        }

        private void UpdateLabels(string tile_type, int defence_bonus, int evasion_bonus, int hp_bonus, int mp_bonus)
        {
            TerrainName.Text = tile_type;
            DefenceBonus.Text = defence_bonus.ToString();
            EvasionBonus.Text = evasion_bonus.ToString() + "%";
            HPBonus.Text = hp_bonus.ToString() + "%";
            MPBonus.Text = mp_bonus.ToString() + "%";
        }

        private void Show(bool show)
        {
            Node2D HUD = (Node2D) GetNode("HUD");
            if(show)
            {
                HUD.Visible = true;
            }
            else
            {
                HUD.Visible = false;
            }
        }

        private void SwapSide(bool move_up)
        {
            Node2D HUD = (Node2D) GetNode("HUD");

            if(move_up)
            {
                HUD.Position = new Vector2(864f, 0f);
            }
            else
            {
                HUD.Position = new Vector2(864f, 480f);
            }
        }
    }
}