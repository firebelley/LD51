using System.Collections.Generic;
using System.Linq;
using Game.Effect;
using Game.Manager;
using Godot;
using GodotUtilities;

namespace Game
{
    public class GameBoard : Node
    {
        [Signal]
        public delegate void TileClicked(Vector2 tileClicked);

        [Node]
        private ResourcePreloader resourcePreloader;
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
                    EmitSignal(nameof(TileClicked), tile);
                }
                GetTree().SetInputAsHandled();
            }
        }

        public override void _Ready()
        {
            validTiles = tileMap.GetUsedCells().Cast<Vector2>().ToHashSet();
            TurnManager.Connect(nameof(TurnManager.TurnChanged), this, nameof(OnTurnChanged));
        }

        public HashSet<Vector2> GetValidTiles()
        {
            return validTiles;
        }

        public bool IsTileValid(Vector2 tile)
        {
            return validTiles.Contains(tile);
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

        public void IndicateValidTiles(Vector2[] tiles)
        {
            foreach (var tile in tiles)
            {
                var indicator = resourcePreloader.InstanceSceneOrNull<ValidIndicator>();
                tileMap.AddChild(indicator);
                indicator.GlobalPosition = TileToWorld(tile);
            }
        }

        public void ClearIndicators()
        {
            GetTree().CallGroup(nameof(ValidIndicator), nameof(ValidIndicator.Die));
        }

        private void OnTurnChanged()
        {
        }
    }
}
