using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class Cursor : CanvasLayer
    {
        [Node]
        private AnimationPlayer animationPlayer;
        [Node]
        private Node2D visuals;
        [Node]
        private Sprite attackSprite;
        [Node]
        private Sprite normalSprite;

        private static Cursor instance;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                instance = this;
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            Input.MouseMode = Input.MouseModeEnum.Hidden;
        }

        public override void _Process(float delta)
        {
            if (Input.IsActionJustPressed("click"))
            {
                animationPlayer.Stop(true);
                animationPlayer.Play("default");
            }
            visuals.GlobalPosition = visuals.GetGlobalMousePosition();
        }

        public static void UseAttack()
        {
            instance.attackSprite.Visible = true;
            instance.normalSprite.Visible = false;
        }

        public static void UseNormal()
        {
            instance.attackSprite.Visible = false;
            instance.normalSprite.Visible = true;
        }
    }
}
