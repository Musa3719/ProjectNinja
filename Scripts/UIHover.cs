using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class UIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI _text;
    private Vector3 _baseScale;
    private Coroutine _textAnimation;

    private void Awake()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
        _baseScale = _text.transform.localScale;
    }
    private void OnEnable()
    {
        _text.transform.localScale = _baseScale;
    }
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        MakeTextBigger();
        GameObject cam = GameManager._instance == null ? Camera.main.gameObject : GameManager._instance.MainCamera;
        SoundManager._instance.PlaySound(SoundManager._instance.ButtonHover, cam.transform.position, 0.15f, false, UnityEngine.Random.Range(0.7f, 0.8f));

    }
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        MakeTextSmaller();
    }
    private void MakeTextBigger()
    {
        if (_textAnimation != null)
            StopCoroutine(_textAnimation);
        _textAnimation = StartCoroutine(TextAnimation(_baseScale * 1.5f));
    }
    private void MakeTextSmaller()
    {
        if (_textAnimation != null)
            StopCoroutine(_textAnimation);
        _textAnimation = StartCoroutine(TextAnimation(_baseScale));
    }

    private IEnumerator TextAnimation(Vector3 targetScale)
    {
        float startTime = Time.realtimeSinceStartup;
        while (startTime + 1f > Time.realtimeSinceStartup)
        {
            _text.transform.localScale = Vector3.Lerp(_text.transform.localScale, targetScale, Time.unscaledDeltaTime * 10f);
            yield return null;
        }
    }
}
