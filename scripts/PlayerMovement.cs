using Godot;
using Steamworks;
using System;
using System.Collections.Generic;

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

    private void UpdateRemoteLocation(Vector3 location, Vector3 rotation)
    {
        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            {"DataType", "PlayerUpdate"},
            {"PlayerId", FriendData.Id.ToString()},
            {"LocationX", location.X.ToString()},
            {"LocationY", location.Y.ToString()},
            {"LocationZ", location.Z.ToString()},
            {"RotationX", rotation.X.ToString()},
            {"RotationY", rotation.Y.ToString()},
            {"RotationZ", rotation.Z.ToString()},
        };
        if (SteamManager.Instance.IsHost)
        {
            SteamManager.Instance.SendMessageToAll(OwnJsonParser.Serialize(data));
        }
        else
        {
            SteamManager.Instance.SteamConnectionManager.Connection.SendMessage(OwnJsonParser.Serialize(data));
        }
    }

    private void OnPlayerUpdate(Dictionary<string, string> data)
    {
        if (data["PlayerId"] == SteamManager.Instance.PlayerId.ToString())
            return;
        
        if (data["PlayerId"] == FriendData.Id.ToString())
        {
            _newPosition = new Vector3(float.Parse(data["LocationX"]), float.Parse(data["LocationY"]), float.Parse(data["LocationZ"]));
            Rotation = new Vector3(float.Parse(data["RotationX"]), float.Parse(data["RotationY"]), float.Parse(data["RotationZ"]));
        }
    }

    private int currentframe = 0;
    private int frameRate = 10;

    private Vector3 _newPosition = Vector3.Zero;
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        DataParser.OnPlayerUpdate += OnPlayerUpdate;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (ControlledByPlayer)
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
            currentframe++;
            if (currentframe == frameRate)
            {
                currentframe = 0;
                UpdateRemoteLocation(GlobalPosition, RotationDegrees);
            }
        }
        else
        {
            Position = Position.Lerp(_newPosition,(float)delta * 15f);
        }
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
