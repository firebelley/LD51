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
        [Signal]
        public delegate void Damaged();

        [Node]
        private AnimationPlayer animationPlayer;
        [Node]
        private Node2D visuals;
        [Node]
        private ResourcePreloader resourcePreloader;
        [Node]
        private Node shieldHitPlayer;
        [Node]
        private Node footstepPlayer;
        [Node]
        private Node footstepPlayer2;

        private GameBoard gameBoard;
        private SceneTreeTween tween;
        private List<Vector2> validMovementTiles = new();
        private List<Vector2> validEnemyTiles = new();
        private ShieldIndicator shieldIndicator;

        private int health = 3;
        private bool isActing = false;
        private int enemyCount;
        private bool IsDying => health <= 0;

        private TileCursor tileCursor;

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
            foreach (var enemy in GetTree().GetNodesInGroup<Enemy>())
            {
                enemyCount++;
                enemy.Connect(nameof(Enemy.Died), this, nameof(OnEnemyDied));
            }

            tileCursor = resourcePreloader.InstanceSceneOrNull<TileCursor>();
            gameBoard.TileMap.AddChild(tileCursor);
        }

        public override void _Process(float delta)
        {
            var mouseTile = gameBoard.WorldToTile(GetGlobalMousePosition());
            if (gameBoard.TurnManager.IsPlayerTurn)
            {
                if (validEnemyTiles.Contains(mouseTile))
                {
                    Cursor.UseAttack();
                    tileCursor.SetValid(true);
                }
                else
                {
                    tileCursor.SetValid(validMovementTiles.Contains(mouseTile));
                    Cursor.UseNormal();
                }
            }
            else
            {
                Cursor.UseNormal();
                tileCursor.SetValid(false);
            }
            tileCursor.Visible = gameBoard.GetValidTiles().Contains(mouseTile);
            tileCursor.GlobalPosition = gameBoard.TileToWorld(mouseTile);
        }

        public void ConnectUI(GameUI gameUI)
        {
            gameUI.Connect(nameof(GameUI.ShieldPressed), this, nameof(OnShieldPressed));
            gameUI.Connect(nameof(GameUI.LeapPressed), this, nameof(OnLeapPressed));
            gameUI.Connect(nameof(GameUI.SkipPressed), this, nameof(OnSkipPressed));
            gameUI.ConnectPlayer(this);
        }

        public async void Damage()
        {
            GameCamera.Shake();
            if (IsInstanceValid(shieldIndicator))
            {
                shieldHitPlayer.Call("play_times", 2);
                var text = gameBoard.FloatingTextManager.SpawnText("Blocked!");
                text.GlobalPosition = GlobalPosition + (Vector2.Up * 24f);
                return;
            }
            health--;
            var text2 = gameBoard.FloatingTextManager.SpawnText(health <= 0 ? "Dead!" : "Oof!");
            text2.GlobalPosition = GlobalPosition + (Vector2.Up * 24f);
            text2.SetRed();
            EmitSignal(nameof(Damaged));

            if (IsDying)
            {
                animationPlayer.Play("die");
                await ToSignal(animationPlayer, "animation_finished");
                LevelManager.Restart();
            }
        }

        private async Task MoveToTile(Vector2 tile)
        {
            animationPlayer.Stop(true);
            animationPlayer.Play("move");

            tween = CreateTween();
            tween.SetParallel();
            tween.TweenProperty(this, "global_position", gameBoard.TileToWorld(tile), .3f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
            tween.TweenCallback(footstepPlayer, "play").SetDelay(.2f);
            tween.TweenCallback(footstepPlayer2, "play").SetDelay(.2f);
            tween.Chain();
            tween.TweenCallback(this, nameof(KillTween));

            FaceTile(tile);

            await ToSignal(tween, "finished");
            animationPlayer.Play("idle");
        }


        private async Task AttackTile(Vector2 tile)
        {
            animationPlayer.Play("attack");

            var originalPosition = GlobalPosition;
            tween = CreateTween();
            tween.TweenInterval(.15f);
            tween.TweenCallback(gameBoard.GetEnemyAtTile(tile), nameof(Enemy.Damage));
            tween.TweenInterval(.3f);
            tween.TweenCallback(this, nameof(KillTween));

            FaceTile(tile);

            await ToSignal(tween, "finished");
            animationPlayer.Play("idle");
        }

        private void FaceTile(Vector2 tile)
        {
            var xsign = Mathf.Sign((tile - gameBoard.WorldToTile(GlobalPosition)).x);
            if (xsign != Mathf.Sign(visuals.Scale.x) && xsign != 0)
            {
                var scaleTween = CreateTween();
                scaleTween.TweenProperty(visuals, "scale", new Vector2(xsign, 1f), .3f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
            }
        }

        private void PopulateValidMovementTiles(int range = 1)
        {
            validMovementTiles.Clear();
            var tilePos = gameBoard.WorldToTile(GlobalPosition);
            var attackTiles = gameBoard.GetAttackTiles();
            foreach (var direction in moveDirections)
            {
                var newTile = (direction * range) + tilePos;
                if (gameBoard.IsTileValid(newTile) && !validEnemyTiles.Contains(newTile) && !attackTiles.Contains(newTile) && gameBoard.GetEnemyAtTile(newTile) == null)
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
            isActing = true;
            if (validEnemyTiles.Contains(tile))
            {
                await AttackTile(tile);
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
            if (IsDying) return;
            PopulateValidEnemyTiles();
            PopulateValidMovementTiles();
            ClearShield();
        }

        private async void OnTileClicked(Vector2 tile)
        {
            if (tween?.IsValid() == true || enemyCount == 0 || IsDying)
            {
                return;
            }

            var success = await HandleClick(tile);
            isActing = false;
            if (success && enemyCount > 0)
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
            if (isActing) return;
            shieldIndicator = resourcePreloader.InstanceSceneOrNull<ShieldIndicator>();
            AddChild(shieldIndicator);
            var text = gameBoard.FloatingTextManager.SpawnText("Shields up!");
            text.GlobalPosition = GlobalPosition + (Vector2.Up * 24f);
            EndTurn();
        }

        private void OnLeapPressed()
        {
            gameBoard.ClearIndicators();
            PopulateValidMovementTiles(3);
        }

        private void OnSkipPressed()
        {
            EndTurn();
        }

        private async void OnEnemyDied()
        {
            enemyCount--;
            if (enemyCount <= 0)
            {
                await ToSignal(GetTree().CreateTimer(1f), "timeout");
                LevelManager.Advance();
            }
        }
    }
}
