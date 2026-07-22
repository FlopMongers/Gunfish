using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup masterAudioMixerGroup;
    private float rootMasterVolume = 0f;
    [SerializeField] private AudioMixerGroup musicAudioMixerGroup;
    private float rootMusicVolume = 0f;
    [SerializeField] private AudioMixerGroup sfxAudioMixerGroup;
    private float rootSFXVolume = 0f;
    [SerializeField] private AudioMixerGroup announcerAudioMixerGroup;
    private float rootAnnouncerVolume = 0f;

    private void Awake() {
        masterAudioMixerGroup.audioMixer.GetFloat("MasterVolume", out rootMasterVolume);
        musicAudioMixerGroup.audioMixer.GetFloat("MusicVolume", out rootMusicVolume);
        sfxAudioMixerGroup.audioMixer.GetFloat("SFXVolume", out rootSFXVolume);
        announcerAudioMixerGroup.audioMixer.GetFloat("AnnouncerVolume", out rootAnnouncerVolume);

        // Load saved volume settings from PlayerPrefs
        float savedMasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        float savedAnnouncerVolume = PlayerPrefs.GetFloat("AnnouncerVolume", 1.0f);

        SetMasterVolume(savedMasterVolume);
        SetMusicVolume(savedMusicVolume);
        SetSFXVolume(savedSFXVolume);
        SetAnnouncerVolume(savedAnnouncerVolume);      
    }

    // Converts a linear volume value (0.0 to 1.0) to decibels for use with the AudioMixer.
    // 1.0 = root volume, 0.0 = -80 dB (silence)
    private float ConvertToDecibels(float volume, float rootVolume) {
        if (volume <= 0.0f) {
            return -80.0f; // Minimum volume in decibels
        }
        // this is not a lerp since audio falls off logarithmically.
        return Mathf.Clamp(Mathf.Log10(volume) * 20.0f + rootVolume, -80.0f, 20.0f);
    }


    public void SetMasterVolume(float volume) {
        masterAudioMixerGroup.audioMixer.SetFloat("MasterVolume", ConvertToDecibels(volume, rootMasterVolume));
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume) {
        musicAudioMixerGroup.audioMixer.SetFloat("MusicVolume", ConvertToDecibels(volume, rootMusicVolume));
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume) {
        sfxAudioMixerGroup.audioMixer.SetFloat("SFXVolume", ConvertToDecibels(volume, rootSFXVolume));
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetAnnouncerVolume(float volume) {
        announcerAudioMixerGroup.audioMixer.SetFloat("AnnouncerVolume", ConvertToDecibels(volume, rootAnnouncerVolume));
        PlayerPrefs.SetFloat("AnnouncerVolume", volume);
    }
}
