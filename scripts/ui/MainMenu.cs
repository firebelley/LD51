using Game.Manager;
using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class MainMenu : CanvasLayer
    {
        [Node]
        private Button startButton;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            startButton.Connect("pressed", this, nameof(OnStartPressed));
        }

        private void OnStartPressed()
        {
            LevelManager.Start();
        }
    }
}
