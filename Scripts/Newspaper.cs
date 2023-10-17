using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Newspaper : MonoBehaviour
{
    private string _text;
    private int _number;

    private void Awake()
    {
        _number = SceneController._instance.SceneBuildIndex - 1;
    }
    public void OpenNewspaper()
    {
        GameManager._instance.NewspaperUI.SetActive(true);
        GameManager._instance.NewspaperUI.transform.Find("NewspaperImage").GetComponentInChildren<TextMeshProUGUI>().text = _text;
        GameManager._instance.InGameScreen.SetActive(false);
    }
    private void OnEnable()
    {
        Localization._LanguageChangedEvent += SetText;
        SetText();
    }
    private void OnDisable()
    {
        Localization._LanguageChangedEvent -= SetText;
    }
    public void SetText()
    {
        if (Localization._instance.Newspapers[_number] != null)
            _text = Localization._instance.Newspapers[_number];
    }
}
