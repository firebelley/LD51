using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Effect;
using Game.Manager;
using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class Enemy : Node2D
    {
        [Signal]
        public delegate void Died();

        [Node]
        private ResourcePreloader resourcePreloader;
        [Node]
        private AnimationPlayer animationPlayer;
        [Node]
        private AnimationPlayer dieAnimationPlayer;
        [Node]
        private Node randomAudioStreamPlayer;
        [Node]
        private Node shieldHitPlayer;

        private GameBoard gameBoard;
        private int health = 2;
        private bool isInvulnerable = true;
        private bool wasHit = false;
        private Vector2 lastDirection;
        private bool isDying;

        private enum AttackType
        {
            Line,
            Wall
        }

        private AttackType[] attackTypes = new AttackType[] {
            AttackType.Line,
            AttackType.Line,
            AttackType.Wall
        };

        private Vector2[] directions = new Vector2[] {
            Vector2.Right,
            Vector2.Up,
            Vector2.Left,
            Vector2.Down
        };

        private static int turns;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                turns = 0;
                this.AddToGroup();
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            gameBoard = this.GetAncestor<GameBoard>();
            gameBoard.TurnManager.Connect(nameof(TurnManager.PlayerTurnStarted), this, nameof(OnPlayerTurnStarted));
            gameBoard.TurnManager.Connect(nameof(TurnManager.EnemyTurnStarted), this, nameof(OnEnemyTurnStarted));

            UpdatePosition();
            UpdateShield();
        }

        public void Damage()
        {
            GameCamera.Shake();
            wasHit = true;
            if (isInvulnerable)
            {
                shieldHitPlayer.Call("play_times", 2);
                var text = gameBoard.FloatingTextManager.SpawnText("Blocked!");
                text.SetRed();
                text.GlobalPosition = GlobalPosition + (Vector2.Up * 24f);
                return;
            }
            health--;
            randomAudioStreamPlayer.Call("play_times", 2);
            shieldHitPlayer.Call("play");
            if (health <= 0)
            {
                var text = gameBoard.FloatingTextManager.SpawnText("Slain!");
                text.GlobalPosition = GlobalPosition + (Vector2.Up * 24f);
                gameBoard.EnemyPositions.Remove(this);
                EmitSignal(nameof(Died));
                animationPlayer.Stop(true);
                dieAnimationPlayer.Play("die");
                isDying = true;
            }
            else
            {
                var text = gameBoard.FloatingTextManager.SpawnText("Hit!");
                text.GlobalPosition = GlobalPosition + (Vector2.Up * 24f);
            }
        }

        private void UpdateShield()
        {
            if (isInvulnerable)
            {
                var text = gameBoard.FloatingTextManager.SpawnText("Invulnerable!");
                text.GlobalPosition = GlobalPosition + (Vector2.Up * 24f);
                var shield = resourcePreloader.InstanceSceneOrNull<ShieldIndicator>();
                AddChild(shield);
                shield.Position = Vector2.Down * 4f;
            }
            else
            {
                var text = gameBoard.FloatingTextManager.SpawnText("Vulnerable!");
                text.GlobalPosition = GlobalPosition + (Vector2.Up * 24f);
                this.GetFirstNodeOfType<ShieldIndicator>()?.Die();
            }
        }

        private void UpdatePosition()
        {
            gameBoard.EnemyPositions[this] = gameBoard.WorldToTile(GlobalPosition);
        }

        private void DoAttackStraight()
        {
            var directions = new Vector2[] {
                Vector2.Up,
                Vector2.Left,
                Vector2.Down,
                Vector2.Right
            };
            var currentTile = gameBoard.WorldToTile(GlobalPosition);
            directions = directions.Where(x => x != lastDirection && gameBoard.IsTileValid(currentTile + x)).ToArray();

            var chosenDirectionIndex = MathUtil.RNG.RandiRange(0, directions.Length - 1);
            var chosenDirection = directions[chosenDirectionIndex];

            var chosenAttackIndex = MathUtil.RNG.RandiRange(0, attackTypes.Length - 1);
            var chosenAttack = attackTypes[chosenAttackIndex];


            var attackSquares = new List<Vector2>();
            var emptyTiles = gameBoard.GetEmptyTiles().ToHashSet();
            var attackCount = chosenAttack == AttackType.Line ? 1 : 3;

            var rootTile = currentTile + chosenDirection;
            for (int i = 0; i < attackCount; i++)
            {
                if (i == 0)
                {
                    attackSquares.Add(rootTile);
                    continue;
                }
                var perp = chosenDirection.Perpendicular();
                if (i == 2)
                {
                    perp *= -1;
                }

                var desiredPos = rootTile + perp;
                if (emptyTiles.Contains(desiredPos))
                {
                    attackSquares.Add(rootTile + perp);
                }
            }

            foreach (var attackSquare in attackSquares)
            {
                var attack = resourcePreloader.InstanceSceneOrNull<AttackStraight>();
                gameBoard.Entities.AddChild(attack);
                attack.GlobalPosition = gameBoard.TileToWorld(attackSquare);
                attack.SetInitialTile(attackSquare);
                attack.Direction = chosenDirection;
            }

            lastDirection = chosenDirection;
        }

        private async Task DoTeleport()
        {
            var emptyTiles = gameBoard.GetEmptyTiles();
            var emptyHash = emptyTiles.ToHashSet();
            emptyTiles = emptyTiles.Where((x) =>
            {
                foreach (var dir in directions)
                {
                    if (!emptyHash.Contains(x + dir))
                    {
                        return false;
                    }
                }
                return true;
            }).ToArray();
            if (emptyTiles.Length == 0) return;

            var index = MathUtil.RNG.RandiRange(0, emptyTiles.Length - 1);
            var destinationTile = emptyTiles[index];

            animationPlayer.Play("teleport_out");
            await ToSignal(animationPlayer, "animation_finished");

            GlobalPosition = gameBoard.TileToWorld(destinationTile);
            UpdatePosition();
            animationPlayer.Play("teleport_in");
            await ToSignal(animationPlayer, "animation_finished");

            animationPlayer.Play("idle");
        }

        private async void DoTurn()
        {
            if (wasHit)
            {
                await DoTeleport();
            }
            else
            {
                DoAttackStraight();
            }
            await ToSignal(GetTree().CreateTimer(.4f), "timeout");

            wasHit = false;
            turns--;
            if (turns == 0)
            {
                gameBoard.TurnManager.EndTurn();
            }
        }

        private void OnPlayerTurnStarted(bool isTenthTurn)
        {
            if (isTenthTurn)
            {
                isInvulnerable = !isInvulnerable;
                UpdateShield();
            }
        }

        private void OnEnemyTurnStarted(bool isTenthTurn)
        {
            if (IsQueuedForDeletion() || isDying) return;
            turns++;
            if (isTenthTurn)
            {
                isInvulnerable = !isInvulnerable;
            }
            DoTurn();
        }
    }
}
