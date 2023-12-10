using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

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

    private Vector3 _targetVelocity = Vector3.Zero;

    private float _stamina;

    public enum PlayerState
    {
        Idle, Walk, Run, Crouch
    }

    private PlayerState _playerState;

    private Vector3 GetDirectionInput()
    {
        return new Vector3(Input.GetAxis("move_left", "move_right"), 0f, Input.GetAxis("move_up", "move_down"));
    }

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        SwitchState(PlayerState.Idle);
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

        switch(_playerState)
        {
            case PlayerState.Run:
                DepleteStamina((float)delta);
                if (inputAxis.Length() == 0f)
                    SwitchState(PlayerState.Idle);
                break;
            case PlayerState.Idle:
                RegenStamina((float)delta);
                if (inputAxis.Length() > 0f)
                    SwitchState(PlayerState.Walk);
                break;
            case PlayerState.Walk:
                RegenStamina((float)delta);
                if (inputAxis.Length() == 0f)
                    SwitchState(PlayerState.Idle);
                break;
            default:
                RegenStamina((float)delta);
                break;
        }
        
        GD.Print(_stamina);
        GD.Print("Player State :", _playerState);

        Velocity = _targetVelocity;
        MoveAndSlide();
    }

    private void SwitchState(PlayerState newState)
    {
        _playerState = newState;

        switch (_playerState)
        {
            case PlayerState.Idle:
                _moveSpeed = 0f;
                break;
            case PlayerState.Walk:
                _moveSpeed = _playerData.WalkSpeed;
                break;
            case PlayerState.Run:
                _moveSpeed = _playerData.RunSpeed;
                break;
            case PlayerState.Crouch:
                _moveSpeed = _playerData.CrouchSpeed;
                break;
        }
    }

    private void RegenStamina(float delta)
    {
        _stamina = Mathf.MoveToward(_stamina, _playerData.MaxStamina, _playerData.StaminaRegenRate * delta);
    }

    private void DepleteStamina(float delta)
    {
        _stamina = Mathf.MoveToward(_stamina, 0f, _playerData.StaminaDepletionRate * delta);

        if (_stamina <= 0f)
            StopRun();
    }

    private void CameraRotation(Vector2 mouseMotion)
    {
        RotateY(Mathf.DegToRad(-mouseMotion.X * 0.1f));
        _camera3D.RotateX(Mathf.DegToRad(-mouseMotion.Y * 0.1f));
        _camera3D.RotationDegrees = new Vector3(Mathf.Clamp(_camera3D.RotationDegrees.X, -90f, 90f), _camera3D.RotationDegrees.Y, _camera3D.RotationDegrees.Z);
    }

    private void Jump()
    {
        _targetVelocity.Y = _playerData.JumpForce;
        GD.Print("JUMP");   
    }

    private void Crouch()
    {
        SwitchState(PlayerState.Crouch);

        _standUpCollider.Disabled = true;

        Tween cameraTween = CreateTween();
        cameraTween.TweenProperty(_camera3D, "position", _cameraCrouch, _crouchTransitionTime);
    }

    private bool CanUnCrouch()
    {
        return !_crouchRayCastChecker.IsColliding();
    }

    private void UnCrouch()
    {
        if (!CanUnCrouch())
            return;
        
        SwitchState(PlayerState.Idle);
        
        _standUpCollider.Disabled = false;

        Tween cameraTween = CreateTween();
        cameraTween.TweenProperty(_camera3D, "position", _cameraUp, _crouchTransitionTime);
    }

    private void ToogleCrouch()
    {
        switch(_playerState)
        {
            case PlayerState.Crouch:
                UnCrouch();
                break;
            default:
                Crouch();
                break;
        }
        
        GD.Print("CROUCH");
    }

    private void StartRun()
    {
        UnCrouch();

        SwitchState(PlayerState.Run);
    }

    private void StopRun()
    {
        SwitchState(_targetVelocity.Length() > 0f ? PlayerState.Walk : PlayerState.Idle);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            CameraRotation(mouseMotion.Relative);
        }

        if (@event.IsActionPressed("jump") && IsOnFloor())
        {
            Jump();
        }
        else if (@event.IsActionPressed("crouch") && IsOnFloor())
        {
            ToogleCrouch();
        }
        else if (@event.IsActionPressed("run") && CanUnCrouch())
        {
            StartRun();
        }
        else if (@event.IsActionReleased("run") && _playerState == PlayerState.Run)
        {
            StopRun();
        }
    }
}
