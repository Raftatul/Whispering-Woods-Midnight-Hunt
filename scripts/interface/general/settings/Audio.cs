using Godot;
using System;

public partial class Audio : Control
{
    [Export]
    private Slider _mainVolumeSlider;

    [Export]
    private Slider _musicVolumeSlider;

    [Export]
    private Slider _sfxVolumeSlider;

    public override void _Ready()
    {
        _mainVolumeSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master")));
        _musicVolumeSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Music")));
        _sfxVolumeSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("SFX")));

        _mainVolumeSlider.ValueChanged += OnMainVolumeSliderValueChanged;
        _musicVolumeSlider.ValueChanged += OnMusicVolumeSliderValueChanged;
        _sfxVolumeSlider.ValueChanged += OnSfxVolumeSliderValueChanged;
    }

    private void OnMainVolumeSliderValueChanged(double value)
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), Mathf.LinearToDb((float)value));
        AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), value < 0.05);
    }

    private void OnMusicVolumeSliderValueChanged(double value)
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), Mathf.LinearToDb((float)value));
        AudioServer.SetBusMute(AudioServer.GetBusIndex("Music"), value < 0.05);
    }

    private void OnSfxVolumeSliderValueChanged(double value)
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), Mathf.LinearToDb((float)value));
        AudioServer.SetBusMute(AudioServer.GetBusIndex("SFX"), value < 0.05);
    }
}
