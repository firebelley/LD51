using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class GameCamera : Node2D
    {

        [Node]
        private Camera2D shakyCamera2d;

        private Vector2 targetPosition;

        private static GameCamera instance;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                instance = this;
                this.WireNodes();
            }
        }

        public override void _Process(float delta)
        {
            // targetPosition = GetTree().GetFirstNodeInGroup<Player>()?.GlobalPosition ?? targetPosition;
            // GlobalPosition = GlobalPosition.LinearInterpolate(targetPosition, 1f - Mathf.Exp(-10f * delta));
        }

        public static void Shake()
        {
            instance.shakyCamera2d.Call("shake");
        }
    }
}
