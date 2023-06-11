using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandMeshFollow : MonoBehaviour
{
    public Vector3 _positionOffset;

    [SerializeField]
    private Transform _camTransform;

    private Vector3 _lastCamAngle;

    private void LateUpdate()
    {
        Vector3 targetRot = new Vector3(_camTransform.transform.eulerAngles.x < 180 ? _camTransform.transform.eulerAngles.x / 1.6f : (_camTransform.transform.eulerAngles.x - 360) / 1.1f, _camTransform.transform.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Euler(targetRot);

        Vector3 positionOffsetForward = _positionOffset.y * _camTransform.up + _positionOffset.z * _camTransform.forward;
        Vector3 targetPos = _camTransform.transform.position + positionOffsetForward;
        transform.position = targetPos;

        //ArrangeByCameraMovement();

        _lastCamAngle = _camTransform.transform.eulerAngles;
    }
   
    private void ArrangeByCameraMovement()
    {
        if(_lastCamAngle.x > _camTransform.transform.eulerAngles.x)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x + 3.5f, transform.eulerAngles.y, transform.eulerAngles.z);
        }
        else if (_lastCamAngle.x < _camTransform.transform.eulerAngles.x)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x - 3.5f, transform.eulerAngles.y, transform.eulerAngles.z);
        }

        if (_lastCamAngle.y > _camTransform.transform.eulerAngles.y)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 5f, transform.eulerAngles.z);
        }
        else if (_lastCamAngle.y < _camTransform.transform.eulerAngles.y)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y - 5f, transform.eulerAngles.z);
        }
    }
}
