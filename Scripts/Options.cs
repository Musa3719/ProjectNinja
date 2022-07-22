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
    public Slider SensitivitySlider;

    private void Awake()
    {
        _instance = this;
        SoundVolume = PlayerPrefs.GetFloat("Sound", 0.5f);
        MusicVolume = PlayerPrefs.GetFloat("Music", 0.5f);
        SoundSlider.value = SoundVolume;
        MusicSlider.value = MusicVolume;
    }
    private void Start()
    {
        SensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 0.75f);
        CameraController._instance._mouseSensitivity = SensitivitySlider.value * CameraController._instance._maxSensitivity;

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
    public void MouseSensitivityVolumeChanged(float newValue)
    {
        if (newValue <= 0.2f)
        {
            newValue = 0.2f;
            SensitivitySlider.value = 0.2f;
        }

        PlayerPrefs.SetFloat("Sensitivity", newValue);

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 0)
        {
            CameraController._instance._mouseSensitivity = newValue * CameraController._instance._maxSensitivity;
        }
    }
}
