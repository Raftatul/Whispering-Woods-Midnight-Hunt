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
    private AudioStreamPlayer _audioStreamRecorder;

    private Timer _sendRecordingTimer;

    private AudioStreamPlayer3D _audioStreamPlayer3D;

    private AudioEffectRecord effect;

    private AudioStreamWav recording;

    private Godot.Collections.Dictionary<string, AudioStreamPlayer3D> _audioStreamPlayer3Ds = new Godot.Collections.Dictionary<string, AudioStreamPlayer3D>();

    public override void _Ready()
    {
        Initialize();
    }

    private void Initialize()
    {
        var idx = AudioServer.GetBusIndex("Record");
        effect = AudioServer.GetBusEffect(idx, 0) as AudioEffectRecord;
        effect.SetRecordingActive(true);

        _sendRecordingTimer = new Timer();
        _sendRecordingTimer.WaitTime = 0.5f;
        _sendRecordingTimer.Timeout += OnSendRecordingTimerTimeout;
        AddChild(_sendRecordingTimer);
        _sendRecordingTimer.Start();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SendRecordingData(string audioPlayerPath, byte[] recData)
    {
        string relativePath = audioPlayerPath.Substr(audioPlayerPath.Find(GameManager.PlayerInstanceName), audioPlayerPath.Length);

        if (!_audioStreamPlayer3Ds.ContainsKey(relativePath))
        {
            audioPlayerPath = audioPlayerPath.ToString().Replace("/root/", "");
            _audioStreamPlayer3Ds.Add(relativePath, GetTree().Root.GetNode<AudioStreamPlayer3D>(audioPlayerPath));
        }

        AudioStreamWav sample = new AudioStreamWav()
        {
            Data = recData,
            Format = AudioStreamWav.FormatEnum.Format16Bits,
            MixRate = (int)AudioServer.GetMixRate() * 2
        };

        _audioStreamPlayer3Ds[relativePath].Stream = sample;
        _audioStreamPlayer3Ds[relativePath].Play();
    }

    private void OnSendRecordingTimerTimeout()
    {
        if (Multiplayer.MultiplayerPeer != null)
        {
            if (Multiplayer.GetPeers().Length > 0)
            {
                GD.Print("Sending recording data");
                recording = effect.GetRecording();
                effect.SetRecordingActive(false);
                Rpc(nameof(SendRecordingData), _audioStreamPlayer3D.GetPath(), recording.Data);
                effect.SetRecordingActive(true);
            }
        }
    }

    public void SetAudioOutput(AudioStreamPlayer3D audioStreamPlayer)
    {
        _audioStreamPlayer3D = audioStreamPlayer;
    }
}
