using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Painting : MonoBehaviour
{
    public string _Text;
    public int _Number;
    public Sprite _Image;

    private void OnEnable()
    {
        Localization._LanguageChangedEvent += SetText;
        SetText();
    }
    private void OnDisable()
    {
        Localization._LanguageChangedEvent -= SetText;
    }
    private void OnMouseDown()
    {
        OpenPainting();
    }
    public void OpenPainting()
    {
        GameManager._instance.InGameScreen.SetActive(false);
        GameManager._instance.PaintingUI.SetActive(true);
        GameManager._instance.PaintingUI.GetComponentInChildren<TextMeshProUGUI>().text = _Text;
        GameManager._instance.PaintingUI.GetComponentInChildren<Image>().sprite = _Image;
        GameManager._instance.lookingToPainting = this;
    }
    public void ClosePainting()
    {
        GameManager._instance.InGameScreen.SetActive(true);
        GameManager._instance.PaintingUI.SetActive(false);
        GameManager._instance.lookingToPainting = null;
    }
    public void SetText()
    {
        if (Localization._instance.GetComponent<Localization>().Paintings[_Number] != null)
            _Text = Localization._instance.Paintings[_Number];
    }
}
