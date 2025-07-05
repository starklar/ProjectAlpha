using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

public partial class GameBoard: Node2D{
    private static readonly Vector2[] DIRECTIONS = { Vector2.Left, Vector2.Right, Vector2.Up, Vector2.Down };

    private enum GAME_STATES { 
        PLAYER_TURN_START,
        ENEMY_TURN_START,
        MAIN_MENU,
        UNIT_MENU,
        SKILL_MENU,
        SKILL_FORECAST,
        PLAYER_TURN,
        ENEMY_TURN,
        MOVING,
        TARGETING,
        COMBAT,
        SUPPORT_ANIMATION,
        ENEMY_COMBAT,
        ENEMY_SUPPORT_ANIMATION
    };
    private GAME_STATES _gameState;

    [Export]
    public Resource Grid;

    public Unit ActiveUnit { get; set; }
    public Unit TargetUnit { get; set; }

    private Node _playerUnitsNode;
    private Node _enemyUnitsNode;
    private Godot.Collections.Dictionary<Vector2, Unit> _playerUnits = new Godot.Collections.Dictionary<Vector2, Unit>();
    private Godot.Collections.Dictionary<Vector2, Unit> _enemyUnits = new Godot.Collections.Dictionary<Vector2, Unit>();
    private Array<Vector2I> _walkableCells;
    private Array<Vector2I> _targetableCells;

    private UnitOverlay _unitOverlay;
    private UnitPath _unitPath;
    private Grid _grid;
    private UnitMenu _unitMenu;
    private UseSkillMenu _useSkillMenu;
    private CombatScene _combatScene;

    private Vector2 _previousCell;
    private Vector2I _cellToMoveTo;
    private Vector2I _currentCell;

    private ActiveSkill _currentSkill;

    private CombatCalculations _combatCalculations;

    [Export]
    private int _doubleThreshold = 5;

    //private SaveData _saveData;

    public override void _Ready(){
        _grid = (Grid) Grid;
        _unitOverlay = GetNode("UnitOverlay") as UnitOverlay;
        _unitPath = GetNode("UnitPath") as UnitPath;
        _playerUnitsNode = GetNode("PlayerUnits");
        _enemyUnitsNode = GetNode("EnemyUnits");
        _unitMenu = GetNode("UnitMenu") as UnitMenu;
        _useSkillMenu = GetNode("UseSkillMenu") as UseSkillMenu;
        _combatScene = GetNode("CombatScene") as CombatScene;
        //Reinitialize();

        EventBus.Instance.acceptPressed += OnCursorAcceptPressed;
        EventBus.Instance.moved += OnCursorMoved;

        EventBus.Instance.unitMenuButtonPressed += OnUnitMenuButtonPressed;
        EventBus.Instance.useSkillMenuButtonPressed += OnUseSkillMenuButtonPressed;

        EventBus.Instance.initializeBoard += InitializeBoard;

        EventBus.Instance.exitCombat += ExitCombat;

        _combatCalculations = new CombatCalculations();



        //EventBus.Instance.EmitSignal("saveGame", "savefile1");
        /*GD.Print("Start");
        EventBus.Instance.EmitSignal("loadGame", "savefile1");
        GD.Print("End");*/
    }

    //Deal with later
    /*public override void _UnhandledInput(InputEvent inEvent){
        if(_activeUnit != null && inEvent.IsActionPressed("ui_cancel")){
            DeselectActiveUnit();
            ClearActiveUnit();
        }
    }*/

    private string _GetConfigurationWarning(){
        string warning = "";
        if(Grid == null){
            warning = "Grid resource missing";
        }

        return warning;
    }

    private bool IsOccupiedByPlayer(Vector2 cell){
        return _playerUnits.ContainsKey(cell);
    }

    private bool IsOccupiedByEnemy(Vector2 cell){
        return _enemyUnits.ContainsKey(cell);
    }

    private bool IsOccupied(Vector2 cell){
        return _playerUnits.ContainsKey(cell) || _enemyUnits.ContainsKey(cell);
    }

    private void InitializeBoard(){
        _playerUnits.Clear();
        _enemyUnits.Clear();

        foreach(Node child in GetNode("PlayerUnits").GetChildren()){
            if (child is not Unit unit)
            {
                continue;
            }
            _playerUnits[unit.Cell] = unit;
        }

        foreach(Node child in GetNode("EnemyUnits").GetChildren()){
            if (child is not Unit unit)
            {
                continue;
            }
            _enemyUnits[unit.Cell] = unit;
        }

        _gameState = GAME_STATES.PLAYER_TURN;
    }

    private Array<Vector2I> MoveFloodFill(Vector2I cell, int maxDistance){
        Array<Vector2I> list = new Array<Vector2I>();
        Queue<Vector2I> cellStack = new Queue<Vector2I>();
        cellStack.Enqueue(cell);
        Queue<int> distStack = new Queue<int>();
        distStack.Enqueue(0);

        while(cellStack.Count > 0){
            Vector2I curr = cellStack.Dequeue();
            int currDist = distStack.Dequeue();

            if(!_grid.IsWithinBounds(curr) || list.Contains(curr)){
                continue;
            }

            if(currDist > maxDistance){
                continue;
            }

            list.Add(curr);

            foreach(Vector2I dir in DIRECTIONS.Select(v => (Vector2I)v))
            {
                Vector2I coord = curr + dir;
                if(IsOccupiedByEnemy(coord) || list.Contains(coord) || cellStack.Contains(coord)){
                    continue;
                }

                distStack.Enqueue(currDist + 1);
                cellStack.Enqueue(coord);
            }
        }

        return list;
    }

    private void DeselectUnit(){
        
    }

    private async void MoveActiveUnit(Vector2I newCell){
        if((IsOccupied(newCell) && ActiveUnit.Cell != newCell) || !_walkableCells.Contains(newCell)){
            return;
        }

        if(ActiveUnit.Cell != newCell){
            _playerUnits.Remove(ActiveUnit.Cell);
            ActiveUnit.WalkAlong(_unitPath.CurrentPath);
            await ToSignal(ActiveUnit, "walkFinished");
        }
        _currentCell = newCell;

        _unitOverlay.Hide();

        //Change later
        Array<string> options = new Array<string>{ "Attack", "Wait" };
        //Change later once map details is up and running
        if(false){
            options.Add("Special"); // Change eventually once I get this sorted out
        }
        if(HasSupportSkills()){
            options.Add("Support");
        }

        EventBus.Instance.EmitSignal("enterUnitMenu", options);

        EventBus.Instance.EmitSignal("enableCursor", false);

        _gameState = GAME_STATES.UNIT_MENU;
    }

    private bool HasSupportSkills(){
        foreach(int skill in ActiveUnit.ActiveSkills){
            int type = GlobalVariables.Instance.ActiveSkillList[skill].Type;

            if(type >= 7){
                return true;
            }
        }

        return false;
    }

    private void PlaceActiveUnit(){
        DeselectActiveUnit();
        _unitOverlay.Visible = true;
        _playerUnits.Remove(ActiveUnit.Cell);
        _playerUnits[_currentCell] = ActiveUnit;
        ClearActiveUnit();
    }

    private void SelectUnit(Vector2 cell){
        if(!_playerUnits.ContainsKey(cell)){
            return;
        }

        _previousCell = cell;
        ActiveUnit = _playerUnits[cell];
        ActiveUnit.IsSelected = true;
        _walkableCells = MoveFloodFill(ActiveUnit.Cell, ActiveUnit.MoveRange);
        _unitOverlay.Draw(_walkableCells, 0);
        _unitPath.Initialize(_walkableCells);
    }

    private void DeselectActiveUnit(){
        ActiveUnit.IsSelected = false;
        _unitOverlay.Clear();
        _unitPath.Stop();
        SetProcessInput(true);
    }

    private void ClearActiveUnit(){
        ActiveUnit = null;
        _walkableCells.Clear();
    }

    private void ExitCombat(int leftHP, int leftMP, int rightHP, int rightMP){
        ActiveUnit.Stats.CurrentHealth = leftHP;
        ActiveUnit.Stats.CurrentMana = leftMP;
        TargetUnit.Stats.CurrentHealth = rightHP;
        TargetUnit.Stats.CurrentMana = rightMP;

        if(ActiveUnit.Stats.CurrentHealth <= 0){
            //Kill unit
            foreach(KeyValuePair<Vector2, Unit> unit in _playerUnits){
                if(unit.Value == ActiveUnit){
                    _playerUnits.Remove(unit.Key);
                }
            }
            _playerUnitsNode.RemoveChild(ActiveUnit);
        }

        if(TargetUnit.Stats.CurrentHealth <= 0){
            //Kill Unit
            foreach(KeyValuePair<Vector2, Unit> unit in _enemyUnits){
                if(unit.Value == TargetUnit){
                    _enemyUnits.Remove(unit.Key);
                }
            }
            _enemyUnitsNode.RemoveChild(TargetUnit);
            TargetUnit = null;
            EventBus.Instance.EmitSignal("hideMapUI");
        }
        else{
            ShowMapUI();
        }

        ReturnToDefault();

        ShowMap();
    }

    private void EmitLeftCombatCalculations(){
        int damage = _combatCalculations.LeftAttack();
        int hitrate = _combatCalculations.LeftHitRate();
        int criticalRate = _combatCalculations.LeftCritRate();

        //Probably need to change this later
        int criticalMultiplier = 3;

        EventBus.Instance.EmitSignal("setUpLeftNumbers", damage, hitrate, criticalRate, criticalMultiplier);
    }

    private void EmitRightCombatCalculations(){
        int damage = _combatCalculations.RightAttack();
        int hitrate = _combatCalculations.RightHitRate();
        int criticalRate = _combatCalculations.RightCritRate();

        //Probably need to change this later
        int criticalMultiplier = 3;

        EventBus.Instance.EmitSignal("setUpRightNumbers", damage, hitrate, criticalRate, criticalMultiplier);
    }

    private void OnCursorCancelPressed(){
        /*
        private enum GAME_STATES { 
            PLAYER_TURN_START,
            ENEMY_TURN_START,
            MAIN_MENU,
            UNIT_MENU,
            SKILL_MENU,
            SKILL_FORECAST,
            PLAYER_TURN,
            ENEMY_TURN,
            MOVING,
            TARGETING,
            COMBAT,
            SUPPORT_ANIMATION,
            ENEMY_COMBAT,
            ENEMY_SUPPORT_ANIMATION
        };
        */
        if(_gameState == GAME_STATES.MOVING || _gameState == GAME_STATES.MAIN_MENU){
            _gameState = GAME_STATES.PLAYER_TURN;
        }
        else if(_gameState == GAME_STATES.UNIT_MENU){
            _gameState = GAME_STATES.MOVING;
        }
        else if(_gameState == GAME_STATES.TARGETING){
            _gameState = GAME_STATES.SKILL_MENU;
        }
    }

    private void OnCursorAcceptPressed(Vector2I cell){
        if(_gameState == GAME_STATES.PLAYER_TURN){
            SelectUnit(cell);

            _gameState = GAME_STATES.MOVING;
        }
        else if(_gameState == GAME_STATES.MOVING){
            MoveActiveUnit(cell);

            _gameState = GAME_STATES.SKILL_MENU;
        }
        else if(_gameState == GAME_STATES.TARGETING){
            if(_targetableCells.Contains(cell)){
                int type = _currentSkill.Type;

                if(type == GlobalVariables.HEAL_TYPE_INDEX || type == GlobalVariables.SUPPORT_TYPE_INDEX){
                    if(IsOccupiedByPlayer(cell)){
                        //Support skill target allies
                        //TODO
                        TargetUnit = _playerUnits[cell];
                        GD.Print("SUPPORT ALLIES");

                        ReturnToDefault();
                    }
                }
                else if(_currentSkill.Type == GlobalVariables.MOVEMENT_TYPE_INDEX){
                    if(IsOccupiedByPlayer(cell)){
                        //TODO: Check for ending space is clear here too
                    }
                }
                else if(_currentSkill.Type == GlobalVariables.DISRUPT_TYPE_INDEX){
                    if(IsOccupiedByEnemy(cell)){
                        //Support skill target enemies
                        TargetUnit = _enemyUnits[cell];
                        GD.Print("SUPPORT ENEMIES");
                        ReturnToDefault();
                    }
                }
                else if(IsOccupiedByEnemy(cell)){
                    //Combat with enemy
                    //TODO
                    TargetUnit = _enemyUnits[cell];

                    int currentRange = Math.Abs(ActiveUnit.Cell.X - cell.X) + Math.Abs(ActiveUnit.Cell.Y - cell.Y);
                    
                    ActiveSkill targetSkill = GlobalVariables.Instance.ActiveSkillList[TargetUnit.EquipedSkill];
                    
                    _combatCalculations.SetUp(ActiveUnit, _currentSkill, TargetUnit, currentRange);

                    _combatScene.SetUpLeftUnit(ActiveUnit.Name, _currentSkill, "background", ActiveUnit.Stats);
                    _combatScene.SetUpRightUnit(TargetUnit.Name, targetSkill, "background", TargetUnit.Stats);
                    
                    EmitLeftCombatCalculations();
                    EmitRightCombatCalculations();

                    //turn = true for left, false = right
                    Array<bool> turnOrder = new Array<bool>
                    {
                        true
                    };

                    bool targetCanRetaliate = targetSkill.MaxRange >= currentRange && targetSkill.MinRange <= currentRange;

                    if(targetCanRetaliate){
                        turnOrder.Add(false);
                    }

                    int speedDiff = _combatCalculations.SpeedDifference();

                    if(speedDiff >= _doubleThreshold){
                        turnOrder.Add(true);
                    }
                    else if(targetCanRetaliate && speedDiff <= -_doubleThreshold){
                        turnOrder.Add(false);
                    }

                    //Maybe passives here for turn order changing

                    HideMap();

                    _combatScene.StartCombat(turnOrder);

                    _gameState = GAME_STATES.COMBAT;
                }
                else{
                    return;
                }

                _gameState = GAME_STATES.SKILL_FORECAST;
            }
        }
    }

    private void HideMap(){
        GetTree().CallGroup("CombatMapNodes", "hide");
        EventBus.Instance.EmitSignal("hideMapUI");
    }

    private void ShowMap(){
        GetTree().CallGroup("CombatMapNodes", "show");
    }

    private void ReturnToDefault(){
        PlaceActiveUnit();
        _currentSkill = null;
        _walkableCells = null;
        _targetableCells = null;
        EventBus.Instance.EmitSignal("exitUseSkillMenu");
        EventBus.Instance.EmitSignal("exitUnitMenu");
        
        ActiveUnit = null;
        TargetUnit = null;

        _gameState = GAME_STATES.PLAYER_TURN;
    }

    private void ShowMapUI(){
        string name = TargetUnit.UnitName;
        string hp = TargetUnit.Stats.CurrentHealth + "/" + TargetUnit.Stats.Health;
        string mp = TargetUnit.Stats.CurrentMana + "/" + TargetUnit.Stats.Mana;
        string resists = "";
        foreach(int res in TargetUnit.Stats.Resistances){
            resists += GlobalVariables.RESISTANCE_LABELS[res] + ",";
        }

        ActiveSkill skill = GlobalVariables.Instance.ActiveSkillList[TargetUnit.EquipedSkill];

        string skillName = skill.Name;
        //string skillType = GlobalVariables.SKILL_TYPES[skill.Type];
        string affinityBonus = "";
        int bonus = TargetUnit.Stats.Affinities[skill.Type];
        if(bonus < 0){
            affinityBonus += "-" + bonus;
        }
        else if(bonus > 0){
            affinityBonus += "+" + bonus;
        }

        EventBus.Instance.EmitSignal("setUpMapUI", name, hp, mp, resists, skillName, affinityBonus);
    }

    private void OnCursorMoved(Vector2I newCell){
        if(_gameState == GAME_STATES.MOVING){
            _unitPath.Draw(ActiveUnit.Cell, newCell);
        }

        if(_gameState == GAME_STATES.PLAYER_TURN || _gameState == GAME_STATES.TARGETING || _gameState == GAME_STATES.MOVING){
            if(_playerUnits.ContainsKey(newCell)){
                TargetUnit = _playerUnits[newCell];
            }
            else if(_enemyUnits.ContainsKey(newCell)){
                TargetUnit = _enemyUnits[newCell];
            }
            else if(ActiveUnit != null){
                TargetUnit = ActiveUnit;
            }
            else{
                TargetUnit = null;
                EventBus.Instance.EmitSignal("hideMapUI");

                return;
            }

            ShowMapUI();
        }
    }

    private void OnUnitMenuButtonPressed(string option){
        GD.Print(_gameState);
        if(option == "Attack"){
            EventBus.Instance.EmitSignal("enterUseSkillMenu", ActiveUnit.BasicAttack, ActiveUnit.ActiveSkills);
            _unitMenu.Hide();
            _unitPath.Hide();

            _gameState = GAME_STATES.SKILL_MENU;
        }
        else if(option == "Support"){
            EventBus.Instance.EmitSignal("enterUseSkillMenu", -1, ActiveUnit.ActiveSkills);
            _unitMenu.Hide();

            _gameState = GAME_STATES.SKILL_MENU;
        }
        else if(option == "Set"){
            //TODO
            //_gameState = GAME_STATES.
        }
        else if(option == "Wait"){
            EventBus.Instance.EmitSignal("exitUnitMenu");
            PlaceActiveUnit();
            EventBus.Instance.EmitSignal("enableCursor", true);

            _gameState = GAME_STATES.PLAYER_TURN;
        }
        else{   //For all the special map interaction stuff

        }
        GD.Print(_gameState);
    }

    private void OnUseSkillMenuButtonPressed(int option){
        _useSkillMenu.Hide();
        int index;

        if(option == -1){
            index = ActiveUnit.BasicAttack;
            _currentSkill = GlobalVariables.Instance.ActiveSkillList[index];
        }
        else{
            index = ActiveUnit.ActiveSkills[option];
        }

        _currentSkill = GlobalVariables.Instance.ActiveSkillList[index];

        int type = 2;
        if(_currentSkill.Type < GlobalVariables.HEAL_TYPE_INDEX){
            //Any attacking type move
            type = 1;
        }

        _targetableCells = TargettingFloodFill(ActiveUnit.Cell);
        _unitOverlay.Draw(_targetableCells, type);
        _unitOverlay.Visible = true;

        EventBus.Instance.EmitSignal("enableCursor", true);

        _gameState = GAME_STATES.TARGETING;
    }

    private Array<Vector2I> TargettingFloodFill(Vector2I cell){
        Array<Vector2I> list = new Array<Vector2I>();
        Queue<Vector2I> cellStack = new Queue<Vector2I>();
        cellStack.Enqueue(cell);
        Queue<int> distStack = new Queue<int>();
        distStack.Enqueue(0);

        while(cellStack.Count > 0){
            Vector2I curr = cellStack.Dequeue();
            int currDist = distStack.Dequeue();

            if(!_grid.IsWithinBounds(curr) || list.Contains(curr)){
                continue;
            }

            if(currDist > _currentSkill.MaxRange){
                return list;
            }

            if(currDist >= _currentSkill.MinRange){
                list.Add(curr);
            }

            foreach(Vector2I dir in DIRECTIONS.Select(v => (Vector2I)v))
            {
                Vector2I coord = curr + dir;
                if(list.Contains(coord) || cellStack.Contains(coord)){
                    continue;
                }

                distStack.Enqueue(currDist + 1);
                cellStack.Enqueue(coord);
            }
        }

        return list;
    }
}