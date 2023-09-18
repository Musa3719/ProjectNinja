using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
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

    public static SceneController _instance;
    public static AsyncOperation NextSceneAsyncOperation;
    private void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex == 7 || SceneManager.GetActiveScene().buildIndex == 0)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        _instance = this;
        Time.timeScale = 1f;
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            int levelReached = PlayerPrefs.GetInt("Level", 0);
            int i = 1;
            foreach (Transform level in GameObject.Find("Canvas").transform.Find("SelectEpisode").transform)
            {
                foreach (Transform room in level)
                {
                    if (levelReached >= i)
                    {
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
        else
        {
            if (PlayerPrefs.GetInt("Level", 0) < SceneManager.GetActiveScene().buildIndex)
                PlayerPrefs.SetInt("Level", SceneManager.GetActiveScene().buildIndex);
        }
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
    public void PlayButtonSound()
    {
        GameObject cam = GameManager._instance == null ? Camera.main.gameObject : GameManager._instance.MainCamera;
        SoundManager._instance.PlaySound(SoundManager._instance.Button, cam.transform.position, 0.1f, false, UnityEngine.Random.Range(0.65f, 0.75f));
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
        GameObject.Find("Canvas").transform.Find("BackToMenuScreen").gameObject.SetActive(true);
        GameObject.Find("Canvas").transform.Find("SelectEpisode").gameObject.SetActive(true);
    }
    public void CloseEpisodeSelectScreen()
    {
        GameObject.Find("Canvas").transform.Find("MainMenu").gameObject.SetActive(true);
        GameObject.Find("Canvas").transform.Find("BackToMenuScreen").gameObject.SetActive(false);
        GameObject.Find("Canvas").transform.Find("SelectEpisode").gameObject.SetActive(false);
    }
    public void ContinueGameFromLastRoom()
    {
        int levelReached = PlayerPrefs.GetInt("Level", 0);
        if (levelReached == 0)
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

        if (levelReached == 0)
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
