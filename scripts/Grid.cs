using Godot;
using System;

public partial class Grid : Resource {
    [Export]
    public Vector2I Size = new Vector2I(20, 20);

    [Export]
    public Vector2I CellSize = new Vector2I(80, 80);

    public Vector2I CalculateMapPosition(Vector2I gridPosition)
	{
        return gridPosition * CellSize + CellSize / 2;
    }

    public Vector2I CalculateGridCoordinates(Vector2 mapPosition){
        return (Vector2I) (mapPosition / CellSize).Floor();
    }

    public bool IsWithinBounds(Vector2 cellCoordinates){
        bool xOutbound = cellCoordinates.X >= 0 && cellCoordinates.X < Size.X;
        bool yOutbound = cellCoordinates.Y >= 0 && cellCoordinates.Y < Size.Y;
        
        return xOutbound && yOutbound;
    }

    public Vector2I Clamp(Vector2I gridPosition){
        Vector2 returnVector = gridPosition;
        returnVector.X = Mathf.Clamp(returnVector.X, 0.0f, Size.X - 1.0f);
        returnVector.Y = Mathf.Clamp(returnVector.Y, 0.0f, Size.Y - 1.0f);

        return (Vector2I) returnVector;
    }

    public int AsIndex(Vector2 cell){
        return (int) (cell.X + Size.X * cell.Y);
    }
}