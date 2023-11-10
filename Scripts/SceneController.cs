using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [SerializeField]
    private Sprite CinematicImage1;
    [SerializeField]
    private Sprite CinematicImage2;
    [SerializeField]
    private Sprite CinematicImage3;
    [SerializeField]
    private Sprite CinematicImage4;

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
        SceneBuildIndex = 1;

        _lastMenuCameraNumber = -1;

        if (SceneBuildIndex == 0)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        _instance = this;
        Time.timeScale = 1f;
        if (SceneBuildIndex == 0)
        {
            ArrangeMenuLevels();
        }
        else
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

        if (_cinematicImageChangeCoroutine != null)
            StopCoroutine(_cinematicImageChangeCoroutine);
        _cinematicImageChangeCoroutine = StartCoroutine(CinematicImageChangeCoroutine(CinematicImage1, "Cutter"));

        yield return new WaitForSeconds(4f);

        if (_cinematicImageChangeCoroutine != null)
            StopCoroutine(_cinematicImageChangeCoroutine);
        _cinematicImageChangeCoroutine = StartCoroutine(CinematicImageChangeCoroutine(CinematicImage2, "Samurai"));

        yield return new WaitForSeconds(3f);

        if (_cinematicImageChangeCoroutine != null)
            StopCoroutine(_cinematicImageChangeCoroutine);
        _cinematicImageChangeCoroutine = StartCoroutine(CinematicImageChangeCoroutine(CinematicImage3, "Grave Digger"));

        yield return new WaitForSeconds(3f);

        if (_cinematicImageChangeCoroutine != null)
            StopCoroutine(_cinematicImageChangeCoroutine);
        _cinematicImageChangeCoroutine = StartCoroutine(CinematicImageChangeCoroutine(CinematicImage4, "Traitor"));

        yield return new WaitForSeconds(3f);

        OpeningCinematic.transform.Find("Loading").gameObject.SetActive(true);

        yield return null;

        LoadSceneAsync(1);
    }
    private IEnumerator CinematicImageChangeCoroutine(Sprite newImage, string text)
    {
        SoundManager._instance.PlaySound(SoundManager._instance.TutorialText, Camera.main.transform.position, 0.25f, false, UnityEngine.Random.Range(0.55f, 0.65f));

        OpeningCinematic.GetComponentInChildren<TextMeshProUGUI>().text = text;

        var cinematicObjects = OpeningCinematic.GetComponentsInChildren<Image>();

        OpeningCinematic.transform.Find("Image").GetComponent<RectTransform>().anchoredPosition = new Vector2(200f, OpeningCinematic.transform.Find("Image").GetComponent<RectTransform>().anchoredPosition.y);

        foreach (var item in cinematicObjects)
        {
            item.sprite = newImage;
            item.color = new Color(item.color.r, item.color.g, item.color.b, 0f);
        }
        float startTime = Time.time;
        while (Time.time < startTime + 1f)
        {
            foreach (var item in cinematicObjects)
            {
                item.color = new Color(item.color.r, item.color.g, item.color.b, item.color.a + Time.deltaTime * 2f);
                if (item.name == "Image")
                    item.GetComponent<RectTransform>().anchoredPosition = new Vector2(item.GetComponent<RectTransform>().anchoredPosition.x - Time.deltaTime * 50f, item.GetComponent<RectTransform>().anchoredPosition.y);
            }
            yield return null;
        }

        foreach (var item in cinematicObjects)
        {
            item.color = new Color(item.color.r, item.color.g, item.color.b, 1f);
        }

        while (true)
        {
            foreach (var item in cinematicObjects)
            {
                if (item.name == "Image")
                    item.GetComponent<RectTransform>().anchoredPosition = new Vector2(item.GetComponent<RectTransform>().anchoredPosition.x - Time.deltaTime * 50f, item.GetComponent<RectTransform>().anchoredPosition.y);
            }
            yield return null;
        }
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
