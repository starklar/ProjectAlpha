using Godot;
using System;
using System.Linq;

public partial class MapUi : CanvasLayer
{
    [Export]
    public RichTextLabel UnitName;

    [Export]
    public RichTextLabel HPText;

    [Export]
    public RichTextLabel MPText;

    [Export]
    public HBoxContainer Resistances;

    [Export]
    public RichTextLabel SkillName;

    [Export]
    public RichTextLabel AffinityBonus;

    [Export]
    public TextureRect SkillAffinity;

    public override void _Ready()
	{
        EventBus.Instance.setUpMapUI += SetupUI;
        EventBus.Instance.hideMapUI += HideUI;
    }

    private void SetupUI(string unitName, string unitHP, string unitMP, string resistances, string skillName, string affinityBonus){        
        UnitName.Text = unitName;
        HPText.Text = unitHP;
        MPText.Text = unitMP;

        string[] resValues = resistances.Split(",");

        int idx = 0;

        foreach (Node res in Resistances.GetChildren())
        {
            if(res is RichTextLabel label)
            {
                label.Text = resValues[idx];
                idx++;
            }
        }

        //Add later
        //SkillAffinity.Texture = ;
        SkillName.Text = skillName;
        AffinityBonus.Text = affinityBonus;
        
        Visible = true;
    }

    private void HideUI(){
        Hide();
        GD.Print("Hide");
    }
}
