using System.Collections.Generic;
using System.Linq;
using Game.Manager;
using Godot;
using GodotUtilities;

namespace Game
{
    public class GameBoard : Node
    {
        [Signal]
        public delegate void TileClicked(Vector2 tileClicked, Vector2 globalCenter);

        [Node]
        public TurnManager TurnManager { get; private set; }
        [Node]
        private TileMap tileMap;

        private HashSet<Vector2> validTiles = new();

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        public override void _UnhandledInput(InputEvent evt)
        {
            if (evt.IsActionPressed("click"))
            {
                var tile = MouseToTile();
                if (validTiles.Contains(tile))
                {
                    EmitSignal(nameof(TileClicked), tile, tileMap.MapToWorld(tile) + (tileMap.CellSize / 2f));
                }
                GetTree().SetInputAsHandled();
            }
        }

        public override void _Ready()
        {
            validTiles = tileMap.GetUsedCells().Cast<Vector2>().ToHashSet();
        }

        public HashSet<Vector2> GetValidTiles()
        {
            return validTiles;
        }

        public Vector2 MouseToTile()
        {
            return tileMap.WorldToMap(tileMap.GetGlobalMousePosition());
        }
    }
}
