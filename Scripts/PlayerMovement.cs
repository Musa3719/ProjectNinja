using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement _instance;
    
    private ConstantForce _constantForceUp;

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
    public Coroutine JumpCoroutine;

    public bool isJumped;
    public bool isJumpedFromWall;


    private List<Collider> _touchingWallColliders;

    private void Awake()
    {
        _instance = this;
        _canRunWithStamina = true;
        _touchingWallColliders = new List<Collider>();
        _needStaminaForJump = 10f;
        _staminaDecreasePerSecond = 8f;
        _staminaIncreasePerSecond = 5f;
        _Stamina = 100f;
        _xBound = GetComponent<Collider>().bounds.extents.x;
        _zBound = GetComponent<Collider>().bounds.extents.z;
        _distToGround = GetComponent<Collider>().bounds.extents.y;
        _constantForceUp = GetComponent<ConstantForce>();
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
        bool firstRay = Physics.Raycast(transform.position - Vector3.up * _distToGround * 9f / 10f + Vector3.right * _xBound * 9f / 10f + Vector3.forward * _zBound * 9f / 10f, -Vector3.up, 0.3f);
        bool secondRay = Physics.Raycast(transform.position - Vector3.up * _distToGround * 9f / 10f + Vector3.right * _xBound * 9f / 10f - Vector3.forward * _zBound * 9f / 10f, -Vector3.up, 0.3f);
        bool thirdRay = Physics.Raycast(transform.position - Vector3.up * _distToGround * 9f / 10f, -Vector3.up, 0.3f);
        bool fourthRay = Physics.Raycast(transform.position - Vector3.up * _distToGround * 9f / 10f - Vector3.right * _xBound * 9f / 10f + Vector3.forward * _zBound * 9f / 10f, -Vector3.up, 0.3f);
        bool fifthRay = Physics.Raycast(transform.position - Vector3.up * _distToGround * 9f / 10f - Vector3.right * _xBound * 9f / 10f - Vector3.forward * _zBound * 9f / 10f, -Vector3.up, 0.3f);
        
        ArrangeGravity(firstRay, secondRay, thirdRay, fourthRay, fifthRay);

        return firstRay || secondRay || thirdRay || fourthRay || fifthRay;
    }
    private void ArrangeGravity(bool firstRay, bool secondRay, bool thirdRay, bool fourthRay, bool fifthRay)
    {
        if (firstRay || secondRay || thirdRay || fourthRay || fifthRay)
        {
            _constantForceUp.enabled = true;
        }
        else
        {
            _constantForceUp.enabled = false;
        }
    }
    public bool IsTouching()
    {
        return _touchingWallColliders.Count > 0;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject == null || collider.gameObject.layer != LayerMask.NameToLayer("WallsAndPlanes") || !collider.CompareTag("Wall")) return;
        if (!_touchingWallColliders.Contains(collider))
            _touchingWallColliders.Add(collider);
    }
    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject == null || collider.gameObject.layer != LayerMask.NameToLayer("WallsAndPlanes") || !collider.CompareTag("Wall")) return;
        if (_touchingWallColliders.Contains(collider))
            _touchingWallColliders.Remove(collider);
    }
    public void Crouch(Rigidbody rb)
    {
        CrouchCoroutine = StartCoroutine("CrouchRoutine");
    }
    public void SlideFromWall(Rigidbody rb)
    {
        float lerpSpeed = 10f;
        Vector3 targetVelocity = -Vector3.up * 15f;
        rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.deltaTime * lerpSpeed / (targetVelocity - rb.velocity).magnitude);
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
        Vector3 forwardBySpeed = Mathf.Clamp(rb.velocity.magnitude / 5f, 0.25f, 100f) * transform.forward;
        rb.velocity = new Vector3(rb.velocity.x, _JumpPower, rb.velocity.z) + forwardBySpeed;
        _Stamina -= _needStaminaForJump;
        if (JumpCoroutine != null)
            StopCoroutine(JumpCoroutine);
        JumpCoroutine = StartCoroutine(JumpButtonHolding(rb));

        isJumped = true;
        Invoke("CloseIsJumped", 0.2f);
    }
    public void JumpFromWall(Rigidbody rb, float amount)
    {
        float jumpPower = _JumpPower * amount * 6f;
        rb.velocity = PlayerStateController._instance._cameraController.transform.forward * jumpPower;

        isJumpedFromWall = true;
        Invoke("CloseIsJumpedFromWall", 0.1f);
    }
    private void CloseIsJumped()
    {
        isJumped = false;
    }
    private void CloseIsJumpedFromWall()
    {
        isJumpedFromWall = false;
    }
    IEnumerator JumpButtonHolding(Rigidbody rb)
    {
        while (Input.GetButton("Jump"))
        {
            yield return null;
        }
        if (rb.velocity.y > 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 3f / 4f, rb.velocity.z);
            yield return new WaitForSeconds(0.1f);
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 2f / 3f, rb.velocity.z);
        }
    }
    public void Walk(Rigidbody rb)
    {
        Movement(rb, _MoveSpeed, 10f);
    }
    public void Run(Rigidbody rb)
    {
        Movement(rb, _RunSpeed, 10f);
        _Stamina -= Time.deltaTime * _staminaDecreasePerSecond;
    }
    public void WallWalk(Rigidbody rb)
    {
        WallMovement(rb, _MoveSpeed * 2f, 15f);
    }
    public void WallRun(Rigidbody rb)
    {
        WallMovement(rb, _RunSpeed, 15f);
        _Stamina -= Time.deltaTime * _staminaDecreasePerSecond;
    }
    private void Movement(Rigidbody rb, float speed, float lerpSpeed)
    {
        float xInput = Input.GetAxisRaw("Horizontal");
        float yInput = Input.GetAxisRaw("Vertical");

        Vector3 rightDirection = (rb.transform.right * xInput);
        Vector3 forwardDirection = (rb.transform.forward * yInput);
        Vector3 direction = (forwardDirection + rightDirection).normalized - rightDirection * (0.38f + rb.velocity.magnitude / 20f / 6f);

        if (yInput < 0)
        {
            direction -= forwardDirection * 0.38f;
        }
        var targetVelocity = direction * speed;
        targetVelocity.y = rb.velocity.y;
        rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.deltaTime * lerpSpeed / (targetVelocity - rb.velocity).magnitude);
    }
    private void WallMovement(Rigidbody rb, float speed, float lerpSpeed)
    {
        if (PlayerMovement._instance._touchingWallColliders.Count == 0) return;

        float yInput = Input.GetAxisRaw("Vertical");

        Vector2 first = new Vector2(PlayerMovement._instance._touchingWallColliders[PlayerMovement._instance._touchingWallColliders.Count - 1].transform.parent.transform.forward.x, PlayerMovement._instance._touchingWallColliders[PlayerMovement._instance._touchingWallColliders.Count - 1].transform.parent.transform.forward.z);
        Vector2 second = new Vector2(transform.forward.x, transform.forward.z);
        float isForward = 0f;
        if (Vector2.Dot(first, second) > 0)//angle lower than 90
        {
            isForward = 1;
        }
        else
        {
            isForward = -1;
        }
        Vector3 forwardDirection = (PlayerMovement._instance._touchingWallColliders[PlayerMovement._instance._touchingWallColliders.Count - 1].transform.parent.transform.forward * yInput * isForward);

        if (yInput < 0)
        {
            forwardDirection -= forwardDirection * 0.38f;
        }

        var targetVelocity = forwardDirection * speed;
        targetVelocity.y = rb.velocity.y;
        if (targetVelocity.y < -5f)
            targetVelocity.y = -5f;
        rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.deltaTime * lerpSpeed / (targetVelocity - rb.velocity).magnitude);
    }

}

