using Godot;
using System;

public partial class PlayerMovement : CharacterBody3D
{
    [Export]
    private Camera3D _camera3D;

    private Vector3 _cameraUp = new Vector3(0f, 0.5f, 0f);
    private Vector3 _cameraCrouch = Vector3.Zero;
    private float _crouchTransitionTime = 0.15f;
    private bool _crouching;

    [Export]
    private CollisionShape3D _standUpCollider;

    [Export]
    private RayCast3D _crouchRayCastChecker;

    [Export]
    private float _moveSpeed = 5f;

    [Export]
    private float _jumpForce = 5f;

    [Export]
    private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    private bool _isGrounded = false;

    private Vector3 _targetVelocity = Vector3.Zero;

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
        GD.Print(_standUpCollider.Shape.Get("height"));
    }

    private void ToogleCrouch()
    {
        if (_crouchRayCastChecker.IsColliding())
            return;
        
        Tween cameraTween = CreateTween();

        switch(_crouching)
        {
            case true:
                cameraTween.TweenProperty(_camera3D, "position", _cameraUp, _crouchTransitionTime);
                _crouching = false;
                break;
            case false:
                cameraTween.TweenProperty(_camera3D, "position", _cameraCrouch, _crouchTransitionTime);
                _crouching = true;
                break;
        }
        
        _standUpCollider.Disabled = _crouching;
        GD.Print("CROUCH");
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
        else if (@event.IsActionPressed("crouch") && IsOnFloor())
        {
            ToogleCrouch();
        }
    }
}
