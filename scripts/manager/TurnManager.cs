using Godot;
using GodotUtilities;

namespace Game.Manager
{
    public class TurnManager : Node
    {
        [Signal]
        public delegate void PlayerTurnStarted(float isTenthTurn);
        [Signal]
        public delegate void EnemyTurnStarted(float isTenthTurn);
        [Signal]
        public delegate void TurnChanged(int turnCounter);

        public bool IsPlayerTurn { get; private set; } = true;
        private int turnCounter;
        public bool IsInvulnerabilityStage = true;

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
            CallDeferred(nameof(Init));
        }

        public void EndTurn()
        {
            IsPlayerTurn = !IsPlayerTurn;
            if (IsPlayerTurn)
            {
                turnCounter++;
            }
            var isTenthTurn = turnCounter == 5;

            if (isTenthTurn)
            {
                IsInvulnerabilityStage = !IsInvulnerabilityStage;
                turnCounter = 0;
            }
            EmitSignal(nameof(TurnChanged), turnCounter);
            EmitSignal(IsPlayerTurn ? nameof(PlayerTurnStarted) : nameof(EnemyTurnStarted), isTenthTurn);
        }

        private void Init()
        {
            EmitSignal(nameof(PlayerTurnStarted), false);
        }
    }
}
