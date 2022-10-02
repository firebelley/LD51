using Game.Manager;
using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class Player : Node2D
    {
        [Signal]
        public delegate void Moved(Vector2 toTile);

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
            gameBoard.Connect(nameof(GameBoard.TileClicked), this, nameof(OnTileClicked));
            gameBoard.TurnManager.Connect(nameof(TurnManager.PlayerTurnStarted), this, nameof(OnPlayerTurnStarted));
            gameBoard.PlayerTile = gameBoard.WorldToTile(GlobalPosition);
        }

        private void OnPlayerTurnStarted(bool isTenthTurn)
        {
            // hasControl = true;
        }

        private void OnTileClicked(Vector2 tile, Vector2 globalCenter)
        {
            if (gameBoard.EnemyTile == tile)
            {
                GetTree().GetFirstNodeInGroup<Enemy>().Damage();
            }
            else
            {
                GlobalPosition = globalCenter;
            }
            gameBoard.TurnManager.EndTurn();
        }
    }
}
