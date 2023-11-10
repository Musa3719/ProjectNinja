using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public enum Language
{
    EN,
    TR,
    SC,
    JP
}
public class Localization : MonoBehaviour
{
    public static Localization _instance;

    public TMP_FontAsset CJKFont;
    public TMP_FontAsset TutorialFont;

    public Language _ActiveLanguage { get; private set; }

    public List<string> Newspapers;
    public List<string> Dialogues;
    public List<string> UI;
    public List<string> Tutorial;

    public static event Action _LanguageChangedEvent;

    private void Awake()
    {
        _instance = this;
        _ActiveLanguage = (Language)PlayerPrefs.GetInt("Language", 0);
        Newspapers = new List<string>();
        Dialogues = new List<string>();
        UI = new List<string>();
        Tutorial = new List<string>();
        SetLanguage(_ActiveLanguage);
    }

    public void SetLanguage(Language language)
    {
        _ActiveLanguage = language;
        PlayerPrefs.SetInt("Language", (int)language);

        LocalizeTexts();
    }
    public void SetLanguage(int number)
    {
        _ActiveLanguage = (Language)number;
        PlayerPrefs.SetInt("Language", number);

        LocalizeTexts();
    }

    private void LocalizeTexts()
    {
        ArrangeList("/Newspapers/", Newspapers);
        ArrangeList("/Dialogues/", Dialogues);
        ArrangeList("/Tutorial/", Tutorial);
        ArrangeUI();

        _LanguageChangedEvent?.Invoke();

        if (GameManager._instance != null)
            ArrangeTutorialFonts();

        if (SceneController._instance.SceneBuildIndex == 0)
        {
            if (_ActiveLanguage == Language.SC || _ActiveLanguage == Language.JP)
                Options._instance.JoystickUI.transform.Find("ControllerImage").Find("PressedButton").GetComponent<TextMeshProUGUI>().font = CJKFont;
            else
                Options._instance.JoystickUI.transform.Find("ControllerImage").Find("PressedButton").GetComponent<TextMeshProUGUI>().font = Options._instance.QualityUI.GetComponentInChildren<TextMeshProUGUI>().font;

            Options._instance.JoystickUI.transform.Find("ControllerImage").Find("PressedButton").GetComponent<TextMeshProUGUI>().text = "";
        }
    }
    private void ArrangeTutorialFonts()
    {
        switch (Localization._instance._ActiveLanguage)
        {
            case Language.EN:
            case Language.TR:
                GameManager._instance.TutorialTextUI.transform.Find("Text").GetComponent<TextMeshProUGUI>().font = TutorialFont;
                GameManager._instance.TutorialTextUI.transform.Find("Text (1)").GetComponent<TextMeshProUGUI>().font = TutorialFont;
                break;
            case Language.SC:
            case Language.JP:
                GameManager._instance.TutorialTextUI.transform.Find("Text").GetComponent<TextMeshProUGUI>().font = CJKFont;
                GameManager._instance.TutorialTextUI.transform.Find("Text (1)").GetComponent<TextMeshProUGUI>().font = CJKFont;
                break;
            default:
                break;
        }
    }
    private void ArrangeList(string lastDirectory, List<string> list)
    {
        list.Clear();
        string languageFilePath = (_ActiveLanguage).ToString();
        string path = Application.streamingAssetsPath + "/Texts/" + languageFilePath + lastDirectory;

        DirectoryInfo dir = new DirectoryInfo(path);
        FileInfo[] info = dir.GetFiles("*.*");
        foreach (FileInfo f in info)
        {
            if (f.Name.Length >= 5 && f.Name.EndsWith(".meta"))
                continue;
            string textString = "";
            using (StreamReader sr = f.OpenText())
            {
                var s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    if (s != "")
                        textString += "\n";
                    textString += s;
                }
            }
            list.Add(textString);
        }
    }
    private void ArrangeUI()
    {
        UI.Clear();
        string fileName = "";
        if (SceneManager.GetActiveScene().buildIndex == 0 || SceneManager.GetActiveScene().name=="DemoEnded" || SceneManager.GetActiveScene().name == "GameEnded")
            fileName = "Menu.txt";
        else
            fileName = "Game.txt";
        string languageFilePath = (_ActiveLanguage).ToString();
        string path = Application.streamingAssetsPath + "/Texts/" + languageFilePath + "/UI/" + fileName;

        StreamReader reader = new StreamReader(path);
        string line = "";
        while ((line = reader.ReadLine()) != null)
        {
            UI.Add(line);
        }
        reader.Close();
    }
}
