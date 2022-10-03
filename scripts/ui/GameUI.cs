using Game.Effect;
using Game.GameObject;
using Game.Manager;
using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class GameUI : CanvasLayer
    {
        [Signal]
        public delegate void ShieldPressed();

        [Node]
        private Label turnLabel;
        [Node]
        private Button shieldButton;
        [Node]
        private HBoxContainer heartContainer;

        private GameBoard gameBoard;
        private int shieldCooldown;

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
            gameBoard.TurnManager.Connect(nameof(TurnManager.TurnChanged), this, nameof(OnTurnChanged));
            gameBoard.TurnManager.Connect(nameof(TurnManager.PlayerTurnStarted), this, nameof(OnPlayerTurnStarted));

            shieldButton.Connect("pressed", this, nameof(OnShieldPressed));
        }

        public void ConnectPlayer(Player player)
        {
            player.Connect(nameof(Player.Damaged), this, nameof(OnPlayerDamaged));
        }

        private void UpdateShieldButton()
        {
            shieldButton.Text = shieldCooldown > 0 ? $"Shield ({shieldCooldown})" : "Shield";
            shieldButton.Disabled = !gameBoard.TurnManager.IsPlayerTurn || shieldCooldown > 0;
        }

        private void OnTurnChanged(int turnCount)
        {
            turnLabel.Text = (10 - turnCount).ToString();
            UpdateShieldButton();
        }

        private void OnPlayerTurnStarted(object _)
        {
            shieldCooldown = Mathf.Max(shieldCooldown - 1, 0);
            UpdateShieldButton();
        }

        private void OnShieldPressed()
        {
            shieldCooldown = 3;
            UpdateShieldButton();
            EmitSignal(nameof(ShieldPressed));
        }

        private void OnPlayerDamaged()
        {
            var childCount = heartContainer.GetChildCount();
            if (childCount == 0) return;
            heartContainer.GetChild<Heart>(childCount - 1).Die();
        }
    }
}
