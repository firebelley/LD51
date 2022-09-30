using System.Collections.Generic;
using Godot;
using GodotUtilities;

namespace Game.GameObject
{
    public class Grapple : Node2D
    {
        private const int SEGMENT_LENGTH = 8;

        private float waveAmplitude = 0f;
        private float waveFrequency = 20f;
        private Vector2 connectedPosition;

        [Node]
        private Line2D line2d;

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.WireNodes();
            }
        }

        // TODO: amp should be tween
        public override void _Process(float delta)
        {
            waveAmplitude = Mathf.Max(waveAmplitude - delta, 0f);
        }

        public void Shoot()
        {
            var direction = this.GetMouseDirection();
            ClearGrapple();
            var raycast = GetTree().Root.World2d.DirectSpaceState.Raycast(GlobalPosition, direction * 1000f, null, 1 << 0, true, false);
            if (raycast != null)
            {
                connectedPosition = raycast.Position;
                waveAmplitude = 8f;
                UpdateGrapple();
            }
        }

        private void UpdateGrapple()
        {
            ClearGrapple();

            var straightLineDirection = (connectedPosition - GlobalPosition).Normalized();
            var length = (connectedPosition - GlobalPosition).Length();
            var segments = Mathf.CeilToInt(length / SEGMENT_LENGTH);

            var initialPoint = GlobalPosition;
            var points = new List<Vector2> {
                line2d.ToLocal(initialPoint)
            };

            for (int i = 1; i < segments; i++)
            {
                var previousPoint = points[i - 1];
                var strictPoint = previousPoint + (straightLineDirection * SEGMENT_LENGTH);

                points.Add(strictPoint);
            }

            line2d.Points = points.ToArray();
        }

        private void ClearGrapple()
        {
            line2d.ClearPoints();
        }
    }
}
