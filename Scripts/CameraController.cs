using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController _instance;
    private Camera _mainCamera;

    [SerializeField]
    private Vector3 _cameraOffset;

    private Transform _playerTransform;

    private Vector3 _playerForwardBefore;

    private HeadBobber _headBobber;

    private float _verticalCameraModifier;
    public float _mouseSensitivity;
    public float _maxSensitivity { get; private set; }


    void Awake()
    {
        _instance = this;
        _headBobber = GetComponent<HeadBobber>();
        _mainCamera = GetComponentInChildren<Camera>();
        _playerForwardBefore = Vector3.forward;
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _verticalCameraModifier = 0.75f;
        _maxSensitivity = 1.8f;
        transform.position = _playerTransform.position + _cameraOffset;

        transform.localEulerAngles = _playerTransform.eulerAngles;
    }

    void LateUpdate()
    {
        transform.localPosition = _playerTransform.position + _cameraOffset + _headBobber.bobOffset;
    }

    public void ArrangeFOV(float Fov)
    {
        _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, Fov, Time.deltaTime * 10f);
    }
    public void LookAround(bool isVelocityToForward)
    {
        float xOffset = -Input.GetAxisRaw("Mouse Y") * _mouseSensitivity * _verticalCameraModifier * Mathf.Cos(PlayerStateController._instance._rb.velocity.magnitude / PlayerMovement._instance._RunSpeed * Mathf.PI / 3.5f); // cos func for slowing camera 
        float yOffset = Input.GetAxisRaw("Mouse X") * _mouseSensitivity * Mathf.Cos(PlayerStateController._instance._rb.velocity.magnitude / PlayerMovement._instance._RunSpeed * Mathf.PI / 3.5f);

        _playerForwardBefore = _playerTransform.forward;

        _playerTransform.eulerAngles = new Vector3(_playerTransform.eulerAngles.x, _playerTransform.eulerAngles.y + yOffset, _playerTransform.eulerAngles.z);

        transform.localEulerAngles = _playerTransform.localEulerAngles + new Vector3(transform.localEulerAngles.x + xOffset, 0f, 0f);
        if (transform.eulerAngles.x > 70 && transform.eulerAngles.x < 290)
        {
            float newX = Mathf.Abs(transform.localEulerAngles.x - 70f) < Mathf.Abs(transform.localEulerAngles.x - 290) ? 70f : 290f;
            transform.localEulerAngles = new Vector3(newX, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }

        if(isVelocityToForward && PlayerMovement._instance._isAllowedToVelocityForward)//is allowed is always true now
            VelocityToForward();
    }
    private void VelocityToForward()
    {
        Vector3 targetVelocity = Quaternion.FromToRotation(_playerForwardBefore, _playerTransform.forward) * PlayerStateController._instance._rb.velocity;
        if (targetVelocity == PlayerStateController._instance._rb.velocity)
            PlayerStateController._instance._rb.velocity = Vector3.Lerp(PlayerStateController._instance._rb.velocity, targetVelocity, Time.deltaTime * 16f * 2.5f);
        else
            PlayerStateController._instance._rb.velocity = Vector3.Lerp(PlayerStateController._instance._rb.velocity, targetVelocity, Time.deltaTime * 16f / (targetVelocity - PlayerStateController._instance._rb.velocity).magnitude);
    }
}