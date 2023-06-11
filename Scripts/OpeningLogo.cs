using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpeningLogo : MonoBehaviour
{
    private Image image;
    private TextMeshProUGUI text1;
    private TextMeshProUGUI text2;

    [SerializeField]
    private GameObject back;
    private void Awake()
    {
        image = GetComponent<Image>();
        text1 = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        text2 = transform.Find("Text (1)").GetComponent<TextMeshProUGUI>();
        StartCoroutine(LogoCoroutine());
    }
    private IEnumerator LogoCoroutine()
    {
        Color color;
        Color color2;
        Color color3;
        Color color4;

        var backImage = back.GetComponent<Image>();

        yield return new WaitForSeconds(0.5f);

        float startTime = Time.time;

        while (Time.time < startTime + 1f)
        {
            if (InputHandler.GetButtonDown("Esc")) { Destroy(gameObject); Destroy(back); }

            color = image.color;
            image.color = new Color(color.r, color.g, color.b, color.a + Time.deltaTime * 3f);

            color2 = text1.color;
            text1.color = new Color(color2.r, color2.g, color2.b, color2.a + Time.deltaTime * 3f);

            color3 = text2.color;
            text2.color = new Color(color3.r, color3.g, color3.b, color3.a + Time.deltaTime * 3f);

            color4 = backImage.color;
            backImage.color = new Color(color4.r, color4.g, color4.b, color4.a - Time.deltaTime * 3f);

            yield return null;
        }

        Destroy(back);

        color = image.color;
        image.color = new Color(color.r, color.g, color.b, 1f);


        while (Time.time < startTime + 2f)
        {
            if (InputHandler.GetButtonDown("Esc")) { Destroy(gameObject); }

            color = image.color;
            image.color = new Color(color.r, color.g, color.b, color.a - Time.deltaTime * 3f);

            color2 = text1.color;
            text1.color = new Color(color2.r, color2.g, color2.b, color2.a - Time.deltaTime * 3f);

            color3 = text2.color;
            text2.color = new Color(color3.r, color3.g, color3.b, color3.a - Time.deltaTime * 3f);

            yield return null;
        }
        Destroy(gameObject);
    }
}
