using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            int levelReached = PlayerPrefs.GetInt("Level", 0);
            int i = 1;
            foreach (Transform item in GameObject.Find("Canvas").transform.Find("SelectEpisode").transform)
            {
                if (levelReached >= i)
                {
                    item.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                }
                else
                {
                    item.GetComponent<UnityEngine.UI.Image>().color = Color.gray;
                }
                i++;
            }
        }
        else
        {
            if (PlayerPrefs.GetInt("Level", 0) < SceneManager.GetActiveScene().buildIndex)
                PlayerPrefs.SetInt("Level", SceneManager.GetActiveScene().buildIndex);
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
    public void NextScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void LoadScene(int index)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(index);
    }
    public void ToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
