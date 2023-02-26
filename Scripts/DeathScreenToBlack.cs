using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenToBlack : MonoBehaviour
{
    private Image image;
    private float lerpSpeed;
    private void Awake()
    {
        lerpSpeed = 2f;
        image = GetComponent<Image>();
    }
    private void Update()
    {
        image.color = Color.Lerp(image.color, new Color(0f, 0f, 0f), Time.unscaledDeltaTime * lerpSpeed);
    }
}
