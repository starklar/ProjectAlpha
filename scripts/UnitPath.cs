using Godot;
using System;

public partial class UnitPath : TileMapLayer {

    public PathFinder PathFinder;
    public Godot.Collections.Array<Vector2I> CurrentPath;

    private Grid _grid = new Grid();

    public void Initialize(Godot.Collections.Array<Vector2I> walkableCells){
        _grid = new Grid();
        PathFinder = new PathFinder();
        PathFinder.Init(_grid, walkableCells);
    }

    public new void Draw(Vector2I cellStart, Vector2I cellEnd){
        Clear();
        CurrentPath = PathFinder.CalculatePointPath(cellStart, cellEnd);
        SetCellsTerrainConnect(CurrentPath, 0, 0);
    }

    public void Stop(){
        PathFinder = null;
        Clear();
    }
}