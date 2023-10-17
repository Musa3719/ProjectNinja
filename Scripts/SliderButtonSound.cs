using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SliderButtonSound : MonoBehaviour, IPointerDownHandler
{
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        GameObject cam = GameManager._instance == null ? Camera.main.gameObject : GameManager._instance.MainCamera;
        SoundManager._instance.PlaySound(SoundManager._instance.Button, cam.transform.position, 0.15f, false, UnityEngine.Random.Range(0.7f, 0.8f));
    }

}
