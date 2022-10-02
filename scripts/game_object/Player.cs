using System.Threading.Tasks;
using Game.Manager;
using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class Player : Node2D
    {
        [Signal]
        public delegate void Moved(Vector2 toTile);

        [Node]
        private AnimationPlayer animationPlayer;

        private GameBoard gameBoard;
        private SceneTreeTween tween;

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

        private async Task MoveToTile(Vector2 tile)
        {
            animationPlayer.Stop(true);
            animationPlayer.Play("move");

            tween = CreateTween();
            tween.TweenProperty(this, "global_position", gameBoard.TileToWorld(tile), .3f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
            tween.TweenCallback(this, nameof(KillTween));

            await ToSignal(tween, "finished");
        }

        private void OnPlayerTurnStarted(bool isTenthTurn)
        {
            // hasControl = true;
        }

        private async void OnTileClicked(Vector2 tile)
        {
            if (tween?.IsValid() == true)
            {
                return;
            }
            if (gameBoard.EnemyTile == tile)
            {
                GetTree().GetFirstNodeInGroup<Enemy>().Damage();
            }
            else
            {
                await MoveToTile(tile);
            }
            gameBoard.TurnManager.EndTurn();
        }

        private void KillTween()
        {
            if (tween?.IsValid() == true)
            {
                tween.Kill();
            }
        }
    }
}
