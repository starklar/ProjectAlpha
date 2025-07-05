using Godot;
using System.Collections.Generic;
using System.Text.Json;

public partial class GlobalVariables : Node
{
    public static GlobalVariables Instance { get; private set; }

    public static List<string> SKILL_TYPES = new() { "Physical", "Fire", "Ice", "Wind", "Earth", "Aether", "Heal", "Support", "Movement", "Disrupt", "Passive" };
    public static int HEAL_TYPE_INDEX = 6;
    public static int SUPPORT_TYPE_INDEX = 7;
    public static int MOVEMENT_TYPE_INDEX = 8;
    public static int DISRUPT_TYPE_INDEX = 9;
    public static int PASSIVE_TYPE_INDEX = 10;

    public static int MAX_SKILL_COUNT = 3;

    public static List<string> RESISTANCE_LABELS = new() { "Wk", "--", "Rs", "Nu" } ;

    public List<ActiveSkill> ActiveSkillList { get; set; }
    public List<PassiveSkill> PassiveSkillList { get; set; }

    public override void _Ready(){
        Instance = this;
        
        string activeJsonString = FileAccess.Open("res://JSONs/ActiveSkills.json", FileAccess.ModeFlags.Read).GetAsText();
        string passiveJsonString = FileAccess.Open("res://JSONs/PassiveSkills.json", FileAccess.ModeFlags.Read).GetAsText();
 
        ActiveSkillList = JsonSerializer.Deserialize<List<ActiveSkill>>(activeJsonString);
        PassiveSkillList = JsonSerializer.Deserialize<List<PassiveSkill>>(passiveJsonString);
    }


}
