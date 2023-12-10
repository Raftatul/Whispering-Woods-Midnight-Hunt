using Godot;
using System;

public partial class PlayerMovement : CharacterBody3D
{
    [Export]
    private Camera3D _camera3D;

    [Export]
    private AnimationTree _animationTree;

    [Export]
    private Node3D _mesh;

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

        if (IsMultiplayerAuthority())
        {
            // _mesh.Visible = false;
        }
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

        Vector2 targetBlendPosition = _animationTree.Get("parameters/Walk/blend_position").AsVector2();
        targetBlendPosition.X = Mathf.Lerp(targetBlendPosition.X, inputAxis.X, 0.1f);
        targetBlendPosition.Y = Mathf.Lerp(targetBlendPosition.Y, inputAxis.Z, 0.1f);

        _animationTree.Set("parameters/Walk/blend_position", targetBlendPosition);

        if (IsOnFloor() && !_isGrounded)
        {
            _isGrounded = true;
            OnGrounded();
        }
        else if (!IsOnFloor() && _isGrounded)
        {
            _isGrounded = false;
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
            _animationTree.Set("parameters/OneShot/request", ((int)AnimationNodeOneShot.OneShotRequest.Fire));
            GD.Print("JUMP");
        }
    }

    private void OnGrounded()
    {
        _animationTree.Set("parameters/OneShot/request", ((int)AnimationNodeOneShot.OneShotRequest.Abort));
    }
}
