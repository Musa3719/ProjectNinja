using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class CameraController : MonoBehaviour
{
    public static CameraController _instance;
    private Camera _mainCamera;

    [SerializeField]
    private Vector3 _cameraOffset;
    [SerializeField]
    private PlayerHandMeshFollow HandMeshFollowScript;

    private Transform _playerTransform;

    private Vector3 _playerForwardBefore;

    private HeadBobber _headBobber;

    private float _verticalCameraModifier;
    private float _velocityForwardLerpSpeed;

    private Queue<float> _xOffsets;
    private Queue<float> _yOffsets;

    private bool _isHeadRotateValueIncreasing;
    private float _headRotateValue;

    void Awake()
    {
        _instance = this;
        //Application.targetFrameRate = 10;
        _headBobber = GetComponent<HeadBobber>();
        _mainCamera = GetComponentInChildren<Camera>();
        _xOffsets = new Queue<float>();
        _yOffsets = new Queue<float>();
        _playerForwardBefore = Vector3.forward;
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _verticalCameraModifier = 0.85f;
        _velocityForwardLerpSpeed = 28f;
        transform.position = _playerTransform.position + _cameraOffset;

        transform.localEulerAngles = _playerTransform.eulerAngles;
        _headRotateValue = 0.1f;
        _isHeadRotateValueIncreasing = true;
    }
    

    void LateUpdate()
    {
        if (PlayerCombat._instance.IsDead) return;

        Vector3 targetPosition = _playerTransform.position + _cameraOffset + _headBobber.bobOffset;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 25f);

        if (PlayerStateController._instance._rb.velocity.magnitude < 0.25f || !PlayerMovement._instance.IsGrounded())
        {
            _headRotateValue = 0f;
            _isHeadRotateValueIncreasing = true;
        }

        if (_isHeadRotateValueIncreasing)
        {
            _headRotateValue += PlayerStateController._instance._rb.velocity.magnitude * Time.deltaTime * 1.75f * (1.1f - Mathf.Abs(_headRotateValue));
            _headRotateValue = Mathf.Clamp(_headRotateValue, -1f, 1f);
            if (_headRotateValue >= 1.25f)
                _isHeadRotateValueIncreasing = false;
        }
        else
        {
            _headRotateValue -= PlayerStateController._instance._rb.velocity.magnitude * Time.deltaTime * 1.75f * (-Mathf.Abs(_headRotateValue) - (-1.1f));
            _headRotateValue = Mathf.Clamp(_headRotateValue, -1f, 1f);
            if (_headRotateValue <= -1.25f)
                _isHeadRotateValueIncreasing = true;
        }
        
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, _headRotateValue * 0.42f);
    }

    public static void ShakeCamera(float magnitude = 1.5f, float roughness = 1f, float fadeInTime = 0.1f, float fadeOutTime = 0.5f)
    {
        CameraShaker.Instance.ShakeOnce(magnitude, roughness, fadeInTime, fadeOutTime);
    }
    public void DeathMove()
    {
        transform.SetParent(PlayerStateController._instance.gameObject.transform);
        StartCoroutine(DeathMoveCoroutine());
    }
    private IEnumerator DeathMoveCoroutine()
    {
        float xRotationSpeed = Random.Range(25f, 35f);
        float zRotationSpeed = Random.Range(12f, 18f);
        zRotationSpeed = Random.Range(0, 2) == 0 ? zRotationSpeed : -zRotationSpeed;
        float fallSpeed = 2f;
        while (true)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x - Time.deltaTime * xRotationSpeed, transform.eulerAngles.y, transform.eulerAngles.z + Time.deltaTime * zRotationSpeed);
            transform.position -= Vector3.up * fallSpeed * Time.deltaTime + PlayerStateController._instance.transform.forward * fallSpeed * 0.85f * Time.deltaTime;
            HandMeshFollowScript._positionOffset.z -= Time.deltaTime * 0.65f;
            yield return null;
        }
    }

    public void ArrangeFOV(float Fov)
    {
        _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, Fov, Time.deltaTime * 2f);
    }
    public void LookAround(bool isVelocityToForward)
    {
        if (PlayerCombat._instance.IsDead) return;

        Vector3 speed = PlayerStateController._instance._rb.velocity;
        speed.y = 0f;
        
        float xOffset = -InputHandler.GetAxis("Mouse Y") * Time.deltaTime * 60f / 12f * Options._instance.MouseSensitivity * _verticalCameraModifier * Mathf.Cos(speed.magnitude / PlayerMovement._instance._RunSpeed * Mathf.PI / 4f); // cos func for slowing camera 
        float yOffset = InputHandler.GetAxis("Mouse X") * Time.deltaTime * 60f / 12f * Options._instance.MouseSensitivity * Mathf.Cos(speed.magnitude / PlayerMovement._instance._RunSpeed * Mathf.PI / 4f);

        if (PlayerCombat._instance._IsAttacking)
        {
            xOffset /= 2f;
            yOffset /= 2f;
        }

        float[] offsets = GetAvarageMouseOffsets(xOffset, yOffset, 2);


        _playerForwardBefore = _playerTransform.forward;

        _playerTransform.eulerAngles = new Vector3(_playerTransform.eulerAngles.x, _playerTransform.eulerAngles.y + offsets[1], _playerTransform.eulerAngles.z);

        transform.localEulerAngles = _playerTransform.localEulerAngles + new Vector3(transform.localEulerAngles.x + offsets[0], 0f, 0f);
        if (transform.eulerAngles.x > 60f && transform.eulerAngles.x < 290)
        {
            float newX = Mathf.Abs(transform.localEulerAngles.x - 60f) < Mathf.Abs(transform.localEulerAngles.x - 290) ? 60f : 290f;
            transform.localEulerAngles = new Vector3(newX, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }

        if(isVelocityToForward && PlayerMovement._instance._isAllowedToVelocityForward)//is allowed is always true now except attackDeflectedMove
            VelocityToForward();

        ArrangeOnWallCamera();
    }
    private void ArrangeOnWallCamera()
    {
        if (!(PlayerStateController._instance._playerState is PlayerStates.OnWall)) return;

        Collider wallCollider = PlayerMovement._instance._touchingWallColliders[PlayerMovement._instance._touchingWallColliders.Count - 1];

        Vector3 tempCameraDirection = transform.right;
        tempCameraDirection.y = 0f;

        Vector3 tempWallDirection = wallCollider.transform.right;
        tempWallDirection.y = 0f;

        float angle = Vector3.SignedAngle(tempWallDirection, tempCameraDirection, Vector3.up);
        if (angle < 0f)
        {
            if (Vector3.Angle(tempWallDirection, tempCameraDirection) > 90f)
            {
                angle = Vector3.SignedAngle(-tempWallDirection, tempCameraDirection, Vector3.up);
                angle = Mathf.Clamp(angle, -70f, 70f);
                _playerTransform.Rotate(Vector3.up, -angle * Time.deltaTime * 10f);
                transform.Rotate(Vector3.up, -angle * Time.deltaTime * 10f);
            }
            else
            {
                angle = Mathf.Clamp(angle, -70f, 70f);
                _playerTransform.Rotate(Vector3.up, -angle * Time.deltaTime * 10f);
                transform.Rotate(Vector3.up, -angle * Time.deltaTime * 10f);
            }
            
        }
    }
    private float[] GetAvarageMouseOffsets(float xOffset, float yOffset, int howMuchFrameOffset)
    {
        if (_xOffsets.Count > howMuchFrameOffset)
            _xOffsets.Dequeue();
        _xOffsets.Enqueue(xOffset);

        if (_yOffsets.Count > howMuchFrameOffset)
            _yOffsets.Dequeue();
        _yOffsets.Enqueue(yOffset);

        if (_xOffsets.Count == 0)
            Debug.LogError("xOffsets lenght is zero");
        if (_yOffsets.Count == 0)
            Debug.LogError("yOffsets lenght is zero");

        float total = 0;
        int i = 0;
        foreach (var item in _xOffsets)
        {
            total += item;
            i++;
        }
        float xAvarage = total / i;

        total = 0;
        i = 0;
        foreach (var item in _yOffsets)
        {
            total += item;
            i++;
        }
        float yAvarage = total / i;

        return new float[] { xAvarage, yAvarage };
    }
    private void VelocityToForward()
    {
        Vector3 targetVelocity = Quaternion.FromToRotation(_playerForwardBefore, _playerTransform.forward) * PlayerStateController._instance._rb.velocity;
        if (targetVelocity == PlayerStateController._instance._rb.velocity || (!PlayerMovement._instance.IsGrounded() && PlayerStateController._instance._playerState is PlayerStates.Movement))
            PlayerStateController._instance._rb.velocity = Vector3.Lerp(PlayerStateController._instance._rb.velocity, targetVelocity, Time.deltaTime * 44f);
        else
            PlayerStateController._instance._rb.velocity = Vector3.Lerp(PlayerStateController._instance._rb.velocity, targetVelocity, Time.deltaTime * _velocityForwardLerpSpeed);
    }
}