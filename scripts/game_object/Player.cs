using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class Player : KinematicBody2D
    {
        [Node]
        private Grapple grapple;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _UnhandledInput(InputEvent evt)
        {
            if (evt.IsActionPressed("grapple"))
            {
                grapple.Shoot();
            }
        }
    }
}
