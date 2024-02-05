using Godot;

public partial class PlayerCard : Control
{
    [Export]
    public Label PlayerNameLabel { get; set; }

    [Export]
    public TextureRect PlayerAvatar { get; set; }

    [Export]
    public RichTextLabel PlayerStatus { get; set; }

    public void SetLabels(string playerName, ImageTexture playerAvatar)
    {
        PlayerNameLabel.Text = playerName;
        PlayerAvatar.Texture = playerAvatar;
    }

    public void SetStatus(bool status)
    {
        if (status)
        {
            PlayerStatus.Text = "Ready";
        }
        else
        {
            PlayerStatus.Text = "Not Ready";
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}