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
    private CanvasLayer _canvasLayer;

    [Export]
    private InteractionRaycast _interactionRaycast;

    [Export]
    public AnimationManager AnimationManager;

    [Export]
    public AnimationPlayer CameraAnimPlayer;

    [Export]
    private Node3D _mesh;

    [ExportCategory("Player Data")]
    [Export]
    public float MoveSpeed = 5f;

    [Export]
    public float CurrentStamina;
    
    [Export]
    public PlayerData PlayerData;

    public Vector3 TargetVelocity = Vector3.Zero;

    public Friend FriendData { get; set; }

    [ExportCategory("AnimationParameters")]
    
    [Export]
    public string TransCrouch = "parameters/Trans_Crouch/transition_request";

    [Export]
    public string TransJump = "parameters/Trans_Jump/transition_request";

    [Export]
    public string BsLook = "parameters/BS_Look/blend_position";

    [Export]
    public string BsWalk = "parameters/Walk/blend_position";

    [Export]
    public string BsCrouch = "parameters/BS_Crouch/blend_position";

    [Export]
    public string Emote1 = "parameters/Emote1/request";

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

        Position = new Vector3(0f, 10f, 0f);

        if (IsMultiplayerAuthority())
        {
            VoiceChat.Instance.SetAudioOutput(voiceOutput);
            _mesh.Visible = false;
        }

        _canvasLayer.Visible = IsMultiplayerAuthority();

        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsMultiplayerAuthority())
            return;

        UpdateBlendAnimation(GetDirectionInput(), (float)delta);
    }

    private void CameraRotation(Vector2 mouseMotion)
    {
        RotateY(Mathf.DegToRad(-mouseMotion.X * 0.1f));
        PlayerCamera.RotateX(Mathf.DegToRad(-mouseMotion.Y * 0.1f));
        PlayerCamera.RotationDegrees = new Vector3(Mathf.Clamp(PlayerCamera.RotationDegrees.X, -75f, 75f), 0f, 0f);
    }

    private void UpdateBlendAnimation(Vector3 inputAxis, float delta)
    {
        float look = PlayerCamera.GlobalRotationDegrees.X / 75f;
        AnimationManager.SetFloat(BsLook, look, 1f);

        int targetBlend = MoveSpeed == PlayerData.WalkSpeed ? 1 : MoveSpeed == PlayerData.RunSpeed ? 2 : 0;
        Vector2 flatInput = new Vector2(inputAxis.X, -inputAxis.Z).Normalized();

        AnimationManager.SetVector2(BsWalk, flatInput * targetBlend);
        AnimationManager.SetVector2(BsCrouch, flatInput);
    }

    public void RegenStamina(float delta)
    {
        CurrentStamina = Mathf.MoveToward(CurrentStamina, PlayerData.MaxStamina, PlayerData.StaminaRegenRate * delta);

        GameDebug.Instance.AddDebugProperty("Stamina", CurrentStamina);
    }

    public void DepleteStamina(float delta)
    {
        CurrentStamina = Mathf.MoveToward(CurrentStamina, 0f, PlayerData.StaminaDepletionRate * delta);

        GameDebug.Instance.AddDebugProperty("Stamina", CurrentStamina);
    }

    public override void _Input(InputEvent @event)
    {
        if (!IsMultiplayerAuthority())
            return;
        
        if (@event is InputEventMouseMotion mouseMotion)
        {
            CameraRotation(mouseMotion.Relative);
        }

        if (@event.IsActionPressed("interact"))
        {
            _interactionRaycast.Interact();
        }
    }

    public Vector3 GetDirectionInput()
    {
        return new Vector3(Input.GetAxis("move_left", "move_right"), 0f, Input.GetAxis("move_up", "move_down"));
    }

    public void UpdateGravity(float delta)
    {
        if (IsOnFloor())
            return;
        
        TargetVelocity.Y -= PlayerData.Gravity * delta;
    }

    public void UpdateInput()
    {
        Vector3 inputAxis = GetDirectionInput().Normalized();

        Vector3 direction = Basis * inputAxis;

        if (direction != Vector3.Zero)
        {
            TargetVelocity.X = Mathf.Lerp(TargetVelocity.X, direction.X * MoveSpeed, PlayerData.Acceleration);
            TargetVelocity.Z = Mathf.Lerp(TargetVelocity.Z, direction.Z * MoveSpeed, PlayerData.Acceleration);
        }
        else
        {
            TargetVelocity.X = Mathf.MoveToward(TargetVelocity.X, 0f, PlayerData.Deceleration);
            TargetVelocity.Z = Mathf.MoveToward(TargetVelocity.Z, 0f, PlayerData.Deceleration);
        }
    }

    public void UpdateVelocity()
    {
        Velocity = TargetVelocity;
        MoveAndSlide();
    }
}