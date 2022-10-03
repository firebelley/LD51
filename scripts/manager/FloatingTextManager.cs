using Game.UI;
using Godot;
using GodotUtilities;

namespace Game.Manager
{
    public class FloatingTextManager : Node2D
    {
        [Node]
        private ResourcePreloader resourcePreloader;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.AddToGroup();
                this.WireNodes();
            }
        }

        public FloatingText SpawnText(string text, Color? color = null)
        {
            var floatingText = resourcePreloader.InstanceSceneOrNull<FloatingText>();
            AddChild(floatingText);
            floatingText.SetText(text);
            return floatingText;
        }
    }
}
