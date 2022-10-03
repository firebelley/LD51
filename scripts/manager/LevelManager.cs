using Godot;
using GodotUtilities;

namespace Game.Manager
{
    public class LevelManager : Node
    {
        [Export]
        private Color transitionColor;
        [Export]
        private PackedScene[] levels = new PackedScene[0];

        private static LevelManager instance;
        private int currentLevel;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                instance = this;
                this.AddToGroup();
                this.WireNodes();
            }
        }

        public static void Start()
        {
            instance.currentLevel = 0;
            instance.Switch();
        }

        public static void Advance()
        {
            instance.currentLevel++;
            instance.Switch();
        }

        public static void Restart()
        {
            instance.Switch();
        }

        private void Switch()
        {
            GetNode("/root/ScreenTransition").Call("set_transition_color", transitionColor);
            if (currentLevel >= levels.Length)
            {
                GetNode("/root/ScreenTransition").Call("transition_to_scene", "res://scenes/ui/Complete.tscn");
            }
            else
            {
                GetNode("/root/ScreenTransition").Call("transition_to_scene", levels[currentLevel].ResourcePath);
            }
        }
    }
}
