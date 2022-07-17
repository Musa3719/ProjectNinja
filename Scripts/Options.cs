using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    public static Options _instance;
    public float SoundVolume { get; private set; }
    public float MusicVolume { get; private set; }

    public Slider SoundSlider;
    public Slider MusicSlider;

    private void Awake()
    {
        _instance = this;
        SoundVolume = PlayerPrefs.GetFloat("Sound", 0.5f);
        MusicVolume = PlayerPrefs.GetFloat("Music", 0.5f);
        SoundSlider.value = SoundVolume;
        MusicSlider.value = MusicVolume;
    }
    public void SoundVolumeChanged(float newValue)
    {
        SoundVolume = newValue;
        PlayerPrefs.SetFloat("Sound", newValue);
    }
    public void MusicVolumeChanged(float newValue)
    {
        MusicVolume = newValue;
        PlayerPrefs.SetFloat("Music", newValue);
    }
}
