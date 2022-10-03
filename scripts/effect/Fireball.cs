using System.Threading.Tasks;
using Godot;
using GodotUtilities;

namespace Game.Effect
{
    public class Fireball : Node2D
    {
        [Node]
        private Particles2D particles2d;
        [Node]
        private Node randomAudioStreamPlayer;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public void Start()
        {
            if (!particles2d.Emitting)
            {
                randomAudioStreamPlayer.Call("play");
            }
            particles2d.Emitting = true;
        }

        public async Task Die()
        {
            particles2d.Emitting = false;
            await ToSignal(GetTree().CreateTimer(.5f), "timeout");
        }
    }
}
