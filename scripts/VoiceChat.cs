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
    private void SendRecordingData(byte[] recData)
    {
        AudioStreamWav sample = new AudioStreamWav()
        {
            Data = recData,
            Format = AudioStreamWav.FormatEnum.Format16Bits,
            MixRate = (int)AudioServer.GetMixRate() * 2
        };
        GD.Print("Received recording data");
        _audioStreamPlayer3D.Stream = sample;
        _audioStreamPlayer3D.Play();
    }

    private void OnSendRecordingTimerTimeout()
    {
        if (Multiplayer.MultiplayerPeer != null)
        {
            GD.Print(Multiplayer.GetPeers().Length);
            if (Multiplayer.GetPeers().Length > 0)
            {
                GD.Print("Sending recording data");
                recording = effect.GetRecording();
                effect.SetRecordingActive(false);
                Rpc(nameof(SendRecordingData), recording.Data);
                effect.SetRecordingActive(true);
            }
        }
    }
}
