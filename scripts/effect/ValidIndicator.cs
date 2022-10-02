using Godot;
using GodotUtilities;

namespace Game.Effect
{
    public class ValidIndicator : Node2D
    {
        [Node]
        private AnimationPlayer animationPlayer;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.AddToGroup();
                this.WireNodes();
            }
        }

        public void Die()
        {
            animationPlayer.Play("die");
        }
    }
}
