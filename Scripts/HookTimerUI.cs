using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HookTimerUI : MonoBehaviour
{
    private Coroutine _timerCoroutine;
    private Image _image;
    private void Awake()
    {
        _image = transform.Find("EmptyIcon").GetComponent<Image>();
    }

    public void StartTimer(float waitTime)
    {
        if (_timerCoroutine != null)
            GameManager._instance.StopCoroutine(_timerCoroutine);
        _timerCoroutine = GameManager._instance.StartCoroutine(TimerCoroutine(waitTime));
    }
    private IEnumerator TimerCoroutine(float waitTime)
    {
        _image.enabled = true;
        float startTime = Time.time;
        while (Time.time < startTime + waitTime)
        {
            _image.fillAmount = 1 - (Time.time - startTime) / waitTime;
            yield return null;
        }
        _image.enabled = false;
    }
}
