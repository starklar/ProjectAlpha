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

        public int MaxX { get; set; }
        public int MaxY { get; set; }

        public override void _Ready()
        {
            
        }

        public void Start(int start_x, int start_y, int max_x, int max_y)
        {
            CurrX = start_x;
            CurrY = start_y;
            MaxX = max_x;
            MaxY = max_y;
            Position = new Vector2(CurrX * Global.MAP_SCALE + Global.MAP_SCALE / 2, CurrY * Global.MAP_SCALE + Global.MAP_SCALE / 2);

            this.GetParent().Connect("MoveCursorSignal", this, "MoveTo");

            Show();
            
            var animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
            animatedSprite.Play();
        }

        private void MoveTo(int new_x, int new_y)
        {
            Console.Write("Hee ho");
            CurrX = new_x;
            CurrY = new_y;
            Position = new Vector2(CurrX * Global.MAP_SCALE + Global.MAP_SCALE / 2, CurrY * Global.MAP_SCALE + Global.MAP_SCALE / 2);
        }

        public override void _Input(InputEvent inputEvent)
        {
            bool moved = false;

            if (inputEvent.IsActionPressed("move_cursor_right") && CurrX < MaxX - 1)
            {
                CurrX += 1;
                moved = true;
            }

            if (inputEvent.IsActionPressed("move_cursor_left") && CurrX > 0)
            {
                CurrX -= 1;
                moved = true;
            }

            if (inputEvent.IsActionPressed("move_cursor_down") && CurrY < MaxY - 1)
            {
                CurrY += 1;
                moved = true;
            }

            if (inputEvent.IsActionPressed("move_cursor_up") && CurrY > 0)
            {
                CurrY -= 1;
                moved = true;
            }

            if(moved == true)
            {
                Position = new Vector2(CurrX * Global.MAP_SCALE + Global.MAP_SCALE / 2, CurrY * Global.MAP_SCALE + Global.MAP_SCALE / 2);
                EmitSignal("CursorMovedSignal", CurrX, CurrY);
            }
        }
    }
}