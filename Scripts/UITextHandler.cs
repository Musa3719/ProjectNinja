using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITextHandler : MonoBehaviour
{
    public int _Number;

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
        if (Localization._instance.UI[_Number] != null)
            GetComponent<TextMeshProUGUI>().text = Localization._instance.UI[_Number];
    }
}
