using Godot;

[GlobalClass]
public partial class InputAction : Resource
{
    [Export]
    public Godot.Collections.Array<string> Actions = new Godot.Collections.Array<string>();
}