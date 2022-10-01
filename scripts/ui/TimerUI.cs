using Game.Manager;
using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class TimerUI : CanvasLayer
    {
        [Node]
        private Label label;

        private EnemyManager enemyManager;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _Process(float delta)
        {
            label.Text = string.Format("{0:0.0}", enemyManager?.Timer?.TimeLeft ?? 0);
        }

        public void ConnectEnemyManager(EnemyManager enemyManager)
        {
            this.enemyManager = enemyManager;
        }
    }
}
