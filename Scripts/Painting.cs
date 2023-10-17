using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Painting : MonoBehaviour
{
    /*public string _Text;
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
    private void Update()
    {
        if((GameManager._instance.PlayerRb.transform.position-transform.position).magnitude < 1.4f && InputHandler.GetButtonDown("Stamina") && !GameManager._instance.PaintingUI.activeInHierarchy)
            OpenPainting();
    }
    public void OpenPainting()
    {
        GameManager._instance.InGameScreen.SetActive(false);
        GameManager._instance.PaintingUI.SetActive(true);
        GameManager._instance.PaintingUI.GetComponentInChildren<TextMeshProUGUI>().text = _Text;
        GameManager._instance.PaintingUI.GetComponentInChildren<Image>().sprite = _Image;
        GameManager._instance.lookingToPainting = this;

        Time.timeScale = 0f;
        SoundManager._instance.PauseMusic();
        SoundManager._instance.PauseAllSound();
        GameManager._instance.isGameStopped = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void ClosePainting()
    {
        GameManager._instance.InGameScreen.SetActive(true);
        GameManager._instance.PaintingUI.SetActive(false);
        GameManager._instance.lookingToPainting = null;

        Time.timeScale = 1f;
        SoundManager._instance.ContinueMusic();
        SoundManager._instance.ContinueAllSound();
        GameManager._instance.isGameStopped = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void SetText()
    {
        if (Localization._instance.GetComponent<Localization>().Paintings[_Number] != null)
            _Text = Localization._instance.Paintings[_Number];
    }*/
}
