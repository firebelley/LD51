using Godot;
using GodotUtilities;

namespace Game.Entity
{
    public class Clock : KinematicBody2D
    {
        [Node]
        private Area2D area2d;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            area2d.Connect("area_entered", this, nameof(OnAreaEntered));
        }

        private void OnAreaEntered(object _)
        {
            this.GetAncestor<Main>().SecondsKilled(.5f);
            QueueFree();
        }
    }
}
