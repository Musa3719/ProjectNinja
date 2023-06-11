using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    private Light[] lights;
    private void Awake()
    {
        lights = GetComponentsInChildren<Light>();
        StartCoroutine(FlickerCoroutine());
    }
    private IEnumerator FlickerCoroutine()
    {
        while (true)
        {
            if (lights[0].enabled)
            {
                if (Random.Range(0f, 1000f) > 999f)
                {
                    foreach (var light in lights)
                    {
                        light.enabled = false;
                    }
                    SoundManager._instance.PlaySound(SoundManager._instance.LightFlicker, transform.position, 0.2f, false, Random.Range(0.6f, 0.8f));
                    GameManager._instance.CallForAction(OpenLight, Random.Range(0.75f, 2.5f));
                }
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    private void OpenLight()
    {
        foreach (var light in lights)
        {
            light.enabled = true;
        }
        SoundManager._instance.PlaySound(SoundManager._instance.LightFlicker, transform.position, 0.2f, false, Random.Range(0.6f, 0.8f));
    }
}
