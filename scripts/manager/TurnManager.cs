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
        public delegate void TenthTurnStarted();

        public bool IsPlayerTurn { get; private set; } = true;
        private int turnCounter;

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
            turnCounter++;
            var isTenthTurn = turnCounter == 10;

            EmitSignal(IsPlayerTurn ? nameof(PlayerTurnStarted) : nameof(EnemyTurnStarted), isTenthTurn);
            if (isTenthTurn)
            {
                turnCounter = 0;
                EmitSignal(nameof(TenthTurnStarted));
            }
        }

        private void Init()
        {
            EmitSignal(nameof(PlayerTurnStarted), false);
        }
    }
}