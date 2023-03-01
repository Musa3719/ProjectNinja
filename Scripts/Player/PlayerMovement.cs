using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerStates;
using System;
using UnityEngine.Rendering.HighDefinition;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement _instance;
    
    public ConstantForce _constantForceUp { get; private set; }
    public Transform _hookConnectedTransform { get; private set; }
    public Vector3 _hookConnectedPositionOffset { get; private set; }


    [SerializeField]
    private float _moveSpeed;
    public float _MoveSpeed => _moveSpeed;

    [SerializeField]
    private float _jumpPower;
    public float _JumpPower => _jumpPower;

    [SerializeField]
    private float _runSpeed;
    public float _RunSpeed => _runSpeed;
    [SerializeField]
    private float _dodgeSpeed;
    public float _DodgeSpeed => _dodgeSpeed;
    [SerializeField]
    private float _attackBlockedSpeed;
    public float _AttackBlockedSpeed => _attackBlockedSpeed;
    [SerializeField]
    private float _attackMoveSpeed;
    public float _AttackMoveSpeed => _attackMoveSpeed;
    [SerializeField]
    private float _forwardLeapSpeed;
    public float _ForwardLeapSpeed => _forwardLeapSpeed;

    [SerializeField]
    private float _hookMovementSpeed;
    public float _HookMovementSpeed => _hookMovementSpeed;

    [SerializeField]
    private float _maxHookLenght;
    public float _MaxHookLenght => _maxHookLenght;

    private Transform _HandOffsetForHook;
    private float _hookAnimTime;

    public float _MaxStamina { get; private set; }
    private float _stamina;
    public float _Stamina { get { return _stamina; } 
        set 
        {
            if (value > _MaxStamina) _stamina = _MaxStamina;
            else if (value < 0f) _stamina = 0f;
            else _stamina = value; 
        } 
    }

    private float _isGroundedCounter;

    public float _needStaminaForJump { get; private set; }
    public float _hookStaminaUse { get; private set; }
    public float _attackStaminaUse { get; private set; }
    public float _forwardLeapStaminaUse { get; private set; }
    public float _dodgeStaminaUse { get; private set; }
    public float _blockedStaminaUse { get; private set; }
    public float _attackDeflectedStaminaUse { get; private set; }
    public float _staminaDecreasePerSecond { get; private set; }
    public float _staminaIncreasePerSecond { get; private set; }


    public static float crouchedYScale;
    public static float normalYScale;

    private float _xBound;
    private float _zBound;
    public float _distToGround { get; set; }

    public Coroutine _crouchCoroutine;
    public Coroutine _jumpCoroutine;
    public Coroutine _wallJumpCoroutine;
    public Coroutine _openVelocityForwardCoroutine;
    public Coroutine _isAllowedHookCoroutine;
    public Coroutine _hookCoroutine;
    public Coroutine _attackMoveCoroutine;
    public Coroutine _attackDeflectedMoveCoroutine;

    public bool _canRunWithStamina { get; private set; }
    public bool _isJumped { get; private set; }
    public bool _isOnAttackOrAttackDeflectedMove { get; private set; }
    public bool _isAllowedToVelocityForward { get; private set; }
    public bool _isHookAllowed { get; private set; }
    public bool _isHookAllowedForAir { get; private set; }
    public bool _isDodgeAllowedForAir { get; private set; }
    public bool _isAllowedToWallRun { get; set; }

    private float _hookWaitTime;
    private float _hookTime;

    private float _airStopMultiplier;
    private float _airFrictionMultiplier;

    private float _crouchTimerForGrounded;

    private float _lastCrouchStartTime;

    private bool _isUpHookLastTime;
    private bool _isHookDisabled;



    public List<Collider> _touchingWallColliders { get; private set; }
    public List<Collider> _touchingGroundColliders { get; private set; }


    private float _walkSoundCounter;
    private float _fastLandCounter;
    public float _lastTimeFastLanded { get; private set; }
    public float _lastTimeOnWallFastLanded { get; private set; }

    public float _lastHookTime;
    public float _lastHookNotReadyTime;

    public bool _isGroundedWorkedThisFrame;

    [SerializeField]
    private Transform _leftHandTransform;
    [SerializeField]
    private Transform _rightHandTransform;
    [SerializeField]
    private Transform _HandOffsetForUpHook;

    private void OnEnable()
    {
        GameManager._instance._staminaGainEvent += StaminaGain;
        GameManager._instance._pushEvent += Pushed;
        GameManager._instance._playAnimEvent += ChangeAnimFromEvent;
        GameManager._instance._bornEvent += Born;
    }
    private void OnDisable()
    {
        GameManager._instance._staminaGainEvent -= StaminaGain;
        GameManager._instance._pushEvent -= Pushed;
        GameManager._instance._playAnimEvent -= ChangeAnimFromEvent;
        GameManager._instance._bornEvent -= Born;
    }
    private void StaminaGain(float additionalStamina)
    {
        _Stamina += additionalStamina;
    }
    private void Born()
    {
        SoundManager._instance.PlaySound(SoundManager._instance.Born, transform.position, 0.35f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        PlayerStateController._instance.ChangeAnimation("Born");

        //PlayerStateController._instance.BladeSpinSoundObject = SoundManager._instance.PlaySound(SoundManager._instance.BladeSpin, transform.position, 0.12f, true, 1f);
    }
    private void Pushed(Vector3 direction)
    {
        _Stamina -= 27.5f;
        PlayerStateController._instance.ChangeAnimation("Stun");
        SoundManager._instance.PlaySound(SoundManager._instance.SmallCrash, transform.position, 0.5f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        PlayerStateController._instance.EnterAnimState(new PlayerAnimations.WaitForOneAnim(1f));
        PushMove(direction);
    }
    private void ChangeAnimFromEvent(string name, float time)
    {
        if (time != 0f)
            PlayerStateController._instance.EnterAnimState(new PlayerAnimations.WaitForOneAnim(time));

        PlayerStateController._instance.ChangeAnimation(name);
    }
    private void Awake()
    {
        _instance = this;
        _MaxStamina = 100f;
        _hookWaitTime = 3f;
        _hookTime = 0.25f;
        _isAllowedToVelocityForward = true;
        _isHookAllowed = true;
        _isHookAllowedForAir = true;
        _isDodgeAllowedForAir = true;
        _canRunWithStamina = true;
        _touchingWallColliders = new List<Collider>();
        _touchingGroundColliders = new List<Collider>();
        _needStaminaForJump = 10f;
        _staminaDecreasePerSecond = 10f;
        _staminaIncreasePerSecond = 5f;
        _Stamina = 100f;
        _hookStaminaUse = 32f;
        _attackStaminaUse = 8f;
        _forwardLeapStaminaUse = 16f;
        _dodgeStaminaUse = 30f;
        _blockedStaminaUse = 16f;
        _attackDeflectedStaminaUse = 20f;
        _xBound = GetComponent<Collider>().bounds.extents.x;
        _zBound = GetComponent<Collider>().bounds.extents.z;
        _distToGround = GetComponent<Collider>().bounds.extents.y;
        _constantForceUp = GetComponent<ConstantForce>();
        normalYScale = 1f;
        crouchedYScale = 0.4f;
        _airStopMultiplier = 1.75f;
        _airFrictionMultiplier = 0.75f;
        _lastTimeFastLanded = -1f;
        _lastCrouchStartTime = -1f;
        _lastTimeOnWallFastLanded = -1f;
        _lastHookTime = -1f;
        _lastHookNotReadyTime = -1f;
        _HandOffsetForHook = _rightHandTransform;
        GameManager._instance._isGroundedWorkedThisFrameEvent += () => _isGroundedWorkedThisFrame = false;
        GameManager._instance.PlayerRunningSpeed = _MoveSpeed;
        _hookAnimTime = 0.15f;
    }
    private void Update()
    {
        if (GameManager._instance.isPlayerDead || GameManager._instance.isGameStopped || GameManager._instance.isOnCutscene) return;

        if (_Stamina <= 3f)
            _canRunWithStamina = false;
        else if (_Stamina >= 15f)
            _canRunWithStamina = true;

        if (PlayerStateController._instance._isStaminaManual)
        {
            _Stamina += Time.deltaTime * _staminaIncreasePerSecond * Mathf.Clamp((PlayerStateController._instance._staminaManualCounter + 1f) * 1.5f, 1.75f, 5f);
        }
        else if(PlayerCombat._instance._IsBlocking)
        {
            _Stamina += Time.deltaTime * _staminaIncreasePerSecond * 0.1f;
        }
        else
        {
            _Stamina += Time.deltaTime * _staminaIncreasePerSecond;
        }

    }
    private void LateUpdate()
    {
        ArrangeLineRendererPlayerPosition();
    }
    private void ArrangeLineRendererPlayerPosition()
    {
        Vector3 offsetType = Vector3.zero;
        if (_isUpHookLastTime)
            offsetType = _HandOffsetForUpHook.position;
        else
            offsetType = _HandOffsetForHook.position;

        //Vector3 offsetByRotation = transform.up * offsetType.y + transform.right * offsetType.x + transform.forward * offsetType.z;

        PlayerStateController._instance._lineRenderer.SetPosition(0, offsetType);
        if (_hookConnectedTransform != null && !_isHookDisabled)
        {
            if (PlayerMovement._instance._lastHookTime + _hookAnimTime > Time.time)
            {
                PlayerStateController._instance._lineRenderer.SetPosition(1, Vector3.Lerp(offsetType, _hookConnectedTransform.position + _hookConnectedPositionOffset, (Time.time - PlayerMovement._instance._lastHookTime) / _hookAnimTime));
            }
            else
                PlayerStateController._instance._lineRenderer.SetPosition(1, _hookConnectedTransform.position + _hookConnectedPositionOffset);
            PlayerStateController._instance._hookAnchor.transform.position = PlayerStateController._instance._lineRenderer.GetPosition(1);
            PlayerStateController._instance._hookAnchor.transform.forward = (PlayerStateController._instance._lineRenderer.GetPosition(1) - PlayerStateController._instance._lineRenderer.GetPosition(0)).normalized;
        }
    }
    private bool IsGroundedInside()
    {

        float downDistance = 0.3f;
        if (PlayerStateController._instance._playerState is PlayerStates.Movement && (PlayerStateController._instance._playerState as PlayerStates.Movement).isCrouching)
        {
            _crouchTimerForGrounded = 0.1f;
            downDistance *= 1.5f;
        }
        else if (_crouchTimerForGrounded > 0)
        {
            downDistance *= 1.5f;
            if (!_isGroundedWorkedThisFrame)
                _crouchTimerForGrounded -= Time.deltaTime;
        }

        RaycastHit[] hits = new RaycastHit[6];
        bool[] rays = new bool[6];
        rays[0] = Physics.Raycast(transform.position - Vector3.up * _distToGround * 9f / 10f + Vector3.right * _xBound * 9f / 10f + Vector3.forward * _zBound * 9f / 10f, -Vector3.up, out hits[0], 0.3f);
        rays[1] = Physics.Raycast(transform.position - Vector3.up * _distToGround * 9f / 10f + Vector3.right * _xBound * 9f / 10f - Vector3.forward * _zBound * 9f / 10f, -Vector3.up, out hits[1], 0.3f);
        rays[2] = Physics.Raycast(transform.position - Vector3.up * _distToGround * 9f / 10f, -Vector3.up, out hits[2], downDistance);
        rays[3] = Physics.Raycast(transform.position - Vector3.up * _distToGround * 9f / 10f - Vector3.right * _xBound * 9f / 10f + Vector3.forward * _zBound * 9f / 10f, -Vector3.up, out hits[3], 0.3f);
        rays[4] = Physics.Raycast(transform.position - Vector3.up * _distToGround * 9f / 10f - Vector3.right * _xBound * 9f / 10f - Vector3.forward * _zBound * 9f / 10f, -Vector3.up, out hits[4], 0.3f);

        rays[5] = Physics.Raycast(transform.position, -Vector3.up, out hits[5], 1.15f);

        /*if(!(rays[0] || rays[1] || rays[2] || rays[3] || rays[4]))
        {
            if (rays[5])
            {
                Debug.Log("only five");
            }
        }*/

        int i = 0;
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.isTrigger && rays[i] == true)
            {
                rays[i] = false;
            }
            i++;
        }

        if (!_isGroundedWorkedThisFrame)
        {
            ArrangePlaneSound(hits);
            ArrangeIsAllowedInAir(rays[0], rays[1], rays[2], rays[3], rays[4], rays[5]);
            ArrangeGravity(rays[0], rays[1], rays[2], rays[3], rays[4], rays[5]);
        }

        _isGroundedWorkedThisFrame = true;
        if (PlayerStateController._instance._rb.velocity.y < -10f)
            return IsGroundedFromList();
        return rays[0] || rays[1] || rays[2] || rays[3] || rays[4] || rays[5];
    }
    public bool IsGrounded()
    {
        if (IsGroundedInside())
        {
            _isGroundedCounter = 0f;
            return true;
        }
        else
        {
            _isGroundedCounter += Time.deltaTime;
            if (_isGroundedCounter > 0.15f) return false;
            return true;
        }
    }
    private void ArrangePlaneSound(RaycastHit[] hits)
    {
        if (PlayerStateController._instance._playerState is OnWall || (PlayerStateController._instance._playerState is Movement && (PlayerStateController._instance._playerState as Movement).isCrouching)) return;

        float speed = PlayerStateController._instance._rb.velocity.magnitude;
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.GetComponent<PlaneSound>() != null)
            {
                while (_walkSoundCounter <= 0f)//not using if because it could be lower than -1 when frame drops etc
                {
                    SoundManager._instance.PlayPlaneOrWallSound(hit.collider.GetComponent<PlaneSound>().PlaneSoundType, speed);
                    SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.ArmorWalkSounds), transform.position, 0.02f, false, UnityEngine.Random.Range(0.93f, 1.07f));
                    _walkSoundCounter += 1f;
                }
                if (PlayerStateController._instance._rb.velocity.magnitude > _MoveSpeed + 1f)
                {
                    _walkSoundCounter -= Time.deltaTime * speed / 6f;
                }
                else
                {
                    _walkSoundCounter -= Time.deltaTime * speed / 3.2f;
                }

                break;
            }
        }
    }
    public void ArrangeOnWallSound()
    {
        if (PlayerStateController._instance._playerState is Movement) return;

        float speed = PlayerStateController._instance._rb.velocity.magnitude;
        if (_touchingWallColliders.Count == 0) return;
        if (_touchingWallColliders[_touchingWallColliders.Count - 1].GetComponent<PlaneSound>() != null)
        {
            while (_walkSoundCounter <= 0f)//not using if because it could be lower than -1 when frame drops etc
            {
                SoundManager._instance.PlayPlaneOrWallSound(_touchingWallColliders[_touchingWallColliders.Count - 1].GetComponent<PlaneSound>().PlaneSoundType, speed);
                _walkSoundCounter += 1f;
            }
            
            _walkSoundCounter -= Time.deltaTime * speed / 8f;

        }
    }
    private void ArrangeIsAllowedInAir(bool firstRay, bool secondRay, bool thirdRay, bool fourthRay, bool fifthRay, bool sixthRay)
    {
        if (firstRay || secondRay || thirdRay || fourthRay || fifthRay || sixthRay)
        {
            _isHookAllowedForAir = true;
            _isDodgeAllowedForAir = true;
        }
    }
    private void ArrangeGravity(bool firstRay, bool secondRay, bool thirdRay, bool fourthRay, bool fifthRay, bool sixthRay)
    {
        if (firstRay || secondRay || thirdRay || fourthRay || fifthRay || sixthRay)
        {
            _constantForceUp.enabled = true;
        }
        else
        {
            //_constantForceUp.enabled = false;
        }
    }
    /// <summary>
    /// Checks for touching any walls
    /// </summary>
    public bool IsTouching()
    {
        return _touchingWallColliders.Count > 0;
    }
    public bool IsGroundedFromList()
    {
        if (_touchingGroundColliders.Count > 0) return true;
        return false;
    }
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject == null || !collider.CompareTag("WallTrigger")) return;
        if (!_touchingWallColliders.Contains(collider))
        {
            _touchingWallColliders.Add(collider);
        }
    }
    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject == null || !collider.CompareTag("WallTrigger")) return;
        if (_touchingWallColliders.Contains(collider))
        {
            _touchingWallColliders.Remove(collider);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == null || collision.collider.gameObject.layer != LayerMask.NameToLayer("Grounds")) return;
        if (!_touchingGroundColliders.Contains(collision.collider))
        {
            _touchingGroundColliders.Add(collision.collider);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider == null || collision.collider.gameObject.layer != LayerMask.NameToLayer("Grounds")) return;
        if (_touchingGroundColliders.Contains(collision.collider))
        {
            _touchingGroundColliders.Remove(collision.collider);
        }
    }
    private void PushMove(Vector3 direction)
    {
        if (_attackMoveCoroutine != null)
            StopCoroutine(_attackMoveCoroutine);
        if (_attackDeflectedMoveCoroutine != null)
            StopCoroutine(_attackDeflectedMoveCoroutine);
        _attackDeflectedMoveCoroutine = StartCoroutine(PushMoveCoroutine(PlayerStateController._instance._rb, direction));
    }
    private IEnumerator PushMoveCoroutine(Rigidbody rb, Vector3 direction)
    {
        _isOnAttackOrAttackDeflectedMove = true;
        _isAllowedToVelocityForward = false;
        float startTime = Time.time;
        while (startTime + 0.5f * 0.7f > Time.time)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, (direction * 2f * _AttackMoveSpeed), Time.deltaTime * 16f);
            yield return null;
        }
        rb.velocity = (direction * 2f * _AttackMoveSpeed);
        while (startTime + 0.5f > Time.time)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.deltaTime * 10f);
            yield return null;
        }
        _isOnAttackOrAttackDeflectedMove = false;
        _isAllowedToVelocityForward = true;
    }
    public void AttackDeflectedMove(Rigidbody rb)
    {
        CameraController.ShakeCamera(2f, 1f, 0.05f, 0.1f);

        if (!IsGrounded() && rb.velocity.y < 0) return;

        if (_attackMoveCoroutine != null)
            StopCoroutine(_attackMoveCoroutine);
        if (_attackDeflectedMoveCoroutine != null)
            StopCoroutine(_attackDeflectedMoveCoroutine);
        _attackDeflectedMoveCoroutine = StartCoroutine(AttackDeflectedMoveCoroutine(rb, 1.75f));
    }
    private IEnumerator AttackDeflectedMoveCoroutine(Rigidbody rb, float multiplier = 1f)
    {
        _isOnAttackOrAttackDeflectedMove = true;
        _isAllowedToVelocityForward = false;
        float startTime = Time.time;
        while (startTime + 0.5f * 0.7f > Time.time)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, -(transform.forward * 2f * _AttackMoveSpeed * multiplier), Time.deltaTime * 12f);
            yield return null;
        }
        rb.velocity = -(transform.forward * 2f * _AttackMoveSpeed);
        while (startTime + 0.5f > Time.time)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.deltaTime * 7f);
            yield return null;
        }
        _isOnAttackOrAttackDeflectedMove = false;
        _isAllowedToVelocityForward = true;
    }
    public void AttackMove(Rigidbody rb)
    {
        if (!IsGrounded() && rb.velocity.y < 0f) return;

        if (_attackMoveCoroutine != null)
            StopCoroutine(_attackMoveCoroutine);
        _attackMoveCoroutine = StartCoroutine(AttackMoveCoroutine(rb));
    }
    private IEnumerator AttackMoveCoroutine(Rigidbody rb)
    {
        _isOnAttackOrAttackDeflectedMove = true;
        float startTime = Time.time;
        Vector3 tempVel = rb.velocity;
        tempVel.y = 0f;
        while (startTime + 0.3f > Time.time)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, transform.forward * _AttackMoveSpeed * 1.35f + tempVel / 3f, Time.deltaTime * 42f);
            yield return null;
        }
        _isOnAttackOrAttackDeflectedMove = false;
    }
    public void Crouch(Rigidbody rb)
    {
        if (_lastCrouchStartTime + 2.5f < Time.time)
            rb.velocity += transform.forward * 2f;

        _lastCrouchStartTime = Time.time;

        var moveState = PlayerStateController._instance._playerState as PlayerStates.Movement;
        if (moveState != null)
        {
            moveState.isCrouching = true;
        }
        PlayerStateController._instance.SlidingSoundObject = SoundManager._instance.PlaySound(SoundManager._instance.Sliding, transform.position, 0.2f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        _crouchCoroutine = StartCoroutine("CrouchRoutine");
    }
    public void SlideFromWall(Rigidbody rb)
    {
        _lastTimeOnWallFastLanded = Time.time;
        FastLand(rb);
    }
    public void FastLandInAir(Rigidbody rb)
    {
        if (_lastTimeFastLanded + 1f < Time.time)
            _fastLandCounter = 0f;

        FastLand(rb);
        _fastLandCounter = Mathf.Clamp(_fastLandCounter + Time.deltaTime * 1.25f, 0.1f, 1.5f);
        _lastTimeFastLanded = Time.time;
        rb.transform.eulerAngles = new Vector3(rb.transform.eulerAngles.x, rb.transform.eulerAngles.y + Time.deltaTime * 60f * 6f * _fastLandCounter, rb.transform.eulerAngles.z);
    }
    private void FastLand(Rigidbody rb)
    {
        float lerpSpeed = 6f;
        Vector3 targetVelocity = -Vector3.up * 70f;
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
        CameraController.ShakeCamera(1.5f, 1.1f, 0.15f, 0.35f);
        SoundManager._instance.PlaySound(SoundManager._instance.Jump, transform.position, 0.05f, false, UnityEngine.Random.Range(0.93f, 1.07f));

        float jumpPowerBySpeed = _JumpPower + _JumpPower * 0.5f * Mathf.Clamp(rb.velocity.magnitude, _MoveSpeed, _RunSpeed) / _RunSpeed;

        rb.velocity = new Vector3(rb.velocity.x, jumpPowerBySpeed, rb.velocity.z);
        _Stamina -= _needStaminaForJump;

        if (_jumpCoroutine != null)
            StopCoroutine(_jumpCoroutine);
        _jumpCoroutine = StartCoroutine(JumpButtonHolding(rb));

        _isJumped = true;

        Action CloseIsJumped = () => { _isJumped = false; };
        GameManager._instance.CallForAction(CloseIsJumped, 0.2f);
    }
    public void JumpFromWall(Rigidbody rb)
    {
        CameraController.ShakeCamera(1.5f, 1.1f, 0.15f, 0.35f);

        float horizontal = InputHandler.GetAxis("Horizontal");
        float vertical = InputHandler.GetAxis("Vertical");

        Vector3 direction = (transform.forward * vertical + transform.right * horizontal * 0.5f).normalized;
        if (direction.magnitude == 0f)
            direction += transform.forward / 2f;
        else if (vertical == 0)
            direction *= 0.5f;
        Vector3 jumpVelocity = direction * PlayerMovement._instance._jumpPower * 2.4f - rb.velocity.y * Vector3.up / 2f;

        Vector2 tempVel = new Vector2(rb.velocity.x, rb.velocity.z);
        Vector2 tempDir = new Vector2(direction.x, direction.z);
        float angle = Mathf.Clamp(Vector2.Angle(tempVel, tempDir), 1f, 120f);
        jumpVelocity -= jumpVelocity * angle / 240f * Mathf.Clamp(rb.velocity.magnitude, 0f, 15f) / 15f;

        rb.velocity = jumpVelocity;
        _Stamina -= _needStaminaForJump;

        if (_wallJumpCoroutine != null)
            StopCoroutine(_wallJumpCoroutine);
        _wallJumpCoroutine = StartCoroutine(WallJumpButtonHolding(rb));

        _isJumped = true;

        Action CloseIsJumped = () => { _isJumped = false; };
        GameManager._instance.CallForAction(CloseIsJumped, 0.2f);
    }
    IEnumerator JumpButtonHolding(Rigidbody rb)
    {
        while (InputHandler.GetButton("Jump"))
        {
            yield return null;
        }
        if (rb.velocity.y > 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 2f / 3f, rb.velocity.z);
        }

    }
    IEnumerator WallJumpButtonHolding(Rigidbody rb)
    {
        while (InputHandler.GetButton("Jump"))
        {
            yield return null;
        }

        //rb.velocity = new Vector3(rb.velocity.x * 7f / 8f, rb.velocity.y, rb.velocity.z * 7f / 8f);

    }
    public IEnumerator IsAllowedHookTimer()
    {
        GameManager._instance.HookTimerUI.StartTimer(_hookWaitTime);
        yield return new WaitForSeconds(_hookWaitTime);
        _isHookAllowed = true;
        SoundManager._instance.PlaySound(SoundManager._instance.HookReady, transform.position, 0.3f, false, UnityEngine.Random.Range(0.93f, 1.07f));
    }
    private void ArrangeIsVelocityToForward()
    {
        _isAllowedToVelocityForward = false;
        if (_openVelocityForwardCoroutine != null)
            StopCoroutine(_openVelocityForwardCoroutine);
        _openVelocityForwardCoroutine = StartCoroutine(OpenIsAllowedToVelocityForward(0.7f));
    }
    IEnumerator OpenIsAllowedToVelocityForward(float time)
    {
        yield return new WaitForSeconds(time);
        _isAllowedToVelocityForward = true;
    }

    public RaycastHit RaycastForHook()
    {
        //Vector3 offsetByRotation = transform.up * _HandOffsetForHook.position.y + transform.right * _HandOffsetForHook.position.x + transform.forward * _HandOffsetForHook.position.z;
        RaycastHit hit;
        Physics.Raycast(_HandOffsetForHook.position, PlayerStateController._instance._cameraController.transform.forward, out hit, _MaxHookLenght);
        return hit;
    }
    public RaycastHit RaycastForUpHook()
    {
        Vector3 dir = Vector3.zero;
        float velocityMagnitudeWithoutYAxis = new Vector3(PlayerStateController._instance._rb.velocity.x, 0f, PlayerStateController._instance._rb.velocity.z).magnitude;

        if (InputHandler.GetAxis("Horizontal") > 0f)
        {
            dir += PlayerStateController._instance.transform.right;
        }
        else if(InputHandler.GetAxis("Horizontal") < 0f)
        {
            dir -= PlayerStateController._instance.transform.right;
        }

        if (InputHandler.GetAxis("Vertical") > 0f)
        {
            dir += PlayerStateController._instance.transform.forward;
        }
        else if (InputHandler.GetAxis("Vertical") < 0f && velocityMagnitudeWithoutYAxis < _MoveSpeed + 2f)
        {
            dir -= PlayerStateController._instance.transform.forward;
        }

        dir += PlayerStateController._instance.transform.up * 1.75f;
        dir = dir.normalized;

        //Vector3 offsetByRotation = transform.up * _HandOffsetForUpHook.position.y + transform.right * _HandOffsetForUpHook.position.x + transform.forward * _HandOffsetForUpHook.position.z;
        RaycastHit hit;
        Physics.Raycast(_HandOffsetForUpHook.position, dir, out hit, _MaxHookLenght);
        return hit;
    }
    private string GetThrowHookName()
    {
        bool isLeft = false;


        if (PlayerStateController._instance._playerState is PlayerStates.OnWall)
        {
            isLeft = !(PlayerStateController._instance._playerState as PlayerStates.OnWall).isWallOnLeftSide;
        }
        else
        {
            isLeft = UnityEngine.Random.Range(0, 2) == 0 ? true : false;
        }

        if (isLeft)
        {
            _HandOffsetForHook = _leftHandTransform;
            return "LeftHookThrow";
        }
        else
        {
            _HandOffsetForHook = _rightHandTransform;
            return "RightHookThrow";
        }
    }
    public void ThrowUpHook(Rigidbody rb)
    {

        RaycastHit hit = RaycastForUpHook();
        if (hit.collider == null)
        {
            if (_lastHookNotReadyTime + 0.5f < Time.time)
            {
                SoundManager._instance.PlaySound(SoundManager._instance.HookNotReady, transform.position, 0.2f, false, UnityEngine.Random.Range(0.93f, 1.07f));
                _lastHookNotReadyTime = Time.time;
            }
            
            return;
        }
            
        bool isWallOrGround = hit.collider.gameObject.layer == LayerMask.NameToLayer("Grounds") || hit.collider.CompareTag("Wall");
        if (hit.collider != null && isWallOrGround)
        {
            //PlayerStateController._instance._lineRenderer.SetPosition(1, hit.point);
            PlayerStateController._instance._lineRenderer.enabled = true;
            PlayerStateController._instance._hookAnchor.SetActive(true);
            _isHookDisabled = false;

            PlayerStateController._instance.ChangeAnimation("UpHookThrow");
            SoundManager._instance.PlaySound(SoundManager._instance.ThrowHook, transform.position, 0.4f, false, UnityEngine.Random.Range(0.93f, 1.07f));
            CameraController.ShakeCamera(3.5f, 1.2f, 0.15f, 0.7f);

            PlayerMovement._instance._hookConnectedTransform = hit.collider.transform;
            PlayerMovement._instance._hookConnectedPositionOffset = hit.point - hit.collider.transform.position;

            _lastHookTime = Time.time;
            _isHookAllowed = false;
            _isHookAllowedForAir = false;
            _isUpHookLastTime = true;
            GameManager._instance.CallForAction(() => HookPullMovement(rb, true, hit), _hookAnimTime);
            GameManager._instance.CallForAction(PullHook, _hookTime);
            GameManager._instance.CallForAction(()=> { _isHookAllowedForAir = false; }, 0.15f);
        }
        else
        {
            if (_lastHookNotReadyTime + 0.5f < Time.time)
            {
                SoundManager._instance.PlaySound(SoundManager._instance.HookNotReady, transform.position, 0.2f, false, UnityEngine.Random.Range(0.93f, 1.07f));
                _lastHookNotReadyTime = Time.time;
            }
        }

    }
    public void ThrowHook(Rigidbody rb)
    {
        RaycastHit hit = RaycastForHook();
        if (hit.collider == null)
        {
            if (_lastHookNotReadyTime + 0.5f < Time.time)
            {
                SoundManager._instance.PlaySound(SoundManager._instance.HookNotReady, transform.position, 0.2f, false, UnityEngine.Random.Range(0.93f, 1.07f));
                _lastHookNotReadyTime = Time.time;
            }
            return;
        }
        bool isWallOrGround = hit.collider.gameObject.layer == LayerMask.NameToLayer("Grounds") || hit.collider.CompareTag("Wall");
        if (isWallOrGround)
        {
            //PlayerStateController._instance._lineRenderer.SetPosition(1, hit.point);
            PlayerStateController._instance._lineRenderer.enabled = true;
            PlayerStateController._instance._hookAnchor.SetActive(true);
            _isHookDisabled = false;

            PlayerStateController._instance.EnterAnimState(new PlayerAnimations.WaitForOneAnim(0.7f));
            PlayerStateController._instance.ChangeAnimation(GetThrowHookName());
            SoundManager._instance.PlaySound(SoundManager._instance.ThrowHook, transform.position, 0.4f, false, UnityEngine.Random.Range(0.93f, 1.07f));
            CameraController.ShakeCamera(3.5f, 1.2f, 0.15f, 0.7f);

            PlayerMovement._instance._hookConnectedTransform = hit.collider.transform;
            PlayerMovement._instance._hookConnectedPositionOffset = hit.point - hit.collider.transform.position;

            _lastHookTime = Time.time;
            _isHookAllowed = false;
            _isHookAllowedForAir = false;
            _isUpHookLastTime = false;
            GameManager._instance.CallForAction(() => HookPullMovement(rb, false, hit), _hookAnimTime);
            GameManager._instance.CallForAction(PullHook, _hookTime);
            GameManager._instance.CallForAction(() => { _isHookAllowedForAir = false; }, 0.15f);
        }
        else
        {
            if (_lastHookNotReadyTime + 0.5f < Time.time)
            {
                SoundManager._instance.PlaySound(SoundManager._instance.HookNotReady, transform.position, 0.2f, false, UnityEngine.Random.Range(0.93f, 1.07f));
                _lastHookNotReadyTime = Time.time;
            }
        }

    }
    public void PullHook()
    {
        if (_isAllowedHookCoroutine != null)
            StopCoroutine(_isAllowedHookCoroutine);
        _isHookAllowed = false;
        _isHookDisabled = true;
        _isAllowedHookCoroutine = StartCoroutine("IsAllowedHookTimer");
        StartCoroutine(PullHookPositionCoroutine());
    }
    private IEnumerator PullHookPositionCoroutine()
    {
        float startTime = Time.time;
        float localAnimTime = _hookAnimTime * 2f;
        while (Time.time < startTime + localAnimTime)
        {
            PlayerStateController._instance._lineRenderer.SetPosition(1, Vector3.Lerp(_hookConnectedTransform.position + _hookConnectedPositionOffset, PlayerStateController._instance._lineRenderer.GetPosition(0), (Time.time - startTime) / localAnimTime));
            PlayerStateController._instance._hookAnchor.transform.position = PlayerStateController._instance._lineRenderer.GetPosition(1);
            yield return null;
        }
        PlayerMovement._instance._hookConnectedTransform = null;
        PlayerStateController._instance._lineRenderer.enabled = false;
        PlayerStateController._instance._hookAnchor.SetActive(false);
    }
    public void HookPullMovement(Rigidbody rb, bool isUpHook, RaycastHit hit)
    {
        SoundManager._instance.PlaySound(SoundManager._instance.HitWallWithWeapon, hit.point, 0.5f, false, UnityEngine.Random.Range(0.8f, 0.9f));
        GameObject decal = Instantiate(GameManager._instance.HoleDecal, hit.point, Quaternion.identity);
        decal.GetComponent<DecalProjector>().size *= UnityEngine.Random.Range(1.4f, 2.1f);
        decal.transform.forward = hit.transform.right;
        decal.transform.localEulerAngles = new Vector3(decal.transform.localEulerAngles.x, decal.transform.localEulerAngles.y, UnityEngine.Random.Range(0f, 360f));
        GameObject smokeVFX = Instantiate(GameManager._instance.HitSmokeVFX, hit.point + hit.normal * 0.5f, Quaternion.identity);
        smokeVFX.transform.localScale *= 5f;
        Color color = smokeVFX.GetComponentInChildren<SpriteRenderer>().color;
        smokeVFX.GetComponentInChildren<SpriteRenderer>().color = new Color(color.r, color.g, color.b, color.a / 2.5f);
        Destroy(smokeVFX, 5f);

        _Stamina -= _hookStaminaUse;
        Vector3 distance = ((PlayerMovement._instance._hookConnectedTransform.position + PlayerMovement._instance._hookConnectedPositionOffset) - rb.transform.position);
        Vector3 direction = distance.normalized;
        if (_hookCoroutine != null)
            StopCoroutine(_hookCoroutine);

        if (isUpHook)
            _hookCoroutine = StartCoroutine(HookMovement(rb, direction, distance.magnitude));
        else
            _hookCoroutine = StartCoroutine(HookMovement(rb, direction));
    }
    /// <summary>
    /// Hook Coroutine for  Normal Hook
    /// </summary>
    private IEnumerator HookMovement(Rigidbody rb, Vector3 dir)
    {
        float startTime = Time.time;
        float lerpSpeed = 4.5f;
        if (dir.y > 0)
        {
            lerpSpeed *= 1f;
            dir.y *= 1.2f;
        }
        dir.y += 0.1f;

        float speed = PlayerMovement._instance._HookMovementSpeed - PlayerMovement._instance._HookMovementSpeed * 0.25f * Mathf.Clamp(rb.velocity.magnitude, _MoveSpeed, _RunSpeed) / _RunSpeed;

        while (Time.time < startTime + _hookTime)
        {
            if (PlayerCombat._instance._IsDodgingOrForwardLeap) yield break;
            rb.velocity = Vector3.Lerp(rb.velocity, dir * speed, Time.deltaTime * lerpSpeed);
            yield return null;
        }
    }
    /// <summary>
    /// Hook Coroutine for Up Hook
    /// </summary>
    /// <param name="distance">distance between player and Uphook Connected Position</param>
    private IEnumerator HookMovement(Rigidbody rb, Vector3 dir, float distance = 21.5f)
    {

        float startTime = Time.time;
        float lerpSpeed = 6f;
        if (dir.y > 0)
        {
            lerpSpeed *= 1.25f;
            dir.y *= 1.1f;
        }

        float speed = PlayerMovement._instance._HookMovementSpeed * Mathf.Clamp(distance, 13f, 25f) / 21.5f;

        while (Time.time < startTime + _hookTime)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, dir * speed, Time.deltaTime * lerpSpeed);
            yield return null;
        }
    }
    public void Dodge(Rigidbody rb)
    {
        CameraController.ShakeCamera(1.5f, 1f, 0.05f, 0.1f);

        _isDodgeAllowedForAir = false;
        GameManager._instance.CallForAction(() => { _isDodgeAllowedForAir = false; }, 0.15f);

        _Stamina -= _dodgeStaminaUse;
        Vector3 tempForward = rb.transform.forward;
        tempForward.y = 0f;
        float angle = -Vector3.SignedAngle(tempForward, Vector3.forward, Vector3.up);
        Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * new Vector3(InputHandler.GetAxis("Horizontal"), 0f, InputHandler.GetAxis("Vertical")).normalized;
        rb.velocity = direction * _DodgeSpeed;

        if (InputHandler.GetAxis("Vertical") > 0)
        {
            rb.velocity *= 1.25f;
        }
    }
    public void BlockedMove(Rigidbody rb, Vector3 direction)
    {
        CameraController.ShakeCamera(2f, 1f, 0.05f, 0.1f);

        rb.velocity = direction * _AttackBlockedSpeed;

        if (!IsGrounded() && rb.velocity.y < 0) return;

        if (_attackMoveCoroutine != null)
            StopCoroutine(_attackMoveCoroutine);
        if (_attackDeflectedMoveCoroutine != null)
            StopCoroutine(_attackDeflectedMoveCoroutine);
        _attackDeflectedMoveCoroutine = StartCoroutine(AttackDeflectedMoveCoroutine(rb));
    }
    public void ForwardLeap(Rigidbody rb)
    {
        Vector3 tempForward = rb.transform.forward;
        tempForward.y = 0f;
        float angle = -Vector3.SignedAngle(tempForward, Vector3.forward, Vector3.up);
        Vector3 direction = rb.transform.forward;
        direction.y = 0f;

        StartCoroutine(ForwardLeapCoroutine(rb, direction, Time.time));
    }
    private IEnumerator ForwardLeapCoroutine(Rigidbody rb, Vector3 dir, float startTime)
    {
        float localForwardSpeed = _ForwardLeapSpeed;
        if (rb.velocity.magnitude > (dir * _ForwardLeapSpeed).magnitude)
        {
            localForwardSpeed *= 1.65f;
        }

        while (Time.time < startTime + PlayerCombat._instance._forwardLeapTime)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, dir * localForwardSpeed, Time.deltaTime * 2.5f);
            yield return null;
        }
        rb.velocity = dir * _ForwardLeapSpeed;
    }
    public void Walk(Rigidbody rb)
    {
        float lerpSpeed = rb.velocity.magnitude < _MoveSpeed? 40f : 20f;
        Movement(rb, _MoveSpeed, lerpSpeed);
    }
    public void Run(Rigidbody rb)
    {
        Vector3 speed = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 direction = rb.transform.forward;
        direction.y = 0f;
        float directionToSpeedAngle = Vector3.Angle(speed, direction);

        if (rb.velocity.magnitude < 3.5f)
            Movement(rb, _RunSpeed, 45f);
        else if (rb.velocity.magnitude < 7f)
            Movement(rb, _RunSpeed, 40f);
        else if (directionToSpeedAngle > 45f)
            Movement(rb, _RunSpeed * 0.6f, 22f);
        else if (directionToSpeedAngle > 7f)
            Movement(rb, _RunSpeed * 0.85f, 15f);
        else if (rb.velocity.magnitude < _MoveSpeed + (_RunSpeed - _MoveSpeed) / 2f)
            Movement(rb, _RunSpeed, 6f);
        else
            Movement(rb, _RunSpeed, 3f);
        _Stamina -= Time.deltaTime * _staminaDecreasePerSecond;
    }
    public void WallWalk(Rigidbody rb)
    {
        WallMovement(rb, _MoveSpeed * 1.25f, 18f);
    }
    public void WallRun(Rigidbody rb)
    {
        WallMovement(rb, _RunSpeed, 18f);
        _Stamina -= Time.deltaTime * _staminaDecreasePerSecond;
    }
    public void CrouchMovement(Rigidbody rb)
    {
        _constantForceUp.force = Vector3.zero;

        float xInput = InputHandler.GetAxis("Horizontal");
        float yInput = InputHandler.GetAxis("Vertical");

        Vector3 rightDirection = (rb.transform.right * xInput);

        float divider = Mathf.Clamp(-rb.velocity.magnitude / 3f + 6f, 1.25f, 6f);
        if (rb.velocity.magnitude < _MoveSpeed) divider = divider * 2f;
        var targetVelocity = rb.velocity.magnitude / divider * rb.transform.forward + rightDirection * rb.velocity.magnitude / divider;
        targetVelocity.y = rb.velocity.y;

        if (targetVelocity == rb.velocity)
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.deltaTime * 2.5f);
        else
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.deltaTime * 1f);

        EnemyAI.MakeArtificialSoundForPlayer(rb.position, 12f);
    }
    private void Movement(Rigidbody rb, float speed, float lerpSpeed)
    {
        _constantForceUp.force = Vector3.zero;

        float xInput = InputHandler.GetAxis("Horizontal");
        float yInput = InputHandler.GetAxis("Vertical");

        Vector3 rightDirection = (rb.transform.right * xInput);
        Vector3 forwardDirection = (rb.transform.forward * yInput);
        float rightDirDecreaseAmount = (0.3f + rb.velocity.magnitude / 15f * 0.1f);
        Vector3 direction = (forwardDirection + rightDirection).normalized - rightDirection * rightDirDecreaseAmount + (rightDirection * rightDirDecreaseAmount).magnitude * forwardDirection / 2f;

        if (yInput < 0)
        {
            direction -= forwardDirection * 0.38f;
        }


        var targetVelocity = direction * speed;
        if (xInput != 0f || yInput != 0f)
            targetVelocity.y = rb.velocity.y;
        else
            targetVelocity.y -= Time.deltaTime * 3f;
        float targetTemp = new Vector2(targetVelocity.x, targetVelocity.z).magnitude;
        float rbTemp = new Vector2(rb.velocity.x, rb.velocity.z).magnitude;

        if (rbTemp > targetTemp)
        {
            lerpSpeed *= 0.75f;
        }

        if (PlayerCombat._instance._IsBlocking)
        {
            targetVelocity /= 2f;
        }

        if (targetVelocity == rb.velocity)
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.deltaTime * lerpSpeed * 2.5f);
        else
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.deltaTime * lerpSpeed / (targetVelocity - rb.velocity).magnitude);

        if (rb.velocity.magnitude > 0.5f)
            EnemyAI.MakeArtificialSoundForPlayer(rb.position, 20f);
    }
    public Vector3 GetForwardDirectionForWall(Transform transform, float yInput)
    {
        if (PlayerMovement._instance._touchingWallColliders.Count == 0) return Vector3.zero;

        Vector2 first = new Vector2(PlayerMovement._instance._touchingWallColliders[PlayerMovement._instance._touchingWallColliders.Count - 1].transform.forward.x, PlayerMovement._instance._touchingWallColliders[PlayerMovement._instance._touchingWallColliders.Count - 1].transform.forward.z);
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

        Vector3 direction = PlayerMovement._instance._touchingWallColliders[PlayerMovement._instance._touchingWallColliders.Count - 1].transform.forward;
        Vector3 rayDirection = (PlayerMovement._instance._touchingWallColliders[PlayerMovement._instance._touchingWallColliders.Count - 1].transform.position - transform.position).normalized;
        rayDirection.y = 0f;
        Physics.Raycast(transform.position, rayDirection, out RaycastHit hit, 15f, GameManager._instance.WallLayer);
        if (hit.collider != null)
        {
            direction = Quaternion.AngleAxis(-90f, Vector3.up) * hit.normal;
        }

        return (direction * yInput * isForward);
    }
    private void WallMovement(Rigidbody rb, float speed, float lerpSpeed)
    {
        if (PlayerMovement._instance._touchingWallColliders.Count == 0) return;

        _constantForceUp.force = Vector3.up * 15f;
        GameManager._instance.CallForAction(() => { if (PlayerStateController._instance._playerState is Movement) return; _constantForceUp.force = Vector3.up * 7f; }, 0.25f);

        float yInput = InputHandler.GetAxis("Vertical");

        Vector3 forwardDirection = GetForwardDirectionForWall(transform, yInput);

        if (yInput < 0)
        {
            forwardDirection -= forwardDirection * 0.5f;
        }


        var targetVelocity = forwardDirection * speed;
        targetVelocity.y = rb.velocity.y;
        if (targetVelocity.y < -20f)
            targetVelocity.y = -20f;

        
        if (targetVelocity == rb.velocity)
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.deltaTime * lerpSpeed * 2.5f);
        else
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.deltaTime * lerpSpeed / (targetVelocity - rb.velocity).magnitude);

        EnemyAI.MakeArtificialSoundForPlayer(rb.position, 25f);
    }
    public void AirMovement(Rigidbody rb)
    {
        if (!_isJumped && rb.velocity.y > 2f)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.975f, rb.velocity.z);
        }

        float lerpSpeed = 25f;
        _constantForceUp.force = Vector3.up * -140f;

        float yInput = InputHandler.GetAxis("Vertical");

        var targetVelocity = rb.velocity;

        Vector3 tempVelocity = rb.velocity;
        tempVelocity.y = 0f;

        if (yInput > 0f && tempVelocity.magnitude < 7f)
        {
            Vector3 temp = transform.forward * 7f;
            targetVelocity += new Vector3(temp.x, 0f, temp.z) * Time.deltaTime * 1.75f;
        }
        else if (yInput == 0f)
        {
            targetVelocity -= new Vector3(rb.velocity.x, 0f, rb.velocity.z) * Time.deltaTime * _airFrictionMultiplier;
        }
        else if (yInput < 0f)
        {
            targetVelocity -= new Vector3(rb.velocity.x, 0f, rb.velocity.z) * Time.deltaTime * _airStopMultiplier;
        }

        if (targetVelocity == rb.velocity)
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.deltaTime * lerpSpeed * 2.5f);
        else
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.deltaTime * lerpSpeed * 2f);

        EnemyAI.MakeArtificialSoundForPlayer(rb.position, 12f);
    }
    public void StaminaMovement(Rigidbody rb)
    {
        Movement(rb, 2.5f, 12.5f);
    }
    public void DeathMove(Vector3 dir, float killersVelocityMagnitude)
    {
        float deathSpeed = 1f;
        PlayerStateController._instance._rb.velocity += dir * deathSpeed + dir * killersVelocityMagnitude / 5f;
        StartCoroutine(DeathMoveCoroutine(PlayerStateController._instance._rb.velocity));
    }
    private IEnumerator DeathMoveCoroutine(Vector3 vel)
    {
        while (true)
        {

            PlayerStateController._instance._rb.velocity = vel;
            yield return null;
        }
    }
}

