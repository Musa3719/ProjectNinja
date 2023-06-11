using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedCounterTimer : MonoBehaviour
{
    public float _timer;

    void Update()
    {
        _timer = Mathf.Clamp(_timer - Time.deltaTime, 0f, GameManager._instance.RunSpeedAdditionActiveTime);
        GetComponent<SlicedFilledImage>().fillAmount = _timer / GameManager._instance.RunSpeedAdditionActiveTime;
    }
}
