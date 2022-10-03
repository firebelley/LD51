using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class PauseMenu : CanvasLayer
    {
        [Signal]
        public delegate void LevelSelectPressed();

        [Node]
        private Button optionsButton;
        [Node]
        private Button resumeButton;
        [Node]
        private Button quitButton;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
            else if (what == NotificationExitTree)
            {
                GetTree().Paused = false;
            }
        }

        public override void _Ready()
        {
            GetTree().Paused = true;
            optionsButton.Connect("pressed", this, nameof(OnOptionsPressed));
            resumeButton.Connect("pressed", this, nameof(OnResumePressed));
            quitButton.Connect("pressed", this, nameof(OnQuitPressed));
        }

        public override void _UnhandledInput(InputEvent evt)
        {
            if (evt.IsActionPressed("pause"))
            {
                GetTree().SetInputAsHandled();
                OnResumePressed();
            }
        }

        public override void _Process(float delta)
        {
            if (!GetTree().Paused)
            {
                QueueFree();
            }
        }

        private async void OnOptionsPressed()
        {
            GetNode("/root/ScreenTransition").Call("transition");
            await ToSignal(GetNode("/root/ScreenTransition"), "transitioned_halfway");
            var options = GD.Load<PackedScene>("res://scenes/ui/OptionsMenu.tscn");
            AddChild(options.Instance());
        }

        private void OnResumePressed()
        {
            GetTree().Paused = false;
        }

        private async void OnQuitPressed()
        {
            GetNode("/root/ScreenTransition").Call("transition");
            await ToSignal(GetNode("/root/ScreenTransition"), "transitioned_halfway");
            GetTree().Paused = false;
            GetTree().ChangeScene("res://scenes/Title.tscn");
        }
    }
}
