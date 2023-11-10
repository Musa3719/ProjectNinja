using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpeningLogo : MonoBehaviour
{
    private Image image;
    private TextMeshProUGUI text;

    private void Awake()
    {
        image = GetComponent<Image>();
        text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        StartCoroutine(LogoCoroutine());
    }
    private IEnumerator LogoCoroutine()
    {
        Color color;
        Color color2;

        float startTime = Time.time;

        while (Time.time < startTime + 0.2f)
        {
            if (InputHandler.GetButton("Esc")) { Destroy(gameObject); }

            yield return null;
        }

        startTime = Time.time;
        while (Time.time < startTime + 1.5f)
        {
            if (InputHandler.GetButton("Esc")) { Destroy(gameObject); }

            color = image.color;
            image.color = new Color(color.r, color.g, color.b, color.a - Time.deltaTime * 1.5f);

            color2 = text.color;
            text.color = new Color(color2.r, color2.g, color2.b, color2.a - Time.deltaTime * 1.5f);

            yield return null;
        }

        Destroy(gameObject);
    }
}
