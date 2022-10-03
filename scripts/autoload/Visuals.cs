using Godot;

namespace Game.Autoload
{
    public class Visuals : Node
    {
        [Export]
        private Color clearColor;
        [Export]
        private Color transitionColor;

        public override void _EnterTree()
        {
            VisualServer.SetDefaultClearColor(clearColor);
        }

        public override void _Ready()
        {
            GetNode("/root/ScreenTransition").Call("set_transition_color", transitionColor);
        }
    }
}
