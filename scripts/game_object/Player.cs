using Godot;
using GodotUtilities;
using GodotUtilities.Logic;

namespace Game.GameObject
{
    public class Player : KinematicBody2D
    {
        private const float GROUNDED_SPEED = 200f;
        private const float ACCELERATION = 20f;
        private const float AIRBORNE_ACCELERATION = 5f;
        private const float JUMP_FORCE = 428;
        private const float GRAVITY = 1200f;
        private const float JUMP_GRAVITY_MULTIPLIER = 3f;
        private const float GRAPPLE_FORCE_DECAY = 5f;
        private const float MAX_GRAPPLE_FORCE = 1200f;

        [Node]
        private Grapple grapple;

        private Vector2 velocity;
        private bool isGrappling = false;
        private Vector2 connectedPoint;
        private float currentGrappleForce;

        private enum State
        {
            Normal,
            Airborne,
            Grappling
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
            stateMachine.AddState(State.Grappling, StateGrappling);
            stateMachine.SetInitialState(StateAirborne);
            grapple.Connect(nameof(Grapple.ConnectedToPoint), this, nameof(OnGrappleConnected));
            grapple.Connect(nameof(Grapple.Disconnected), this, nameof(OnGrappleDisconnected));
        }

        public override void _UnhandledInput(InputEvent evt)
        {
            if (evt.IsActionPressed("grapple"))
            {
                grapple.Shoot();
            }
        }

        public override void _Process(float delta)
        {
            stateMachine.Update();
        }

        private void StateNormal()
        {
            var delta = GetProcessDeltaTime();
            var moveVec = GetMovementVector();

            velocity.x = Mathf.Lerp(velocity.x, GROUNDED_SPEED * moveVec.x, 1f - Mathf.Exp(-ACCELERATION * delta));

            var gravityMultiplier = 1f;

            if (moveVec.y > 0)
            {
                velocity.y = -JUMP_FORCE;
            }

            if (!Input.IsActionPressed("jump") && velocity.y < 0 && !isGrappling)
            {
                gravityMultiplier = JUMP_GRAVITY_MULTIPLIER;
            }

            velocity.y += GRAVITY * delta * gravityMultiplier;
            velocity = MoveAndSlideWithSnap(velocity, Vector2.Down, Vector2.Up);

            if (!IsOnFloor())
            {
                stateMachine.ChangeState(StateAirborne);
            }
        }

        private void StateAirborne()
        {
            var delta = GetProcessDeltaTime();
            var moveVec = GetMovementVector();
            velocity.x = Mathf.Lerp(velocity.x, GROUNDED_SPEED * moveVec.x, 1f - Mathf.Exp(-AIRBORNE_ACCELERATION * delta));
            velocity.y += GRAVITY * delta;

            velocity = MoveAndSlideWithSnap(velocity, Vector2.Down, Vector2.Up);

            if (IsOnFloor())
            {
                stateMachine.ChangeState(StateNormal);
            }
        }

        private void StateGrappling()
        {
            var delta = GetProcessDeltaTime();
            var direction = (connectedPoint - grapple.GlobalPosition).Normalized();
            currentGrappleForce = Mathf.Lerp(currentGrappleForce, MAX_GRAPPLE_FORCE, 1f - Mathf.Exp(-GRAPPLE_FORCE_DECAY * delta));
            velocity = velocity.LinearInterpolate(direction * currentGrappleForce, 1f - Mathf.Exp(-GRAPPLE_FORCE_DECAY * delta));
            velocity = MoveAndSlide(velocity);
        }

        private Vector2 GetMovementVector()
        {
            var x = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
            var y = Input.IsActionJustPressed("jump") ? Input.GetActionStrength("jump") : 0;
            return new Vector2(x, y);
        }

        private void OnGrappleConnected(Vector2 point)
        {
            stateMachine.ChangeState(StateGrappling);
            currentGrappleForce = 0f;
            isGrappling = true;
            connectedPoint = point;
        }

        private void OnGrappleDisconnected()
        {
            isGrappling = false;
            stateMachine.ChangeState(StateAirborne);
        }
    }
}
