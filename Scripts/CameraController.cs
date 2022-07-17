using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController _instance;

    [SerializeField]
    private Vector3 _cameraOffset;

    private Transform _playerTransform;

    private Vector3 playerForwardBefore;

    void Awake()
    {
        _instance = this;
        playerForwardBefore = Vector3.forward;
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        transform.position = _playerTransform.position + _cameraOffset;

        transform.eulerAngles = _playerTransform.eulerAngles;
    }


    void LateUpdate()
    {
        transform.position = _playerTransform.position + _cameraOffset;
    }

    public void LookAround()
    {
        float xOffset = -Input.GetAxisRaw("Mouse Y") * Mathf.Cos(PlayerStateController._instance._rb.velocity.magnitude / PlayerMovement._instance._RunSpeed * Mathf.PI / 2.9f); // cos func for slowing camera 
        float yOffset = Input.GetAxisRaw("Mouse X") * Mathf.Cos(PlayerStateController._instance._rb.velocity.magnitude / PlayerMovement._instance._RunSpeed * Mathf.PI / 2.9f);

        playerForwardBefore = _playerTransform.forward;

        _playerTransform.eulerAngles = new Vector3(_playerTransform.eulerAngles.x, _playerTransform.eulerAngles.y + yOffset, _playerTransform.eulerAngles.z);

        transform.eulerAngles = _playerTransform.eulerAngles + new Vector3(transform.eulerAngles.x + xOffset, 0f, 0f);
        if (transform.eulerAngles.x > 70 && transform.eulerAngles.x < 290)
        {
            float newX = Mathf.Abs(transform.eulerAngles.x - 70f) < Mathf.Abs(transform.eulerAngles.x - 290) ? 70f : 290f;
            transform.eulerAngles = new Vector3(newX, transform.eulerAngles.y, transform.eulerAngles.z);
        }

        VelocityToForward();
    }
    private void VelocityToForward()
    {
        PlayerStateController._instance._rb.velocity = Quaternion.FromToRotation(playerForwardBefore, _playerTransform.forward) * PlayerStateController._instance._rb.velocity;
    }
}