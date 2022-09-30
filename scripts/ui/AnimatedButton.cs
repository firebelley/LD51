using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class AnimatedButton : Button
    {
        [Node]
        private AnimationPlayer animationPlayer;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            Connect("pressed", this, nameof(OnPressed));
        }

        public override void _Process(float delta)
        {
            RectPivotOffset = RectSize / 2f;
        }

        private void OnPressed()
        {
            if (animationPlayer.IsPlaying())
            {
                animationPlayer.Seek(0f, true);
                animationPlayer.Stop();
            }
            animationPlayer.Play("default");
        }
    }
}
