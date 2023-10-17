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

    public float RunCounter;

    private Transform _playerTransform;

    private Vector3 _playerForwardBefore;

    private HeadBobber _headBobber;

    private float _verticalCameraModifier;
    private float _velocityForwardLerpSpeed;

    private Queue<float> _xOffsets;
    private Queue<float> _yOffsets;

    private bool _isHeadRotateValueIncreasing;
    private float _headRotateValue;
    private bool _isRunningForCamera;

    private float _xAngleRangeLow;
    private float _xAngleRangeHigh;

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
        _velocityForwardLerpSpeed = 23f;
        transform.position = _playerTransform.position + _cameraOffset;

        transform.localEulerAngles = _playerTransform.eulerAngles;
        _headRotateValue = 0.1f;
        _isHeadRotateValueIncreasing = true;
        _xAngleRangeLow = 60f;
        _xAngleRangeHigh = 295f;
    }
    

    void LateUpdate()
    {
        if (PlayerCombat._instance.IsDead) return;

        if (PlayerStateController.IsRunning() && !_isRunningForCamera)
        {
            RunCounter = 1f;
        }
        _isRunningForCamera = PlayerStateController.IsRunning();

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
        float xRotationSpeed = Random.Range(65f, 95f);
        float zRotationSpeed = Random.Range(12f, 18f);
        zRotationSpeed = Random.Range(0, 2) == 0 ? zRotationSpeed : -zRotationSpeed;
        float startTime = Time.time;
        while (true)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x - Time.deltaTime * xRotationSpeed, transform.eulerAngles.y, transform.eulerAngles.z + Time.deltaTime * zRotationSpeed);
            HandMeshFollowScript._positionOffset.z -= Time.deltaTime * 0.65f;
            yield return null;
        }
    }

    public void ArrangeFOV(float Fov)
    {
        if (PlayerCombat._instance._IsAttacking)
            _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, Fov - 5f, Time.deltaTime * 4.5f);
        else
            _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, Fov, Time.deltaTime * 4.5f);
    }
    public void LookAround(bool isVelocityToForward)
    {
        if (PlayerCombat._instance.IsDead) return;

        Vector3 speed = PlayerStateController._instance._rb.velocity;
        speed.y = 0f;


        float xOffset = -InputHandler.GetAxis("Mouse Y") * Time.deltaTime * 60f / 24f * Options._instance.MouseSensitivity * _verticalCameraModifier; // cos func for slowing camera 
        float yOffset = InputHandler.GetAxis("Mouse X") * Time.deltaTime * 60f / 24f * Options._instance.MouseSensitivity;

        if (Time.timeScale != 0f && Time.timeScale != 1f)
        {
            xOffset /= Time.timeScale * 1.125f;
            yOffset /= Time.timeScale * 1.125f;
        }

        if (RunCounter > 0)
        {
            xOffset += RunCounter * Time.deltaTime * 14f;
            RunCounter = Mathf.Lerp(RunCounter, -0.025f, Time.deltaTime * 3f);
        }

        if (PlayerCombat._instance._IsAttacking)
        {
            /*xOffset *= 0.75f;
            yOffset *= 0.75f;*/
        }
        if (Time.timeScale != 0f && Time.timeScale != 1f)
        {
            float[] offsets = GetAvarageMouseOffsets(xOffset, yOffset, 10);
        }


        //float[] offsets = GetAvarageMouseOffsets(xOffset, yOffset, 2);


        _playerForwardBefore = _playerTransform.forward;

        _playerTransform.eulerAngles = new Vector3(_playerTransform.eulerAngles.x, _playerTransform.eulerAngles.y + yOffset, _playerTransform.eulerAngles.z);

        transform.localEulerAngles = _playerTransform.localEulerAngles + new Vector3(transform.localEulerAngles.x + xOffset, 0f, 0f);

        if (GameManager._instance.PlayerRb.velocity.magnitude > 10f)
        {
            _xAngleRangeHigh = 340f;
        }
        else
        {
            _xAngleRangeHigh = 295f;
        }
        if (transform.eulerAngles.x > _xAngleRangeLow && transform.eulerAngles.x < _xAngleRangeHigh)
        {
            float newX = Mathf.Abs(transform.localEulerAngles.x - _xAngleRangeLow) < Mathf.Abs(transform.localEulerAngles.x - _xAngleRangeHigh) ? _xAngleRangeLow : _xAngleRangeHigh;
            transform.localEulerAngles = new Vector3(Mathf.Lerp(transform.localEulerAngles.x, newX, Time.deltaTime * 5f), transform.localEulerAngles.y, transform.localEulerAngles.z);
        }

        if(isVelocityToForward && PlayerMovement._instance._isAllowedToVelocityForward)
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
        PlayerStateController._instance._rb.velocity = Vector3.Lerp(PlayerStateController._instance._rb.velocity, targetVelocity, Time.deltaTime * _velocityForwardLerpSpeed);
    }
}