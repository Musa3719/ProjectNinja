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
        Vector3 targetRot = new Vector3(_camTransform.transform.eulerAngles.x, _camTransform.transform.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Euler(targetRot);

        Vector3 positionOffsetForward = _positionOffset.y * _camTransform.up + _positionOffset.z * _camTransform.forward;
        Vector3 targetPos = _camTransform.transform.position + positionOffsetForward;
        transform.position = targetPos;

        //ArrangeByCameraMovement();

        _lastCamAngle = _camTransform.transform.eulerAngles;

        if (PlayerStateController._instance._playerState is PlayerStates.OnWall && PlayerMovement._instance._touchingWallColliders.Count > 0)
        {
            Collider wallCollider = PlayerMovement._instance._touchingWallColliders[PlayerMovement._instance._touchingWallColliders.Count - 1];

            Vector3 tempCameraDirection = _camTransform.right;
            tempCameraDirection.y = 0f;

            Vector3 tempWallDirection = wallCollider.transform.right;
            tempWallDirection.y = 0f;

            float angle = Vector3.Angle(tempWallDirection, tempCameraDirection);
            if (angle < 25f)
                OnWallSparks();
            else
                PlayerMovement._instance._isAllowedToWallRun = false;
        }
    }
    private void OnWallSparks()
    {
        //sound and vfx
        PlayerMovement._instance._isAllowedToWallRun = true;
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
