using Godot;
using System;

[GlobalClass]
public partial class State : Node
{
    [Signal]
    public delegate void TransitionEventHandler(string newStateName);

    public virtual void Enter() {}

    public virtual void Update(float delta) {}

    public virtual void PhysicsUpdate(float delta) {}

    public virtual void Exit() {}

    public virtual void Input(InputEvent @event) {}
}
