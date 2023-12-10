using Godot;
using System;

public partial class PlayerMovement : CharacterBody3D
{
    [Export]
    private Camera3D _camera3D;

    private Vector3 _cameraUp = new Vector3(0f, 0.5f, 0f);

    private Vector3 _cameraCrouch = Vector3.Zero;

    [Export]
    private CollisionShape3D _standUpCollider;

    [Export]
    private RayCast3D _crouchRayCastChecker;

    [Export]
    private PlayerData _playerData;

    private float _moveSpeed;

    private float _crouchTransitionTime = 0.15f;

    private bool _isGrounded = false;

    private bool _crouching;

    private Vector3 _targetVelocity = Vector3.Zero;

    private Vector3 GetDirectionInput()
    {
        return new Vector3(Input.GetAxis("move_left", "move_right"), 0f, Input.GetAxis("move_up", "move_down"));
    }

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        _moveSpeed = _playerData.WalkSpeed;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsOnFloor())
            _targetVelocity.Y -= _playerData.Gravity * (float)delta;

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

    private void Jump()
    {
        _targetVelocity.Y = _playerData.JumpForce;
        GD.Print("JUMP");   
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
                _moveSpeed = Input.IsActionPressed("run") ? _playerData.RunSpeed : _playerData.WalkSpeed;
                break;
            case false:
                cameraTween.TweenProperty(_camera3D, "position", _cameraCrouch, _crouchTransitionTime);
                _crouching = true;
                _moveSpeed = _playerData.CrouchSpeed;
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
            Jump();
        }
        else if (@event.IsActionPressed("crouch") && IsOnFloor())
        {
            ToogleCrouch();
        }
        else if (@event.IsActionPressed("run") && !_crouching)
        {
            _moveSpeed = _playerData.RunSpeed;
        }
        else if (@event.IsActionReleased("run") && !_crouching)
        {
            _moveSpeed = _playerData.WalkSpeed;
        }
    }
}
