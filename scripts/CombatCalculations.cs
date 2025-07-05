using Godot;
using System;
using System.Collections.Generic;

public class CombatCalculations {
    private UnitCalculations _leftUnitCalculations;
    private UnitCalculations _rightUnitCalculations;

    private int _leftResistance;
    private int _rightResistance;

    private ActiveSkill _leftSkill;
    private ActiveSkill _rightSkill;

    private int _range;

    public void SetUp(Unit leftUnit, ActiveSkill leftSkill, Unit rightUnit, int range){
        _leftUnitCalculations = new UnitCalculations();
        _rightUnitCalculations = new UnitCalculations();

        _leftSkill = leftSkill;
        _rightSkill = GlobalVariables.Instance.ActiveSkillList[rightUnit.EquipedSkill];

        _leftResistance = leftUnit.Stats.Resistances[_rightSkill.Type];
        _rightResistance = rightUnit.Stats.Resistances[_leftSkill.Type];

        _leftUnitCalculations.SetUp(leftUnit.Stats, _leftSkill, leftUnit.PassiveSkills);
        _rightUnitCalculations.SetUp(rightUnit.Stats, _rightSkill, rightUnit.PassiveSkills);

        _range = range;
    }

    public int LeftAttack(){
        int damage = _leftUnitCalculations.CalculateEffectiveAttack(_rightResistance);

        if(_leftSkill.IsMagic){
            damage -= _rightUnitCalculations.CalculateSpirit();
        }
        else{
            damage -= _rightUnitCalculations.CalculateEndurance();
        }

        if(damage <= 0){
            damage = 1;
        }

        return damage;
    }

    public int LeftHitRate(){
        return _leftUnitCalculations.CalculateAccuracy() - _rightUnitCalculations.CalculateAvoid();
    }

    public int LeftCritRate(){
        return _leftUnitCalculations.CalculateCriticalRate() - _rightUnitCalculations.CalculateCriticalDodge();
    }

    public int RightAttack(){
        int damage = _rightUnitCalculations.CalculateEffectiveAttack(_leftResistance);

        if(_rightSkill.IsMagic){
            damage -= _leftUnitCalculations.CalculateSpirit();
        }
        else{
            damage -= _leftUnitCalculations.CalculateEndurance();
        }

        if(damage <= 0){
            damage = 1;
        }

        //Damage and hit stuff

        //Maybe extra effects here

        return damage;
    }

    public int RightHitRate(){
        return _rightUnitCalculations.CalculateAccuracy() - _leftUnitCalculations.CalculateAvoid();
    }

    public int RightCritRate(){
        return _rightUnitCalculations.CalculateCriticalRate() - _leftUnitCalculations.CalculateCriticalDodge();
    }

    public int SpeedDifference(){
        return _leftUnitCalculations.CalculateAgility() - _rightUnitCalculations.CalculateAgility();
    }
}