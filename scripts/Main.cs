using Godot;

namespace Game
{
    public class Main : Node
    {
        public override void _EnterTree()
        {
            GD.Print("hello");
        }
    }
}
