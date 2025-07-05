using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Unit : Path2D {
    [Signal]
    public delegate void walkFinishedEventHandler();

    private Grid _grid = new Grid();

    [Export]
    public int MoveRange = 5;

    [Export]
    public float MoveSpeed = 1200.0f;

    [Export]
    public Texture2D Skin {
        get { return _skin; }
        set{
            _skin = value;
            LoadNodes();
            _sprite.Texture = value;
        }
    }

    [Export]
    public Vector2 SkinOffset {
        get { return _skinOffset; }
        set{
            _skinOffset = value;
            LoadNodes();
            _sprite.Position = value;
        }
    }

    public Vector2I Cell {
        get { return _cell; }
        set{
            LoadNodes();
            _cell = _grid.Clamp(value);
        }
    }

    public bool IsSelected {
        get { return _isSelected; }
        set{
            _isSelected = value;
            LoadNodes();
            if(_isSelected){
                _animPlayer.Play("selected");
            }
            else{
                _animPlayer.Play("idle");
            }
        }
    }

    public bool IsWalking {
        get { return _isWalking; }
        set{
            _isWalking = value;
            SetProcess(_isWalking);
        }
    }

    private Texture2D _skin;
    private Vector2 _skinOffset = Vector2.Zero;
    private Vector2I _cell = Vector2I.Zero;
    private bool _isSelected = false;
    private bool _isWalking = false;

    private Sprite2D _sprite;
    private AnimationPlayer _animPlayer;
    private PathFollow2D _pathFollow;

    //Indexes for the skills to pull from later as needed
    public int BasicAttack { get; set; }
    public Array<int> ActiveSkills { get; set;}
    public Array<int> PassiveSkills { get; set; }

    //-1 = basic attack
    //0-2 = active skills
    public int EquipedSkill { get; set; }

    public UnitStats Stats { get; set; }
    //public CombatCalculations Calculations { get; set; }
    public string UnitName { get; set; }

    //public Vector2I MapPosition = new Vector2I(0, 0);
    

    private void LoadNodes(){
        if(_sprite == null){
            _sprite = GetNode("PathFollow2D/Sprite") as Sprite2D;
        }
        if(_animPlayer == null){
		    _animPlayer = GetNode("UnitAnimations") as AnimationPlayer;
        }
        if(_pathFollow == null){
		    _pathFollow = GetNode("PathFollow2D") as PathFollow2D;
        }
    }

    public override void _Ready(){
        _grid = new Grid();

		LoadNodes();
    
        SetProcess(false);

        _animPlayer.Play("idle");
        _pathFollow.Rotates = false;

        if(!Engine.IsEditorHint()){
            Curve = new Curve2D();
        }

        EquipedSkill = BasicAttack;

        Stats = new UnitStats();

        Stats.SetZero();

        AddToGroup("CombatMapNodes");
    }

    public override void _Process(double delta){
        if(_pathFollow == null){
            _pathFollow = GetNode("PathFollow2D") as PathFollow2D;
        }
        _pathFollow.Progress += (float) (MoveSpeed * delta);

        if(_pathFollow.ProgressRatio >= 1.0f){
            _isWalking = false;
            _pathFollow.Progress = 0.00001f;
            Position = _grid.CalculateMapPosition(Cell);
            Curve.ClearPoints();
            EmitSignal("walkFinished");
            SetProcess(false);
        }
    }

    public void Place(Vector2I cell){
        Position = _grid.CalculateMapPosition(cell);
    }

    public void WalkAlong(Array<Vector2I> path){
        if (path.Count == 0){
			return;
		}

		Curve.AddPoint(Vector2.Zero);
		
		foreach (Vector2I point in path){
			Curve.AddPoint(_grid.CalculateMapPosition(point) - Position);
		}

		Cell = path[^1];
		_isWalking = true;
        SetProcess(true);
    }

    public Godot.Collections.Dictionary<string, string> Save(){
        return new Godot.Collections.Dictionary<string, string>()
        {
            { "UnitName", UnitName },
            { "PosX", Cell.X.ToString() }, // Vector2 is not supported by JSON
            { "PosY", Cell.Y.ToString() }, //Temporary for now, probably remove later once map positions implemented
            { "Health", Stats.Health.ToString() },
            { "Mana", Stats.Mana.ToString() },
            { "Strength", Stats.Strength.ToString() },
            { "Magic", Stats.Magic.ToString() },
            { "Dexterity", Stats.Dexterity.ToString() },
            { "Agility", Stats.Agility.ToString() },
            { "Luck", Stats.Luck.ToString() },
            { "Endurance", Stats.Endurance.ToString() },
            { "Spirit", Stats.Spirit.ToString() },
            { "Movement", Stats.Movement.ToString() },
            { "Affinities", string.Join(",", Stats.Affinities) },
            { "Resistances", string.Join(",", Stats.Resistances) },
            { "BasicAttack",  BasicAttack.ToString() },
            { "ActiveSkills", string.Join(",", ActiveSkills) + "," },
            { "PassiveSkills", string.Join(",", PassiveSkills) + "," },
            { "EquipedSkill",  EquipedSkill.ToString() },
        };
    }

    //Will need to add safety to this later down the line
    public void Load(Godot.Collections.Dictionary<string, string> unitData){
        UnitName = unitData["UnitName"];
        
        //Cell = new Vector2I(unitData["PosX"].ToInt(), unitData["PosY"].ToInt());
        Cell = new Vector2I(unitData["PosX"].ToInt(), unitData["PosY"].ToInt());
        Position = _grid.CalculateMapPosition(Cell);

        Stats.Health = unitData["Health"].ToInt();
        Stats.CurrentHealth = unitData["Health"].ToInt();
        Stats.Mana = unitData["Mana"].ToInt();
        Stats.CurrentMana = unitData["Mana"].ToInt();
        Stats.Strength = unitData["Strength"].ToInt();
        Stats.Magic = unitData["Magic"].ToInt();
        Stats.Dexterity = unitData["Dexterity"].ToInt();
        Stats.Agility = unitData["Agility"].ToInt();
        Stats.Luck = unitData["Luck"].ToInt();
        Stats.Endurance = unitData["Endurance"].ToInt();
        Stats.Dexterity = unitData["Dexterity"].ToInt();
        Stats.Spirit = unitData["Spirit"].ToInt();
        Stats.Movement = unitData["Movement"].ToInt();

        string affinityString = unitData["Affinities"];
        Stats.Affinities = System.Array.ConvertAll(affinityString.Split(','), int.Parse).ToList();

        string resistancesString = unitData["Resistances"];
        Stats.Resistances = System.Array.ConvertAll(resistancesString.Split(','), int.Parse).ToList();

        BasicAttack = unitData["BasicAttack"].ToInt();
        string activeSkillsString = unitData["ActiveSkills"];
        ActiveSkills = new Array<int>{};
        if(activeSkillsString.Contains(',')){
            ActiveSkills.AddRange(System.Array.ConvertAll(activeSkillsString.Split(','), int.Parse));
        }
        else if(activeSkillsString.Length > 0){
            ActiveSkills.Add(activeSkillsString.ToInt());
        }
        string passiveSkillsString = unitData["PassiveSkills"];
        PassiveSkills = new Array<int>{};
        if(passiveSkillsString.Contains(',')){
            PassiveSkills.AddRange(System.Array.ConvertAll(passiveSkillsString.Split(','), int.Parse));
        }
        else if(passiveSkillsString.Length > 0){
            PassiveSkills.Add(passiveSkillsString.ToInt());
        }

        Skin = GD.Load<Texture2D>("res://" + UnitName + ".png");

        EquipedSkill = unitData["EquipedSkill"].ToInt();
    }
}