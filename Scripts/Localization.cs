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
    TR
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
    }
    private void Start()
    {
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
        ArrangeList("/Newspapers/", Newspapers, 24);
        ArrangeList("/Dialogues/", Dialogues, 10);
        ArrangeListOld("/Tutorial/", Tutorial);
        ArrangeUI();

        _LanguageChangedEvent?.Invoke();
    }

    private void ArrangeList(string lastDirectory, List<string> list, int lenght)
    {
        list.Clear();
        string languageFilePath = (_ActiveLanguage).ToString();
        string path = Application.streamingAssetsPath + "/Texts/" + languageFilePath + lastDirectory;

        for (int i = 0; i < lenght; i++)
        {
            string path2 = Application.streamingAssetsPath + "/Texts/" + languageFilePath + lastDirectory + "/" + (i + 1).ToString() + ".txt";
            StreamReader reader = new StreamReader(path2);
            string textString = "";
            var line = "";
            while ((line = reader.ReadLine()) != null)
            {
                if (line != "")
                    textString += "\n";
                textString += line;
            }
            list.Add(textString);
            reader.Close();
        }
    }
    private void ArrangeListOld(string lastDirectory, List<string> list)
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
        if (SceneManager.GetActiveScene().buildIndex == 0 || SceneManager.GetActiveScene().buildIndex == SceneManager.sceneCountInBuildSettings - 1)
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
