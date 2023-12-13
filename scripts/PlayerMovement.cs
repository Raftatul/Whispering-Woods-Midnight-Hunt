using Godot;
using Steamworks;
using System;

public partial class PlayerMovement : CharacterBody3D
{
    [Export]
    private Camera3D _camera3D;

    [Export]
    private float _moveSpeed = 5f;

    [Export]
    private float _jumpForce = 5f;

    [Export]
    private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    private bool _isGrounded = false;

    public bool ControlledByPlayer { get; set; } = false;

    private Vector3 _targetVelocity = Vector3.Zero;

    public Friend FriendData { get; set; }

    private Vector3 GetDirectionInput()
    {
        return new Vector3(Input.GetAxis("move_left", "move_right"), 0f, Input.GetAxis("move_up", "move_down"));
    }

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsOnFloor())
            _targetVelocity.Y -= _gravity * (float)delta;

        Vector3 inputAxis = GetDirectionInput().Normalized();
        Vector3 movement = Basis * inputAxis;

        if (movement != Vector3.Zero)
        {
            _targetVelocity.X = movement.X * _moveSpeed;
            _targetVelocity.Z = movement.Z * _moveSpeed;
        }
        else
        {
            _targetVelocity.X = Mathf.MoveToward(_targetVelocity.X, 0f, _moveSpeed);
            _targetVelocity.Z = Mathf.MoveToward(_targetVelocity.Z, 0f, _moveSpeed);
        }
        
        Velocity = _targetVelocity;
        MoveAndSlide();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            RotateY(Mathf.DegToRad(-mouseMotion.Relative.X * 0.1f));
            _camera3D.RotateX(Mathf.DegToRad(-mouseMotion.Relative.Y * 0.1f));
            _camera3D.RotationDegrees = new Vector3(Mathf.Clamp(_camera3D.RotationDegrees.X, -90f, 90f), _camera3D.RotationDegrees.Y, _camera3D.RotationDegrees.Z);
        }

        if (@event.IsActionPressed("jump") && IsOnFloor())
        {
            _targetVelocity.Y = _jumpForce;
            GD.Print("JUMP");
        }
    }
}
