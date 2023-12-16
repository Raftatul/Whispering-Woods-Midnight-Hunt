using Godot;
using Steamworks;
using System;
using System.Collections.Generic;


public partial class PlayerMovement : CharacterBody3D
{
    [Export]
    public Camera3D PlayerCamera;

    [Export]
    private Node3D _cameraUp;

    [Export]
    private Node3D _cameraCrouch;

    [Export]
    private AnimationTree _animationTree;

    [Export]
    private Node3D _mesh;

    [Export]
    private float _moveSpeed = 5f;

    [Export]
    private CollisionShape3D _standUpCollider;

    [Export]
    private RayCast3D _crouchRayCastChecker;

    [Export]
    private PlayerData _playerData;

    private float _crouchTransitionTime = 0.15f;

    private bool _isGrounded = false;

    public bool ControlledByPlayer { get; set; } = false;

    private Vector3 _targetVelocity = Vector3.Zero;

    public Friend FriendData { get; set; }
    
    private float _stamina;

    public enum PlayerState
    {
        Idle, Walk, Run, Crouch
    }

    private PlayerState _playerState;

    [Signal]
    public delegate void OnPlayerInitializedEventHandler();

    [Export]
    private MultiplayerSynchronizer _multiplayerSynchronizer;

    private Vector3 GetDirectionInput()
    {
        return new Vector3(Input.GetAxis("move_left", "move_right"), 0f, Input.GetAxis("move_up", "move_down"));
    }

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(int.Parse(Name));
    }

    public override void _Ready()
    {
        ControlledByPlayer = IsMultiplayerAuthority();
        PlayerCamera.Current = IsMultiplayerAuthority();

        Input.MouseMode = Input.MouseModeEnum.Captured;

        SwitchState(PlayerState.Idle);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsMultiplayerAuthority())
            return;
        
        if (!IsOnFloor())
            _targetVelocity.Y -= _playerData.Gravity * (float)delta;

        Vector3 inputAxis = GetDirectionInput().Normalized();
        Vector3 movement = Basis * inputAxis;

        int targetBlend = _moveSpeed == _playerData.WalkSpeed ? 1 : _moveSpeed == _playerData.RunSpeed ? 2 : 0;

        Vector2 targetBlendPosition = _animationTree.Get("parameters/Walk/blend_position").AsVector2();
        targetBlendPosition.X = Mathf.Lerp(targetBlendPosition.X, inputAxis.X * targetBlend, 0.1f);
        targetBlendPosition.Y = Mathf.Lerp(targetBlendPosition.Y, -inputAxis.Z * targetBlend, 0.1f);

        _animationTree.Set("parameters/Walk/blend_position", targetBlendPosition);

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
        
        if (IsOnFloor() && !_isGrounded)
        {
            _isGrounded = true;
            OnGrounded();
        }
        else if (!IsOnFloor() && _isGrounded)
            _isGrounded = false;

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
        PlayerCamera.RotateX(Mathf.DegToRad(-mouseMotion.Y * 0.1f));
        PlayerCamera.RotationDegrees = new Vector3(Mathf.Clamp(PlayerCamera.RotationDegrees.X, -90f, 90f), PlayerCamera.RotationDegrees.Y, PlayerCamera.RotationDegrees.Z);
    }

    private void Jump()
    {
        _targetVelocity.Y = _playerData.JumpForce;
        GD.Print("JUMP");
        _animationTree.Set("parameters/OneShot/request", ((int)AnimationNodeOneShot.OneShotRequest.Fire));
    }

    private void Crouch()
    {
        SwitchState(PlayerState.Crouch);

        _standUpCollider.Disabled = true;

        Tween cameraTween = CreateTween();
        cameraTween.TweenProperty(PlayerCamera, "position", _cameraCrouch.Position, _crouchTransitionTime);
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
        cameraTween.TweenProperty(PlayerCamera, "position", _cameraUp.Position, _crouchTransitionTime);
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
        if (!ControlledByPlayer)
        {
            return;
        }
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

    private void OnGrounded()
    {
        _animationTree.Set("parameters/OneShot/request", ((int)AnimationNodeOneShot.OneShotRequest.Abort));
    }
}