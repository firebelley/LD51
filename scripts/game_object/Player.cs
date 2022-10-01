using Godot;
using GodotUtilities;
using GodotUtilities.Logic;

namespace Game.GameObject
{
    public class Player : KinematicBody2D
    {
        private const float GROUNDED_SPEED = 200f;
        private const float ACCELERATION = 20f;
        private const float AIRBORNE_ACCELERATION = 3f;
        private const float JUMP_FORCE = 600f;
        private const float GRAVITY = 1200f;
        private const float MAX_Y_VELOCITY = 1200f;
        private const float JUMP_GRAVITY_MULTIPLIER = 5f;
        private const float GRAPPLE_FORCE_DECAY = 5f;
        private const float MAX_GRAPPLE_FORCE = 2000f;
        private const float KNOCKBACK_FORCE = 1000f;

        [Node]
        private Grapple grapple;
        [Node]
        private Area2D area2d;
        [Node]
        private Timer knockbackTimer;
        [Node]
        private Node2D visuals;
        [Node]
        private Sprite sprite;

        private Vector2 velocity;
        private Vector2 grappleConnectedPoint;
        private float currentGrappleForce;
        private bool wasGrounded;

        private Vector2 knockbackDirection;

        private enum State
        {
            Normal,
            Airborne,
            Grappling,
            Knockback
        }
        private StateMachine<State> stateMachine = new();

        public override void _Notification(int what)
        {
            if (what == NotificationInstanced)
            {
                this.AddToGroup();
                this.WireNodes();
            }
        }

        public override void _Ready()
        {
            stateMachine.AddState(State.Normal, StateNormal);
            stateMachine.AddState(State.Airborne, StateAirborne);
            stateMachine.AddLeaveState(State.Airborne, LeaveStateAirborne);
            stateMachine.AddEnterState(State.Grappling, EnterStateGrappling);
            stateMachine.AddState(State.Grappling, StateGrappling);
            stateMachine.AddLeaveState(State.Grappling, LeaveStateGrappling);
            stateMachine.AddEnterState(State.Knockback, EnterStateKnockback);
            stateMachine.AddState(State.Knockback, StateKnockback);
            stateMachine.SetInitialState(StateAirborne);
            grapple.Connect(nameof(Grapple.ConnectedToPoint), this, nameof(OnGrappleConnected));
            grapple.Connect(nameof(Grapple.Disconnected), this, nameof(OnGrappleDisconnected));
            area2d.Connect("area_entered", this, nameof(OnAreaEntered));
        }

        public override void _UnhandledInput(InputEvent evt)
        {
            if (evt.IsActionPressed("grapple") && stateMachine.GetCurrentState() != State.Knockback)
            {
                grapple.Shoot();
            }
        }

        public override void _Process(float delta)
        {
            stateMachine.Update();
            visuals.Scale = new Vector2(velocity.x > 0 ? 1 : -1, 1);
            GD.Print(visuals.Scale);
        }

        private void StateNormal()
        {
            var delta = GetProcessDeltaTime();
            var moveVec = GetMovementVector();

            velocity.x = Mathf.Lerp(velocity.x, 0f, 1f - Mathf.Exp(-ACCELERATION * delta));

            if (moveVec.y > 0)
            {
                velocity.y = -JUMP_FORCE;
                wasGrounded = true;
            }

            ApplyGravity();
            velocity = MoveAndSlideWithSnap(velocity, Vector2.Down, Vector2.Up);

            if (!IsOnFloor())
            {
                stateMachine.ChangeState(StateAirborne);
            }
        }

        private void StateAirborne()
        {
            var delta = GetProcessDeltaTime();
            velocity.x = Mathf.Lerp(velocity.x, 0f, 1f - Mathf.Exp(-AIRBORNE_ACCELERATION * delta));

            var gravityMultiplier = 1f;
            if (!Input.IsActionPressed("jump") && velocity.y < 0 && wasGrounded)
            {
                gravityMultiplier = JUMP_GRAVITY_MULTIPLIER;
            }

            ApplyGravity(gravityMultiplier);

            velocity = MoveAndSlideWithSnap(velocity, Vector2.Down, Vector2.Up);

            if (IsOnFloor())
            {
                stateMachine.ChangeState(StateNormal);
            }
        }

        private void LeaveStateAirborne()
        {
            wasGrounded = false;
        }

        private void EnterStateGrappling()
        {
            currentGrappleForce = 0f;
            GlobalPosition += Vector2.Up;
        }

        private void StateGrappling()
        {
            var delta = GetProcessDeltaTime();
            var direction = (grappleConnectedPoint - grapple.GlobalPosition).Normalized();
            currentGrappleForce = Mathf.Lerp(currentGrappleForce, MAX_GRAPPLE_FORCE, 1f - Mathf.Exp(-GRAPPLE_FORCE_DECAY * delta));
            velocity = velocity.LinearInterpolate(direction * currentGrappleForce, 1f - Mathf.Exp(-GRAPPLE_FORCE_DECAY * delta));
            velocity = MoveAndSlideWithSnap(velocity, Vector2.Down, Vector2.Up);
            if (IsOnFloor() || IsOnCeiling() || IsOnWall())
            {
                GameCamera.Shake();
                grapple.DisconnectGrapple();
            }

            sprite.LookAt(sprite.GlobalPosition - (velocity.Perpendicular() * Mathf.Sign(visuals.Scale.x)));
        }

        private void LeaveStateGrappling()
        {
            sprite.Rotation = 0f;
        }

        private void EnterStateKnockback()
        {
            knockbackTimer.Start();
            velocity = knockbackDirection * KNOCKBACK_FORCE;
            grapple.ClearGrapple();
        }

        private void StateKnockback()
        {
            ApplyGravity();
            velocity.x = Mathf.Lerp(velocity.x, 0f, 1f - Mathf.Exp(-1f * GetProcessDeltaTime()));
            velocity = MoveAndSlide(velocity);

            if (knockbackTimer.IsStopped())
            {
                stateMachine.ChangeState(State.Airborne);
            }
        }

        private Vector2 GetMovementVector()
        {
            var x = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
            var y = Input.IsActionJustPressed("jump") ? Input.GetActionStrength("jump") : 0;
            return new Vector2(x, y);
        }

        private void ApplyGravity(float multiplier = 1f)
        {
            velocity.y += GRAVITY * GetProcessDeltaTime() * multiplier;
            velocity.y = Mathf.Min(velocity.y, MAX_Y_VELOCITY);
        }

        private void OnGrappleConnected(Vector2 point)
        {
            stateMachine.ChangeState(StateGrappling);
            grappleConnectedPoint = point;
        }

        private void OnGrappleDisconnected()
        {
            stateMachine.ChangeState(StateAirborne);
        }

        private void OnAreaEntered(Area2D otherArea)
        {
            if (otherArea.Owner is Enemy enemy)
            {
                knockbackDirection = enemy.Direction;
                stateMachine.ChangeState(StateKnockback);
            }
        }
    }
}
