using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class GameCamera : Node2D
    {

        [Node]
        private Camera2D shakyCamera2d;

        private static GameCamera instance;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                instance = this;
                this.WireNodes();
            }
        }

        public static void Shake()
        {
            instance.shakyCamera2d.Call("shake");
        }
    }
}
