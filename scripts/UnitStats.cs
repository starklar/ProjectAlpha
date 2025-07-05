using Godot;
using System;
using System.Collections.Generic;

public partial class UnitStats : Resource {
    public int Health { get; set; }
    public int Mana { get; set; }
    public int Strength { get; set; } //Affects physical damage
    public int Magic { get; set; } //Affects magical damage
    public int Dexterity { get; set; } //Affects hit and crit rates
    public int Agility { get; set; } //5 >= to follow up
    public int Luck { get; set; } //Affects avoid and crit dodge rates
    public int Endurance { get; set; } //Affects physical damage taken
    public int Spirit { get; set; } //Affetcs magical damage taken
    public int Movement { get; set; }

    public int CurrentHealth { get; set; }
    public int CurrentMana { get; set; }
    public List<int> Affinities { get; set; } //Should affect skill power (+/- 1), accuracy (+/- 10), and cost (+/- 15%) based on level, maybe alternate to +/- 9 level, for a max of 3 power, 30 accuracy, 45% cost?
    public List<int> Resistances { get; set; } //Should affect skill power, Weak: 3x, Strong: 0.5x, Null: 0x, being hit by weak should remove follow up
    //Resistances: 0 = weak, 1 = neutral, 2 = Strong, 3 = Null

    private int strengthModifier = 0;
    private int magicModifier = 0;
    private int dexterityModifier = 0;
    private int agilityModifier = 0;
    private int luckModifier = 0;
    private int enduranceModifier = 0;
    private int spiritModifier = 0;
    private int movementModifier = 0;

    public int StrengthModifier {
        get{ return strengthModifier; }
        set{
            if(strengthModifier >= 0 && value >= 0){
                if(strengthModifier < value){
                    strengthModifier = value;
                }
            }
            else if(strengthModifier <= 0 && value <= 0){
                if(strengthModifier > value){
                    strengthModifier = value;
                }
            }
            else{
                strengthModifier = value;
            }
        } 
    }
    public int MagicModifier {
        get{ return magicModifier; }
        set{
            if(magicModifier >= 0 && value >= 0){
                if(magicModifier < value){
                    magicModifier = value;
                }
            }
            else if(magicModifier <= 0 && value <= 0){
                if(magicModifier > value){
                    magicModifier = value;
                }
            }
            else{
                magicModifier = value;
            }
        } 
    }
    public int DexterityModifier {
        get{ return dexterityModifier; }
        set{
            if(dexterityModifier >= 0 && value >= 0){
                if(dexterityModifier < value){
                    dexterityModifier = value;
                }
            }
            else if(dexterityModifier <= 0 && value <= 0){
                if(dexterityModifier > value){
                    dexterityModifier = value;
                }
            }
            else{
                dexterityModifier = value;
            }
        } 
    }
    public int AgilityModifier {
        get{ return agilityModifier; }
        set{
            if(agilityModifier >= 0 && value >= 0){
                if(agilityModifier < value){
                    agilityModifier = value;
                }
            }
            else if(agilityModifier <= 0 && value <= 0){
                if(agilityModifier > value){
                    agilityModifier = value;
                }
            }
            else{
                agilityModifier = value;
            }
        } 
    }
    public int LuckModifier {
        get{ return luckModifier; }
        set{
            if(luckModifier >= 0 && value >= 0){
                if(luckModifier < value){
                    luckModifier = value;
                }
            }
            else if(luckModifier <= 0 && value <= 0){
                if(luckModifier > value){
                    luckModifier = value;
                }
            }
            else{
                luckModifier = value;
            }
        } 
    }
    public int EnduranceModifier {
        get{ return enduranceModifier; }
        set{
            if(enduranceModifier >= 0 && value >= 0){
                if(enduranceModifier < value){
                    enduranceModifier = value;
                }
            }
            else if(enduranceModifier <= 0 && value <= 0){
                if(enduranceModifier > value){
                    enduranceModifier = value;
                }
            }
            else{
                enduranceModifier = value;
            }
        } 
    }
    public int SpiritModifier {
        get{ return spiritModifier; }
        set{
            if(spiritModifier >= 0 && value >= 0){
                if(spiritModifier < value){
                    spiritModifier = value;
                }
            }
            else if(spiritModifier <= 0 && value <= 0){
                if(spiritModifier > value){
                    spiritModifier = value;
                }
            }
            else{
                spiritModifier = value;
            }
        } 
    }
    public int MovementModifier {
        get{ return movementModifier; }
        set{
            if(movementModifier >= 0 && value >= 0){
                if(movementModifier < value){
                    movementModifier = value;
                }
            }
            else if(movementModifier <= 0 && value <= 0){
                if(movementModifier > value){
                    movementModifier = value;
                }
            }
            else{
                movementModifier = value;
            }
        } 
    }

    public void ResetModifiers(){
        strengthModifier = 0;
        magicModifier = 0;
        dexterityModifier = 0;
        agilityModifier = 0;
        luckModifier = 0;
        enduranceModifier = 0;
        spiritModifier = 0;
        movementModifier = 0;
    }

    public void SetZero(){
        Health = 0;
        Mana = 0;
        Strength = 0;
        Magic = 0;
        Dexterity = 0;
        Agility = 0;
        Luck = 0;
        Endurance = 0;
        Spirit = 0;
        Movement = 0;

        Affinities = new List<int>() {1, 2};
        Resistances = new List<int>() {1,2};
    }
}