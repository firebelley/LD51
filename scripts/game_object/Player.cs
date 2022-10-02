using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class Player : Node2D
    {
        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }
    }
}
