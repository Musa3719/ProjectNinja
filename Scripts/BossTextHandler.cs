using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossTextHandler : MonoBehaviour
{
    private Coroutine _textColorCoroutine;
    private Coroutine _textOpenCoroutine;
    private GameObject _bossTalkSound;
    private void Awake()
    {
        GameObject.FindGameObjectWithTag("Boss").GetComponent<MonoBehaviour>().StartCoroutine(Phase1Talk());
    }
    private void Update()
    {
        if (_bossTalkSound != null)
            _bossTalkSound.transform.position = GameManager._instance.MainCamera.transform.position;
    }
    private IEnumerator Phase1Talk()
    {
        yield return new WaitForSeconds(5.5f);
        if (NewDialogue(0))
            yield return new WaitForSeconds(12f);
        if (NewDialogue(1))
            yield return new WaitForSeconds(12f);
        if (NewDialogue(2))
            yield return new WaitForSeconds(12f);
        if (NewDialogue(3))
            yield return new WaitForSeconds(12f);
        if (NewDialogue(4))
            yield return new WaitForSeconds(12f);
        if (NewDialogue(5))
            yield return new WaitForSeconds(12f);
        if (NewDialogue(6))
            yield return new WaitForSeconds(18f);
        if (NewDialogue(7))
            yield return new WaitForSeconds(18f);
        if (NewDialogue(8))
            yield return new WaitForSeconds(18f);
        if (NewDialogue(9))
            yield return new WaitForSeconds(18f);
        CloseUI();
    }

    public bool NewDialogue(int number)
    {
        if (PlayerPrefs.GetInt("Dialogue", 0) > number || PlayerPrefs.GetInt("Dialogue", 0) == 9) return false;
        else
            PlayerPrefs.SetInt("Dialogue", number);
        string text = Localization._instance.Dialogues[number];
        if (_textColorCoroutine != null)
            StopCoroutine(_textColorCoroutine);
        _textColorCoroutine = StartCoroutine(TextCoroutine(1f, GameManager._instance.BossTextUI.transform.Find("Text").GetComponent<TextMeshProUGUI>()));
        if (_textOpenCoroutine != null)
            StopCoroutine(_textOpenCoroutine);
        _textOpenCoroutine = StartCoroutine(TriggerText(text, number));

        return true;

    }
    public IEnumerator TriggerText(string text, int number)
    {
        GameManager._instance.BossTextUI.SetActive(true);
        TextMeshProUGUI childText = GameManager._instance.BossTextUI.transform.Find("Text").GetComponent<TextMeshProUGUI>();

        if (_bossTalkSound != null)
            Destroy(_bossTalkSound);
        _bossTalkSound = SoundManager._instance.PlaySound(SoundManager._instance.BossDialogues[number], Camera.main.transform.position, 0.8f, false);

        string newText = "";
        int newLineCounter = 1;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '.')
                yield return new WaitForSeconds(0.2f);
            else if (text[i] == ',' || text[i] == ':' || text[i] == ';')
                yield return new WaitForSeconds(0.1f);

            if (i > 75 * newLineCounter && text[i] == ' ')
            {
                newLineCounter++;
                newText += "\n";
            }
            else
            {
                //SoundManager._instance.PlaySound(SoundManager._instance.TextSound, Camera.main.transform.position, 0.15f, false, UnityEngine.Random.Range(0.7f, 0.9f));
                newText += text[i];
            }

            childText.text = newText;
            yield return new WaitForSeconds(0.04f);
        }


        var color = childText.color;
        childText.color = new Color(color.r, color.g, color.b, 0f);
        color = GameManager._instance.BossTextUI.GetComponentInChildren<Image>().color;
        GameManager._instance.BossTextUI.GetComponentInChildren<Image>().color = new Color(color.r, color.g, color.b, 0f);

        if (_textColorCoroutine != null)
            StopCoroutine(_textColorCoroutine);
        _textColorCoroutine = StartCoroutine(TextCoroutine(1f, childText));
    }
    public void CloseUI()
    {
        if (_textColorCoroutine != null)
            StopCoroutine(_textColorCoroutine);
        _textColorCoroutine = StartCoroutine(TextCoroutine(0f, GameManager._instance.BossTextUI.transform.Find("Text").GetComponent<TextMeshProUGUI>()));
    }
    private IEnumerator TextCoroutine(float targetTransparency, TextMeshProUGUI childText)
    {
        float startTime = Time.time;
        while (startTime + 1f > Time.time)
        {
            var color = childText.color;
            childText.color = Color.Lerp(color, new Color(color.r, color.g, color.b, targetTransparency), Time.deltaTime * 5f);
            color = GameManager._instance.BossTextUI.GetComponentInChildren<Image>().color;
            GameManager._instance.BossTextUI.GetComponentInChildren<Image>().color = Color.Lerp(color, new Color(color.r, color.g, color.b, targetTransparency * 0.85f), Time.deltaTime * 5f);
            yield return null;
        }

        if (targetTransparency == 0f) GameManager._instance.BossTextUI.SetActive(false);
    }
    public void StopTalking()
    {
        if (_bossTalkSound != null) Destroy(_bossTalkSound);
        GameManager._instance.BossTextUI.SetActive(false);

        StopAllCoroutines();
    }
}
