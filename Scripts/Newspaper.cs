using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Newspaper : MonoBehaviour
{
    public string _Text;
    public int _Number;

    public void OpenNewspaper()
    {
        GameManager._instance.NewspaperUI.SetActive(true);
        GameManager._instance.NewspaperUI.GetComponentInChildren<TextMeshProUGUI>().text = _Text;
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
        if (Localization._instance.Newspapers[_Number] != null)
            _Text = Localization._instance.Newspapers[_Number];
    }
}
