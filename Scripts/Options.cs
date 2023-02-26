using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    public static Options _instance;
    public float SoundVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float MouseSensitivity { get; private set; }
    public float MaxSensitivity { get; private set; }



    public Slider SoundSlider;
    public Slider MusicSlider;
    public Slider SensitivitySlider;

    private void Awake()
    {
        Debug.unityLogger.logEnabled = Debug.isDebugBuild;
        _instance = this;
        MaxSensitivity = 3f;
        SoundVolume = PlayerPrefs.GetFloat("Sound", 0.5f);
        MusicVolume = PlayerPrefs.GetFloat("Music", 0.5f);
        SoundSlider.value = SoundVolume;
        MusicSlider.value = MusicVolume;
    }

    private void Start()
    {
        SensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 0.75f);
        MouseSensitivity = SensitivitySlider.value * MaxSensitivity;
    }
    public void SoundVolumeChanged(float newValue)
    {
        SoundVolume = newValue;
        PlayerPrefs.SetFloat("Sound", newValue);
        ArrangeActiveSoundVolumes(newValue);
    }
    public void MusicVolumeChanged(float newValue)
    {
        MusicVolume = newValue;
        PlayerPrefs.SetFloat("Music", newValue);
        ArrangeActiveMusicVolumes(newValue);
    }
    public void MouseSensitivityVolumeChanged(float newValue)
    {
        PlayerPrefs.SetFloat("Sensitivity", newValue);

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 0)
        {
            MouseSensitivity = newValue * MaxSensitivity;
        }
    }

    private void ArrangeActiveSoundVolumes(float newValue)
    {
        if (SoundManager._instance != null && SoundManager._instance.SoundObjectsParent != null)
        {
            foreach (Transform sound in SoundManager._instance.SoundObjectsParent.transform)
            {
                if (newValue != 0f)
                    sound.GetComponent<AudioSource>().volume = newValue * sound.transform.localEulerAngles.x;
            }
        }
    }
    private void ArrangeActiveMusicVolumes(float newValue)
    {
        if(SoundManager._instance != null && SoundManager._instance.CurrentMusicObject != null)
        {
            if (newValue != 0f)
                SoundManager._instance.CurrentMusicObject.GetComponent<AudioSource>().volume = newValue * SoundManager._instance.CurrentMusicObject.transform.localEulerAngles.x;
        }
    }
}