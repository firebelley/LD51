using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Effect;
using Game.Manager;
using Game.UI;
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
        [Node]
        private ResourcePreloader resourcePreloader;

        private GameBoard gameBoard;
        private SceneTreeTween tween;
        private List<Vector2> validMovementTiles = new();
        private List<Vector2> validEnemyTiles = new();
        private ShieldIndicator shieldIndicator;

        private int health = 3;

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
                this.AddToGroup();
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            gameBoard = this.GetAncestor<GameBoard>();
            gameBoard.Connect(nameof(GameBoard.TileClicked), this, nameof(OnTileClicked));
            gameBoard.TurnManager.Connect(nameof(TurnManager.PlayerTurnStarted), this, nameof(OnPlayerTurnStarted));
            gameBoard.TurnManager.Connect(nameof(TurnManager.TurnChanged), this, nameof(OnTurnChanged));
        }

        public void ConnectUI(GameUI gameUI)
        {
            gameUI.Connect(nameof(GameUI.ShieldPressed), this, nameof(OnShieldPressed));
        }

        public void Damage()
        {
            health--;
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
                if (gameBoard.IsTileValid(newTile) && !validEnemyTiles.Contains(newTile))
                {
                    validMovementTiles.Add(newTile);
                }
            }
            gameBoard.IndicateValidTiles(validMovementTiles.ToArray());
        }

        private void PopulateValidEnemyTiles()
        {
            var tilePos = gameBoard.WorldToTile(GlobalPosition);
            foreach (var direction in moveDirections)
            {
                var newTile = direction + tilePos;
                if (gameBoard.GetEnemyAtTile(newTile) != null)
                {
                    validEnemyTiles.Add(newTile);
                }
            }
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
            if (validEnemyTiles.Contains(tile))
            {
                gameBoard.GetEnemyAtTile(tile).Damage();
                return true;
            }
            else if (validMovementTiles.Contains(tile))
            {
                await MoveToTile(tile);
                return true;
            }
            return false;
        }

        private void ClearShield()
        {
            this.GetFirstNodeOfType<ShieldIndicator>()?.Die();
        }

        private void EndTurn()
        {
            gameBoard.ClearIndicators();
            gameBoard.TurnManager.EndTurn();
        }

        private void OnPlayerTurnStarted(bool isTenthTurn)
        {
            PopulateValidEnemyTiles();
            PopulateValidMovementTiles();
            ClearShield();
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
                EndTurn();
            }
        }

        private void OnTurnChanged(int turnCount)
        {
            validMovementTiles.Clear();
            validEnemyTiles.Clear();
        }

        private void OnShieldPressed()
        {
            shieldIndicator = resourcePreloader.InstanceSceneOrNull<ShieldIndicator>();
            AddChild(shieldIndicator);
            EndTurn();
        }
    }
}
