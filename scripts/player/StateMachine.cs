using Godot;
using System;

[GlobalClass]
public partial class StateMachine : Node
{
    [Export]
    private State currentState;

    [Export]
    private Label _stateDisplayer;

    private Godot.Collections.Dictionary<string, State> _states = new Godot.Collections.Dictionary<string, State>();

    public override async void _Ready()
    {
        foreach (Node child in GetChildren())
        {
            if (child is State state)
            {
                _states.Add(state.Name, state);
                state.Transition += newStateName => Rpc(MethodName.ChangeState, newStateName);
            }
            else
                GD.PrintErr("State machine child is not a state: " + child.Name);
        }

        await ToSignal(Owner, "ready");
        currentState.Enter();
        _stateDisplayer.Text = "State: " + currentState.Name;
        if (!Owner.IsMultiplayerAuthority())
        {
            SetPhysicsProcess(false);
            SetProcess(false);
        }
    }

    public override void _Process(double delta)
    {
        if (!Owner.IsMultiplayerAuthority())
            return;

        if (currentState != null)
            currentState.Update((float)delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!Owner.IsMultiplayerAuthority())
            return;

        if (currentState != null)
            currentState.PhysicsUpdate((float)delta);
    }

    public override void _Input(InputEvent @event)
    {
        if (!Owner.IsMultiplayerAuthority())
            return;

        if (currentState != null)
            currentState.Input(@event);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void ChangeState(string newStateName)
    {
        State newState = _states[newStateName];

        if (newState != null)
        {
            if (currentState != newState)
            {
                _stateDisplayer.Text = "State: " + newStateName;
                currentState.Exit();
                newState.Enter();
                currentState = newState;
            }
        }
        else
            GD.PrintErr("State machine does not contain state: " + newStateName);
    }
}
