using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skirmish
{
    public class Pos
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Pos(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    class Step
    {
        public Pos CurrPos { get; set; }
        public List<Pos> Path { get; set; }
        public int RemainingSteps { get; set; }

        public Step(Pos curr_pos, List<Pos> path, int remaining_steps)
        {
            CurrPos = curr_pos;
            Path = new List<Pos>(path);
            RemainingSteps = remaining_steps;
        }
    }

    class InitiatePos
    {
        public Pos TargetPos { get; set; }
        public List<Pos> PathToPos { get; set; }

        public InitiatePos(Pos target_pos, List<Pos> path)
        {
            TargetPos = target_pos;
            PathToPos = new List<Pos>(path);
        }
    }

    class SkillsAtRange
    {
        public int Range { get; set; }
        public List<BattleSkill> UsableSkills { get; set; }

        public SkillsAtRange(int range, List<BattleSkill> usable_skills)
        {
            Range = range;
            UsableSkills = new List<BattleSkill>(usable_skills);
        }
    }

    class TileCost
    {
        public Pos P { get; set;}
        public List<Pos> Path { get; set;}
        public double G { get; set; }

        public TileCost(Pos p, List<Pos> path, double g)
        {
            P = p;
            Path = new List<Pos>(path);
            G = g;
        }
    }

    public class AiMove
    {
        public Pos TargetPos { get; set; }
        public List<Pos> PathToPos { get; set; }
        //"Attack", "Heal", "Buff", "Debuff", "Wait"
        public string Action { get; set; }

        public AiMove(Pos target, List<Pos> path, string action)
        {
            TargetPos = target;
            PathToPos = new List<Pos>(path);
            Action = action;
        }
    }

    public class SkirmishMap
    {
        private Tile[,] Map;
        private int MaxX;
        private int MaxY;

        private Pos[] Directions = { new Pos(1,0), new Pos(-1,0), new Pos(0,1), new Pos(0,-1)};
        private List<Step> SPQueue;
        private List<Step> SPSet;
        private List<Pos> SelectableAttackTiles;
        private List<Pos> SelectableSupportTiles;

        //x, y, List<(x, y)> path to tile to perform action from
        private List<InitiatePos> EnemyAttackRange;
        private List<InitiatePos> EnemySupportRange;

        private List<Pos> PlayerUnitLocations;
        private List<Pos> EnemyUnitLocations;
        private List<Pos> OtherUnitLocations;
        
        public SkirmishMap(string map_name, int map_size_x, int map_size_y)
        {
            Map = new Tile[map_size_x, map_size_y];

            PlayerUnitLocations = new List<Pos>();
            EnemyUnitLocations = new List<Pos>();
            OtherUnitLocations = new List<Pos>();

            MaxX = map_size_x;
            MaxY = map_size_y;

            string fileName = "res://Tiles/" + map_name + ".txt";
            Console.Write(fileName);

            var mapFile = new File();
            if (!mapFile.FileExists(fileName))
            {
                //TODO: replace error message
                Console.Write("ERROR, MAP TILES DATA NOT FOUND");
            }
            
            mapFile.Open(fileName, File.ModeFlags.Read);
            string currLine = "";

            int indexX = 0;

            for(int indexY = 0; indexY < map_size_y; indexY++)
            {
                indexX = 0;
                currLine = mapFile.GetLine();
                
                foreach(string tileType in currLine.Split(","))
                {
                    Map[indexX, indexY] = Global.TILE_TYPES[tileType.ToInt()].Clone();
                    indexX += 1;
                }
            }
        }

        public Tile GetTile(int x, int y)
        {
            return Map[x, y];
        }

        public UnitScene GetUnit(int x, int y)
        {
            return Map[x, y].CurrUnit;
        }

        public void PlaceUnit(int x, int y, UnitScene new_unit)
        {
            if(new_unit.Team == 0)
            {
                PlayerUnitLocations.Add(new Pos(x, y));
            }
            else if(new_unit.Team == 1)
            {
                EnemyUnitLocations.Add(new Pos(x, y));
            }
            else if(new_unit.Team == 2)
            {
                OtherUnitLocations.Add(new Pos(x, y));
            }

            Map[x, y].CurrUnit = new_unit;
        }

        public void RemoveUnit(int x, int y)
        {
            if(Map[x, y].CurrUnit != null)
            {
                if(Map[x, y].CurrUnit.Team == 0)
                {
                    PlayerUnitLocations.RemoveAll(p => p.X == x && p.Y == y);
                }
                else if(Map[x, y].CurrUnit.Team == 1)
                {
                    EnemyUnitLocations.RemoveAll(p => p.X == x && p.Y == y);
                }
                else if(Map[x, y].CurrUnit.Team == 2)
                {
                    OtherUnitLocations.RemoveAll(p => p.X == x && p.Y == y);
                }
                Map[x, y].CurrUnit = null;
            }
        }

        public void ChangeTile(int x, int y, string object_name, string tile_name, int defence_bonus, int evasion_bonus, int allowed_types, int movement_penalty)
        {
            Map[x, y] = new Tile(tile_name, defence_bonus, evasion_bonus, allowed_types, movement_penalty);
        }


        public List<Pos> SpawnMoveTiles(int move_type, int team, int max_move, int curr_x, int curr_y)
        {
            //(x, y, remaining moves left once this step is reached)
            SPSet = new List<Step>();
            SPQueue = new List<Step>();
            SPSet.Add(new Step(new Pos(curr_x, curr_y), new List<Pos>(), max_move));
            SPQueue.Add(new Step(new Pos(curr_x, curr_y), new List<Pos>(), max_move));

            while(SPQueue.Count() > 0)
            {
                SpawnMoveTiles(move_type, team, SPQueue[0], ref SPSet, ref SPQueue);
            }

            List<Pos> selectableTiles = new List<Pos>();
            
            foreach(Step t in SPSet)
            {
                selectableTiles.Add(t.CurrPos);
            }
            
            return selectableTiles;
        }

        private void SpawnMoveTiles(int move_type, int team, Step current_tile, ref List<Step> sp_set, ref List<Step> sp_queue)
        {
            sp_queue.Remove(current_tile);
            Pos curr_loc = current_tile.CurrPos;

            if(current_tile.RemainingSteps <= 0)
            {
                return;
            }

            if(Map[curr_loc.X, curr_loc.Y].CurrUnit != null && Map[curr_loc.X, curr_loc.Y].CurrUnit.Team != team)
            {
                return;
            }

            List<Pos> path = current_tile.Path;

            foreach (Pos d in Directions)
            {
                int newX = curr_loc.X + d.X;
                int newY = curr_loc.Y + d.Y;

                if (newX < 0 || newX >= MaxX)
                {
                    continue;
                }
                if (newY < 0 || newY >= MaxY)
                {
                    continue;
                }

                Pos nextTile = new Pos(newX, newY);

                int remaining_moves = current_tile.RemainingSteps - 1;

                if (Map[newX, newY].AllowedTypes > move_type)
                {
                    remaining_moves -= Map[newX, newY].MovementPenalty;

                    if(remaining_moves < 0 && Map[newX, newY].MovementPenalty > 0)
                    {
                        continue;
                    }
                }

                List<Pos> updatedPath = new List<Pos>(path);
                updatedPath.Add(nextTile);

                if(sp_set.Exists(s => s.CurrPos.X == nextTile.X && s.CurrPos.Y == nextTile.Y))
                {
                    Step sp = sp_set.Find(s => s.CurrPos.X == nextTile.X && s.CurrPos.Y == nextTile.Y);

                    if(sp.RemainingSteps < remaining_moves)
                    {
                        sp_set.Remove(sp);
                        foreach(Pos t in sp.Path)
                        {
                            sp_queue.Add(new Step(t, path, remaining_moves));
                        }
                    }
                }
                else
                {
                    if(Map[nextTile.X, nextTile.Y].CurrUnit == null)
                    {
                        sp_set.Add(new Step(nextTile, updatedPath, remaining_moves));
                    }
                }
                sp_queue.Add(new Step(nextTile, updatedPath, remaining_moves));
            }
        }

        private void SpawnAttackTiles(int min_range, int max_range, int curr_x, int curr_y, ref List<Pos> attack_tiles)
        {
            for(int x = 0; x < MaxX; x++)
            {
                for(int y = 0; y < MaxY; y++)
                {
                    int range = Math.Abs(x - curr_x) + Math.Abs(y - curr_y);
                    if(range <= max_range && range >= min_range)
                    {
                        if(!attack_tiles.Exists(t => t.X == x && t.Y == y))
                        {
                            attack_tiles.Add(new Pos(x, y));
                        }
                    }
                }
            }
        }

        private void SpawnSupportTiles(int min_range, int max_range, int curr_x, int curr_y, ref List<Pos> support_tiles)
        {
            for(int x = 0; x < MaxX; x++)
            {
                for(int y = 0; y < MaxY; y++)
                {
                    int range = Math.Abs(x - curr_x) + Math.Abs(y - curr_y);
                    if(range <= max_range && range >= min_range)
                    {
                        if(!support_tiles.Exists(t => t.X == x && t.Y == y))
                        {
                            support_tiles.Add(new Pos(x, y));
                        }
                    }
                }
            }
        }

        public List<Pos> GetMovementPath(int x, int y)
        {
            if(SPSet.Exists(s => s.CurrPos.X == x && s.CurrPos.Y == y))
            {
                return SPSet.Find(s => s.CurrPos.X == x && s.CurrPos.Y == y).Path;
            }
            return null;
        }

        public List<Pos> GetAttackRange(UnitScene unit)
        {
            SelectableAttackTiles = new List<Pos>();
            SpawnAttackTiles(unit.StandardAttack.MinRange, unit.StandardAttack.MaxRange, unit.CurrX, unit.CurrY, ref SelectableAttackTiles);

            foreach(BattleSkill skill in unit.BattleSkills)
            {
                if(skill.Type >= 7 || unit.CurrMP < skill.Cost)
                {
                    continue;
                }

                int minRange = skill.MinRange;
                int maxRange = skill.MaxRange;

                SpawnAttackTiles(minRange, maxRange, unit.CurrX, unit.CurrY, ref SelectableAttackTiles);
            }
            return SelectableAttackTiles;
        }

        public List<Pos> GetSupportRange(UnitScene unit)
        {
            SelectableSupportTiles = new List<Pos>();
            foreach(BattleSkill skill in unit.BattleSkills)
            {
                if(skill.Type < 7 || unit.CurrMP < skill.Cost)
                {
                    continue;
                }

                int minRange = skill.MinRange;
                int maxRange = skill.MaxRange;

                SpawnSupportTiles(minRange, maxRange, unit.CurrX, unit.CurrY, ref SelectableSupportTiles);
            }
            return SelectableSupportTiles;
        }

        public bool IsInAttackRange(int x, int y)
        {
            return SelectableAttackTiles.Exists(t => t.X == x && t.Y == y);
        }

        public bool IsInSupportRange(int x, int y)
        {
            return SelectableSupportTiles.Exists(t => t.X == x && t.Y == y);
        }

        private void CalculateAIAttackRange(AIUnitScene unit, int steps, int curr_x, int curr_y, ref List<Pos> current_list)
        {
            if(steps < 0)
            {
                return;
            }

            if(Map[curr_x, curr_y].CurrUnit != null && Map[curr_x, curr_y].CurrUnit.Team != unit.Team)
            {
                return;
            }

            List<Pos> attackTiles = new List<Pos>();
            SpawnAttackTiles(unit.StandardAttack.MinRange, unit.StandardAttack.MaxRange, curr_x, curr_y, ref attackTiles);

            foreach(BattleSkill skill in unit.BattleSkills)
            {
                if(skill.Type >= 7 || skill.Cost > unit.CurrMP)
                {
                    continue;
                }

                int minRange = skill.MinRange;
                int maxRange = skill.MaxRange;

                SpawnAttackTiles(minRange, maxRange, curr_x, curr_y, ref attackTiles);
            }

            foreach(Pos tile in attackTiles)
            {
                if(!current_list.Exists(t => t.X == tile.X && t.Y == tile.Y))
                {
                    current_list.Add(tile);
                }
            }

            foreach (Pos d in Directions)
            {
                int newX = curr_x + d.X;
                int newY = curr_y + d.Y;

                if (newX < 0 || newX >= MaxX)
                {
                    continue;
                }
                if (newY < 0 || newY >= MaxY)
                {
                    continue;
                }

                int remainingMoves = steps - 1;

                if(Map[newX, newY].AllowedTypes > unit.MovementType)
                {
                    remainingMoves -= Map[newX, newY].MovementPenalty;

                    if(remainingMoves < 0 && Map[newX, newY].MovementPenalty > 0)
                    {
                        continue;
                    }
                }
                CalculateAIAttackRange(unit, remainingMoves, newX, newY, ref current_list);
            }
        }

        //TODO: Finish this
        private void CalculateAISupportRange(AIUnitScene unit, int steps, int curr_x, int curr_y, ref List<Pos> current_list)
        {
            if(steps < 0)
            {
                return;
            }

            List<Pos> supportTiles = new List<Pos>();
            //SpawnSupportTiles(0, min_range, max_range, curr_x, curr_y, ref support_tiles);
            foreach(BattleSkill skill in unit.BattleSkills)
            {
                if(skill.Type < 7 || skill.Cost > unit.CurrMP)
                {
                    continue;
                }

                int minRange = skill.MinRange;
                int maxRange = skill.MaxRange;

                SpawnSupportTiles(minRange, maxRange, curr_x, curr_y, ref supportTiles);
            }

            foreach(Pos tile in supportTiles)
            {
                if(!current_list.Exists(t => t.X == tile.X && t.Y == tile.Y))
                {
                    current_list.Add(tile);
                }
            }

            foreach (Pos d in Directions)
            {
                int new_x = curr_x + d.X;
                int new_y = curr_y + d.Y;

                if (new_x < 0 || new_x >= MaxX)
                {
                    continue;
                }
                if (new_y < 0 || new_y >= MaxY)
                {
                    continue;
                }

                int remaining_moves = steps - 1;

                if (Map[new_x, new_y].AllowedTypes > unit.MovementType)
                {
                    remaining_moves -= Map[new_x, new_y].MovementPenalty;

                    if(remaining_moves <= 0 && Map[new_x, new_y].MovementPenalty > 0)
                    {
                        continue;
                    }
                }
                
                CalculateAISupportRange(unit, remaining_moves, new_x, new_y, ref current_list);
            }
        }

        public List<Pos> GetEnemyAttackRange()
        {
            List<Pos> totalList = new List<Pos>();

            foreach(Pos location in EnemyUnitLocations)
            {
                AIUnitScene unit = (AIUnitScene) Map[location.X, location.Y].CurrUnit;

                List<Pos> tempList = new List<Pos>();

                CalculateAIAttackRange(unit, unit.Stats[2] + unit.StatMods[0], unit.CurrX, unit.CurrY, ref tempList);

                foreach(Pos tile in tempList)
                {
                    if(!totalList.Exists(t => t.X == tile.X && t.Y == tile.Y))
                    {
                        totalList.Add(tile);
                    }
                }
            }

            return totalList;
        }

        public List<Pos> GetEnemySupportRange()
        {
            List<Pos> totalList = new List<Pos>();

            foreach(Pos location in EnemyUnitLocations)
            {
                AIUnitScene unit = (AIUnitScene) Map[location.X, location.Y].CurrUnit;

                List<Pos> tempList = new List<Pos>();
                CalculateAISupportRange(unit, unit.Stats[2] + unit.StatMods[0], unit.CurrX, unit.CurrY, ref tempList);

                foreach(Pos tile in tempList)
                {
                    if(!totalList.Exists(t => t.X == tile.X && t.Y == tile.Y))
                    {
                        totalList.Add(tile);
                    }
                }
            }

            return totalList;
        }

        //Return value: Item1 = path of tiles for unit to take, Item2 = tile to target
        //If Item2 == (-1, -1), no attack is to be made
        public AiMove GetAIMove(AIUnitScene ai)
        {
            //TODO: ACTUALLY CALL CORRECT AI ALGORITHM
            if(ai.GetMapAIPattern() == "Basic")
            {
                return BasicAIAlgorithm(ai);
            }
            else
            {
                return NothingAIAlgorithm(ai);
            }
        }

        public double GetStepsRequired(UnitScene unit, int dest_x, int dest_y, double max_steps)
        {
            //(tile, g value)
            List<TileCost> openList = new List<TileCost>() { new TileCost(new Pos(unit.CurrX, unit.CurrY), new List<Pos>() { new Pos(unit.CurrX, unit.CurrY) }, 0) };
            List<TileCost> closedList = new List<TileCost>();

            double f = int.MaxValue;

            TileCost currTile = null;

            while(openList.Count > 0)
            {
                f = int.MaxValue;
                
                foreach(TileCost tc in openList)
                {
                    double tempG = tc.G;
                    double tempH = (1 + 1 / (MaxX * MaxY)) * (Math.Abs(tc.P.X - dest_x) + Math.Abs(tc.P.Y - dest_y));
                    double tempF = tempG + tempH;
                    
                    if(f > tempF)
                    {
                        f = tempF;
                        currTile = tc;
                    }
                }

                if(currTile.G >= max_steps)
                {
                    return int.MaxValue;
                }

                openList.Remove(currTile);
                closedList.Add(currTile);

                if(currTile.P.X == dest_x && currTile.P.Y == dest_y)
                {
                    return currTile.G;
                }
                else
                {
                    foreach(Pos d in Directions)
                    {
                        int newX = currTile.P.X + d.X;
                        int newY = currTile.P.Y + d.Y;
                        Pos newPos = new Pos(newX, newY);

                        List<Pos> copyPath = new List<Pos>(currTile.Path);
                        copyPath.Add(newPos);
                        double newG = currTile.G;

                        if (newX < 0 || newX >= MaxX)
                        {
                            continue;
                        }
                        if (newY < 0 || newY >= MaxY)
                        {
                            continue;
                        }

                        if(Map[newX, newY].AllowedTypes > unit.MovementType)
                        {
                            newG += Map[newX, newY].MovementPenalty;
                        }
                        else
                        {
                            newG += 1;
                        }

                        if(openList.Exists(pos => pos.P.X == newPos.X && pos.P.Y == newPos.Y))
                        {
                            TileCost foundTC = openList.Find(pos => pos.P.X == newPos.X && pos.P.Y == newPos.Y);
                            if(newG < foundTC.G)
                            {
                                openList.Remove(foundTC);
                                openList.Add(new TileCost(newPos, copyPath, newG));
                            }
                        }
                        else if(closedList.Exists(pos => pos.P.X == newPos.X && pos.P.Y == newPos.Y))
                        {
                            TileCost foundTC = closedList.Find(pos => pos.P.X == newPos.X && pos.P.Y == newPos.Y);
                            if(newG < foundTC.G)
                            {
                                closedList.Remove(foundTC);
                                openList.Add(foundTC);
                            }
                        }
                        else
                        {
                            openList.Add(new TileCost(newPos, copyPath, newG));
                        }
                    }
                }
                
            }

            return int.MaxValue;
        }

        //TODO: GET COMPLETE PATH TO DESTINATION, RETURN PATH THAT UNIT CAN CURRENTLY MOVE TO
        public List<Pos> GetPathTo(UnitScene unit, int dest_x, int dest_y)
        {
            //(tile, g value)
            List<TileCost> openList = new List<TileCost>() { new TileCost(new Pos(unit.CurrX, unit.CurrY), new List<Pos>() { new Pos(unit.CurrX, unit.CurrY) }, 0) };
            List<TileCost> closedList = new List<TileCost>();

            double f = int.MaxValue;

            TileCost currTile = null;

            while(openList.Count > 0)
            {
                f = int.MaxValue;

                foreach(TileCost tc in openList)
                {
                    double tempG = tc.G;
                    double tempH = (1 + 1 / (MaxX * MaxY)) * (Math.Abs(tc.P.X - dest_x) + Math.Abs(tc.P.Y - dest_y));
                    double tempF = tempG + tempH;
                    
                    if(f > tempF)
                    {
                        f = tempF;
                        currTile = tc;
                    }
                }

                openList.Remove(currTile);

                if(currTile.P.X == dest_x && currTile.P.Y == dest_y)
                {
                    int stepsNeeded = 0;
                    List<Pos> finalPath = new List<Pos>();
                    finalPath.Add(new Pos(unit.CurrX, unit.CurrY));

                    for(int i = 1; i < currTile.Path.Count; i++)
                    {
                        Pos tempPos = currTile.Path[i];
                        if(Map[tempPos.X, tempPos.Y].AllowedTypes > unit.MovementType)
                        {
                            stepsNeeded += Map[tempPos.X, tempPos.Y].MovementPenalty;
                        }
                        else
                        {
                            stepsNeeded += 1;
                        }
                        if(stepsNeeded > unit.StatMods[0] + unit.Stats[2])
                        {
                            return finalPath;
                        }

                        finalPath.Add(tempPos);
                    }

                    return currTile.Path;
                }
                else
                {
                    closedList.Add(currTile);

                    foreach(Pos d in Directions)
                    {
                        int newX = currTile.P.X + d.X;
                        int newY = currTile.P.Y + d.Y;
                        Pos newPos = new Pos(newX, newY);

                        if (newX < 0 || newX >= MaxX)
                        {
                            continue;
                        }
                        if (newY < 0 || newY >= MaxY)
                        {
                            continue;
                        }

                        List<Pos> copyPath = new List<Pos>(currTile.Path);
                        copyPath.Add(newPos);
                        double newG = currTile.G;

                        if(Map[newX, newY].AllowedTypes > unit.MovementType)
                        {
                            newG += Map[newX, newY].MovementPenalty;
                        }
                        else
                        {
                            newG += 1;
                        }

                        if(openList.Exists(x => x.P == newPos))
                        {
                            TileCost foundTC = openList.Find(x => x.P == newPos);
                            if(newG < foundTC.G)
                            {
                                openList.Remove(foundTC);
                                openList.Add(new TileCost(newPos, copyPath, newG));
                            }
                        }
                        else if(closedList.Exists(x => x.P == newPos))
                        {
                            TileCost foundTC = closedList.Find(x => x.P == newPos);
                            if(newG < foundTC.G)
                            {
                                closedList.Remove(foundTC);
                                openList.Add(foundTC);
                            }
                        }
                        else
                        {
                            openList.Add(new TileCost(newPos, copyPath, newG));
                        }
                    }
                }
            }

            return new List<Pos>() { new Pos(unit.CurrX, unit.CurrY) };
        }

        //Always just waits in place, never initiate attack
        private AiMove NothingAIAlgorithm(AIUnitScene ai)
        {
            return new AiMove(new Pos(-1, -1), new List<Pos>(){ new Pos(ai.CurrX, ai.CurrY)}, "Wait");
        }

        private AiMove BasicAIAlgorithm(AIUnitScene ai)
        {
            //Get all AI unit's skills attack range
            List<SkillsAtRange> skillRange = new List<SkillsAtRange>();

            for(int x = ai.StandardAttack.MinRange; x <= ai.StandardAttack.MaxRange; x++)
            {
                skillRange.Add(new SkillsAtRange(x, new List<BattleSkill>() { ai.StandardAttack }));
            }

            foreach(BattleSkill skill in ai.BattleSkills)
            {
                if(skill.Type >= 7 || ai.CurrMP < skill.Cost)
                {
                    continue;
                }

                for(int x = skill.MinRange; x <= skill.MaxRange; x++)
                {
                    if(!skillRange.Exists(r => r.Range == x))
                    {
                        skillRange.Add(new SkillsAtRange(x, new List<BattleSkill>() { skill }));
                    }
                    else
                    {
                        skillRange.Find(r => r.Range == x).UsableSkills.Add(skill);
                    }
                }
            }

            //Get AI movement range
            int maxMove = ai.Stats[2] + ai.StatMods[0];

            List<Step> Set = new List<Step>();
            List<Step> Queue = new List<Step>();
            Set.Add(new Step(new Pos(ai.CurrX, ai.CurrY), new List<Pos>(), maxMove));
            Queue.Add(new Step(new Pos(ai.CurrX, ai.CurrY), new List<Pos>(), maxMove));

            while(Queue.Count() > 0)
            {
                SpawnMoveTiles(ai.MovementType, ai.Team, Queue[0], ref Set, ref Queue);
            }

            List<Pos> reachableTiles = new List<Pos>();
            
            foreach(Step t in Set)
            {
                reachableTiles.Add(t.CurrPos);
            }

            //Determine who should be targeted
            List<Pos> targetList = new List<Pos>();
            List<Pos> tempLst = new List<Pos>();

            int closestDist = int.MaxValue;
            Pos closestLoc = new Pos(-1, -1);

            if(ai.Team == 1)
            {
                tempLst = new List<Pos>();
                foreach(Pos p in PlayerUnitLocations)
                {
                    tempLst.Add(p);
                }
                foreach(Pos p in OtherUnitLocations)
                {
                    tempLst.Add(p);
                }

                while(tempLst.Count() > 0)
                {
                    foreach(Pos location in tempLst)
                    {
                        int dist = Math.Abs(location.X - ai.CurrX) + Math.Abs(location.Y - ai.CurrY);
                        if(dist < closestDist)
                        {
                            closestLoc = location;
                            closestDist =  dist;
                        }
                    }
                    targetList.Add(closestLoc);
                    tempLst.Remove(closestLoc);
                    closestDist = int.MaxValue;
                }
            }
            else
            {
                tempLst = new List<Pos>(EnemyUnitLocations);
                
                while(tempLst.Count() > 0)
                {
                    foreach(Pos location in tempLst)
                    {
                        int dist = Math.Abs(location.X - ai.CurrX) + Math.Abs(location.Y - ai.CurrY);
                        if(dist < closestDist)
                        {
                            closestLoc = location;
                            closestDist = dist;
                        }
                    }
                    targetList.Add(closestLoc);
                    tempLst.Remove(closestLoc);
                    closestDist = int.MaxValue;
                }
            }

            //For each enemy, determine all possible locations to attack from, mark down info for best location
            Pos finalTarget = new Pos(-1, -1);
            Pos finalLocation = new Pos(ai.CurrX, ai.CurrY);
            bool inRange = false;
            bool canKO = false;
            bool canCounter = true;
            int bestObstructionCount = int.MaxValue;
            int bestAccuracy = int.MinValue;
            int bestTotalDamage = int.MinValue;
            int bestDefence = int.MinValue;
            int bestEvasion = int.MinValue;
            double shortestDist = int.MaxValue;

            foreach(Pos target in targetList)
            {
                Tile potentialTile = Map[target.X, target.Y];
                UnitScene targetUnit = Map[target.X, target.Y].CurrUnit;

                List<int> targetRange = new List<int>();

                for(int r = targetUnit.StandardAttack.MinRange; r <= targetUnit.StandardAttack.MaxRange; r++)
                {
                    if(!targetRange.Contains(r))
                    {
                        targetRange.Add(r);
                    }
                }

                foreach(BattleSkill skill in targetUnit.BattleSkills)
                {
                    if(skill.Type >= 7 || ai.CurrMP < skill.Cost)
                    {
                        continue;
                    }

                    for(int r = skill.MinRange; r <= skill.MaxRange; r++)
                    {
                        if(!targetRange.Contains(r))
                        {
                            targetRange.Add(r);
                        }
                    }
                }

                int potentialSpeedDifference = ai.Stats[5] + ai.StatMods[3] - targetUnit.Stats[5] - targetUnit.StatMods[3];

                foreach(SkillsAtRange r in skillRange)
                {
                    List<Pos> attackPostions = new List<Pos>();

                    for(int shift = -r.Range; shift <= r.Range; shift++)
                    {
                        int x = target.X + shift;
                        int y1 = target.Y + r.Range - Math.Abs(shift);
                        int y2 = target.Y - r.Range + Math.Abs(shift);

                        if (x < 0 || x >= MaxX)
                        {
                            continue;
                        }
                        if (y1 >= 0 && y1 < MaxY)
                        {
                            if(Map[x, y1].AllowedTypes <= ai.MovementType || Map[x, y1].MovementPenalty < maxMove)
                            {
                                if(Map[x, y1].CurrUnit == null || Map[x, y1].CurrUnit == ai)
                                {
                                    attackPostions.Add(new Pos(x, y1));
                                }
                            }
                        }
                        if (y1 != y2 && y2 >= 0 && y2 < MaxY)
                        {
                            if(Map[x, y2].AllowedTypes <= ai.MovementType || Map[x, y2].MovementPenalty < maxMove)
                            {
                                if(Map[x, y2].CurrUnit == null || Map[x, y2].CurrUnit == ai)
                                {
                                    attackPostions.Add(new Pos(x, y2));
                                }
                            }
                        }
                    }

                    foreach(Pos pos in attackPostions)
                    {
                        if(reachableTiles.Exists(t => t.X == pos.X && t.Y == pos.Y))
                        {
                            int potentialDefence = Map[pos.X, pos.Y].DefenceBonus;
                            int potentialEvasion = Map[pos.X, pos.Y].EvasionBonus;

                            foreach(BattleSkill skill in r.UsableSkills)
                            {
                                int potentialAccuracy = Global.CalculateAccuracy(ai, targetUnit, skill, potentialTile.EvasionBonus);
                                int potentialDamage = Global.CalculateDamage(ai, targetUnit, skill, potentialTile.DefenceBonus, false);
                                
                                if(potentialSpeedDifference >= Global.FOLLOW_UP_THREASHOLD && ai.CurrMP >= skill.Cost * 2)
                                {
                                    potentialDamage *= 2;
                                }

                                if(potentialDamage >= targetUnit.CurrHP)
                                {
                                    if(canKO == false || IsBetterAttackingPosition(targetRange, r.Range, canCounter, potentialDefence, bestDefence, potentialEvasion, bestEvasion, potentialAccuracy, bestAccuracy))
                                    {
                                        bestAccuracy = potentialAccuracy;
                                        bestTotalDamage = potentialDamage;
                                        canKO = true;
                                        canCounter = targetRange.Contains(r.Range);
                                        inRange = true;
                                        finalTarget = target;
                                        finalLocation = pos;
                                        bestDefence = potentialDefence;
                                        bestEvasion = potentialEvasion;
                                    }
                                }
                                else if(canKO == false)
                                {
                                    if(potentialDamage > bestTotalDamage)
                                    {
                                        if(IsBetterAttackingPosition(targetRange, r.Range, canCounter, potentialDefence, bestDefence, potentialEvasion, bestEvasion, potentialAccuracy, bestAccuracy))
                                        {
                                            bestAccuracy = potentialAccuracy;
                                            bestTotalDamage = potentialDamage;
                                            canKO = false;
                                            canCounter = targetRange.Contains(r.Range);
                                            inRange = true;
                                            finalTarget = target;
                                            finalLocation = pos;
                                            bestDefence = potentialDefence;
                                            bestEvasion = potentialEvasion;
                                        }
                                    }
                                    else if(potentialDamage == bestTotalDamage)
                                    {
                                        if(IsBetterAttackingPosition(targetRange, r.Range, canCounter, potentialDefence, bestDefence, potentialEvasion, bestEvasion, potentialAccuracy, bestAccuracy))
                                        {
                                            bestAccuracy = potentialAccuracy;
                                            bestTotalDamage = potentialDamage;
                                            canKO = false;
                                            canCounter = targetRange.Contains(r.Range);
                                            inRange = true;
                                            finalTarget = target;
                                            finalLocation = pos;
                                            bestDefence = potentialDefence;
                                            bestEvasion = potentialEvasion;
                                        }
                                    }
                                }
                            }
                        }
                        else if(!inRange)
                        {
                            double tempPosDist = GetStepsRequired(ai, pos.X, pos.Y, shortestDist);

                            if(shortestDist > tempPosDist)
                            {
                                List<Pos> path = GetPathTo(ai, pos.X, pos.Y);
                                int tempObstructionCount = 0;
                                int x = -1;
                                int y = -1;
                                for(int idx = path.Count - 1; idx >= 0; idx--)
                                {
                                    x = path[idx].X;
                                    y = path[idx].Y;
                                    if(Map[x, y].CurrUnit != null && ai.CurrX != x && ai.CurrY != y)
                                    {
                                        tempObstructionCount++;
                                    }
                                    else
                                    {
                                        idx = -1;
                                    }
                                }

                                if(bestObstructionCount >= tempObstructionCount)
                                {
                                    bestObstructionCount = tempObstructionCount;
                                    finalLocation = path[path.Count - 1 - tempObstructionCount];
                                    shortestDist = tempPosDist;
                                }
                            }
                        }
                    }
                }
            }

            //Console.WriteLine("\nFinal Target Location: X: " + finalTarget.X + " y: " + finalTarget.Y);
            string action = "Wait";
            if(inRange)
            {
                action = "Attack";
            }
            return new AiMove(finalTarget, GetPathTo(ai, finalLocation.X, finalLocation.Y), action);
        }

        private bool IsBetterAttackingPosition(List<int> target_range, int range, bool can_counter, int potential_defence, int best_defence, int potential_evasion, int best_evasion, int potential_accuracy, int best_accuracy)
        {
            if(potential_accuracy >= 15)
            {
                if(can_counter == true)
                {
                    if(potential_defence > best_defence)
                    {
                        return true;
                    }
                    else if(potential_evasion > best_evasion)
                    {
                        if(potential_defence >= best_defence)
                        {
                            return true;
                        }
                    }
                }
                else if(!target_range.Contains(range))
                {
                    if(potential_accuracy > best_accuracy)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}