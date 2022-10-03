using Godot;
using GodotUtilities;

namespace Game
{
    public class OptionsMenu : CanvasLayer
    {
        [Node]
        private Button sfxUpButton;
        [Node]
        private Button sfxDownButton;
        [Node]
        private Button musicUpButton;
        [Node]
        private Button musicDownButton;
        [Node]
        private Label sfxLabel;
        [Node]
        private Label musicLabel;
        [Node]
        private Button backButton;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            sfxUpButton.Connect("pressed", this, nameof(OnVolumeChanged), new Godot.Collections.Array { "sfx", 1 });
            sfxDownButton.Connect("pressed", this, nameof(OnVolumeChanged), new Godot.Collections.Array { "sfx", -1 });
            musicUpButton.Connect("pressed", this, nameof(OnVolumeChanged), new Godot.Collections.Array { "music", 1 });
            musicDownButton.Connect("pressed", this, nameof(OnVolumeChanged), new Godot.Collections.Array { "music", -1 });
            backButton.Connect("pressed", this, nameof(OnBackButtonPressed));

            UpdateText();
        }

        private void UpdateText()
        {
            sfxLabel.Text = Mathf.Round(GetBusVolume("sfx") * 10f).ToString();
            musicLabel.Text = Mathf.Round(GetBusVolume("music") * 10f).ToString();
        }

        private float GetBusVolume(string busName)
        {
            var busIdx = AudioServer.GetBusIndex(busName);
            var dbVol = AudioServer.GetBusVolumeDb(busIdx);
            var percent = GD.Db2Linear(dbVol);
            return percent;
        }

        private void SetBusVolume(string busName, float percent)
        {
            var busIdx = AudioServer.GetBusIndex(busName);
            var db = GD.Linear2Db(percent);
            AudioServer.SetBusVolumeDb(busIdx, db);
        }

        private void OnVolumeChanged(string bus, int change)
        {
            var busVolume = Mathf.Round(GetBusVolume(bus) * 10);
            busVolume = Mathf.Clamp(busVolume + change, 0, 10);
            SetBusVolume(bus, busVolume / 10f);
            UpdateText();
        }

        private async void OnBackButtonPressed()
        {
            GetNode("/root/ScreenTransition").Call("transition");
            await ToSignal(GetNode("/root/ScreenTransition"), "transitioned_halfway");
            QueueFree();
        }
    }
}
