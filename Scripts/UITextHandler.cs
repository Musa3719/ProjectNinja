using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITextHandler : MonoBehaviour
{
    public int Number;
    public bool IsMoving = true;
    private TMP_FontAsset _defaultFont;

    private RectTransform _rectTransform;
    private Coroutine _openingMovementCoroutine;
    private TextMeshProUGUI _text;
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _text = GetComponent<TextMeshProUGUI>();
        _defaultFont = _text.font;
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
        switch (Localization._instance._ActiveLanguage)
        {
            case Language.EN:
            case Language.TR:
                _text.font = _defaultFont;
                break;
            case Language.SC:
            case Language.JP:
                _text.font = Localization._instance.CJKFont;
                break;
            default:
                break;
        }

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
        float firstXPos = _rectTransform.anchoredPosition.x;
        _rectTransform.anchoredPosition += -Vector2.right * 75f;
        _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 0f);

        float startTime = Time.realtimeSinceStartup;
        while (startTime + 0.75f > Time.realtimeSinceStartup)
        {
            _rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(_rectTransform.anchoredPosition.x, firstXPos, Time.unscaledDeltaTime * 7f), _rectTransform.anchoredPosition.y);
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, Mathf.Lerp(_text.color.a, 1f, Time.unscaledDeltaTime * 3.25f));
            yield return null;
        }

        _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 1f);
        _rectTransform.anchoredPosition = new Vector2(firstXPos, _rectTransform.anchoredPosition.y);
    }
}
