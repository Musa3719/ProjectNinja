using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffLight : MonoBehaviour
{
    [SerializeField]
    private float LerpSpeed;
    private Light lightComponent;
    private void Awake()
    {
        lightComponent = GetComponent<Light>();
    }
    private void Update()
    {
        lightComponent.intensity = Mathf.Lerp(lightComponent.intensity, 0f, Time.deltaTime * LerpSpeed);
    }
}
