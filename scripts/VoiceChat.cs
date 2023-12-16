using Godot;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

public partial class VoiceChat : Node3D
{
    [Export]
    private PlayerMovement _player;

    [Export]
    private AudioStreamPlayer _audioStreamPlayer3D;

    [Export]
    private AudioStreamPlayer _audioStreamRecorder;

    [Export]
    private Timer _sendRecordingTimer;

    private AudioEffectRecord effect;

    private AudioStreamWav recording;

    public override void _Ready()
    {
        if (_player.ControlledByPlayer)
            Initialize();
    }

    private void Initialize()
    {
        var idx = AudioServer.GetBusIndex("Record");
        effect = AudioServer.GetBusEffect(idx, 0) as AudioEffectRecord;
        effect.SetRecordingActive(true);

        _sendRecordingTimer.Timeout += OnSendRecordingTimerTimeout;

        // DataParser.OnVoiceChat += SendRecordingData;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SendRecordingData(AudioStreamWav recData)
    {
        _audioStreamPlayer3D.Stream = recData;
        _audioStreamPlayer3D.Play();
    }

    private void OnSendRecordingTimerTimeout()
    {
        recording = effect.GetRecording();
        effect.SetRecordingActive(false);

        // Dictionary<string, string> allData = new Dictionary<string, string>()
        // {
        //     {"DataRecording", Convert.ToBase64String(recording.Data)},
        //     {"Format", recording.Format.ToString()},
        //     {"MixRate", recording.MixRate.ToString()}
        // };

        // Dictionary<string, string> data = new Dictionary<string, string>()
        // {
        //     {"DataType", "Rpc"},
        //     {"PATH", GetPath()},
        //     {"MethodName", nameof(SendRecordingData)},
        //     {"Data", OwnJsonParser.Serialize(allData)}
        // };

        // if (SteamManager.Instance.IsHost)
        //     SteamManager.Instance.SendMessageToAll(OwnJsonParser.Serialize(data));
        // else
        //     SteamManager.Instance.SteamConnectionManager.Connection.SendMessage(OwnJsonParser.Serialize(data));

        //SendRecordingData(OwnJsonParser.Deserialize(OwnJsonParser.Serialize(data)));

        Rpc(nameof(SendRecordingData), recording);
        
        effect.SetRecordingActive(true);
    }
}
