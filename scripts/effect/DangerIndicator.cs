using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class DangerIndicator : Node2D
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
