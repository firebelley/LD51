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
        public Node2D Entities { get; private set; }
        [Node]
        private TileMap tileMap;

        private HashSet<Vector2> validTiles = new();

        public Vector2 PlayerTile;
        public Vector2 EnemyTile;

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
                    EmitSignal(nameof(TileClicked), tile, TileToWorld(tile));
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

        public Vector2 WorldToTile(Vector2 position)
        {
            return tileMap.WorldToMap(position);
        }

        public Vector2 TileToWorld(Vector2 tile)
        {
            return tileMap.MapToWorld(tile) + (tileMap.CellSize / 2f);
        }
    }
}
