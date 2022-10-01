using Game.GameObject;
using Godot;
using GodotUtilities;

namespace Game.Manager
{
    public class EnemyManager : Node
    {
        private const float SPAWN_OFFSET = 128f;

        [Node]
        public Timer Timer { get; private set; }
        [Node]
        private ResourcePreloader resourcePreloader;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            Timer.Connect("timeout", this, nameof(OnTimerTimeout));
        }

        private Enemy SpawnEnemy()
        {
            var player = GetTree().GetFirstNodeInGroup<Player>();
            if (player == null) return null;

            var enemy = resourcePreloader.InstanceSceneOrNull<Enemy>();
            GetParent().AddChild(enemy);

            var direction = Vector2.Right.Rotated(MathUtil.RNG.RandfRange(0, Mathf.Tau));
            enemy.GlobalPosition = player.GlobalPosition + (direction * SPAWN_OFFSET);

            return enemy;
        }

        private void OnTimerTimeout()
        {
            var enemy = SpawnEnemy();
            if (enemy != null)
            {
                enemy.Connect(nameof(Enemy.OnDied), this, nameof(OnEnemyDied));
            }
            else
            {
                Timer.Start();
            }
        }

        private void OnEnemyDied()
        {
            Timer.Start();
        }
    }
}
