using Godot;
using System;

namespace Skirmish
{
    public class Cursor : Node2D
    {
        public int CurrX { get; set; }
        public int CurrY { get; set; }

        [Signal]
        delegate void CursorMovedSignal(int x, int y);

        [Signal]
        delegate void ChangeUnitHUDSideSignal(bool move_up);

        [Signal]
        delegate void ChangeTerrainHUDSideSignal(bool move_up);


        private Camera2D Camera;

        public int MaxX { get; set; }
        public int MaxY { get; set; }

        private float MaxCameraX;
        private float MaxCameraY;

        public override void _Ready()
        {
            
        }

        public void Start(int start_x, int start_y, int max_x, int max_y)
        {
            CurrX = start_x;
            CurrY = start_y;
            MaxX = max_x;
            MaxY = max_y;
            MaxCameraX = (MaxX - Global.HORIZONTAL_TILE_COUNT / 2) * Global.MAP_SCALE;
            MaxCameraY = (MaxY - Global.VERTICLE_TILE_COUNT / 2) * Global.MAP_SCALE;

            Position = new Vector2(CurrX * Global.MAP_SCALE + Global.MAP_SCALE / 2, CurrY * Global.MAP_SCALE + Global.MAP_SCALE / 2);

            this.GetParent().Connect("MoveCursorSignal", this, "MoveTo");

            Show();
            
            var animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
            animatedSprite.Play();

            Camera = this.GetParent().GetNode<Camera2D>("Camera2D");
            Camera.Current = true;
            
            MoveCamera(CurrX, CurrY, "");
        }

        private void MoveTo(int new_x, int new_y)
        {
            CurrX = new_x;
            CurrY = new_y;
            Position = new Vector2(CurrX * Global.MAP_SCALE + Global.MAP_SCALE, CurrY * Global.MAP_SCALE + Global.MAP_SCALE);
        }

        private void MoveCamera(int new_x, int new_y, string direction)
        {
            float currCameraX = Camera.Position.x;
            float currCameraY = Camera.Position.y;
            float currCursorX = this.Position.x + Global.WINDOW_WIDTH / 2;
            float currCursorY = this.Position.y + Global.WINDOW_HEIGHT / 2;

            float newCameraX = Camera.Position.x;
            float newCameraY = Camera.Position.y;

            if(direction == "right" && currCursorX >= currCameraX + Global.WINDOW_WIDTH * 3 / 4)
            {
                newCameraX = (CurrX - Global.HORIZONTAL_TILE_COUNT / 5) * Global.MAP_SCALE;
            }
            if(direction == "left" && currCursorX <= currCameraX + Global.WINDOW_WIDTH * 1 / 4)
            {
                newCameraX = (CurrX + Global.HORIZONTAL_TILE_COUNT / 5 + 1) * Global.MAP_SCALE;
            }

            if(direction == "down" && currCursorY >= currCameraY + Global.WINDOW_HEIGHT * 3 / 4)
            {
                EmitSignal("ChangeUnitHUDSideSignal", true);
                EmitSignal("ChangeTerrainHUDSideSignal", true);
                newCameraY = (CurrY - Global.VERTICLE_TILE_COUNT / 5) * Global.MAP_SCALE;
            }
            if(direction == "up" && currCursorY <= currCameraY + Global.WINDOW_HEIGHT * 1 / 4)
            {
                EmitSignal("ChangeUnitHUDSideSignal", false);
                EmitSignal("ChangeTerrainHUDSideSignal", false);
                newCameraY = (CurrY + Global.VERTICLE_TILE_COUNT / 5 + 1) * Global.MAP_SCALE;
            }

            if(newCameraX < Global.WINDOW_WIDTH / 2)
            {
                newCameraX = Global.WINDOW_WIDTH / 2;
            }
            else if(newCameraX > MaxCameraX)
            {
                newCameraX = MaxCameraX;
            }

            if(newCameraY < Global.WINDOW_HEIGHT / 2)
            {
                newCameraY = Global.WINDOW_HEIGHT / 2;
            }
            else if(newCameraY > MaxCameraY)
            {
                newCameraY = MaxCameraY;
            }

            Camera.Position = new Vector2(newCameraX, newCameraY);
        }

        public override void _Input(InputEvent inputEvent)
        {
            bool moved = false;
            string direction = "";

            if (inputEvent.IsActionPressed("move_cursor_right") && CurrX < MaxX - 1)
            {
                CurrX += 1;
                direction = "right";
                moved = true;
            }

            if (inputEvent.IsActionPressed("move_cursor_left") && CurrX > 0)
            {
                CurrX -= 1;
                direction = "left";
                moved = true;
            }

            if (inputEvent.IsActionPressed("move_cursor_down") && CurrY < MaxY - 1)
            {
                CurrY += 1;
                direction = "down";
                moved = true;
            }

            if (inputEvent.IsActionPressed("move_cursor_up") && CurrY > 0)
            {
                CurrY -= 1;
                direction = "up";
                moved = true;
            }

            if(moved == true)
            {
                Position = new Vector2(CurrX * Global.MAP_SCALE + Global.MAP_SCALE / 2, CurrY * Global.MAP_SCALE + Global.MAP_SCALE / 2);
                MoveCamera(CurrX, CurrY, direction);
                EmitSignal("CursorMovedSignal", CurrX, CurrY);
            }
        }
    }
}