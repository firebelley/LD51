using System.Collections.Generic;
using System.Linq;
using Game.Effect;
using Game.GameObject;
using Game.Manager;
using Game.UI;
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
        public TileMap TileMap { get; private set; }
        [Node]
        private GameUI gameUi;
        [Node]
        private Player player;

        private HashSet<Vector2> validTiles = new();
        public Dictionary<Vector2, Enemy> EnemyPositions = new();

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
            validTiles = TileMap.GetUsedCells().Cast<Vector2>().ToHashSet();
            player.ConnectUI(gameUi);
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
            return TileMap.WorldToMap(TileMap.GetGlobalMousePosition());
        }

        public Vector2 WorldToTile(Vector2 position)
        {
            return TileMap.WorldToMap(position);
        }

        public Vector2 TileToWorld(Vector2 tile)
        {
            return TileMap.MapToWorld(tile) + (TileMap.CellSize / 2f);
        }

        public void IndicateValidTiles(Vector2[] tiles)
        {
            foreach (var tile in tiles)
            {
                var indicator = resourcePreloader.InstanceSceneOrNull<ValidIndicator>();
                TileMap.AddChild(indicator);
                indicator.GlobalPosition = TileToWorld(tile);
            }
        }

        public void ClearIndicators()
        {
            GetTree().CallGroup(nameof(ValidIndicator), nameof(ValidIndicator.Die));
        }

        public Enemy GetEnemyAtTile(Vector2 tile)
        {
            EnemyPositions.TryGetValue(tile, out var enemy);
            return enemy;
        }
    }
}
