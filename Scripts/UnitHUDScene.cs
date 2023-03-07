using Godot;
using System.Collections.Generic;

namespace Skirmish
{
    public class UnitHUDScene : CanvasLayer
    {
        private Label UnitName;
        private Label Level;
        private Label HP;
        private Label MP;
        private List<Label> ElementalResistances = new List<Label>(6);

        public override void _Ready()
        {
            UnitName = (Label) GetNode("HUD/UnitName/Name");
            Level = (Label) GetNode("HUD/UnitImage/Level");
            HP = (Label) GetNode("HUD/HP/HPLabel");
            MP = (Label) GetNode("HUD/MP/MPLabel");

            ElementalResistances.Add((Label) GetNode("HUD/ElementResistance/PhysicalResistance"));
            ElementalResistances.Add((Label) GetNode("HUD/ElementResistance/PierceResistance"));
            ElementalResistances.Add((Label) GetNode("HUD/ElementResistance/FireResistance"));
            ElementalResistances.Add((Label) GetNode("HUD/ElementResistance/WaterResistance"));
            ElementalResistances.Add((Label) GetNode("HUD/ElementResistance/WindResistance"));
            ElementalResistances.Add((Label) GetNode("HUD/ElementResistance/EarthResistance"));

            Node2D HUD = (Node2D) GetNode("HUD");
            //HUD.Position = new Vector2(0f, 480f);
            //HUD.Position = new Vector2(0f, 0f);

            this.GetParent().GetParent().Connect("UnitCheckSignal", this, "UpdateLabels");
            this.GetParent().GetParent().Connect("ShowUnitHUDSignal", this, "Show");
        }

        private void UpdateLabels(UnitScene unit)
        {
            UnitName.Text = unit.UnitName;
            Level.Text = "Lv. " + unit.Level.ToString();
            HP.Text = unit.CurrHP.ToString() + " /" + unit.Stats[0].ToString();
            MP.Text = unit.CurrMP.ToString() + " /" + unit.Stats[1].ToString();

            for(int i = 0; i < unit.ElementRes.Count - 1; i++)
            {
                ElementalResistances[i].Text = unit.ElementRes[i];
            }
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