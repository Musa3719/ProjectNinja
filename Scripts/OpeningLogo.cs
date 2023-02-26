using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpeningLogo : MonoBehaviour
{
    private Image image;

    [SerializeField]
    private GameObject back;
    private void Awake()
    {
        image = GetComponent<Image>();
        StartCoroutine(LogoCoroutine());
    }
    private IEnumerator LogoCoroutine()
    {
        Color color;
        float startTime = Time.time;
        while (Time.time < startTime + 1f)
        {
            if (InputHandler.GetButtonDown("Esc")) { Destroy(gameObject); Destroy(back); }

            color = image.color;
            image.color = new Color(color.r, color.g, color.b, color.a + Time.deltaTime * 3f);
            yield return null;
        }

        Destroy(back);

        color = image.color;
        image.color = new Color(color.r, color.g, color.b, 1f);

        startTime = Time.time;
        while (Time.time < startTime + 1.25f)
        {
            if (InputHandler.GetButtonDown("Esc")) Destroy(gameObject);

            color = image.color;
            image.color = new Color(color.r, color.g, color.b, color.a - Time.deltaTime * 1f);
            yield return null;
        }
        Destroy(gameObject);
    }
}
