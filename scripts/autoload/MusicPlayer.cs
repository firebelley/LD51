using Godot;
using GodotUtilities;

namespace Game.Autoload
{
    public class MusicPlayer : Node
    {
        [Node]
        private AudioStreamPlayer audioStreamPlayer;
        [Node]
        private Timer timer;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            audioStreamPlayer.Connect("finished", this, nameof(OnFinished));
            timer.Connect("timeout", this, nameof(OnTimeout));
        }

        private void OnTimeout()
        {
            audioStreamPlayer.Play();
        }

        private void OnFinished()
        {
            timer.Start();
        }
    }
}
