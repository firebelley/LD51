using Godot;
using GodotUtilities;

namespace Game.Effect
{
    public class Heart : CenterContainer
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

        public void Die()
        {
            animationPlayer.Play("die");
        }
    }
}
