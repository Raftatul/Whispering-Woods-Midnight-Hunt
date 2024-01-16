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
    private InteractionRaycast _interactionRaycast;

    [Export]
    private AnimationManager _animationManager;

    [Export]
    private AnimationPlayer _crouchAnimationPlayer;

    [Export]
    private AnimationPlayer _walkAnimationPlayer;

    [Export]
    private Node3D _mesh;

    [ExportCategory("Player Data")]
    [Export]
    private float _currentMoveSpeed = 5f;

    [Export]
    private float _currentStamina;
    
    [Export]
    private PlayerData _playerData;

    private Vector3 _targetVelocity = Vector3.Zero;

    public Friend FriendData { get; set; }

    public enum PlayerState
    {
        IDLE, WALK, RUN, CROUCH, AIR, CROUCH_WALK
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
        PlayerCamera.Current = IsMultiplayerAuthority();

        AudioStreamPlayer3D voiceOutput = new AudioStreamPlayer3D();
        AddChild(voiceOutput);
        voiceOutput.Name = "VoiceOutput";

        Position += Vector3.Up * 10f;

        if (IsMultiplayerAuthority())
        {
            VoiceChat.Instance.SetAudioOutput(voiceOutput);
            _mesh.Visible = false;
        }

        Input.MouseMode = Input.MouseModeEnum.Captured;

        SwitchState(PlayerState.IDLE);
        ConnectSignals();
    }

    private void ConnectSignals()
    {
        OnGrounded += () => _animationManager.Rpc("RequestTransition", "Trans_Jump/transition_request", "land");
        OnAir += () => _animationManager.Rpc("RequestTransition", "Trans_Jump/transition_request", "jump");
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

        Velocity = _targetVelocity;
        MoveAndSlide();
    }

    private void SwitchState(PlayerState newState)
    {
        _playerState = newState;

        switch (_playerState)
        {
            case PlayerState.IDLE:
                _currentMoveSpeed = 0f;
                _walkAnimationPlayer.Pause();
                break;
            case PlayerState.WALK:
                _currentMoveSpeed = _playerData.WalkSpeed;
                _walkAnimationPlayer.Play("Walk");
                break;
            case PlayerState.RUN:
                _currentMoveSpeed = _playerData.RunSpeed;
                _walkAnimationPlayer.Play("Walk", customSpeed: 2f);
                break;
            case PlayerState.CROUCH:
                _currentMoveSpeed = 0f;
                _walkAnimationPlayer.Pause();
                break;
            case PlayerState.CROUCH_WALK:
                _currentMoveSpeed = _playerData.CrouchSpeed;
                _walkAnimationPlayer.Play("Walk", customSpeed: 1.25f);
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
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Crouch()
    {
        SwitchState(PlayerState.CROUCH);

        _standUpCollider.Disabled = true;
        _animationManager.RequestTransition("Trans_Crouch/transition_request", "crouch");
        _crouchAnimationPlayer.Play("Crouch");
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
        
        SwitchState(PlayerState.IDLE);
        
        _standUpCollider.Disabled = false;

        _animationManager.RequestTransition("Trans_Crouch/transition_request", "uncrouch");
        _crouchAnimationPlayer.PlayBackwards("Crouch");
    }

    private void ToogleCrouch()
    {
        switch(_playerState)
        {
            case PlayerState.CROUCH:
                Rpc(MethodName.UnCrouch);
                break;
            case PlayerState.CROUCH_WALK:
                Rpc(MethodName.UnCrouch);
                break;
            default:
                Rpc(MethodName.Crouch);
                break;
        }
    }

    private void StartRun()
    {
        if (_playerState == PlayerState.AIR)
            return;
        
        if(_playerState == PlayerState.CROUCH || _playerState == PlayerState.CROUCH_WALK)
            UnCrouch();

        SwitchState(PlayerState.RUN);
    }

    private void StopRun()
    {
        if (_playerState == PlayerState.AIR)
            return;
        
        SwitchState(_targetVelocity.Length() > 0f ? PlayerState.WALK : PlayerState.IDLE);
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
        bool isOnFloor = IsOnFloor();

        if (isOnFloor && _playerState == PlayerState.AIR)
        {
            _playerState = PlayerState.IDLE;

            EmitSignal(SignalName.OnGrounded);
        }
        else if (!isOnFloor && _playerState != PlayerState.AIR)
        {
            _playerState = PlayerState.AIR;

            EmitSignal(SignalName.OnAir);
        }
    }

    private void HandlePlayerState(Vector3 inputAxis)
    {
        switch(_playerState)
        {
            case PlayerState.IDLE:
                if (inputAxis.Length() > 0f)
                    SwitchState(PlayerState.WALK);
                break;
            case PlayerState.WALK:
                if (inputAxis.Length() == 0f)
                    SwitchState(PlayerState.IDLE);
                break;
            case PlayerState.RUN:
                if (inputAxis.Length() == 0f)
                    SwitchState(PlayerState.IDLE);
                break;
            case PlayerState.CROUCH:
                if (inputAxis.Length() > 0f)
                    SwitchState(PlayerState.CROUCH_WALK);
                break;
            case PlayerState.CROUCH_WALK:
                if (inputAxis.Length() == 0f)
                    SwitchState(PlayerState.CROUCH);
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
            case PlayerState.RUN:
                DepleteStamina((float)delta);
                break;
            default:
                RegenStamina((float)delta);
                break;
        }
    }

    private void DeferredInteraction()
    {
        if (!_interactionRaycast.IsColliding())
            return;

        if (_interactionRaycast.GetCollider() is IInteractable interactable)
            interactable.Rpc(nameof(IInteractable.Interact));
    }

    public override void _Input(InputEvent @event)
    {
        if (!IsMultiplayerAuthority())
            return;
        
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
        else if (@event.IsActionReleased("run") && _playerState == PlayerState.RUN)
        {
            StopRun();
        }
        else if (@event.IsActionPressed("interact"))
        {
            _interactionRaycast.Interact();
        }
    }
}