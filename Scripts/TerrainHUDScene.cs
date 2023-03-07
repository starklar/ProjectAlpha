using Godot;
using System;

namespace Skirmish
{
    public class TerrainHUDScene : CanvasLayer
    {
        private Label TerrainName;
        private Label DefenceBonus;
        private Label EvasionBonus;

        public override void _Ready()
        {
            TerrainName = (Label) GetNode("HUD/Name");
            DefenceBonus = (Label) GetNode("HUD/DefenceBonus");
            EvasionBonus = (Label) GetNode("HUD/EvasionBonus");
            Node2D HUD = (Node2D) GetNode("HUD");
            //HUD.Position = new Vector2(864f, 0f);
            //HUD.Position = new Vector2(864f, 480f);

            this.GetParent().GetParent().Connect("TerrainCheckSignal", this, "UpdateLabels");
            this.GetParent().GetParent().Connect("ShowTerrainHUDSignal", this, "Show");
        }

        private void UpdateLabels(string tile_type, int defence_bonus, int evasion_bonus)
        {
            TerrainName.Text = tile_type;
            DefenceBonus.Text = defence_bonus.ToString();
            EvasionBonus.Text = evasion_bonus.ToString();
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
    }
}