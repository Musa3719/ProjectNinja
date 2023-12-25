using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class SceneController : MonoBehaviour
{
    private enum ButtonNamesForMenu
    {
        PlayHover,
        EpisodeScreenHover,
        EpisodeScreen,
        QuitHover,
        Default,
        JoystickSettings
    }

    [SerializeField]
    private GameObject LevelNotReachedObject;
    [SerializeField]
    public GameObject LoadingObject;
    [SerializeField]
    public GameObject OpeningCinematic;

    private Coroutine LevelNotReachedCoroutine;
    private Coroutine _cinematicImageChangeCoroutine;
    private Coroutine _openingCinematicArrangementCoroutine;
    private Coroutine _menuCameraToPosAndRotCoroutine;

    private Dictionary<ButtonNamesForMenu, Vector3> _numberToPos;
    private Dictionary<ButtonNamesForMenu, Vector3> _numberToAngles;
    private int _lastMenuCameraNumber;

    public static SceneController _instance;
    public static AsyncOperation NextSceneAsyncOperation;

    public int SceneBuildIndex { get; private set; }

    private void Awake()
    {
        SceneBuildIndex = SceneManager.GetActiveScene().buildIndex;

        _lastMenuCameraNumber = -1;

        if (SceneBuildIndex == 0 || IsLastScene())
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        _instance = this;
        Time.timeScale = 1f;
        if (SceneBuildIndex == 0)
        {
            ArrangeMenuLevels();

            if (Gamepad.current == null)
            {
                GameObject.Find("Canvas").transform.Find("MainMenu").Find("TutorialSettingText").gameObject.SetActive(false);
                GameObject.Find("Canvas").transform.Find("MainMenu").Find("TutorialSetting").gameObject.SetActive(false);
            }
        }
        else if (!IsLastScene())
        {
            if (PlayerPrefs.GetInt("Level", 0) < SceneBuildIndex)
                PlayerPrefs.SetInt("Level", SceneBuildIndex);
        }

        _numberToPos = new Dictionary<ButtonNamesForMenu, Vector3>();
        _numberToAngles = new Dictionary<ButtonNamesForMenu, Vector3>();

        _numberToPos.Add(ButtonNamesForMenu.PlayHover, new Vector3(-5.80431f, 0.38f, 3.72f));
        _numberToAngles.Add(ButtonNamesForMenu.PlayHover, new Vector3(-31.283f, 94.402f, 0f));

        _numberToPos.Add(ButtonNamesForMenu.EpisodeScreenHover, new Vector3(-2.377943f, 1.852566f, 6.161628f));
        _numberToAngles.Add(ButtonNamesForMenu.EpisodeScreenHover, new Vector3(5.844f, 178.082f, 0.003f));

        _numberToPos.Add(ButtonNamesForMenu.EpisodeScreen, new Vector3(-2.286085f, 0.5624065f, 4.339514f));
        _numberToAngles.Add(ButtonNamesForMenu.EpisodeScreen, new Vector3(-25.783f, 179.455f, -0.004f));

        _numberToPos.Add(ButtonNamesForMenu.QuitHover, new Vector3(0.3561263f, 1.995896f, 4.293057f));
        _numberToAngles.Add(ButtonNamesForMenu.QuitHover, new Vector3(3.438f, 37.754f, 0.004f));

        _numberToPos.Add(ButtonNamesForMenu.Default, Camera.main.transform.position);
        _numberToAngles.Add(ButtonNamesForMenu.Default, Camera.main.transform.eulerAngles);

        _numberToPos.Add(ButtonNamesForMenu.JoystickSettings, new Vector3(-1.234567f, 2.632975f, 3.015662f));
        _numberToAngles.Add(ButtonNamesForMenu.JoystickSettings, new Vector3(28.257f, 133.558f, 0f));

    }

    private void Start()
    {
        GameObject debug = GameObject.Find("[Debug Updater]");
        if (debug != null)
            debug.SetActive(false);
    }
    public bool IsLastScene()
    {
        if (SceneBuildIndex == SceneManager.sceneCountInBuildSettings - 1) return true;
        return false;
    }
    public void CallForAction(Action action, float time)
    {
        StartCoroutine(CallForActionCoroutine(action, time));
    }
    private IEnumerator CallForActionCoroutine(Action action, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        action?.Invoke();
    }
    private void ArrangeMenuLevels()
    {
        Transform bloods = GameObject.Find("MenuLevel").transform.Find("Bloods");

        int levelReached = PlayerPrefs.GetInt("Level", 0);
        int i = 1;
        foreach (Transform level in GameObject.Find("Canvas").transform.Find("SelectEpisode").transform)
        {
            Image image = level.GetComponent<UnityEngine.UI.Image>();
            if (levelReached >= i)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0.9f);
            }
            else
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0.25f);
            }

            foreach (Transform room in level)
            {
                if (levelReached >= i)
                {
                    if (bloods.GetChild(i - 1) != null)
                        bloods.GetChild(i - 1).GetComponent<DecalProjector>().enabled = true;

                    //item.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                    room.gameObject.SetActive(true);
                    int localIndex = i;
                    room.GetComponent<Button>().onClick.AddListener(() => { LoadScene(localIndex); });
                }
                else
                {
                    //item.GetComponent<UnityEngine.UI.Image>().color = Color.gray;
                    room.gameObject.SetActive(false);
                }
                i++;
            }
        }
    }
    public void MenuCameraToPosAndRot(int number)
    {
        if (_lastMenuCameraNumber == number) return;

        Vector3 pos = _numberToPos[(ButtonNamesForMenu)number];
        Vector3 angles = _numberToAngles[(ButtonNamesForMenu)number];

        _lastMenuCameraNumber = number;

        CoroutineCall(ref _menuCameraToPosAndRotCoroutine, MenuCameraToPosAndRotCoroutine(pos, angles));
    }
    public void CoroutineCall(ref Coroutine coroutine, IEnumerator method)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(method);
    }
    private IEnumerator MenuCameraToPosAndRotCoroutine(Vector3 pos, Vector3 angles)
    {
        Quaternion targetRot = Quaternion.Euler(angles);
        float startTime = Time.time;
        float lerpSpeed = 0f;
        while (startTime + 5f > Time.time)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, pos, Time.deltaTime * lerpSpeed);
            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, targetRot, Time.deltaTime * lerpSpeed);
            lerpSpeed += Time.deltaTime * 3.5f;
            yield return null;
        }
    }
    public void PlayButtonSound()
    {
        GameObject cam = GameManager._instance == null ? Camera.main.gameObject : GameManager._instance.MainCamera;
        SoundManager._instance.PlaySound(SoundManager._instance.Button, cam.transform.position, 0.15f, false, UnityEngine.Random.Range(0.65f, 0.75f)).transform.parent = cam.transform;
    }
    private IEnumerator OpeningCinematicArrangement()
    {
        Destroy(SoundManager._instance.CurrentMusicObject);
        OpeningCinematic.SetActive(true);
        GameObject.Find("Canvas").SetActive(false);

        foreach (Transform item in SoundManager._instance.SoundObjectsParent.transform)
        {
            if (item.GetComponent<AudioSource>() != null && item.GetComponent<AudioSource>().clip.name.Equals("Burning_1"))
                item.GetComponent<AudioSource>().Stop();
        }

        StartCoroutine(OpeningCinematicTextCoroutine());

        float startTime = Time.time;
        while (startTime + 90f > Time.time)
        {
            if (InputHandler.GetButtonDown("Esc"))
            {
                if (!GameObject.Find("OpeningCinematic").transform.Find("PassCinematic").gameObject.activeInHierarchy)
                {
                    GameObject.Find("OpeningCinematic").transform.Find("PassCinematic").gameObject.SetActive(true);
                    Time.timeScale = 0f;
                    GameObject.Find("OpeningCinematic").transform.Find("RawImage").GetComponent<VideoPlayer>().Pause();
                }
                else
                {
                    GameObject.Find("OpeningCinematic").transform.Find("PassCinematic").gameObject.SetActive(false);
                    Time.timeScale = 1f;
                    GameObject.Find("OpeningCinematic").transform.Find("RawImage").GetComponent<VideoPlayer>().Play();
                }

            }
            yield return null;
        }
        if (OpeningCinematic.transform.Find("Loading").gameObject.activeInHierarchy) yield break;

        OpeningCinematic.transform.Find("Loading").gameObject.SetActive(true);

        yield return null;

        LoadSceneAsync(1);
    }
    private IEnumerator OpeningCinematicTextCoroutine()
    {
        SetOpeningCinematicText(0);
        int i = 1;
        while (true)
        {
            bool canChangeText = (i == 1 && GameObject.Find("OpeningCinematic").transform.Find("RawImage").GetComponent<VideoPlayer>().time > 10f) ||
                (i == 2 && GameObject.Find("OpeningCinematic").transform.Find("RawImage").GetComponent<VideoPlayer>().time > 20f) ||
                (i == 3 && GameObject.Find("OpeningCinematic").transform.Find("RawImage").GetComponent<VideoPlayer>().time > 30f) ||
                (i == 4 && GameObject.Find("OpeningCinematic").transform.Find("RawImage").GetComponent<VideoPlayer>().time > 40f) ||
                (i == 5 && GameObject.Find("OpeningCinematic").transform.Find("RawImage").GetComponent<VideoPlayer>().time > 47.5f) ||
                (i == 6 && GameObject.Find("OpeningCinematic").transform.Find("RawImage").GetComponent<VideoPlayer>().time > 55f) ||
                (i == 7 && GameObject.Find("OpeningCinematic").transform.Find("RawImage").GetComponent<VideoPlayer>().time > 61f) ||
                (i == 8 && GameObject.Find("OpeningCinematic").transform.Find("RawImage").GetComponent<VideoPlayer>().time > 71f) ||
                (i == 9 && GameObject.Find("OpeningCinematic").transform.Find("RawImage").GetComponent<VideoPlayer>().time > 81f);
            if (canChangeText)
            {
                SetOpeningCinematicText(i);
                i++;
            }
            yield return null;
        }
    }
    private void SetOpeningCinematicText(int i)
    {
        string languageFilePath = (Localization._instance._ActiveLanguage).ToString();
        string path = Application.streamingAssetsPath + "/Texts/" + languageFilePath + "/Cinematic/Opening.txt";
        StreamReader reader = new StreamReader(path);
        string line = "";
        int c = 0;
        while ((line = reader.ReadLine()) != null)
        {
            if (c == i)
            {
                OpeningCinematic.GetComponentInChildren<TextMeshProUGUI>().text = line;
                break;
            }
            c++;
        }
        reader.Close();
    }
    public void PassOpeningCinematic()
    {
        InputHandler._isAllowedToInput = false;
        GameObject.Find("OpeningCinematic").transform.Find("PassCinematic").gameObject.SetActive(false);
        OpeningCinematic.transform.Find("Loading").gameObject.SetActive(true);
        CallForAction(() => LoadSceneAsync(1), 0.1f); 
    }
    public void OpenEpisodeSelectScreen()
    {
        GameObject.Find("Canvas").transform.Find("MainMenu").gameObject.SetActive(false);
        GameObject.Find("Canvas").transform.Find("JoystickUI").gameObject.SetActive(false);
        GameObject.Find("Canvas").transform.Find("BackToMenuScreen").gameObject.SetActive(true);
        GameObject.Find("Canvas").transform.Find("SelectEpisode").gameObject.SetActive(true);
    }
    public void OpenJoystickScreen()
    {
        GameObject.Find("Canvas").transform.Find("MainMenu").gameObject.SetActive(false);
        GameObject.Find("Canvas").transform.Find("BackToMenuScreen").gameObject.SetActive(true);
        GameObject.Find("Canvas").transform.Find("JoystickUI").gameObject.SetActive(true);
        GameObject.Find("Canvas").transform.Find("SelectEpisode").gameObject.SetActive(false);
    }
    public void CloseEpisodeSelectScreen()
    {
        GameObject.Find("Canvas").transform.Find("MainMenu").gameObject.SetActive(true);
        GameObject.Find("Canvas").transform.Find("BackToMenuScreen").gameObject.SetActive(false);
        GameObject.Find("Canvas").transform.Find("JoystickUI").gameObject.SetActive(false);
        GameObject.Find("Canvas").transform.Find("SelectEpisode").gameObject.SetActive(false);
    }
    public void ContinueGameFromLastRoom()
    {
        int levelReached = PlayerPrefs.GetInt("Level", 0);
        if (levelReached == 0 || levelReached == 1)
        {
            if (_openingCinematicArrangementCoroutine == null)
                _openingCinematicArrangementCoroutine = StartCoroutine(OpeningCinematicArrangement());
        }
        else
        {
            LoadSceneAsync(levelReached);
        }
    }
    public void NextScene()
    {
        LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void LoadSceneAsync(int index)
    {
        if (LoadingObject.activeInHierarchy) return;

        LoadingObject.SetActive(true);
        InputHandler._isAllowedToInput = false;
        CallForAction(() => SceneManager.LoadSceneAsync(index), 0.25f);
    }
    public void LoadScene(int index)
    {
        int levelReached = PlayerPrefs.GetInt("Level", 0);
        if (levelReached < index)
        {
            if (LevelNotReachedCoroutine != null)
                StopCoroutine(LevelNotReachedCoroutine);
            LevelNotReachedCoroutine = StartCoroutine(LevelNotReached());
            return;
        }

        if (index == 1)
        {
            if (_openingCinematicArrangementCoroutine == null)
                _openingCinematicArrangementCoroutine = StartCoroutine(OpeningCinematicArrangement());
        }
        else
        {
            LoadSceneAsync(index);
        }
    }
    private IEnumerator LevelNotReached()
    {
        LevelNotReachedObject.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        LevelNotReachedObject.SetActive(false);
    }
    public void RestartLevel()
    {
        LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
    public void ToMenu()
    {
        Cursor.visible = true;

        if (GameObject.FindGameObjectWithTag("PhaseCounter") != null)
        {
            GameObject.FindGameObjectWithTag("PhaseCounter").transform.position = new Vector3(1f, 0f, 0f);
        }
        if (GameObject.FindGameObjectWithTag("LevelNumber") != null)
        {
            GameObject.FindGameObjectWithTag("LevelNumber").transform.position = new Vector3(0f, 0f, 0f);
        }
        if (SoundManager._instance.CurrentMusicObject != null)
        {
            Destroy(SoundManager._instance.CurrentMusicObject);
        }
        if (SoundManager._instance.CurrentAtmosphereObject != null)
        {
            Destroy(SoundManager._instance.CurrentAtmosphereObject);
        }
        CallForAction(() => LoadSceneAsync(0), 0.25f);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
