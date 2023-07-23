using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    Slider slider;
    public AudioMixer mixer;

    public static float volume = 1f;

    public void Start()
    {
        // set slider
        // to mixer value
        slider = GetComponent<Slider>();
        slider.value = volume;
    }

    public void SetLevel(float sliderValue)
    {
        volume = sliderValue;
        if (sliderValue <= 0)
            sliderValue = -40;
        else
            sliderValue = Mathf.Log10(sliderValue) * 20;
        mixer.SetFloat("MasterVolume", sliderValue);
    }

}
