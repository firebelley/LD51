using Game.Entity;
using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class SpawnArea : CollisionShape2D
    {
        private const float ENEMIES_PER_TILE = .25f;

        [Node]
        private ResourcePreloader resourcePreloader;
        [Node]
        private Timer timer;

        private RectangleShape2D rectShape;
        private int enemiesToSpawn;
        private int currentEnemies;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            rectShape = Shape as RectangleShape2D;
            timer.Connect("timeout", this, nameof(OnTimerTimeout));
            CallDeferred(nameof(Initialize));
        }

        private void SpawnEnemy()
        {
            var enemy = resourcePreloader.InstanceSceneOrNull<Clock>();
            this.GetAncestor<Main>().Enemies.AddChild(enemy);
            enemy.GlobalPosition = new Vector2(GlobalPosition.x + MathUtil.RNG.RandfRange(-rectShape.Extents.x, rectShape.Extents.x), GlobalPosition.y);
            enemy.Connect(nameof(Clock.Died), this, nameof(OnEnemyDied));
            currentEnemies++;
        }

        private void Initialize()
        {
            var tileSegments = rectShape.Extents.x * 2f / 32f;
            enemiesToSpawn = Mathf.CeilToInt(tileSegments * ENEMIES_PER_TILE);
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                SpawnEnemy();
            }
        }

        private void OnTimerTimeout()
        {
            if (currentEnemies < enemiesToSpawn)
            {
                SpawnEnemy();
            }
            timer.Start();
        }

        private void OnEnemyDied()
        {
            currentEnemies--;
        }
    }
}
