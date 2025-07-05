using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class CombatScene : MarginContainer
{
    //Maybe don't need? Depends on how I show things I guess?
    //private int _currentRange;
    //True = left attack, false = right attack
    private Array<bool> _turnOrder;

    //Left initiates combat
    private TextureRect _leftBackground;
    private CombatUnit _leftSprite;
    private Bar _leftHealthBar;
    private Bar _leftManaBar;

    private int _leftName;
    private ActiveSkill _leftSkill;

    private int _leftMaxHealth;
    private int _leftMaxMana;
    private int _leftCurrentHealth;
    private int _leftCurrentMana;
    private int _leftDamage = 0;
    private int _leftAccuracy = 0;
    private int _leftCriticalRate = 0;
    private int _leftCriticalMultiplier = 0;

    //Right retaliates
    private TextureRect _rightBackground;
    private CombatUnit _rightSprite;
    private Bar _rightHealthBar;
    private Bar _rightManaBar;

    private int _rightName;
    private ActiveSkill _rightSkill;

    private int _rightMaxHealth;
    private int _rightMaxMana;
    private int _rightCurrentHealth;
    private int _rightCurrentMana;
    private int _rightDamage = 0;
    private int _rightAccuracy = 0;
    private int _rightCriticalRate = 0;
    private int _rightCriticalMultiplier = 0;

    public override void _Ready()
    {
        _leftBackground = (TextureRect) GetNode("Backgrounds/LeftBackground");
        _leftSprite = (CombatUnit) GetNode("CombatUnits/LeftCombatUnit");
        _leftHealthBar = (Bar) GetNode("LeftBars/HealthBar");
        _leftManaBar = (Bar) GetNode("LeftBars/ManaBar");

        _rightBackground = (TextureRect) GetNode("Backgrounds/RightBackground");
        _rightSprite = (CombatUnit) GetNode("CombatUnits/RightCombatUnit");
        _rightHealthBar = (Bar) GetNode("RightBars/HealthBar");
        _rightManaBar = (Bar) GetNode("RightBars/ManaBar");

        EventBus.Instance.setUpLeftNumbers += SetUpLeftNumbers;
        EventBus.Instance.setUpRightNumbers += SetUpRightNumbers;
    }

    public void SetUpLeftUnit(string name, ActiveSkill skill, string background, UnitStats stats){
        //add other stuff here later

        _leftSkill = skill;

        _leftMaxHealth = stats.Health;
        _leftMaxMana = stats.Mana;
        _leftCurrentHealth = stats.CurrentHealth;
        _leftCurrentMana = stats.CurrentMana;

        _leftHealthBar.Setup("HP: ", _leftMaxHealth, _leftCurrentHealth);
        _leftManaBar.Setup("MP: ", _leftMaxMana, _leftCurrentMana);
    }

    public void SetUpRightUnit(string name, ActiveSkill skill, string background, UnitStats stats){
        //add other stuff here later

        _rightSkill = skill;

        _rightMaxHealth = stats.Health;
        _rightMaxMana = stats.Mana;
        _rightCurrentHealth = stats.CurrentHealth;
        _rightCurrentMana = stats.CurrentMana;

        _rightHealthBar.Setup("HP: ", _rightMaxHealth, _rightCurrentHealth);
        _rightManaBar.Setup("MP: ", _rightMaxMana, _rightCurrentMana);
    }

    public void SetUpLeftNumbers(int damage, int accuracy, int criticalRate, int criticalMultiplier){
        _leftDamage = damage;
        _leftAccuracy = accuracy;
        _leftCriticalRate = criticalRate;
        _leftCriticalMultiplier = criticalMultiplier;
    }

    public void SetUpRightNumbers(int damage, int accuracy, int criticalRate, int criticalMultiplier){
        _rightDamage = damage;
        _rightAccuracy = accuracy;
        _rightCriticalRate = criticalRate;
        _rightCriticalMultiplier = criticalMultiplier;
    }

    public async Task StartCombat(Array<bool> turnOrder){
        Visible = true;

        //turn = true for left, false = right
        foreach(bool turn in turnOrder){
            if(turn){
                GD.Print("LEFT ATTACK");
                await LeftAttack();

                if(_rightCurrentHealth <= 0){
                    _ = EndCombat();
                    return;
                }
            }
            else{
                GD.Print("RIGHT COUNTER");
                await RightAttack();

                if(_leftCurrentHealth <= 0){
                    _ = EndCombat();
                    return;
                }
            }
        }

        _ = EndCombat();
    }

    public override void _Process(double delta)
    {
        
    }

    public async Task WaitForLeftAction(){
        await ToSignal(_rightHealthBar, "");
    }

    public async Task WaitForRightAction(){
        await ToSignal(_leftHealthBar, "");
    }

    public async Task LeftAttack(){
        int remainingMP = _leftManaBar.Value - _leftSkill.Cost;
        if(remainingMP < 0){
            GD.Print("Left Attack Mana cost fail");
            return;
        }

        GD.Print("Left mana cost");
        await _leftManaBar.ChangeValueAsync(remainingMP);

        _leftCurrentMana = remainingMP;

        Random rand = new Random();

        int damage = _leftDamage;

        if(rand.Next(100) <= _leftAccuracy){
            if(rand.Next(100) < _leftCriticalRate){
                GD.Print("LEFT CRIT");
                damage *= _leftCriticalMultiplier;
            }

            GD.Print("Left successful attack");
            GD.Print(damage);
            await _rightHealthBar.ChangeValueAsync(_rightCurrentHealth - damage);

            _rightCurrentHealth -= damage;
        }
        else{
            GD.Print("Left attack miss");
        }
    }

    public async Task RightAttack(){
        int remainingMP = _rightCurrentMana - _rightSkill.Cost;
        if(remainingMP < 0){
            GD.Print("Right Attack Mana cost fail");
            return;
        }

        GD.Print("Right mana cost");
        await _rightManaBar.ChangeValueAsync(remainingMP);

        _rightCurrentMana = remainingMP;

        Random rand = new Random();

        int damage = _rightDamage;

        if(rand.Next(100) <= _rightAccuracy){
            if(rand.Next(100) < _rightCriticalRate){
                GD.Print("RIGHT CRIT");
                damage *= _rightCriticalMultiplier;
            }

            GD.Print("Right successful attack");
            GD.Print(damage);
            await _leftHealthBar.ChangeValueAsync(_leftCurrentHealth - damage);

            _leftCurrentHealth -= damage;
        }
        else{
            GD.Print("Right attack miss");
        }
    }

    public async Task EndCombat(){
        GD.Print("End Combat");

        await ToSignal(GetTree().CreateTimer(1.5), "timeout");

        Hide();
        SetProcess(false);

        //End combat and return the bar values

        EventBus.Instance.EmitSignal("exitCombat", _leftCurrentHealth, _leftCurrentMana, _rightCurrentHealth, _rightCurrentMana);
    }
}
