using Godot;
using GodotUtilities;

namespace Game.Effect
{
    public class TileCursor : Node2D
    {
        [Export]
        private Color invalidColor;
        [Export]
        private Color validColor;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public void SetValid(bool valid)
        {
            if (valid)
            {
                Modulate = validColor;
            }
            else
            {
                Modulate = invalidColor;
            }
        }
    }
}
