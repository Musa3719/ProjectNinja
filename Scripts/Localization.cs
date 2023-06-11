using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Language
{
    EN,
    TR
}
public class Localization : MonoBehaviour
{
    public static Localization _instance;
    public Language _ActiveLanguage { get; private set; }

    public List<string> Newspapers;
    public List<string> Paintings;
    public List<string> Dialogues;
    public List<string> UI;
    public List<string> Tutorial;

    public static event Action _LanguageChangedEvent;

    private void Awake()
    {
        _instance = this;
        _ActiveLanguage = (Language)PlayerPrefs.GetInt("Language", 0);
        Newspapers = new List<string>();
        Paintings = new List<string>();
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
        ArrangeList("/Paintings/", Paintings);
        ArrangeList("/Dialogues/", Dialogues);
        ArrangeList("/Tutorial/", Tutorial);
        ArrangeUI();

        _LanguageChangedEvent?.Invoke();
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
