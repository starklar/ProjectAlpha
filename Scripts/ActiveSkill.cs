using System;

public class ActiveSkill: Skill{
    public int Cost { get; set; }
    public int Power { get; set; }
    public int MinRange { get; set; }
    public int MaxRange { get; set; }
    public bool IsMagic { get; set; }
    public int Accuracy { get; set; }
    public int CriticalRate { get; set; }
    public bool TargetAll { get; set; }
}