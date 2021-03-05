using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundMenu : MonoBehaviour {
    [SerializeField] private Animator _soundMenu;
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundSlider;
    [SerializeField] private Slider _announcerSlider;
    [SerializeField] private Slider _teammatesSlider;
    private const string MasterVolume = "masterVolume";
    private const string TeammatesVolume = "teammatesVolume";
    private const string AnnouncerVolume = "announcerVolume";
    private const string SoundVolume = "soundVolume";
    private const string MusicVolume = "musicVolume";
    private bool _active;

    private void Start() {
        _audioMixer.GetFloat(MasterVolume, out float master);
        _masterSlider.value = HelperFunctions.DecibelVolumeToLinear(master);

        _audioMixer.GetFloat(MusicVolume, out float music);
        _musicSlider.value = HelperFunctions.DecibelVolumeToLinear(music);

        _audioMixer.GetFloat(SoundVolume, out float sound);
        _soundSlider.value = HelperFunctions.DecibelVolumeToLinear(sound);

        _audioMixer.GetFloat(AnnouncerVolume, out float announcer);
        _announcerSlider.value = HelperFunctions.DecibelVolumeToLinear(announcer);

        _audioMixer.GetFloat(TeammatesVolume, out float mates);
        _teammatesSlider.value = HelperFunctions.DecibelVolumeToLinear(mates);
    }

    [UsedImplicitly]
    public void ChangeMasterVolume(float value) {
        _audioMixer.SetFloat(MasterVolume, HelperFunctions.LinearVolumeToDecibel(value));
    }

    [UsedImplicitly]
    public void ChangeVoiceChatVolume(float value) {
        _audioMixer.SetFloat(TeammatesVolume, HelperFunctions.LinearVolumeToDecibel(value));
    }

    [UsedImplicitly]
    public void ChangeAnnouncerVolume(float value) {
        _audioMixer.SetFloat(AnnouncerVolume, HelperFunctions.LinearVolumeToDecibel(value));
    }

    [UsedImplicitly]
    public void ChangeSoundFXVolume(float value) {
        _audioMixer.SetFloat(SoundVolume, HelperFunctions.LinearVolumeToDecibel(value));
    }

    [UsedImplicitly]
    public void ChangeMusicVolume(float value) {
        _audioMixer.SetFloat(MusicVolume, HelperFunctions.LinearVolumeToDecibel(value));
    }

    public void ToggleSoundMenu() {
        _active = !_active;
        _soundMenu.SetTrigger(_active ? "spawn" : "despawn");

        if (!_active)
            SaveSettingsToConfig();
    }

    private void SaveSettingsToConfig() {
        Configuration config = ConfigurationManager.Configuration;
        config.MasterVolume = _masterSlider.value;
        config.MusicVolume = _musicSlider.value;
        config.SoundFxVolume = _soundSlider.value;
        config.AnnouncerVolume = _announcerSlider.value;
        config.TeammatesVolume = _teammatesSlider.value;
        ConfigurationManager.WriteConfigToFile();
    }
}