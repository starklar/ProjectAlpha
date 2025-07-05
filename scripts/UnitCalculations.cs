using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public class UnitCalculations {
    private UnitStats _stats;
    private ActiveSkill _skill;
    private Array<int> _passives;

    private int _fieldStrengthModifiers = 0;
    private int _fieldMagicModifiers = 0;
    private int _fieldEnduranceModifiers = 0;
    private int _fieldSpiritModifiers = 0;
    private int _fieldAgilityModifiers = 0;
    private int _fieldAccuracyModifiers = 0;
    private int _fieldCriticalModifiers = 0;
    private int _fieldAvoidModifiers = 0;
    private int _fieldDodgeModifiers = 0;

    public void SetUp(UnitStats stats, ActiveSkill skill, Array<int> passives){
        _stats = stats;
        _skill = skill;
        _passives = passives;

        _fieldStrengthModifiers = 0;
        _fieldMagicModifiers = 0;
        _fieldEnduranceModifiers = 0;
        _fieldSpiritModifiers = 0;
        _fieldAccuracyModifiers = 0;
        _fieldCriticalModifiers = 0;
        _fieldAvoidModifiers = 0;
        _fieldDodgeModifiers = 0;
    }

    public void SetSkill(ActiveSkill skill){
        _skill = skill;
    }

    public void SetFieldModifiers(int str, int mag, int en, int sp, int ag, int acc, int crt, int av, int dg){
        _fieldStrengthModifiers = str;
        _fieldMagicModifiers = mag;
        _fieldEnduranceModifiers = en;
        _fieldSpiritModifiers = sp;
        _fieldAgilityModifiers = ag;
        _fieldAccuracyModifiers = acc;
        _fieldCriticalModifiers = crt;
        _fieldAvoidModifiers = av;
        _fieldDodgeModifiers = dg;
    }

    public int CalculateBaseAttack(){
        int currentAttack = _skill.Power;
        currentAttack += _stats.Affinities[_skill.Type] / 3; //Should be every 3 affinity levels for +/-1
        
        //Probably want to do passive increases here

        if(_skill.IsMagic){
            currentAttack += _stats.MagicModifier;
            currentAttack += _stats.Magic;
        }
        else{
            currentAttack += _stats.StrengthModifier;
            currentAttack += _stats.Strength;
        }

        if(currentAttack < 0){
            return 0;
        }

        return currentAttack;
    }

    public int CalculateEffectiveAttack(int targetResistance){
        int currentAttack = _skill.Power;
        currentAttack += _stats.Affinities[_skill.Type] / 3; //Should be every 3 affinity levels for +/-1
        
        //Probably want to do passive increases here

        if(targetResistance == 0){
            currentAttack *= 3;
        }
        else if(targetResistance == 2){
            currentAttack /= 2;
        }
        else if(targetResistance == 3){
            currentAttack = 0;
        }

        if(_skill.IsMagic){
            currentAttack += _stats.MagicModifier;
            currentAttack += _stats.Magic;
            currentAttack += _fieldMagicModifiers;
        }
        else{
            currentAttack += _stats.StrengthModifier;
            currentAttack += _stats.Strength;
            currentAttack += _fieldStrengthModifiers;
        }

        if(currentAttack < 0){
            return 0;
        }

        return currentAttack;
    }

    public int CalculateAccuracy(){
        int currentAccuracy = _skill.Accuracy;
        currentAccuracy += (_stats.Affinities[_skill.Type] + 2) / 3; //Should be every 3 affinity levels starting from 1 for +/-10 accuracy

        currentAccuracy += _stats.DexterityModifier;
        currentAccuracy += _stats.Dexterity;

        currentAccuracy += (_stats.LuckModifier + _stats.Luck) / 2;

        //Probably want to do passive increases here

        currentAccuracy += _fieldAccuracyModifiers;

        if(currentAccuracy < 0){
            return 0;
        }

        return currentAccuracy;
    }

    public int CalculateCriticalRate(){
        int currentCritical = _skill.CriticalRate;

        currentCritical += (_stats.DexterityModifier + _stats.Dexterity) / 2;

        currentCritical += (_stats.LuckModifier + _stats.Luck) / 2;

        //Probably want to do passive increases here

        currentCritical += _fieldCriticalModifiers;

        if(currentCritical < 0){
            return 0;
        }

        return currentCritical;
    }
    
    //Use this to also calculate for own during unit's stat screen
    public int CalculateAvoid(){
        int currentAvoid = _stats.AgilityModifier;
        currentAvoid += _stats.Agility;

        currentAvoid += (_stats.LuckModifier + _stats.Luck) / 2;

        //Probably want to do passive increases here

        currentAvoid += _fieldAvoidModifiers;

        if(currentAvoid < 0){
            return 0;
        }

        return currentAvoid;
    }
    
    //Use this to also calculate for own during unit's stat screen
    public int CalculateCriticalDodge(){
        int currentDodge = _stats.LuckModifier;
        currentDodge += _stats.Luck;

        //Probably want to do passive increases here

        currentDodge += _fieldDodgeModifiers;

        if(currentDodge < 0){
            return 0;
        }

        return currentDodge;
    }

    public int CalculateEndurance(){
        int currentEndurance = _stats.EnduranceModifier;
        currentEndurance += _stats.Endurance;

        //Probably want to do passive increases here

        currentEndurance += _fieldEnduranceModifiers;

        if(currentEndurance < 0){
            return 0;
        }

        return currentEndurance;
    }

    public int CalculateSpirit(){
        int currentSpirit = _stats.SpiritModifier;
        currentSpirit += _stats.Spirit;

        //Probably want to do passive increases here

        currentSpirit += _fieldSpiritModifiers;

        if(currentSpirit < 0){
            return 0;
        }

        return currentSpirit;
    }

    public int CalculateAgility(){
        int currentAgility = _stats.AgilityModifier;
        currentAgility += _stats.Agility;

        //Probably want to do passive increases here

        currentAgility += _fieldAgilityModifiers;

        if(currentAgility < 0){
            return 0;
        }

        return currentAgility;
    }

    public int GetResistance(int type){
        return _stats.Resistances[type];
    }
}