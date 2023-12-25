using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    public static Options _instance;
    public float SoundVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float MouseSensitivity { get; private set; }
    public float MaxSensitivity { get; private set; }

    public float FOV { get; private set; }
    public int Quality { get; private set; }
    public int ControllerForTutorial { get; private set; }

    public GameObject QualityUI;
    public GameObject TutorialButtonsUI;
    public GameObject JoystickUI;
    public GameObject JoystickNeed;

    private Coroutine _graphicsBackToNormalCoroutine;

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
        FOV = PlayerPrefs.GetFloat("FOV", 63f);

        if (SceneController._instance.IsLastScene()) return;

        SoundSlider.value = SoundVolume;
        MusicSlider.value = MusicVolume;
        FovSlider.value = FOV;
        Quality = PlayerPrefs.GetInt("Quality", 2);
        ControllerForTutorial = PlayerPrefs.GetInt("TutorialNames", 0);
        ArrangeTutorailButtonNamesUI();
        SetQuality(Quality);

    }

    private void Start()
    {
        if (SceneController._instance.IsLastScene()) return;

        SensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 0.3f);
        MouseSensitivity = SensitivitySlider.value * MaxSensitivity;
        Options._instance.ArrangeQualityUI();
        CheckGraphicsToLow();
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
        if (SoundManager._instance != null)
            SoundManager._instance.ContinueMusic();
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
        Quality = number;

        if (oldQuality == 2 && number != 2 && GameObject.Find("Level") != null)
        {
            GraphicsBackToNormal();
        }
        else if (oldQuality != 2 && number == 2 && GameObject.Find("Level") != null)
        {
            CheckGraphicsToLow();
        }
        ArrangeQualityUI();
    }
    public void ArrangeQualityUI()
    {
        QualityUI.transform.Find("Low").transform.Find("ActiveImage").gameObject.SetActive(false);
        QualityUI.transform.Find("Medium").transform.Find("ActiveImage").gameObject.SetActive(false);
        QualityUI.transform.Find("High").transform.Find("ActiveImage").gameObject.SetActive(false);

        switch (Quality)
        {
            case 0:
                QualityUI.transform.Find("High").transform.Find("ActiveImage").gameObject.SetActive(true);
                break;
            case 1:
                QualityUI.transform.Find("Medium").transform.Find("ActiveImage").gameObject.SetActive(true);
                break;
            case 2:
                QualityUI.transform.Find("Low").transform.Find("ActiveImage").gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }
    public void SetTutorailButtonNames(int number)
    {
        if (PlayerPrefs.GetInt("TutorialNames") != number)
        {
            PlayerPrefs.SetInt("TutorialNames", number);
            ControllerForTutorial = number;
            ArrangeTutorailButtonNamesUI();
        }
        
    }
    public void ArrangeTutorailButtonNamesUI()
    {
        if (SceneController._instance.SceneBuildIndex != 0) return;

        TutorialButtonsUI.transform.Find("Keyboard").transform.Find("ActiveImage").gameObject.SetActive(false);
        TutorialButtonsUI.transform.Find("Gamepad").transform.Find("ActiveImage").gameObject.SetActive(false);

        switch (ControllerForTutorial)
        {
            case 0:
                TutorialButtonsUI.transform.Find("Keyboard").transform.Find("ActiveImage").gameObject.SetActive(true);
                break;
            case 1:
                TutorialButtonsUI.transform.Find("Gamepad").transform.Find("ActiveImage").gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }
    public void OpenJoystickArrangementNeedText()
    {
        JoystickNeed.SetActive(true);
        Invoke("JoystickNeedClose", 7f);
    }
    private void JoystickNeedClose()
    {
        JoystickNeed.SetActive(false);
    }
    public void CheckGraphicsToLow()
    {
        if (PlayerPrefs.GetInt("Quality") == 2 && GameObject.Find("Level") != null)
        {
            if (_graphicsBackToNormalCoroutine != null)
                StopCoroutine(_graphicsBackToNormalCoroutine);


            //GameObject.Find("Level").transform.Find("ReflectionProbs").gameObject.SetActive(false);
            //GameObject.Find("Level").transform.Find("ReflectionProbs").gameObject.SetActive(true);

            Transform lights = GameObject.Find("Level").transform.Find("Lights");
            foreach (Transform light in lights)
            {
                if (light.name == "Sky and Fog Volume")
                {
                    if (GameManager._instance != null)
                        light.GetComponent<Volume>().profile = GameManager._instance.LowSettingsVolume;
                }
                else if (light.Find("Lights") != null)
                {
                    light.GetComponentInChildren<HDAdditionalLightData>().affectsVolumetric = false;
                }

            }
        }
    }
    public void GraphicsBackToNormal()
    {
        GameManager._instance.CoroutineCall(ref _graphicsBackToNormalCoroutine, GraphicsBackToNormalCoroutine(), this);
    }
    private IEnumerator GraphicsBackToNormalCoroutine()
    {
        Transform lights = GameObject.Find("Level").transform.Find("Lights");
        foreach (Transform light in lights)
        {
            if (light.name == "Sky and Fog Volume")
            {
                if (GameManager._instance != null)
                    light.GetComponent<Volume>().profile = GameManager._instance.NormalSettingsVolume;
            }
            else if (light.Find("Lights") != null)
            {
                light.GetComponentInChildren<HDAdditionalLightData>().affectsVolumetric = true;
            }
            yield return null;
        }
        //GameObject.Find("Level").transform.Find("ReflectionProbs").gameObject.SetActive(false);
        //GameObject.Find("Level").transform.Find("ReflectionProbs").gameObject.SetActive(true);
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
