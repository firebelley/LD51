using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class FloatingText : Node2D
    {
        [Export]
        private Color red;

        [Node]
        private Label label;
        [Node]
        private Label backgroundLabel;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public void SetText(string text)
        {
            label.Text = text;
            backgroundLabel.Text = text;
        }

        public void SetRed()
        {
            label.AddColorOverride("font_color", red);
        }
    }
}
