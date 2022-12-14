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
        [Signal]
        public delegate void LeapPressed();
        [Signal]
        public delegate void SkipPressed();

        [Node]
        private Label turnLabel;
        [Node]
        private Button shieldButton;
        [Node]
        private Button leapButton;
        [Node]
        private Button skipButton;
        [Node]
        private HBoxContainer heartContainer;
        [Node]
        private Label headerLabel;
        [Node]
        private Button optionsButton;

        private GameBoard gameBoard;
        private int shieldCooldown;
        private int leapCooldown;

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
            leapButton.Connect("pressed", this, nameof(OnLeapPressed));
            skipButton.Connect("pressed", this, nameof(OnSkipPressed));
            optionsButton.Connect("pressed", this, nameof(OnOptionsPressed));
        }

        public void ConnectPlayer(Player player)
        {
            player.Connect(nameof(Player.Damaged), this, nameof(OnPlayerDamaged));
        }

        private void UpdateShieldButton()
        {
            shieldButton.Text = shieldCooldown > 0 ? $"({shieldCooldown})" : "SHIELD";
            shieldButton.Disabled = !gameBoard.TurnManager.IsPlayerTurn || shieldCooldown > 0;
        }

        private void UpdateLeapButton()
        {
            leapButton.Text = leapCooldown > 0 ? $"({leapCooldown})" : "LEAP";
            leapButton.Disabled = !gameBoard.TurnManager.IsPlayerTurn || leapCooldown > 0;
        }

        private void UpdateSkipButton()
        {
            skipButton.Disabled = !gameBoard.TurnManager.IsPlayerTurn;
        }

        private void OnTurnChanged(int turnCount)
        {
            turnLabel.Text = (5 - turnCount).ToString();
            if (turnCount == 0)
            {
                var isInvuln = GetTree().GetFirstNodeInGroup<TurnManager>().IsInvulnerabilityStage;
                headerLabel.Text = isInvuln ? "Seconds to\nVulnerability" : "Seconds to\nInvulnerability";
            }
            UpdateShieldButton();
            UpdateLeapButton();
            UpdateSkipButton();
        }

        private void OnPlayerTurnStarted(object _)
        {
            shieldCooldown = Mathf.Max(shieldCooldown - 1, 0);
            leapCooldown = Mathf.Max(leapCooldown - 1, 0);
            UpdateShieldButton();
            UpdateLeapButton();
            UpdateSkipButton();
        }

        private async void OnOptionsPressed()
        {
            GetNode("/root/ScreenTransition").Call("transition");
            await ToSignal(GetNode("/root/ScreenTransition"), "transitioned_halfway");
            GetParent().AddChild(GD.Load<PackedScene>("res://scenes/ui/OptionsMenu.tscn").Instance());
        }

        private void OnShieldPressed()
        {
            shieldCooldown = 3;
            UpdateShieldButton();
            EmitSignal(nameof(ShieldPressed));
        }

        private void OnLeapPressed()
        {
            leapCooldown = 4;
            UpdateLeapButton();
            EmitSignal(nameof(LeapPressed));
        }

        private void OnSkipPressed()
        {
            EmitSignal(nameof(SkipPressed));
        }

        private void OnPlayerDamaged()
        {
            var childCount = heartContainer.GetChildCount();
            if (childCount == 0) return;
            heartContainer.GetChild<Heart>(childCount - 1).Die();
        }
    }
}
