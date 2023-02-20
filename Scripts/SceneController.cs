using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    [SerializeField]
    private GameObject LevelNotReachedObject;

    private Coroutine LevelNotReachedCoroutine;

    public static SceneController _instance;

    private void Awake()
    {
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

            if (levelReached >= 30)
            {
                GameObject.Find("Canvas").transform.Find("BossRun").gameObject.GetComponent<Button>().interactable = true;
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
    public void OpenEpisodeSelectScreen()
    {
        GameObject.Find("Canvas").transform.Find("MainMenu").gameObject.SetActive(false);
        GameObject.Find("Canvas").transform.Find("BackToMenuScreen").gameObject.SetActive(true);
        GameObject.Find("Canvas").transform.Find("BossRun").gameObject.SetActive(true);
        GameObject.Find("Canvas").transform.Find("SelectEpisode").gameObject.SetActive(true);
    }
    public void CloseEpisodeSelectScreen()
    {
        GameObject.Find("Canvas").transform.Find("MainMenu").gameObject.SetActive(true);
        GameObject.Find("Canvas").transform.Find("BackToMenuScreen").gameObject.SetActive(false);
        GameObject.Find("Canvas").transform.Find("BossRun").gameObject.SetActive(false);
        GameObject.Find("Canvas").transform.Find("SelectEpisode").gameObject.SetActive(false);
    }
    public void ContinueGameFromLastRoom()
    {
        int levelReached = PlayerPrefs.GetInt("Level", 1);
        SceneManager.LoadScene(levelReached);
    }
    public void NextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void LoadScene(int index)
    {
        if (PlayerPrefs.GetInt("Level", 0) < index)
        {
            if (LevelNotReachedCoroutine != null)
                StopCoroutine(LevelNotReachedCoroutine);
            LevelNotReachedCoroutine = StartCoroutine(LevelNotReached());
            return;
        }

        SceneManager.LoadScene(index);
    }
    private IEnumerator LevelNotReached()
    {
        LevelNotReachedObject.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        LevelNotReachedObject.SetActive(false);
    }
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

        SceneManager.LoadScene(0);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
