using Godot;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;


public partial class PlayerController : CharacterBody3D
{
    [ExportCategory("Nodes")]
    [Export]
    public Camera3D PlayerCamera;

    [Export]
    private CollisionShape3D _standUpCollider;

    [Export]
    private RayCast3D _crouchRayCastChecker;

    [Export]
    private RayCast3D _interactionRaycast;

    [Export]
    private AnimationManager _animationManager;

    [ExportCategory("Player Data")]
    [Export]
    private float _currentMoveSpeed = 5f;

    [Export]
    private float _currentStamina;
    
    [Export]
    private PlayerData _playerData;

    private bool _isGrounded = true;

    public bool ControlledByPlayer { get; set; } = false;

    private Vector3 _targetVelocity = Vector3.Zero;

    public Friend FriendData { get; set; }

    public enum PlayerState
    {
        Idle, Walk, Run, Crouch
    }

    private PlayerState _playerState;

    [Signal]
    public delegate void OnPlayerInitializedEventHandler();

    [Signal]
    public delegate void OnGroundedEventHandler();

    [Signal]
    public delegate void OnAirEventHandler();

    private Vector3 GetDirectionInput()
    {
        return new Vector3(Input.GetAxis("move_left", "move_right"), 0f, Input.GetAxis("move_up", "move_down"));
    }

    public override void _EnterTree()
    {
        Name = Name.ToString().Replace(GameManager.PlayerInstanceName, "");
        SetMultiplayerAuthority(int.Parse(Name));
        Name = GameManager.PlayerInstanceName + Name;
    }

    public override void _Ready()
    {
        ControlledByPlayer = IsMultiplayerAuthority();
        PlayerCamera.Current = IsMultiplayerAuthority();

        AudioStreamPlayer3D voiceOutput = new AudioStreamPlayer3D();
        AddChild(voiceOutput);
        voiceOutput.Name = "VoiceOutput";

        Position += Vector3.Up * 10f;

        if (IsMultiplayerAuthority())
        {
            VoiceChat.Instance.SetAudioOutput(voiceOutput);
        }

        Input.MouseMode = Input.MouseModeEnum.Captured;

        SwitchState(PlayerState.Idle);
        ConnectSignals();
    }

    private void ConnectSignals()
    {
        OnGrounded += () => _animationManager.Rpc("RequestTransition", "Trans_Jump/transition_request", "land");
        OnAir += () => _animationManager.Rpc("RequestTransition", "Trans_Jump/transition_request", "jump");

        // _animationManager.RequestTransition("Trans_Jump/transition_request", "jump");   
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsMultiplayerAuthority())
            return;

        Vector3 inputAxis = GetDirectionInput().Normalized();

        float look = PlayerCamera.GlobalRotationDegrees.X / 75f;
        _animationManager.SetFloat("BS_Look/blend_position", look, 1f);

        HandleVelocity(inputAxis, (float)delta);
        
        HandleGroundSignal();

        HandlePlayerState(inputAxis);
        HandleStamina((float)delta);

        if (_interactionRaycast.Enabled)
            Interact();

        Velocity = _targetVelocity;
        MoveAndSlide();

        // for (int i = 0; i < GetSlideCollisionCount(); i++)
        // {
        //     var col = GetSlideCollision(i);
        //     if (col.GetCollider() is RigidBody3D truc)
        //     {
        //         truc.ApplyCentralImpulse(-col.GetNormal() * 0.3f);
        //         // truc.ApplyImpulse(-col.GetNormal() * 0.01f, col.GetPosition());
        //     }
        // }
    }

    private void SwitchState(PlayerState newState)
    {
        _playerState = newState;

        switch (_playerState)
        {
            case PlayerState.Idle:
                _currentMoveSpeed = 0f;
                break;
            case PlayerState.Walk:
                _currentMoveSpeed = _playerData.WalkSpeed;
                break;
            case PlayerState.Run:
                _currentMoveSpeed = _playerData.RunSpeed;
                break;
            case PlayerState.Crouch:
                _currentMoveSpeed = _playerData.CrouchSpeed;
                break;
        }
    }

    private void CameraRotation(Vector2 mouseMotion)
    {
        RotateY(Mathf.DegToRad(-mouseMotion.X * 0.1f));
        PlayerCamera.RotateX(Mathf.DegToRad(-mouseMotion.Y * 0.1f));
        PlayerCamera.RotationDegrees = new Vector3(Mathf.Clamp(PlayerCamera.RotationDegrees.X, -75f, 75f), 0f, 0f);
    }

    private void Jump()
    {
        _targetVelocity.Y = _playerData.JumpForce;
        _animationManager.Rpc("RequestTransition", "Trans_Jump/transition_request", "jump");
        // _animationManager.RequestTransition("Trans_Jump/transition_request", "jump");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Crouch()
    {
        SwitchState(PlayerState.Crouch);

        _standUpCollider.Disabled = true;
        // _animationManager.Rpc("RequestTransition", "Trans_Crouch/transition_request", "crouch");
        _animationManager.RequestTransition("Trans_Crouch/transition_request", "crouch");
    }

    private bool CanUnCrouch()
    {
        return !_crouchRayCastChecker.IsColliding();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void UnCrouch()
    {
        if (!CanUnCrouch())
            return;
        
        SwitchState(PlayerState.Idle);
        
        _standUpCollider.Disabled = false;

        // _animationManager.Rpc("RequestTransition", "Trans_Crouch/transition_request", "uncrouch");
        _animationManager.RequestTransition("Trans_Crouch/transition_request", "uncrouch");
    }

    private void ToogleCrouch()
    {
        switch(_playerState)
        {
            case PlayerState.Crouch:
                Rpc(MethodName.UnCrouch);
                // UnCrouch();
                break;
            default:
                Rpc(MethodName.Crouch);
                // Crouch();
                break;
        }
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

    private void HandleVelocity(Vector3 inputAxis, float delta)
    {
        Vector3 movement = Basis * inputAxis;

        int targetBlend = _currentMoveSpeed == _playerData.WalkSpeed ? 1 : _currentMoveSpeed == _playerData.RunSpeed ? 2 : 0;
        Vector2 flatInput = new Vector2(inputAxis.X, -inputAxis.Z);

        _animationManager.SetVector2("Walk/blend_position", flatInput * targetBlend);
        _animationManager.SetVector2("BS_Crouch/blend_position", flatInput);

        if (!IsOnFloor())
            _targetVelocity.Y -= _playerData.Gravity * (float)delta;
        
        if (movement != Vector3.Zero)
        {
            _targetVelocity.X = movement.X * _currentMoveSpeed;
            _targetVelocity.Z = movement.Z * _currentMoveSpeed;
        }
        else
        {
            _targetVelocity.X = Mathf.MoveToward(_targetVelocity.X, 0f, _currentMoveSpeed);
            _targetVelocity.Z = Mathf.MoveToward(_targetVelocity.Z, 0f, _currentMoveSpeed);
        }
    }

    private void HandleGroundSignal()
    {
        if (IsOnFloor() && !_isGrounded)
        {
            _isGrounded = true;
            EmitSignal(SignalName.OnGrounded);
        }
        else if (!IsOnFloor() && _isGrounded)
        {
            _isGrounded = false;
            EmitSignal(SignalName.OnAir);
        }
    }

    private void HandlePlayerState(Vector3 inputAxis)
    {
        switch(_playerState)
        {
            case PlayerState.Idle:
                if (inputAxis.Length() > 0f)
                    SwitchState(PlayerState.Walk);
                break;
            case PlayerState.Walk:
                if (inputAxis.Length() == 0f)
                    SwitchState(PlayerState.Idle);
                break;
            case PlayerState.Run:
                if (inputAxis.Length() == 0f)
                    SwitchState(PlayerState.Idle);
                break;
        }
    }

    private void RegenStamina(float delta)
    {
        _currentStamina = Mathf.MoveToward(_currentStamina, _playerData.MaxStamina, _playerData.StaminaRegenRate * delta);
    }

    private void DepleteStamina(float delta)
    {
        _currentStamina = Mathf.MoveToward(_currentStamina, 0f, _playerData.StaminaDepletionRate * delta);

        if (_currentStamina <= 0f)
            StopRun();
    }

    private void HandleStamina(float delta)
    {
        switch(_playerState)
        {
            case PlayerState.Run:
                DepleteStamina((float)delta);
                break;
            default:
                RegenStamina((float)delta);
                break;
        }
    }
    
    private void SetInteractRaycastEnabled(bool enabled)
    {
        _interactionRaycast.Enabled = enabled;
    }

    private void Interact()
    {
        if (!_interactionRaycast.IsColliding())
            return;

        if (_interactionRaycast.GetCollider() is Interactable interactable)
            interactable.Interact();
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
        else if (@event.IsActionPressed("interact"))
        {
            SetInteractRaycastEnabled(true);
        }
        else if (@event.IsActionReleased("interact"))
        {
            SetInteractRaycastEnabled(false);
        }
    }
}