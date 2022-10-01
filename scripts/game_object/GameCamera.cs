using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class GameCamera : Node2D
    {
        private Vector2 targetPosition;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _Process(float delta)
        {
            targetPosition = GetTree().GetFirstNodeInGroup<Player>()?.GlobalPosition ?? targetPosition;
            GlobalPosition = GlobalPosition.LinearInterpolate(targetPosition, 1f - Mathf.Exp(-7f * delta));
        }
    }
}
