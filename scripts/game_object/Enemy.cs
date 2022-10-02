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
        private int health = 2;
        private bool isInvulnerable = true;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.AddToGroup();
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            gameBoard = this.GetAncestor<GameBoard>();
            gameBoard.TurnManager.Connect(nameof(TurnManager.PlayerTurnStarted), this, nameof(OnPlayerTurnStarted));
            gameBoard.TurnManager.Connect(nameof(TurnManager.EnemyTurnStarted), this, nameof(OnEnemyTurnStarted));
            gameBoard.EnemyTile = gameBoard.WorldToTile(GlobalPosition);
        }

        public void Damage()
        {
            if (isInvulnerable) return;
            health--;
            if (health <= 0)
            {
                QueueFree();
            }
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
            gameBoard.Entities.AddChild(attack);

            attack.SetInitialTile(currentTile + chosenDirection);
            attack.Direction = chosenDirection;
        }

        private void OnPlayerTurnStarted(bool isTenthTurn)
        {
            if (isTenthTurn)
            {
                isInvulnerable = !isInvulnerable;
            }
        }

        private void OnEnemyTurnStarted(bool isTenthTurn)
        {
            if (isTenthTurn)
            {
                isInvulnerable = !isInvulnerable;
            }
            DoAttackStraight();
            gameBoard.TurnManager.EndTurn();
        }
    }
}
