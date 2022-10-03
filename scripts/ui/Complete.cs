using Game.Manager;
using Godot;
using GodotUtilities;

namespace Game
{
    public class Complete : CanvasLayer
    {
        [Node]
        private Button button;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            button.Connect("pressed", this, nameof(OnButtonPressed));
        }

        private void OnButtonPressed()
        {
            LevelManager.Start();
        }
    }
}
