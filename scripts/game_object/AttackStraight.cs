using System.Threading.Tasks;
using Game.Effect;
using Game.Manager;
using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class AttackStraight : Node2D
    {
        [Node]
        private ResourcePreloader resourcePreloader;
        [Node]
        private Fireball fireball;

        public Vector2 Direction;

        private Vector2? currentTile;
        private Vector2? nextTile;

        private GameBoard gameBoard;
        private DangerIndicator dangerIndicator;
        private bool isDying;

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

        public void SetInitialTile(Vector2 initialTile)
        {
            nextTile = initialTile;
            UpdateDangerIndicator();
        }

        private void UpdateDangerIndicator()
        {
            if (IsInstanceValid(dangerIndicator))
            {
                dangerIndicator.Die();
            }

            if (nextTile != null)
            {
                dangerIndicator = resourcePreloader.InstanceSceneOrNull<DangerIndicator>();
                gameBoard.TileMap.AddChild(dangerIndicator);
                dangerIndicator.GlobalPosition = gameBoard.TileToWorld(nextTile.Value);
            }
        }

        private async void Die()
        {
            isDying = true;
            if (IsInstanceValid(dangerIndicator))
            {
                dangerIndicator.Die();
            }
            await fireball.Die();
            QueueFree();
        }

        private bool CheckPlayerCollision()
        {
            var tile = gameBoard.WorldToTile(GlobalPosition);
            var player = GetTree().GetFirstNodeInGroup<Player>();
            if (player != null && tile == gameBoard.WorldToTile(player.GlobalPosition))
            {
                player.Damage();
                Die();
                return true;
            }
            return false;
        }

        private async Task MoveToPosition(Vector2 globalPosition)
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "global_position", globalPosition, .3f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
            await ToSignal(tween, "finished");
        }

        private async void Advance()
        {
            if (isDying) return;

            fireball.Start();
            currentTile = nextTile;

            if (currentTile == null)
            {
                QueueFree();
                UpdateDangerIndicator();
                return;
            }

            await MoveToPosition(gameBoard.TileToWorld(currentTile.Value));
            if (CheckPlayerCollision())
            {
                return;
            }

            var presumptiveNext = currentTile.Value + Direction;
            if (gameBoard.GetValidTiles().Contains(presumptiveNext))
            {
                nextTile = presumptiveNext;
            }
            else
            {
                nextTile = null;
            }
            UpdateDangerIndicator();
        }

        private void OnEnemyTurnStarted(bool isTenthTurn)
        {
            Advance();
        }
    }
}
