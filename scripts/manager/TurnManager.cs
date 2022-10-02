using Godot;
using GodotUtilities;

namespace Game.Manager
{
    public class TurnManager : Node
    {
        [Signal]
        public delegate void PlayerTurnStarted();
        [Signal]
        public delegate void EnemyTurnStarted();

        public bool IsPlayerTurn { get; private set; }

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
            CallDeferred(nameof(EndTurn));
        }

        public void EndTurn()
        {
            IsPlayerTurn = !IsPlayerTurn;
            EmitSignal(IsPlayerTurn ? nameof(PlayerTurnStarted) : nameof(EnemyTurnStarted));
        }
    }
}
