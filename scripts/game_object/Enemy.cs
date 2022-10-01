using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class Enemy : Node2D
    {
        [Signal]
        public delegate void OnDied();

        private const float ATTACK_OFFSET = 256f;

        private Vector2 desiredPoint;
        public Vector2 Direction { get; private set; }

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            CallDeferred(nameof(Init));
        }

        private void Die()
        {
            QueueFree();
            EmitSignal(nameof(OnDied));
        }

        private void Init()
        {
            var player = GetTree().GetFirstNodeInGroup<Player>();
            // TODO: need to define a center position
            Direction = ((player?.GlobalPosition ?? Vector2.Zero) - GlobalPosition).Normalized();
            desiredPoint = GlobalPosition + (Direction * ATTACK_OFFSET);

            var tween = CreateTween();
            tween.TweenProperty(this, "global_position", desiredPoint, 1.5f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Quint);
            tween.TweenCallback(this, nameof(Die));
        }
    }
}
