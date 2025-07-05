using Godot;
using System;

public partial class PathFinder : Resource {
    private static readonly Vector2[] DIRECTIONS = {Vector2.Left, Vector2.Right, Vector2.Up, Vector2.Down};

    private Grid _grid;
    private AStarGrid2D _astar = new AStarGrid2D();

    public void Init(Grid grid, Godot.Collections.Array<Vector2I> walkableCells){
        _grid = grid;
        _astar = new AStarGrid2D();
        _astar.Region = new Rect2I(new Vector2I(0, 0), _grid.Size);
        _astar.CellSize = _grid.CellSize;
        _astar.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
        _astar.DefaultComputeHeuristic = AStarGrid2D.Heuristic.Manhattan;
        _astar.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Manhattan;
        _astar.Update();

        for(int y = 0; y < _grid.Size.Y; y++){
            for(int x = 0; x < _grid.Size.X; x++){
                Vector2I currVector = new Vector2I(x, y);
                if(!walkableCells.Contains(currVector)){
                    _astar.SetPointSolid(currVector);
                }
            }
        }
    }

    public Godot.Collections.Array<Vector2I> CalculatePointPath(Vector2I start, Vector2I end){
        return _astar.GetIdPath(start, end);
    }
}