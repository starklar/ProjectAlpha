using Godot;
using System;

public class MapMenuScene : CanvasLayer
{
    [Signal]
    delegate void ReturnToMapSignal(int mode);

    [Signal]
    delegate void EndTurnSignal();

    private int PointerSlot;
    private int TotalSlots = 4;
    private float XPos = 610.0f;
    private float[] YPos = { 216.0f, 268.0f, 320.0f, 372.0f };

    private KinematicBody2D PointerNode;

    public override void _Ready()
    {
        this.GetNode<Node2D>("MapMenu").Visible = false;
        SetProcessInput(false);
        PointerNode = (KinematicBody2D) this.GetNode<Node2D>("MapMenu").GetNode("Pointer");

        this.GetParent().GetParent().Connect("ShowMapMenuSignal", this, "ShowMenu");
    }

    private void ShowMenu()
    {
        PointerSlot = 0;
        PointerNode.Position = new Vector2(XPos, YPos[0]);
        this.GetNode<Node2D>("MapMenu").Visible = true;
        SetProcessInput(true);
    }

    private void HideMenu()
    {
        this.GetNode<Node2D>("MapMenu").Visible = false;
        SetProcessInput(false);
    }

    private void SelectOption()
    {
        if(PointerSlot == 0)
        {

        }
        else if(PointerSlot == 1)
        {
            
        }
        else if(PointerSlot == 2)
        {
            
        }
        else if(PointerSlot == 3)
        {
            HideMenu();
            EmitSignal("EndTurnSignal");
        }
    }

    public override void _Input(InputEvent inputEvent)
        {
            if (inputEvent.IsActionPressed("move_cursor_up"))
            {
                if (PointerSlot == 0)
                {
                    PointerSlot = TotalSlots - 1;
                }
                else
                {
                    PointerSlot -= 1;
                }

                PointerNode.Position = new Vector2(XPos, YPos[PointerSlot]);
            }
            else if(inputEvent.IsActionPressed("move_cursor_down"))
            {
                if (PointerSlot == TotalSlots - 1)
                {
                    PointerSlot = 0;
                }
                else
                {
                    PointerSlot += 1;
                }

                PointerNode.Position = new Vector2(XPos, YPos[PointerSlot]);
            }
            else if (inputEvent.IsActionPressed("grid_select"))
            {
                SelectOption();
            }
            else if (inputEvent.IsActionPressed("grid_deselect"))
            {
                HideMenu();
                EmitSignal("ReturnToMapSignal", 0);
            }
        }
}
