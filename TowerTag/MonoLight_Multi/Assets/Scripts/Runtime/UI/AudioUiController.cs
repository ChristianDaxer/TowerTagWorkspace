using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

public class AudioUiController : MonoBehaviour {
    #region Audio

    [FormerlySerializedAs("audioMixer")] [Header("Audio")] [SerializeField, Tooltip("The AudioMixer which is used in the game")]
    private AudioMixer _audioMixer;

    [FormerlySerializedAs("masterVolumeName")] [SerializeField, Tooltip("The of the exposed parameter for the master volume in the audio mixer")]
    private string _masterVolumeName;

    [FormerlySerializedAs("musicParameterName")] [SerializeField, Tooltip("The of the exposed parameter for the music volume in the audio mixer")]
    private string _musicParameterName;

    [FormerlySerializedAs("announcerParameterName")] [SerializeField, Tooltip("The of the exposed parameter for the announcer volume in the audio mixer")]
    private string _announcerParameterName;

    [FormerlySerializedAs("soundParameterName")] [SerializeField, Tooltip("The of the exposed parameter for the sound effects volume in the audio mixer")]
    private string _soundParameterName;

    [FormerlySerializedAs("teammatesParameterName")] [SerializeField, Tooltip("The of the exposed parameter for the teammates volume in the audio mixer")]
    private string _teammatesParameterName;

    #endregion

    /* Audio Tab */

    /// <summary>
    /// Pass the master volume to the exposed parameter of the audio mixer
    /// </summary>
    /// <param name="masterVolume"></param>
    public void SetMasterVolume(float masterVolume) {
        _audioMixer.SetFloat(_masterVolumeName, HelperFunctions.LinearVolumeToDecibel(masterVolume));
    }

    /// <summary>
    /// Get the current master volume from the exposed parameter of the audio mixer
    /// </summary>
    /// <returns></returns>
    public float GetMasterVolume() {
        if (!_audioMixer.GetFloat(_masterVolumeName, out float volume)) {
            Debug.LogError(name + ":" + GetType().Name + " - " + "Couldn't get the " + _masterVolumeName + " exposed Parameter of the AudioMixer " + _audioMixer.name);
        }

        return HelperFunctions.DecibelVolumeToLinear(volume);
    }

    /// <summary>
    /// Pass the music volume to the exposed parameter of the audio mixer
    /// </summary>
    /// <param name="musicVolume"></param>
    public void SetMusicVolume(float musicVolume) {
        _audioMixer.SetFloat(_musicParameterName, HelperFunctions.LinearVolumeToDecibel(musicVolume));
    }

    /// <summary>
    /// Get the current music volume from the exposed parameter of the audio mixer
    /// </summary>
    /// <returns></returns>
    public float GetMusicVolume() {
        if (!_audioMixer.GetFloat(_musicParameterName, out float volume)) {
            Debug.LogError(name + ":" + GetType().Name + " - " + "Couldn't get the " + _musicParameterName + " exposed Parameter of the AudioMixer " + _audioMixer.name);
        }

        return HelperFunctions.DecibelVolumeToLinear(volume);
    }

    /// <summary>
    /// Pass the announcer volume to the exposed parameter of the audio mixer
    /// </summary>
    /// <param name="announcerVolume"></param>
    public void SetAnnouncerVolume(float announcerVolume) {
        _audioMixer.SetFloat(_announcerParameterName, HelperFunctions.LinearVolumeToDecibel(announcerVolume));
    }

    /// <summary>
    /// Get the current announcer volume from the exposed parameter of the audio mixer
    /// </summary>
    /// <returns></returns>
    public float GetAnnouncerVolume() {
        if (!_audioMixer.GetFloat(_announcerParameterName, out float volume)) {
            Debug.LogError(name + ":" + GetType().Name + " - " + "Couldn't get the " + _announcerParameterName + " exposed Parameter of the AudioMixer " + _audioMixer.name);
        }

        return HelperFunctions.DecibelVolumeToLinear(volume);
    }

    /// <summary>
    /// Pass the sound volume to the exposed parameter of the audio mixer
    /// </summary>
    /// <param name="soundVolume"></param>
    public void SetSoundVolume(float soundVolume) {
        _audioMixer.SetFloat(_soundParameterName, HelperFunctions.LinearVolumeToDecibel(soundVolume));
    }

    /// <summary>
    /// Get the current sound volume from the exposed parameter of the audio mixer
    /// </summary>
    /// <returns></returns>
    public float GetSoundVolume() {
        if (!_audioMixer.GetFloat(_soundParameterName, out float volume)) {
            Debug.LogError(name + ":" + GetType().Name + " - " + "Couldn't get the " + _soundParameterName + " exposed Parameter of the AudioMixer " + _audioMixer.name);
        }

        return HelperFunctions.DecibelVolumeToLinear(volume);
    }

    /// <summary>
    /// Pass the teammates volume to the exposed parameter of the audio mixer
    /// </summary>
    /// <param name="teammatesVolume"></param>
    public void SetTeammatesVolume(float teammatesVolume) {
        _audioMixer.SetFloat(_teammatesParameterName, HelperFunctions.LinearVolumeToDecibel(teammatesVolume));
    }

    /// <summary>
    /// Get the current teammates volume from the exposed parameter of the audio mixer
    /// </summary>
    /// <returns></returns>
    public float GetTeammatesVolume() {
        if (!_audioMixer.GetFloat(_teammatesParameterName, out float volume)) {
            Debug.LogError(name + ":" + GetType().Name + " - " + "Couldn't get the " + _teammatesParameterName + " exposed Parameter of the AudioMixer " + _audioMixer.name);
        }

        return HelperFunctions.DecibelVolumeToLinear(volume);
    }
}