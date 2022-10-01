using System;
using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class TimerUI : CanvasLayer
    {
        [Node]
        private Label label;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            GetParent<Main>().Connect(nameof(Main.SecondsChanged), this, nameof(OnSecondsChanged));
        }

        private void OnSecondsChanged(float seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            label.Text = string.Format("{0:D1}:{1:D2}", t.Minutes, t.Seconds);
        }
    }
}
