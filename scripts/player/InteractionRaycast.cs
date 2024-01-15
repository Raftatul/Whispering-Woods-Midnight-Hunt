using Godot;
using System;

public partial class InteractionRaycast : RayCast3D
{
    [Export]
    private Label _label;

    public override void _PhysicsProcess(double delta)
    {       
        if (IsColliding())
        {
            IInteractable interactable = GetCollider() as IInteractable;
            SetLabelVisibility(true, interactable.InteractionText);
        }
        else
            SetLabelVisibility(false);
    }

    public void Interact()
    {
        if (!IsColliding())
            return;

        if (GetCollider() is IInteractable interactable)
            interactable.Rpc(nameof(IInteractable.Interact));
    }

    private void SetLabelVisibility(bool visible, string interactableText = "")
    {
        var interactionKey = InputMap.ActionGetEvents("interact")[0].AsText();
        interactionKey = interactionKey.Replace("(Physical)", "");
        interactionKey = interactionKey.Trim();

        _label.Text = "[" + interactionKey + "] " + interactableText;
        _label.Visible = visible;
    }
}
