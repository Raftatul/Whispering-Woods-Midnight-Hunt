using Godot;
using System;

public partial class control : Control
{
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
        var idx = AudioServer.GetBusIndex("Record");
        effect = AudioServer.GetBusEffect(idx, 0) as AudioEffectRecord;
        effect.SetRecordingActive(true);

        _sendRecordingTimer.Timeout += OnSendRecordingTimerTimeout;
    }

    private void _on_start_pressed()
    {
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        peer.CreateServer(7000, 6);
        Multiplayer.MultiplayerPeer = peer;

        Upnp upnp = new Upnp();
        upnp.Discover();
        upnp.AddPortMapping(7000);

        GD.Print("Server started");
    }

    private void _on_join_pressed()
    {
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        peer.CreateClient("", 6);
        Multiplayer.MultiplayerPeer = peer;

        GD.Print("Client started");
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

        Rpc(nameof(SendRecordingData), recording);
        
        effect.SetRecordingActive(true);
    }
}
