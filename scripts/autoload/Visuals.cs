using Godot;

namespace Game.Autoload
{
    public class Visuals : Node
    {
        [Export]
        private Color clearColor;

        public override void _EnterTree()
        {
            VisualServer.SetDefaultClearColor(clearColor);
        }
    }
}
