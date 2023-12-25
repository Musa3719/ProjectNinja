using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitingScreen : MonoBehaviour
{
    private Image image;
    private bool _activated;

    void Update()
    {
        if (!_activated)
        {
            CloseWaitingScreen();
            _activated = true;
        }
    }
    private void CloseWaitingScreen()
    {
        StartCoroutine(CloseWaitingScreenCoroutine());
    }
    private IEnumerator CloseWaitingScreenCoroutine()
    {
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < startTime + 2f)
        {
            if (Time.deltaTime < 1f / 30f)
            {
                yield return new WaitForSecondsRealtime(0.5f);
                break;
            }
            yield return null;
        }
        //InputHandler._isAllowedToInput = true;
        image = GetComponent<Image>();
        startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < startTime + 0.75f)
        {
            Color color = image.color;
            image.color = new Color(color.r, color.g, color.b, color.a - Time.deltaTime * 2f);
            yield return null;
        }
        Destroy(gameObject);
    }
}
