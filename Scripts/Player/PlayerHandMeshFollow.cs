using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandMeshFollow : MonoBehaviour
{
    public Vector3 _positionOffset;

    [SerializeField]
    private Transform _camTransform;

    private Vector3 _lastCamAngle;

    private Vector3 _scale;
    private void Awake()
    {
        _scale = new Vector3(1.5f, 1.5f, 1.9f);
    }
    private void LateUpdate()
    {
        Vector3 targetRot = new Vector3(0f + (_camTransform.transform.eulerAngles.x < 180 ? _camTransform.transform.eulerAngles.x / 2f : (_camTransform.transform.eulerAngles.x - 360) / 1.1f), _camTransform.transform.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Euler(targetRot);

        Vector3 positionOffsetForward = _positionOffset.y * _camTransform.up + _positionOffset.z * _camTransform.forward;
        Vector3 targetPos = _camTransform.transform.position + positionOffsetForward;
        transform.position = targetPos;

        _lastCamAngle = _camTransform.transform.eulerAngles;
        transform.localScale = _scale + _scale* (Options._instance.FOV - 80f) / 70f;
        _positionOffset = new Vector3(0f, -0.35f - (Options._instance.FOV - 90f) / 360f, 0.6f + (Options._instance.FOV - 85f) / 250f);
    }
}
