using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITextHandler : MonoBehaviour
{
    public int Number;
    public bool IsMoving = true;

    private RectTransform _rectTransform;
    private Coroutine _openingMovementCoroutine;
    private TextMeshProUGUI _text;
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        Localization._LanguageChangedEvent += SetText;
        SetText();
        if (IsMoving)
            OpeningMovement();
    }
    private void OnDisable()
    {
        Localization._LanguageChangedEvent -= SetText;
    }
    public void SetText()
    {
        if (Localization._instance.UI[Number] != null)
            GetComponent<TextMeshProUGUI>().text = Localization._instance.UI[Number];
    }
    private void OpeningMovement()
    {
        if (_openingMovementCoroutine != null)
            StopCoroutine(_openingMovementCoroutine);
        _openingMovementCoroutine = StartCoroutine(OpeningMovementCoroutine());
    }
    private IEnumerator OpeningMovementCoroutine()
    {
        if (Time.realtimeSinceStartup < 1f)
            yield return null;
        float firstXPos = _rectTransform.localPosition.x;
        _rectTransform.localPosition += -Vector3.right * 75f;
        _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 0f);

        float startTime = Time.realtimeSinceStartup;
        while (startTime + 0.75f > Time.realtimeSinceStartup)
        {
            _rectTransform.localPosition = new Vector3(Mathf.Lerp(_rectTransform.localPosition.x, firstXPos, Time.unscaledDeltaTime * 7f), _rectTransform.localPosition.y, _rectTransform.localPosition.z);
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, Mathf.Lerp(_text.color.a, 1f, Time.unscaledDeltaTime * 3.25f));
            yield return null;
        }

        _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 1f);
        _rectTransform.localPosition = new Vector3(firstXPos, _rectTransform.localPosition.y, _rectTransform.localPosition.z);
    }
}
