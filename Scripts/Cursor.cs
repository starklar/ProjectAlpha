using Godot;
using System;

[Tool]
public partial class Cursor: Node2D{
    [Export]
    public float uiCooldown = 0.1f;

    public Vector2I Cell{
        get{ return _cell; }
        set{
            _cell = _grid.Clamp(value);
        }
    }

    private Vector2I _cell;

    private Timer _timer;

    private Grid _grid  = new Grid();

    public override void _Ready(){
        _timer = GetNode<Timer>("Timer");
        _timer.WaitTime = uiCooldown;
        Position = new Vector2(40.0f, 40.0f);
        Cell = _grid.CalculateGridCoordinates(Position);
        Position = _grid.CalculateMapPosition(Cell);

        EventBus.Instance.enableCursor += SetInput;
    }

    public override void _UnhandledInput(InputEvent inEvent){
        if (inEvent.IsActionPressed("ui_accept"))
        {
            EventBus.Instance.EmitSignal("acceptPressed", Cell);
            GetViewport().SetInputAsHandled();

            return;
        }
        else if(inEvent.IsActionPressed("ui_select")){  //TEMP IN PLACE OF TITLE SCREEN
            EventBus.Instance.EmitSignal("loadGame", "savefile1");
            EventBus.Instance.EmitSignal("loadChapterData", "chapter1");

            EventBus.Instance.EmitSignal("initializeBoard");
            return;
        }

        if (!_timer.IsStopped()){
            return;
        }

        if(inEvent.IsAction("ui_right")){
            Position += Vector2I.Right * _grid.CellSize;
        }
        if(inEvent.IsAction("ui_left")){
            Position += Vector2I.Left * _grid.CellSize;
        }
        if(inEvent.IsAction("ui_up")){
            Position += Vector2I.Up * _grid.CellSize;
        }
        if(inEvent.IsAction("ui_down")){
            Position += Vector2I.Down * _grid.CellSize;
        }

        Cell = _grid.CalculateGridCoordinates(Position);
        Position = _grid.CalculateMapPosition(Cell);

        EventBus.Instance.EmitSignal("moved", Cell);

        _timer.Start();
    }

    public override void _Draw(){
        DrawRect(new Rect2(-_grid.CellSize / 2, _grid.CellSize), new Color(0, 0, 1, 1), false, 2.0f);
    }

    private void SetInput(bool inputEnabled){
        SetProcessUnhandledInput(inputEnabled);
    }
}