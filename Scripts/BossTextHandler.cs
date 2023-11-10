using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossTextHandler : MonoBehaviour
{
    private Coroutine _textColorCoroutine;
    private Coroutine _textOpenCoroutine;
    private void Awake()
    {
        NewDialogue(0);
    }
    public void NewDialogue(int number)
    {
        string text = Localization._instance.Dialogues[number];
        if (_textColorCoroutine != null)
            StopCoroutine(_textColorCoroutine);
        _textColorCoroutine = StartCoroutine(TextCoroutine(1f, GameManager._instance.BossTextUI.transform.Find("Text").GetComponent<TextMeshProUGUI>()));
        if (_textOpenCoroutine != null)
            StopCoroutine(_textOpenCoroutine);
        _textOpenCoroutine = StartCoroutine(TriggerText(text));
        
    }
    public IEnumerator TriggerText(string text)
    {
        GameManager._instance.BossTextUI.SetActive(true);
        TextMeshProUGUI childText = GameManager._instance.BossTextUI.transform.Find("Text").GetComponent<TextMeshProUGUI>();

        string newText = "";
        int newLineCounter = 1;
        for (int i = 0; i < text.Length; i++)
        {
            if (i > 75 * newLineCounter && text[i] == ' ')
            {
                newLineCounter++;
                newText += "\n";
            }
            else
            {
                (SoundManager._instance.PlaySound(SoundManager._instance.TextSound, Camera.main.transform.position, 0.15f, false, UnityEngine.Random.Range(0.7f, 0.9f))).transform.parent = Camera.main.transform;
                newText += text[i];
            }

            childText.text = newText;
            yield return new WaitForSeconds(0.055f);
        }


        var color = childText.color;
        childText.color = new Color(color.r, color.g, color.b, 0f);
        color = GameManager._instance.BossTextUI.GetComponentInChildren<Image>().color;
        GameManager._instance.BossTextUI.GetComponentInChildren<Image>().color = new Color(color.r, color.g, color.b, 0f);

        if (_textColorCoroutine != null)
            StopCoroutine(_textColorCoroutine);
        _textColorCoroutine = StartCoroutine(TextCoroutine(1f, childText));

        GameManager._instance.CallForAction(() => CloseTutorialUI(), 10f);
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
    public void CloseTutorialUI()
    {
        if (_textColorCoroutine != null)
            StopCoroutine(_textColorCoroutine);
        _textColorCoroutine = StartCoroutine(TextCoroutine(0f, GameManager._instance.BossTextUI.transform.Find("Text").GetComponent<TextMeshProUGUI>()));
    }
}
