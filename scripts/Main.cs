using Game.Manager;
using Game.UI;
using Godot;
using GodotUtilities;

namespace Game
{
    public class Main : Node
    {
        [Node]
        private TimerUI timerUI;
        [Node]
        private EnemyManager enemyManager;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            GD.Print(timerUI);
            timerUI.ConnectEnemyManager(enemyManager);
        }
    }
}
