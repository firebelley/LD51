using Godot;
using GodotUtilities;

namespace Game
{
    public class Main : Node
    {
        [Signal]
        public delegate void SecondsChanged(float seconds);

        [Node]
        public Node2D Enemies { get; private set; }

        private float seconds = 60;
        private float Seconds
        {
            get => seconds;
            set
            {
                seconds = value;
                EmitSignal(nameof(SecondsChanged), value);
            }
        }

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            EmitSignal(nameof(SecondsChanged), Seconds);
        }

        public void SecondsKilled(float seconds)
        {
            Seconds -= seconds;
        }
    }
}
