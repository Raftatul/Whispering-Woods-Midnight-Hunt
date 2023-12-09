using Godot;
using System.IO;
using System.IO.Compression;

public partial class VoiceChat : Node3D
{
    [Export]
    private AudioStreamPlayer3D _audioStreamPlayer3D;

    [Export]
    private AudioStreamPlayer _audioStreamRecorder;

    [Export]
    private Timer _sendRecordingTimer;

    private AudioEffectRecord effect;

    public override void _Ready()
    {
        var idx = AudioServer.GetBusIndex("Record");
        effect = AudioServer.GetBusEffect(idx, 0) as AudioEffectRecord;
        effect.SetRecordingActive(true);

        _sendRecordingTimer.Timeout += OnSendRecordingTimerTimeout;
    }

    private void SendRecordingData(byte[] recData)
    {
        GD.Print("Audio package size : ", recData.Length);
        var sample = new AudioStreamWav();
        sample.Data = Decompress(recData);
        sample.Format = AudioStreamWav.FormatEnum.Format16Bits;
        sample.MixRate = ((int)(AudioServer.GetMixRate() * 2));
        _audioStreamPlayer3D.Stream = sample;
        _audioStreamPlayer3D.Play();
    }

    private void OnSendRecordingTimerTimeout()
    {
        var recording = effect.GetRecording();
        recording.Data = Compress(recording.Data);
        effect.SetRecordingActive(false);
        SendRecordingData(recording.Data);
        effect.SetRecordingActive(true);
    }

    public byte[] Compress(byte[] data)
    {
        MemoryStream output = new MemoryStream();
        using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
        {
            dstream.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    public byte[] Decompress(byte[] data)
    {
        MemoryStream input = new MemoryStream(data);
        MemoryStream output = new MemoryStream();
        using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
        {
            dstream.CopyTo(output);
        }
        return output.ToArray();
    }
}
