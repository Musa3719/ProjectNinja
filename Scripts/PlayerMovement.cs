using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement _instance;

    public static float crouchedYScale;
    public static float normalYScale;

    [SerializeField]
    private float _moveSpeed;
    public float _MoveSpeed => _moveSpeed;

    [SerializeField]
    private float _jumpPower;
    public float _JumpPower => _jumpPower;

    [SerializeField]
    private float _runSpeed;
    public float _RunSpeed => _runSpeed;

    private float _stamina;
    public float _Stamina { get { return _stamina; } 
        private set 
        {
            if (value > 100f) _stamina = 100f;
            else if (value < 0f) _stamina = 0f;
            else _stamina = value;
        } 
    }
    public float _needStaminaForJump { get; private set; }
    public bool _canRunWithStamina { get; private set; }
    private float _staminaDecreasePerSecond;
    private float _staminaIncreasePerSecond;
    private float _xBound;
    private float _zBound;
    public float _distToGround;

    public Coroutine CrouchCoroutine;


    private List<Collider> _touchingColliders;

    private void Awake()
    {
        _instance = this;
        _canRunWithStamina = true;
        _touchingColliders = new List<Collider>();
        _needStaminaForJump = 10f;
        _staminaDecreasePerSecond = 8f;
        _staminaIncreasePerSecond = 5f;
        _Stamina = 100f;
        _xBound = GetComponent<Collider>().bounds.extents.x;
        _zBound = GetComponent<Collider>().bounds.extents.z;
        _distToGround = GetComponent<Collider>().bounds.extents.y;
        normalYScale = 1f;
        crouchedYScale = 0.4f;
    }
    private void Update()
    {
        if (GameManager._instance.isPlayerDead || GameManager._instance.isGameStopped) return;

        if (_Stamina <= 3f)
            _canRunWithStamina = false;
        else if (_Stamina >= 15f)
            _canRunWithStamina = true;

        _Stamina += Time.deltaTime * _staminaIncreasePerSecond;
    }
    public bool IsGrounded()
    {
        //Debug.Log(Physics.Raycast(transform.position - Vector3.up * _distToGround * 9f / 10f, -Vector3.up, 0.2f));
        bool firstRay = Physics.Raycast(transform.position - Vector3.up * _distToGround * 9f / 10f + Vector3.right * _xBound * 9f / 10f + Vector3.forward * _zBound * 9f / 10f, -Vector3.up, 0.2f);
        bool secondRay = Physics.Raycast(transform.position - Vector3.up * _distToGround * 9f / 10f + Vector3.right * _xBound * 9f / 10f - Vector3.forward * _zBound * 9f / 10f, -Vector3.up, 0.2f);
        bool thirdRay = Physics.Raycast(transform.position - Vector3.up * _distToGround * 9f / 10f, -Vector3.up, 0.2f);
        bool fourthRay = Physics.Raycast(transform.position - Vector3.up * _distToGround * 9f / 10f - Vector3.right * _xBound * 9f / 10f + Vector3.forward * _zBound * 9f / 10f, -Vector3.up, 0.2f);
        bool fifthRay = Physics.Raycast(transform.position - Vector3.up * _distToGround * 9f / 10f - Vector3.right * _xBound * 9f / 10f - Vector3.forward * _zBound * 9f / 10f, -Vector3.up, 0.2f);

        return firstRay || secondRay || thirdRay || fourthRay || fifthRay;
    }
    public bool IsTouching()
    {
        return _touchingColliders.Count > 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_touchingColliders.Contains(collision.collider))
            _touchingColliders.Add(collision.collider);
    }
    private void OnCollisionExit(Collision collision)
    {
        if (_touchingColliders.Contains(collision.collider))
            _touchingColliders.Remove(collision.collider);
    }
    public void Crouch(Rigidbody rb)
    {
        CrouchCoroutine = StartCoroutine("CrouchRoutine");
    }
    IEnumerator CrouchRoutine()
    {
        _distToGround = crouchedYScale;

        while ((_instance.transform.localScale.y - crouchedYScale) >= 0.2f)
        {
            float yScaleBefore = _instance.transform.localScale.y;
            _instance.transform.localScale = Vector3.Lerp(_instance.transform.localScale, new Vector3(_instance.transform.localScale.x, crouchedYScale, _instance.transform.localScale.z), 20 * Time.deltaTime);
            _instance.transform.position -= Vector3.up * (PlayerMovement.normalYScale * (yScaleBefore - _instance.transform.localScale.y));
            yield return null;
        }
        _instance.transform.localScale = new Vector3(_instance.transform.localScale.x, crouchedYScale, _instance.transform.localScale.z);
    }
    public void Jump(Rigidbody rb)
    {
        rb.velocity = new Vector3(rb.velocity.x, _JumpPower, rb.velocity.z) + transform.forward;
        _Stamina -= _needStaminaForJump;
    }
    public void Walk(Rigidbody rb)
    {
        Movement(rb, _MoveSpeed, 15f);
    }
    public void Run(Rigidbody rb)
    {
        Movement(rb, _RunSpeed, 10f);
        _Stamina -= Time.deltaTime * _staminaDecreasePerSecond;
    }
    private void Movement(Rigidbody rb, float speed, float lerpSpeed)
    {
        float xInput = Input.GetAxisRaw("Horizontal");
        float yInput = Input.GetAxisRaw("Vertical");

        Vector3 rightDirection = (rb.transform.right * xInput);
        Vector3 forwardDirection = (rb.transform.forward * yInput);
        Vector3 direction = (forwardDirection + rightDirection).normalized - rightDirection * 0.38f;
        if (yInput < 0)
        {
            direction -= forwardDirection * 0.38f;
        }

        rb.velocity = Vector3.Lerp(rb.velocity, direction * speed, Time.deltaTime * lerpSpeed / (direction * speed - rb.velocity).magnitude);
    }
}