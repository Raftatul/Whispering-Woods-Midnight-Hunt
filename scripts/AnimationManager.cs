using Godot;
using System;
using System.Threading.Tasks;

public partial class AnimationManager : AnimationTree
{
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SetFloat(string path, float value, float lerpSpeed = 0.1f)
    {
        float current = Get("parameters/" + path).AsSingle();
        float lerp = Mathf.Lerp(current, value, lerpSpeed);
        
        Set("parameters/" + path, lerp);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SetVector2(string path, Vector2 value, float lerpSpeed = 0.1f)
    {
        Vector2 current = Get("parameters/" + path).AsVector2();
        Vector2 lerp = current.Lerp(value, lerpSpeed);
        
        Set("parameters/" + path, lerp);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SetBool(string path, bool value)
    {
        Set("parameters/" + path, value);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RequestTransition(string path, string state)
    {
        Set("parameters/" + path, state);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RequestOneShot(string path, int request)
    {
        Set("parameters/" + path, (int)request);
    }
}
