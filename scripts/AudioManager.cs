using Godot;
using Godot.Collections;

public partial class AudioManager : Node
{
    [Export]
    private float _inputThreashold = 0.005f;
    [Export]
    public AudioStreamPlayer3D AudioOutput;
    [Export]
    private AudioStreamPlayer _input;

    private int _index;
    private Array<float> _receiveBuffer = new Array<float>();
    private AudioEffectCapture _effect;
    private AudioStreamGeneratorPlayback _playback;

    public override void _Process(double delta)
    {
        if (IsMultiplayerAuthority())
            ProcessMic();
        ProcessVoice();
    }

    public void SetupAudio(long id)
    {
        SetMultiplayerAuthority(((int)id));
        GD.Print("SetupAudio: " + id);

        if (IsMultiplayerAuthority())
        {
            _input.Stream = new AudioStreamMicrophone();
            _input.Play();

            _index = AudioServer.GetBusIndex("Record");
            _effect = AudioServer.GetBusEffect(_index, 0) as AudioEffectCapture;
        }

        _playback = AudioOutput.GetStreamPlayback() as AudioStreamGeneratorPlayback;
    }

    private void ProcessMic()
    {
        var stereoData = _effect.GetBuffer(_effect.GetFramesAvailable());

        if (stereoData.Length > 0)
        {
            var data = new float[stereoData.Length];
            float maxAmplitude = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                var value = (stereoData[i].X * stereoData[i].Y) / 2;
                maxAmplitude = Mathf.Max(value, maxAmplitude);

                data[i] = value;
            }

            if (maxAmplitude < _inputThreashold)
                return;

            Rpc(MethodName.SendData, data);
        }
    }

    private void ProcessVoice()
    {
        if (_receiveBuffer.Count <= 0)
            return;
        
        for (int i = 0; i < Mathf.Min(_playback.GetFramesAvailable(), _receiveBuffer.Count); i++)
        {
            Vector2 value = new Vector2(_receiveBuffer[0], _receiveBuffer[0]);
            _playback.PushFrame(value);
            _receiveBuffer.RemoveAt(0);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    private void SendData(float[] data)
    {
        _receiveBuffer.AddRange(data);
    }
}
