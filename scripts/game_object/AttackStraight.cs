using Game.Manager;
using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class AttackStraight : Node2D
    {
        public Vector2 Direction;

        private Vector2? currentTile;
        private Vector2? nextTile;

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
            CallDeferred(nameof(Initialize));
        }

        public void SetInitialTile(Vector2 initialTile)
        {
            nextTile = initialTile;
        }

        private void Initialize()
        {
            Visible = false;
        }

        private void Advance()
        {
            Visible = true;
            currentTile = nextTile;

            if (currentTile == null)
            {
                QueueFree();
                return;
            }

            GlobalPosition = gameBoard.TileToWorld(currentTile.Value);

            var presumptiveNext = currentTile.Value + Direction;
            if (gameBoard.GetValidTiles().Contains(presumptiveNext))
            {
                nextTile = presumptiveNext;
            }
            else
            {
                nextTile = null;
            }
        }

        private void OnEnemyTurnStarted()
        {
            Advance();
        }
    }
}
