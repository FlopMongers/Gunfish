using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class VolumeController : MonoBehaviour {
    public AudioMixer mixer;
    public Slider slider;

    public string volumeName;
    [Range(0, 1)]
    public float defaultValue = 0.75f;

    void Start() {
        slider.value = PlayerPrefs.GetFloat(volumeName, defaultValue);
    }
    public void SetLevel() {
        float sliderValue = slider.value;
        if (sliderValue > 0) {
            mixer.SetFloat(volumeName, Mathf.Log10(sliderValue) * 20);
        }
        else {
            mixer.SetFloat(volumeName, -80f);
        }
        PlayerPrefs.SetFloat(volumeName, sliderValue);
    }
}