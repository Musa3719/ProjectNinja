using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController _instance;

    [SerializeField]
    private Vector3 _cameraOffset;

    private Transform _playerTransform;

    private Vector3 _playerForwardBefore;


    private HeadBobber headBobber;


    void Awake()
    {
        _instance = this;
        headBobber = GetComponent<HeadBobber>();
        _playerForwardBefore = Vector3.forward;
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        transform.position = _playerTransform.position + _cameraOffset;

        transform.localEulerAngles = _playerTransform.eulerAngles;
    }

    void LateUpdate()
    {
        transform.localPosition = _playerTransform.position + _cameraOffset + headBobber.bobOffset;
    }


    public void LookAround(bool isVelocityToForward)
    {
        float xOffset = -Input.GetAxisRaw("Mouse Y") * Mathf.Cos(PlayerStateController._instance._rb.velocity.magnitude / PlayerMovement._instance._RunSpeed * Mathf.PI / 2.9f); // cos func for slowing camera 
        float yOffset = Input.GetAxisRaw("Mouse X") * Mathf.Cos(PlayerStateController._instance._rb.velocity.magnitude / PlayerMovement._instance._RunSpeed * Mathf.PI / 2.9f);

        _playerForwardBefore = _playerTransform.forward;

        _playerTransform.eulerAngles = new Vector3(_playerTransform.eulerAngles.x, _playerTransform.eulerAngles.y + yOffset, _playerTransform.eulerAngles.z);

        transform.localEulerAngles = _playerTransform.localEulerAngles + new Vector3(transform.localEulerAngles.x + xOffset, 0f, 0f);
        if (transform.eulerAngles.x > 70 && transform.eulerAngles.x < 290)
        {
            float newX = Mathf.Abs(transform.localEulerAngles.x - 70f) < Mathf.Abs(transform.localEulerAngles.x - 290) ? 70f : 290f;
            transform.localEulerAngles = new Vector3(newX, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }

        if(isVelocityToForward)
            VelocityToForward();
    }
    private void VelocityToForward()
    {
        PlayerStateController._instance._rb.velocity = Quaternion.FromToRotation(_playerForwardBefore, _playerTransform.forward) * PlayerStateController._instance._rb.velocity;
    }
}