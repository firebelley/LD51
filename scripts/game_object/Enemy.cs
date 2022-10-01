using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class Enemy : Node2D
    {
        [Signal]
        public delegate void OnDied();

        private const float ATTACK_OFFSET = 256f;
        private const float MAX_SPEED = 800f;

        [Node]
        private Area2D area2d;

        public Vector2 Direction { get; private set; }
        private Vector2 desiredPoint;
        private Vector2 startingPoint;
        private Vector2 velocity;

        private float currentSpeed;
        private bool collided;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            area2d.Connect("area_entered", this, nameof(OnAreaEntered));
            CallDeferred(nameof(Init));
        }

        public override void _Process(float delta)
        {
            var player = GetTree().GetFirstNodeInGroup<Player>();
            var playerPos = player?.GlobalPosition ?? Vector2.Zero;

            if (!collided)
            {
                var desiredDirection = (playerPos - GlobalPosition).Normalized();
                Direction = velocity.Normalized().LinearInterpolate(desiredDirection, 1f - Mathf.Exp(-10f * delta)).Normalized();
            }

            velocity = Direction * currentSpeed;
            GlobalPosition += velocity * delta;
        }

        private void Die()
        {
            QueueFree();
            EmitSignal(nameof(OnDied));
        }

        private void Init()
        {

            startingPoint = GlobalPosition;
            desiredPoint = GlobalPosition + (Direction * ATTACK_OFFSET);

            var tween = CreateTween();
            tween.TweenProperty(this, nameof(currentSpeed), MAX_SPEED, 1.25f).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(this, nameof(currentSpeed), 0f, 1.25f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
            tween.TweenCallback(this, nameof(Die));
        }

        private void OnAreaEntered(Area2D otherArea)
        {
            if (otherArea.Owner is Player)
            {
                collided = true;
                area2d.GetFirstNodeOfType<CollisionShape2D>().SetDeferred("disabled", true);
            }
        }
    }
}
