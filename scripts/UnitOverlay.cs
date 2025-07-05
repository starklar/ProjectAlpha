using Godot;
using System;


public partial class UnitOverlay : TileMapLayer{
    public new void Draw(Godot.Collections.Array<Vector2I> cells, int sourceId){
        Clear();

        foreach(Vector2I cell in cells){
            SetCell(cell, 0, new Vector2I(sourceId, 0));
        }
    }
}