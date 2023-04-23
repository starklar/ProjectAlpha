using Godot;
using System;
using System.Collections.Generic;
using UnitInformation;
using Main;
using Data;

using GameData;

namespace Skirmish
{
    public class MainSkirmish : Node
    {
        [Export]
        public PackedScene TerrainHUDScene;

        [Export]
        public PackedScene UnitHUDScene;

        [Export]
        public PackedScene UnitFullHUDScene;

        [Export]
        public PackedScene ActionMenuScene;

        [Export]
        public PackedScene CombatScene;

        [Export]
        public PackedScene MapMenuScene;

        [Export]
        public PackedScene CombatForecastScene;

        [Signal]
        delegate void TerrainCheckSignal(string tile_type, int defence_bonus, int evasion_bonus);

        [Signal]
        delegate void MoveCursorSignal(int new_x, int new_y);

        [Signal]
        delegate void UnitCheckSignal(UnitScene unit);

        [Signal]
        delegate void ShowUnitHUDSignal(bool show);

        [Signal]
        delegate void ShowUnitFullHUDSignal(bool show);

        [Signal]
        delegate void ShowTerrainHUDSignal(bool show);

        [Signal]
        delegate void ShowActionMenuSignal(string show_menu);

        [Signal]
        delegate void ShowMapMenuSignal();

        [Signal]
        delegate void UpdateCombatForecastSignal(int distance, UnitScene unitA, int tileADefence, int tileAEvasion, UnitScene unitB, int tileBDefence, int tileBEvasion);

        [Signal]
        delegate void UpdateUnitFullHUDSignal(UnitScene unit);
        
        [Signal]
        delegate void ShowCombatForecastSignal(bool show);

        [Signal]
        delegate void GetSupportSkillsSignal(UnitScene unit);

        [Signal]
        delegate void StartCombatSignal(int distance, UnitScene unitA, int tileADefence, int tileAEvasion, UnitScene unitB, int tileBDefence, int tileBEvasion);

        private int MaxX = 0;
        private int MaxY = 0;

        private int CurrX = 0;
        private int CurrY = 0;

        private SkirmishMap Map;
        //private Tile[,] Map;
        private List<Pos> SelectableMoveTiles = new List<Pos>();
        private List<Pos> SelectableAttackTiles = new List<Pos>();
        private List<Pos> SelectableSupportTiles = new List<Pos>();
        private List<UnitScene> PlayerTeam = new List<UnitScene>();
        private List<AIUnitScene> EnemyTeam = new List<AIUnitScene>();
        private List<AIUnitScene> OtherTeam = new List<AIUnitScene>();

        private PlayerUnits PlayerUnits;

        private UnitScene SelectedUnit = null;
        private Node2D Cursor;

        //0: Map cursor movement
        //1: Unit Moving
        //2: Unit Combat Forecast Check
        //3: Unit Combat
        //3: Unit Support
        //4: Unit Interacting
        private int Mode;

        //0: Empty
        //1: Attack range
        //2: Support range
        private int EnemyRangeToggle;

        //0: Player Phase
        //1: Enemy Phase
        //2: Other Phase
        private int Phase;
        private int Turn;

        private string AttackRangeTile = "res://Sprites/AttackRangeTile.png";
        private string MoveRangeTile = "res://Sprites/MoveRangeTile.png";
        private string SupportRangeTile = "res://Sprites/SupportRangeTile.png";

        private string EnemyAttackRangeTile = "res://Sprites/AttackRangeTile.png";
        private string EnemySupportRangeTile = "res://Sprites/SupportRangeTile.png";

        public override void _Ready()
        {
            //TESTING PURPOSE ONLY
            //StartSkirmish("Ch1Skirmish", 0, 0, 22, 13);
            //
            //this.GetTree().CurrentScene.Connect("StartSkirmishSignal", this, "StartSkirmish");
        }

        public void StartSkirmish(string map_name, int cursor_start_x, int cursor_start_y, int map_size_x, int map_size_y)
        {
            PlayerUnits = (PlayerUnits) this.GetNode("PlayerData").Call("GetPlayerUnits");
            
            this.GetNode<Camera2D>("Camera2D").AddChild(TerrainHUDScene.Instance());
            this.GetNode<Camera2D>("Camera2D").AddChild(UnitHUDScene.Instance());
            this.GetNode<Camera2D>("Camera2D").AddChild(ActionMenuScene.Instance());
            this.GetNode<Camera2D>("Camera2D").AddChild(CombatForecastScene.Instance());
            AddChild(CombatScene.Instance());
            AddChild(UnitFullHUDScene.Instance());
            this.GetNode<Camera2D>("Camera2D").AddChild(MapMenuScene.Instance());

            var cursor = GetNode<Cursor>("Cursor");

            MaxX = map_size_x;
            MaxY = map_size_y;

            cursor.Start(cursor_start_x, cursor_start_y, map_size_x, map_size_y);
            Cursor = cursor;
            Map = new SkirmishMap(map_name, map_size_x, map_size_y);
            //LoadMap(map_name, map_size_x, map_size_y);
            CreatePlayers();
            CreateEnemies();
            CreateOthers();

            TerrainCheck(cursor_start_x, cursor_start_y);

            EmitSignal("ShowActionMenuSignal", "hide");

            GetNode<Cursor>("Cursor").Connect("CursorMovedSignal", this, "TerrainCheck");
            this.GetNode<Camera2D>("Camera2D").GetNode<ActionMenuScene>("ActionMenuScene").Connect("UnitUndoMoveSignal", this, "SelectedUnitUndoMove");
            this.GetNode<Camera2D>("Camera2D").GetNode<ActionMenuScene>("ActionMenuScene").Connect("UnitWaitSignal", this, "SelectedUnitWait");
            this.GetNode<Camera2D>("Camera2D").GetNode<ActionMenuScene>("ActionMenuScene").Connect("SpawnAttackTilesSignal", this, "SpawnAttackTiles");
            this.GetNode<Camera2D>("Camera2D").GetNode<ActionMenuScene>("ActionMenuScene").Connect("SpawnSupportTilesSignal", this, "SpawnSupportTiles");
            this.GetNode<Camera2D>("Camera2D").GetNode<ActionMenuScene>("ActionMenuScene").Connect("EnableMapMovementSignal", this, "EnableMapMovement");
            GetNode<CombatScene>("CombatScene").Connect("EndCombatSignal", this, "EndCombat");
            GetNode<UnitFullHUDScene>("UnitFullHUDScene").Connect("CloseUnitHUDSignal", this, "ReturnToCursorCamera");
            this.GetNode<Camera2D>("Camera2D").GetNode<MapMenuScene>("MapMenuScene").Connect("ReturnToMapSignal", this, "EnableMapMovement");
            this.GetNode<Camera2D>("Camera2D").GetNode<MapMenuScene>("MapMenuScene").Connect("EndTurnSignal", this, "EndTurn");
        }

        private void TerrainCheck(int x, int y)
        {
            Tile currTile = Map.GetTile(x, y);
            CurrX = x;
            CurrY = y;
            EmitSignal("TerrainCheckSignal", currTile.TileType, currTile.DefenceBonus, currTile.EvasionBonus, currTile.HPHealBonus, currTile.MPHealBonus);
            
            if(currTile.CurrUnit != null)
            {
                EmitSignal("ShowUnitHUDSignal", true);
                EmitSignal("UnitCheckSignal", currTile.CurrUnit);
            }
            else if(SelectedUnit != null)
            {
                EmitSignal("ShowUnitHUDSignal", true);
                EmitSignal("UnitCheckSignal", SelectedUnit);
            }
            else
            {
                EmitSignal("ShowUnitHUDSignal", false);
            }
        }

        public void ChangeTile(int x, int y, string object_name, string tile_name, int defence_bonus, int evasion_bonus, int hp_heal_bonus, int mp_heal_bonus, int allowed_types, int movement_penalty)
        {
            Map.ChangeTile(x, y, object_name, tile_name, defence_bonus, evasion_bonus, hp_heal_bonus, mp_heal_bonus, allowed_types, movement_penalty);

            var animatedSprite = GetNode<AnimatedSprite>(object_name);
            animatedSprite.Play();
        }

        //TESTING PURPOSE ONLY vvv
        private void CreatePlayers()
        {
            //TO BE WORKED ON NEXT
            //PlayerUnits pt = new PlayerUnits();
            //CurrentGameData cgd = new CurrentGameData();

            //cgd.Load("Save1");

            //pt.SaveParty("Save1");

            int i = 0;
            foreach(UnitInfo unit in PlayerUnits.GetParty())
            {
                i += 1;
                PackedScene playerScene = (PackedScene) ResourceLoader.Load("res://Units/" + unit.UnitName + "Scene.tscn");
                var playerUnit = (UnitScene)playerScene.Instance();

                playerUnit.Create(unit, i * 2, i * 2, 0);
                Map.PlaceUnit(i * 2, i * 2, playerUnit);

                GetNode<Node>("PlayerTeam").AddChild(playerUnit);
                PlayerTeam.Add(playerUnit);
            }
        }

        private void CreateEnemies()
        {
            //For testing purposes only
            int enemy_team_size = 2;

            for(int i = 0; i < enemy_team_size; i++)
            {
                PackedScene enemyScene = (PackedScene) ResourceLoader.Load("res://Units/SlimeScene.tscn");
                var enemyUnit = (AIUnitScene)enemyScene.Instance();
                int[] statsList = { 10, 5, 4, 6, 3, 3, 3, 3 };
                string[] resList = { "Wk", "Rs", "Wk", "Wk", "--", "Nu", "--" };
                int[] affList = { -1, -3, 0, 0, 0, 3, 0, 0, 0 };
                BattleSkill atk = new BattleSkill("Attack", "1 hit Physical attack.", 0, 0, 1, 1, 3, 80, 0, (1, 1), false, null);
                ActiveSkill[] bSkills = {};
                PassiveSkill[] pSkills = {};
                enemyUnit.Create(9 - i * 2, 8 - i * 2, "Slime", 1, 2, 2, 0, statsList, atk, bSkills, pSkills, resList, affList, "Basic", "Basic");
                Map.PlaceUnit(9 - i * 2, 8 - i * 2, enemyUnit);

                GetNode<Node>("EnemyTeam").AddChild(enemyUnit);
                EnemyTeam.Add(enemyUnit);
            }
        }

        private void CreateOthers()
        {

        }

        public List<UnitScene> GetPlayerTeam()
        {
            return PlayerTeam;
        }

        public List<AIUnitScene> GetEnemyTeam()
        {
            return EnemyTeam;
        }

        public List<AIUnitScene> GetOtherTeam()
        {
            return OtherTeam;
        }

        //TESTING PURPOSE ONLY ^^^

        private void SpawnMoveTiles()
        {
            var t = GetNode<Node>("Tiles");

            foreach(Pos s in Map.SpawnMoveTiles(SelectedUnit.MovementType, SelectedUnit.Team, SelectedUnit.Stats[2] + SelectedUnit.StatMods[0], SelectedUnit.CurrX, SelectedUnit.CurrY))
            {
                var selectable = new Sprite();
                selectable.Texture = (Texture) GD.Load(MoveRangeTile);
                selectable.Position = new Vector2(s.X * Global.MAP_SCALE + Global.MAP_SCALE / 2, s.Y * Global.MAP_SCALE + Global.MAP_SCALE / 2);
                selectable.ZIndex = 1;
                selectable.Scale = new Vector2(2, 2);
                selectable.Modulate = new Color(new Color(0, 0, 255), 0.1f);
                t.AddChild(selectable);
            }
        }

        private void CheckEnemyRange()
        {
            RemoveEnemyRangeTiles();
            
            if(EnemyRangeToggle == 1)
            {
                EnemyAttackRange();
            }
            else if(EnemyRangeToggle == 2)
            {
                //TODO: UNDO THIS LATER
                //EnemySupportRange();
            }
        }

        private void SpawnAttackTiles()
        {
            var t = GetNode<Node>("Tiles");

            foreach(Pos s in Map.GetAttackRange(SelectedUnit))
            {
                var selectable = new Sprite();
                selectable.Texture = (Texture) GD.Load(AttackRangeTile);
                selectable.Position = new Vector2(s.X * Global.MAP_SCALE + Global.MAP_SCALE / 2, s.Y * Global.MAP_SCALE + Global.MAP_SCALE / 2);
                selectable.ZIndex = 1;
                selectable.Scale = new Vector2(2, 2);
                selectable.Modulate = new Color(new Color(255, 0, 0), 0.1f);
                t.AddChild(selectable);
            }
        }

        private void SpawnSupportTiles(string skill_name)
        {
            var t = GetNode<Node>("Tiles");

            foreach(Pos s in Map.GetSupportRange(SelectedUnit, skill_name))
            {
                var selectable = new Sprite();
                selectable.Texture = (Texture) GD.Load(SupportRangeTile);
                selectable.Position = new Vector2(s.X * Global.MAP_SCALE + Global.MAP_SCALE / 2, s.Y * Global.MAP_SCALE + Global.MAP_SCALE / 2);
                selectable.ZIndex = 1;
                selectable.Scale = new Vector2(2, 2);
                selectable.Modulate = new Color(new Color(0, 205, 0), 0.1f);
                t.AddChild(selectable);
            }
        }

        private void RemoveRangeTiles()
        {
            var t = GetNode<Node>("Tiles");
            foreach(Node tile in t.GetChildren())
            {
                t.RemoveChild(tile);
            }
        }

        private void RemoveEnemyRangeTiles()
        {
            var t = GetNode<Node>("EnemyRangeTiles");
            foreach(Node tile in t.GetChildren())
            {
                t.RemoveChild(tile);
            }
        }

        private void EnableMapMovement(int new_mode)
        {
            EmitSignal("ShowTerrainHUDSignal", true);
            Mode = new_mode;
            SetProcessInput(true);
            Cursor.SetProcessInput(true);
        }

        private void ReturnToCursorCamera()
        {
            EmitSignal("ShowTerrainHUDSignal", true);
            if(Map.GetTile(CurrX, CurrY).CurrUnit != null)
            {
                EmitSignal("ShowUnitHUDSignal", true);
                EmitSignal("UnitCheckSignal", Map.GetTile(CurrX, CurrY).CurrUnit);
            }
            else if(SelectedUnit != null)
            {
                EmitSignal("ShowUnitHUDSignal", true);
                EmitSignal("UnitCheckSignal", SelectedUnit);
            }

            SetProcessInput(true);
            Cursor.SetProcessInput(true);
            GetNode<Camera2D>("Camera2D").Current = true;
        }

        private void SelectedUnitUndoMove()
        {
            Map.RemoveUnit(SelectedUnit.CurrX, SelectedUnit.CurrY);
            SelectedUnit.UndoMove();
            SpawnMoveTiles();
        }

        private void SelectedUnitWait()
        {
            RemoveRangeTiles();
            Wait(SelectedUnit, CurrX, CurrY);
            SelectedUnit = null;
        }

        private void MoveUnit(UnitScene unit, int new_x, int new_y)
        {
            Map.RemoveUnit(unit.PrevX, unit.PrevY);
            Map.PlaceUnit(new_x, new_y, unit);
            unit.Move(new_x, new_y);

            CheckEnemyRange();
        }

        private void Wait(UnitScene unit, int x, int y)
        {
            unit.Place();
            Map.PlaceUnit(x, y, unit);

            CheckEnemyRange();
        }

        private void EnterCombat(UnitScene unit_a, UnitScene unit_b)
        {
            //TODO: MAY NEED TO CHANGE
            Map.GetUnit(unit_a.CurrX, unit_a.CurrY).Place();
            int TileADef = Map.GetTile(unit_a.CurrX, unit_a.CurrY).DefenceBonus;
            int TileAEv = Map.GetTile(unit_a.CurrX, unit_a.CurrY).EvasionBonus;
            int TileBDef = Map.GetTile(unit_b.CurrX, unit_b.CurrY).DefenceBonus;
            int TileBEv = Map.GetTile(unit_b.CurrX, unit_b.CurrY).EvasionBonus;

            int distance = Math.Abs(unit_a.CurrX - unit_b.CurrX) + Math.Abs(unit_a.CurrY - unit_b.CurrY);

            SetProcessInput(false);
            Cursor.SetProcessInput(false);

            EmitSignal("ShowUnitHUDSignal", false);
            EmitSignal("ShowTerrainHUDSignal", false);
            EmitSignal("StartCombatSignal", distance, unit_a, TileADef, TileAEv, unit_b, TileBDef, TileBEv);
        }

        private void EndCombat(UnitScene unit_a, UnitScene unit_b)
        {
            GetNode<Camera2D>("Camera2D").Current = true;
            
            RemoveRangeTiles();
            Map.PlaceUnit(unit_a.CurrX, unit_a.CurrY, unit_a);
            Map.PlaceUnit(unit_b.CurrX, unit_b.CurrY, unit_b);

            if(unit_a.CurrHP == 0)
            {
                //TODO: May need to change
                Map.GetUnit(unit_a.CurrX, unit_a.CurrY).Dead();
                Map.RemoveUnit(unit_a.CurrX, unit_a.CurrY);
                if(unit_a.Team == 0)
                {
                    PlayerTeam.Remove(unit_a);
                }
                else if(unit_a.Team == 1)
                {
                    EnemyTeam.Remove((AIUnitScene) unit_a);
                }
                else if(unit_a.Team == 2)
                {
                    OtherTeam.Remove((AIUnitScene) unit_a);
                }
            }

            if(unit_b.CurrHP == 0)
            {
                //TODO: May need to change
                Map.GetUnit(unit_b.CurrX, unit_b.CurrY).Dead();
                Map.RemoveUnit(unit_b.CurrX, unit_b.CurrY);

                if(unit_b.Team == 0)
                {
                    PlayerTeam.Remove(unit_b);
                }
                else if(unit_b.Team == 1)
                {
                    EnemyTeam.Remove((AIUnitScene) unit_b);
                }
                else if(unit_b.Team == 2)
                {
                    OtherTeam.Remove((AIUnitScene) unit_b);
                }
            }
            else if(unit_a.Team == 0)
            {
                EmitSignal("UnitCheckSignal", unit_b);
                EmitSignal("ShowUnitHUDSignal", true);
            }

            if(unit_a.Team == 0)
            {
                EnableMapMovement(0);
                SelectedUnit = null;
            }

            if(Phase == 1 || Phase == 2)
            {
                CPUMapAI();
            }

            CheckEnemyRange();
        }

        private void CPUMapAI()
        {
            if(Phase == 1)
            {
                foreach (AIUnitScene enemy in EnemyTeam)
                {
                    this.GetNode<Camera2D>("Camera2D").Current = true;

                    if(enemy.HasMoved)
                    {
                        continue;
                    }
                    int new_x = enemy.CurrX;
                    int new_y = enemy.CurrY;
                    //Replace later with actual AI behaviour
                    AiMove move = Map.GetAIMove(enemy);

                    //TODO: Change to traverse path later

                    int idx = move.PathToPos.Count - 1;

                    int endX = move.PathToPos[idx].X;
                    int endY = move.PathToPos[idx].Y;

                    MoveUnit(enemy, endX, endY);

                    if(move.Action == "Attack")
                    {
                        EnterCombat(enemy, Map.GetUnit(move.TargetPos.X, move.TargetPos.Y));
                        return;
                    }
                    else if(move.Action == "Wait")
                    {
                        Wait(enemy, endX, endY);
                    }
                }
            }
            else if(Phase == 2)
            {
                foreach (AIUnitScene other in OtherTeam)
                {
                    if(other.HasMoved)
                    {
                        continue;
                    }

                    int new_x = other.CurrX;
                    int new_y = other.CurrY;
                    //Replace later with actual AI behaviour
                    MoveUnit(other, new_x, new_y);
                    Wait(other, new_x, new_y);
                }
            }

            EndTurn();
        }

        //TODO: needs some minor changes
        private void EnemyAttackRange()
        {
            var t = GetNode<Node>("EnemyRangeTiles");

            foreach(Pos tile in Map.GetEnemyAttackRange())
            {
                var selectable = new Sprite();
                selectable.Texture = (Texture) GD.Load(AttackRangeTile);
                selectable.Position = new Vector2(tile.X * Global.MAP_SCALE + Global.MAP_SCALE / 2, tile.Y * Global.MAP_SCALE + Global.MAP_SCALE / 2);
                selectable.ZIndex = 2;
                selectable.Scale = new Vector2(2, 2);
                selectable.Modulate = new Color(new Color(200, 0, 0), 0.1f);
                t.AddChild(selectable);
            }
        }

        //TODO: needs some minor changes
        private void EnemySupportRange()
        {
            var t = GetNode<Node>("EnemyRangeTiles");

            foreach(Pos tile in Map.GetEnemySupportRange())
            {
                var selectable = new Sprite();
                selectable.Texture = (Texture) GD.Load(SupportRangeTile);
                selectable.Position = new Vector2(tile.X * Global.MAP_SCALE + Global.MAP_SCALE / 2, tile.Y * Global.MAP_SCALE + Global.MAP_SCALE / 2);
                selectable.ZIndex = 2;
                selectable.Scale = new Vector2(2, 2);
                selectable.Modulate = new Color(new Color(0, 200, 0), 0.1f);
                t.AddChild(selectable);
            }
        }

        private void SetAllToIdle()
        {
            if(Phase == 0)
            {
                foreach(UnitScene unit in PlayerTeam)
                {
                    Tile unit_tile = Map.GetTile(unit.CurrX, unit.CurrY);
                    unit.Refresh();
                    unit.StatModsRefresh();
                    unit.TileHeal(unit_tile.HPHealBonus, unit_tile.MPHealBonus);
                }
            }
            else if(Phase == 1)
            {
                foreach(UnitScene unit in EnemyTeam)
                {
                    Tile unit_tile = Map.GetTile(unit.CurrX, unit.CurrY);
                    unit.Refresh();
                    unit.StatModsRefresh();
                    unit.TileHeal(unit_tile.HPHealBonus, unit_tile.MPHealBonus);
                }
            }
            else if(Phase == 2)
            {
                foreach(UnitScene unit in OtherTeam)
                {
                    Tile unit_tile = Map.GetTile(unit.CurrX, unit.CurrY);
                    unit.Refresh();
                    unit.StatModsRefresh();
                    unit.TileHeal(unit_tile.HPHealBonus, unit_tile.MPHealBonus);
                }
            }
        }

        //0: Player Phase
        //1: Enemy Phase
        //2: Other Phase
        private void EndTurn()
        {
            SetAllToIdle();
            Cursor.Visible = false;
            SetProcessInput(false);
            Cursor.SetProcessInput(false);

            this.GetNode<Camera2D>("Camera2D").Current = true;

            if(Phase == 0)
            {
                if(EnemyTeam.Count > 0)
                {
                    Phase = 1;

                    CPUMapAI();
                }
                else if(OtherTeam.Count > 0)
                {
                    Phase = 2;

                    CPUMapAI();
                }
                else
                {
                    Phase = 0;
                    Cursor.Visible = true;
                    SetProcessInput(true);
                    Cursor.SetProcessInput(true);
                }
            }
            else if(Phase == 1)
            {
                if(OtherTeam.Count > 0)
                {
                    Phase = 2;

                    CPUMapAI();
                }
                else if(PlayerTeam.Count > 0)
                {
                    Phase = 0;
                    Cursor.Visible = true;
                    SetProcessInput(true);
                    Cursor.SetProcessInput(true);
                }
                else
                {
                    Phase = 1;

                    CPUMapAI();
                }
            }
            else if(Phase == 2)
            {
                if(PlayerTeam.Count > 0)
                {
                    Phase = 0;
                    Cursor.Visible = true;
                    SetProcessInput(true);
                    Cursor.SetProcessInput(true);
                }
                else if(EnemyTeam.Count > 0)
                {
                    Phase = 1;

                    CPUMapAI();
                }
                else
                {
                    Phase = 2;

                    CPUMapAI();
                }
            }

            if(Map.GetTile(CurrX, CurrY).CurrUnit != null)
            {
                EmitSignal("UnitCheckSignal", Map.GetTile(CurrX, CurrY).CurrUnit);
            }
        }

        public override void _Input(InputEvent inputEvent)
        {
            if (inputEvent.IsActionPressed("grid_select"))
            {
                var map_curr_unit = Map.GetUnit(CurrX, CurrY);
                if(Mode == 0)
                {
                    if(SelectedUnit == null && map_curr_unit != null && map_curr_unit.Team == 0 && map_curr_unit.HasMoved == false)
                    {
                        SelectedUnit = map_curr_unit;
                        SpawnMoveTiles();
                        Map.RemoveUnit(CurrX, CurrY);
                        Mode = 1;
                    }
                }
                else if(Mode == 1)
                {
                    if(SelectedUnit != null)
                    {
                        //TODO: USE PATH
                        List<Pos> path = Map.GetMovementPath(CurrX, CurrY);
                        if(path != null)
                        {
                            MoveUnit(SelectedUnit, CurrX, CurrY);

                            Cursor.SetProcessInput(false);
                            EmitSignal("GetSupportSkillsSignal", SelectedUnit);
                            EmitSignal("ShowActionMenuSignal", "main");
                            RemoveRangeTiles();
                            SetProcessInput(false);
                        }
                    }
                }
                else if(Mode == 2)
                {
                    if(Map.IsInAttackRange(CurrX, CurrY))
                    {
                        if(Map.GetUnit(CurrX, CurrY) != null)
                        {
                            if(Map.GetUnit(CurrX, CurrY).Team == 1)
                            {
                                int TileADef = Map.GetTile(SelectedUnit.CurrX, SelectedUnit.CurrY).DefenceBonus;
                                int TileAEv = Map.GetTile(SelectedUnit.CurrX, SelectedUnit.CurrY).EvasionBonus;
                                int TileBDef = Map.GetTile(CurrX, CurrY).DefenceBonus;
                                int TileBEv = Map.GetTile(CurrX, CurrY).EvasionBonus;

                                int distance = Math.Abs(SelectedUnit.CurrX - CurrX) + Math.Abs(SelectedUnit.CurrY - CurrY);

                                Cursor.SetProcessInput(false);

                                EmitSignal("ShowUnitHUDSignal", false);
                                EmitSignal("ShowTerrainHUDSignal", false);
                                EmitSignal("UpdateCombatForecastSignal", distance, SelectedUnit, TileADef, TileAEv, Map.GetUnit(CurrX, CurrY), TileBDef, TileBEv);
                                EmitSignal("ShowCombatForecastSignal", true);
                                
                                Mode = 3;
                            }
                        }
                    }
                }
                else if(Mode == 3)
                {
                    if(Map.IsInAttackRange(CurrX, CurrY))
                    {
                        if(Map.GetUnit(CurrX, CurrY) != null)
                        {
                            if(Map.GetUnit(CurrX, CurrY).Team == 1)
                            {
                                EmitSignal("ShowCombatForecastSignal", false);
                                EnterCombat(SelectedUnit, Map.GetUnit(CurrX, CurrY));
                            }
                        }
                    }
                }
                else if(Mode == 4)
                {
                    if(Map.IsInSupportRange(CurrX, CurrY, ref SelectedUnit))
                    {
                        if(Map.GetUnit(CurrX, CurrY) != null)
                        {
                            EmitSignal("UnitCheckSignal", Map.GetUnit(CurrX, CurrY));
                        }
                        
                        Wait(SelectedUnit, SelectedUnit.CurrX, SelectedUnit.CurrY);
                        RemoveRangeTiles();
                        SelectedUnit = null;
                        Mode = 0;
                    }
                }
                else if(Mode == 5)
                {
                }
            }
            else if (inputEvent.IsActionPressed("grid_deselect"))
            {
                if(Mode == 1)
                {
                    if(SelectedUnit != null)
                    {
                        CurrX = SelectedUnit.PrevX;
                        CurrY = SelectedUnit.PrevY;
                        EmitSignal("MoveCursorSignal", CurrX, CurrY);
                        Map.PlaceUnit(SelectedUnit.PrevX, SelectedUnit.PrevY, SelectedUnit);
                        RemoveRangeTiles();
                        SelectedUnit = null;
                        TerrainCheck(CurrX, CurrY);
                        Mode = 0;

                        CheckEnemyRange();
                    }
                }
                else if(Mode == 2)
                {
                    EmitSignal("ShowCombatForecastSignal", false);
                    EmitSignal("ShowActionMenuSignal", "main");
                    EmitSignal("MoveCursorSignal", SelectedUnit.CurrX, SelectedUnit.CurrY);
                    RemoveRangeTiles();
                    CurrX = SelectedUnit.CurrX;
                    CurrY = SelectedUnit.CurrY;
                    Mode = 1;
                    TerrainCheck(CurrX, CurrY);
                    Cursor.SetProcessInput(false);
                    SetProcessInput(false);
                }
                else if(Mode == 3)
                {
                    EmitSignal("ShowCombatForecastSignal", false);
                    Cursor.SetProcessInput(true);
                    Mode = 2;
                }
                else if(Mode == 4)
                {
                    EmitSignal("ShowActionMenuSignal", "support");
                    EmitSignal("MoveCursorSignal", SelectedUnit.CurrX, SelectedUnit.CurrY);
                    RemoveRangeTiles();
                    CurrX = SelectedUnit.CurrX;
                    CurrY = SelectedUnit.CurrY;
                    Mode = 1;
                    TerrainCheck(CurrX, CurrY);
                    Cursor.SetProcessInput(false);
                    SetProcessInput(false);
                }
                else if(Mode == 5)
                {
                }
            }
            else if (inputEvent.IsActionPressed("grid_menu"))
            {
                if(Mode == 0)
                {
                    SetProcessInput(false);
                    Cursor.SetProcessInput(false);
                    EmitSignal("ShowMapMenuSignal");
                }
            }
            else if (inputEvent.IsActionPressed("grid_check_unit"))
            {
                if(Map.GetUnit(CurrX, CurrY) != null)
                {
                    SetProcessInput(false);
                    Cursor.SetProcessInput(false);
                    GetNode<Camera2D>("Camera2D").Current = false;

                    EmitSignal("ShowTerrainHUDSignal", false);
                    EmitSignal("ShowUnitHUDSignal", false);
                    EmitSignal("ShowUnitFullHUDSignal", true);
                    var map_curr_unit = Map.GetUnit(CurrX, CurrY);
                    EmitSignal("UpdateUnitFullHUDSignal", map_curr_unit);
                }
                else if(SelectedUnit != null)
                {
                    SetProcessInput(false);
                    Cursor.SetProcessInput(false);
                    GetNode<Camera2D>("Camera2D").Current = false;

                    EmitSignal("ShowTerrainHUDSignal", false);
                    EmitSignal("ShowUnitHUDSignal", false);
                    EmitSignal("ShowUnitFullHUDSignal", true);
                    EmitSignal("UpdateUnitFullHUDSignal", SelectedUnit);
                }
            }

            else if (inputEvent.IsActionPressed("grid_check_all_enemy_range"))
            {
                EnemyRangeToggle += 1;
                if(EnemyRangeToggle > 2)
                {
                    EnemyRangeToggle = 0;
                }

                CheckEnemyRange();
            }
        }
    }
}