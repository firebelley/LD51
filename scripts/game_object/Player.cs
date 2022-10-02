using System.Collections.Generic;
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
        [Node]
        private Node2D visuals;

        private GameBoard gameBoard;
        private SceneTreeTween tween;
        private List<Vector2> validMovementTiles = new();

        private Vector2[] moveDirections = new Vector2[] {
            Vector2.Right,
            Vector2.Up,
            Vector2.Left,
            Vector2.Down
        };

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
            gameBoard.TurnManager.Connect(nameof(TurnManager.TurnChanged), this, nameof(OnTurnChanged));
            gameBoard.PlayerTile = gameBoard.WorldToTile(GlobalPosition);
        }

        private async Task MoveToTile(Vector2 tile)
        {
            animationPlayer.Stop(true);
            animationPlayer.Play("move");

            tween = CreateTween();
            tween.TweenProperty(this, "global_position", gameBoard.TileToWorld(tile), .3f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
            tween.TweenCallback(this, nameof(KillTween));

            var xsign = Mathf.Sign((tile - gameBoard.WorldToTile(GlobalPosition)).x);
            if (xsign != Mathf.Sign(visuals.Scale.x) && xsign != 0)
            {
                var scaleTween = CreateTween();
                scaleTween.TweenProperty(visuals, "scale", new Vector2(xsign, 1f), .3f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
            }

            await ToSignal(tween, "finished");
        }

        private void PopulateValidMovementTiles()
        {
            var tilePos = gameBoard.WorldToTile(GlobalPosition);
            foreach (var direction in moveDirections)
            {
                var newTile = direction + tilePos;
                if (gameBoard.IsTileValid(newTile))
                {
                    validMovementTiles.Add(newTile);
                }
            }
            gameBoard.IndicateValidTiles(validMovementTiles.ToArray());
        }

        private void KillTween()
        {
            if (tween?.IsValid() == true)
            {
                tween.Kill();
            }
        }

        private async Task<bool> HandleClick(Vector2 tile)
        {
            if (gameBoard.EnemyTile == tile)
            {
                gameBoard.ClearIndicators();
                GetTree().GetFirstNodeInGroup<Enemy>().Damage();
                return true;
            }
            else if (validMovementTiles.Contains(tile))
            {
                gameBoard.ClearIndicators();
                await MoveToTile(tile);
                return true;
            }
            return false;
        }

        private void OnPlayerTurnStarted(bool isTenthTurn)
        {
            PopulateValidMovementTiles();
        }

        private async void OnTileClicked(Vector2 tile)
        {
            if (tween?.IsValid() == true)
            {
                return;
            }

            var success = await HandleClick(tile);
            if (success)
            {
                gameBoard.TurnManager.EndTurn();
            }
        }

        private void OnTurnChanged()
        {
            validMovementTiles.Clear();
        }
    }
}
