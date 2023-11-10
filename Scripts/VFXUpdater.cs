using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXUpdater : MonoBehaviour
{
    [SerializeField] private bool _isTurning;
    [SerializeField] private float _speed;
    void Update()
    {
        if (_isTurning)
            transform.localEulerAngles += Vector3.forward * Time.deltaTime * _speed;
        else
            transform.forward = -((GameManager._instance == null ? Camera.main.transform.position : GameManager._instance.MainCamera.transform.position) - transform.position).normalized;
    }
}
