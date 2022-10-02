using Game.Manager;
using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class Enemy : Node2D
    {
        [Node]
        private ResourcePreloader resourcePreloader;

        private GameBoard gameBoard;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            gameBoard = this.GetAncestor<GameBoard>();
            gameBoard.TurnManager.Connect(nameof(TurnManager.EnemyTurnStarted), this, nameof(OnEnemyTurnStarted));
        }

        private void DoAttackStraight()
        {
            var directions = new Vector2[] {
                Vector2.Up,
                Vector2.Left,
                Vector2.Down,
                Vector2.Right
            };
            var chosenDirectionIndex = MathUtil.RNG.RandiRange(0, directions.Length - 1);
            var chosenDirection = directions[chosenDirectionIndex];
            var currentTile = gameBoard.WorldToTile(GlobalPosition);
            var attack = resourcePreloader.InstanceSceneOrNull<AttackStraight>();
            gameBoard.AddChild(attack);

            attack.SetInitialTile(currentTile + chosenDirection);
            attack.Direction = chosenDirection;
        }

        private void OnEnemyTurnStarted()
        {
            DoAttackStraight();
            gameBoard.TurnManager.EndTurn();
        }
    }
}
