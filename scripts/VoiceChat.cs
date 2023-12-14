using Godot;
using Steamworks;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

public partial class VoiceChat : Node3D
{
    [Export]
    private PlayerMovement _player;

    [Export]
    private AudioStreamPlayer3D _audioStreamPlayer3D;

    [Export]
    private AudioStreamPlayer _audioStreamRecorder;

    [Export]
    private Timer _sendRecordingTimer;

    private AudioEffectRecord effect;

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
        DataParser.OnVoiceChat += SendRecordingData;
    }

    private void SendRecordingData(Dictionary<string, string> recData)
    {
        byte[] data = System.Convert.FromBase64String(recData["Data"]);
        GD.Print("Received recording data raw : ", recData["Data"]);
        GD.Print("Received recording data", data.ToString());

        var sample = new AudioStreamWav
        {
            Data = data,
            Format = AudioStreamWav.FormatEnum.Format16Bits,
            MixRate = ((int)(AudioServer.GetMixRate() * 2))
        };
        _audioStreamPlayer3D.Stream = sample;
        _audioStreamPlayer3D.Play();
    }

    private void OnSendRecordingTimerTimeout()
    {
        var recording = effect.GetRecording();
        recording.Data = recording.Data;
        effect.SetRecordingActive(false);
        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            {"DataType", "VoiceChat"},
            {"Data", System.Convert.ToBase64String(recording.Data)}
        };

        GD.Print("Sending recording data", data["Data"]);

        if (SteamManager.Instance.IsHost)
            SteamManager.Instance.SendMessageToAll(OwnJsonParser.Serialize(data));
        else
            SteamManager.Instance.SteamConnectionManager.Connection.SendMessage(OwnJsonParser.Serialize(data));
        
        effect.SetRecordingActive(true);
    }
}
