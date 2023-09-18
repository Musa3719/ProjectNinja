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

    public float FOV { get; private set; }



    public Slider SoundSlider;
    public Slider MusicSlider;
    public Slider SensitivitySlider;
    public Slider FovSlider;

    private void Awake()
    {
        Debug.unityLogger.logEnabled = Debug.isDebugBuild;
        _instance = this;
        MaxSensitivity = 3f;
        SoundVolume = PlayerPrefs.GetFloat("Sound", 0.33f);
        MusicVolume = PlayerPrefs.GetFloat("Music", 0.33f);
        FOV = PlayerPrefs.GetFloat("FOV", 70f);
        SoundSlider.value = SoundVolume;
        MusicSlider.value = MusicVolume;
        FovSlider.value = FOV;
    }

    private void Start()
    {
        SensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 0.3f);
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
    public void FOVVolumeChanged(float newValue)
    {
        PlayerPrefs.SetFloat("FOV", newValue);
        FOV = newValue;
    }
    public void SetQuality(int number)
    {
        int oldQuality = PlayerPrefs.GetInt("Quality");
        QualitySettings.SetQualityLevel(number);
        PlayerPrefs.SetInt("Quality", number);

        if (oldQuality == 2 && number != 2 && GameObject.Find("Level") != null)
        {
            GameManager._instance.GraphicsBackToNormal();
        }
        else if (oldQuality != 2 && number == 2 && GameObject.Find("Level") != null)
        {
            GameManager._instance.ArrangeGraphicsToLow();
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

        if (SoundManager._instance != null && SoundManager._instance.CurrentAtmosphereObject != null)
        {
            if (newValue != 0f)
                SoundManager._instance.CurrentAtmosphereObject.GetComponent<AudioSource>().volume = newValue * SoundManager._instance.CurrentAtmosphereObject.transform.localEulerAngles.x;
            else
                SoundManager._instance.CurrentAtmosphereObject.GetComponent<AudioSource>().volume = 0f;
        }
    }
    private void ArrangeActiveMusicVolumes(float newValue)
    {
        if(SoundManager._instance != null && SoundManager._instance.CurrentMusicObject != null)
        {
            if (newValue != 0f)
                SoundManager._instance.CurrentMusicObject.GetComponent<AudioSource>().volume = newValue * SoundManager._instance.CurrentMusicObject.transform.localEulerAngles.x;
            else
                SoundManager._instance.CurrentMusicObject.GetComponent<AudioSource>().volume = 0f;
        }
    }
}
