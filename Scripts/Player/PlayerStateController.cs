using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAnimations;
using PlayerStates;
using System;

public class PlayerStateController : MonoBehaviour
{
    public static PlayerStateController _instance;
    public IPlayerState _playerState { get; private set; }
    public IPlayerAnimState _playerAnimState { get; private set; }

    public Rigidbody _rb { get; private set; }
    public CameraController _cameraController { get; private set; }
    public LineRenderer _lineRenderer { get; private set; }

    [SerializeField]
    private Animator _animator;
    public Animator _Animator => _animator;

    public bool _isStaminaManual { get; private set; }
    public float _staminaManualCounter { get; private set; }


    public bool _jumpBuffer { get; set; }
    public bool _attackBuffer { get; set; }
    public bool _throwBuffer { get; set; }
    public bool _isGroundedBuffer { get; set; }
    public bool _isTouchingBuffer { get; set; }


    public Coroutine _jumpCoroutine;
    public Coroutine _attackCoroutine;
    public Coroutine _throwCoroutine;
    public Coroutine _isGroundedCoroutine;
    public Coroutine _isTouchingCoroutine;


    private float _jumpBufferTime;
    private float _attackBufferTime;
    private float _throwBufferTime;
    private float _coyoteTime;

    private GameObject _CurrentTeleportSpot;

    private bool isCreateTeleportAvailable;
    private bool isUseTeleportAvailable;

    private bool isInvertedMirrorAvailable;
    private bool isIceSkillAvailable;

    public GameObject SlidingSoundObject;
    public GameObject BladeSpinSoundObject;
    public GameObject StaminaRegenSoundObject;

    public GameObject OnWallSpark;

    private List<GameObject> SmokeTriggers;

    public bool _isSpinEnding { get; set; }

    public void EnterState(IPlayerState newState)
    {
        if (_playerState != null)
            _playerState.Exit(_rb, newState);
        IPlayerState oldState = _playerState;
        _playerState = newState;
        _playerState.Enter(_rb, oldState);
    }

    public void EnterAnimState(IPlayerAnimState newState)
    {
        if (_playerAnimState != null)
            _playerAnimState.Exit(_rb, newState);
        IPlayerAnimState oldState = _playerAnimState;
        _playerAnimState = newState;
        _playerAnimState.Enter(_rb, oldState);
    }

    public void BlendAnimationLocalPositions(float localX, float localZ)
    {
        _animator.SetFloat("LocalX", localX);
        _animator.SetFloat("LocalZ", localZ);
    }
    public void ChangeAnimation(string name, float fadeTime = 0.2f, bool checkForNameChange = true)
    {
        if (checkForNameChange && PlayerCombat._instance.MeleeWeapon != null)
        {
            if (name == "EmptyRight" || name == "EmptyLeft" || name == "Sliding" || name == "Born" || name == "FastLand") return;
            if (name == "Dodge" || name == "Stun" || name == "HitBreakable") name = PlayerCombat._instance.GetBlockedName();
            if (name != "ThrowWeaponStart" && name != "ThrowWeapon" && name != "TakeWeaponStart" && name != "TakeWeapon")
            {
                name += "Weapon";
            }
        }
        
        _Animator.CrossFadeInFixedTime(name, fadeTime);
    }
    private void ArrangeAnimStateParameter()
    {
        _Animator.SetBool("HasMeleeWeapon", PlayerCombat._instance.MeleeWeapon != null);
        _Animator.SetBool("IsMoving", _rb.velocity.magnitude >= 1f);
        _Animator.SetBool("IsBlocking", PlayerCombat._instance._IsBlocking);
        _Animator.SetBool("IsBlockedOrDeflected", PlayerCombat._instance._IsBlockedOrDeflected);
        _Animator.SetBool("IsStaminaReload", _isStaminaManual);
        _Animator.SetBool("IsAnimNameBlocking", _Animator.GetCurrentAnimatorStateInfo(1).IsName("Blocking"));
        _Animator.SetFloat("RunAnimSpeedMultiplier", _rb.velocity.magnitude / PlayerMovement._instance._MoveSpeed * 0.75f);

        if (_playerAnimState is PlayerAnimations.InAir && !_Animator.IsInTransition(1) && !PlayerCombat._instance._IsBlocking)
        {
            if (PlayerMovement._instance._lastTimeFastLanded + 0.1f >= Time.time && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("FastLand"))
            {
                ChangeAnimation("FastLand");
            }
            else if (PlayerMovement._instance._lastTimeFastLanded + 0.1f < Time.time && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("InAir") && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("InAirWeapon"))
            {
                ChangeAnimation("InAir");
            }
        }
        else if (_playerAnimState is PlayerAnimations.Sliding && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("Sliding") && !_Animator.IsInTransition(1) && !PlayerCombat._instance._IsBlocking)
        {
            ChangeAnimation("Sliding");
        }
        else if (_playerAnimState is PlayerAnimations.OnWall)
        {
            if ((_playerAnimState as PlayerAnimations.OnWall).isWallOnTheLeftSide && !_Animator.IsInTransition(2))
            {
                if (PlayerMovement._instance._lastTimeOnWallFastLanded + 0.1f >= Time.time && _Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWall"))
                    ChangeAnimation("LeftOnWallFastLand");
                else if (PlayerMovement._instance._lastTimeOnWallFastLanded + 0.1f < Time.time && _Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWallFastLand"))
                    ChangeAnimation("LeftOnWall");
            }
            else if (!(_playerAnimState as PlayerAnimations.OnWall).isWallOnTheLeftSide && !_Animator.IsInTransition(3))
            {
                if (PlayerMovement._instance._lastTimeOnWallFastLanded + 0.1f >= Time.time && _Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWall"))
                    ChangeAnimation("RightOnWallFastLand");
                else if (PlayerMovement._instance._lastTimeOnWallFastLanded + 0.1f < Time.time && _Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWallFastLand"))
                    ChangeAnimation("RightOnWall");
            }
        }
        else if ((_playerAnimState is PlayerAnimations.Idle || _playerAnimState is PlayerAnimations.Walk || _playerAnimState is PlayerAnimations.Run) && !_isStaminaManual && !PlayerCombat._instance._IsBlocking && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("BlockingReverse") && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("Blocking") && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("BlockingStart") && PlayerMovement._instance.IsGrounded() && !PlayerMovement._instance._isJumped)
        {
            if (_playerAnimState is PlayerAnimations.Idle && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("Idle") && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("IdleWeapon") && !_Animator.IsInTransition(1))
            {
                ChangeAnimation("Idle");
            }
            else if (_playerAnimState is PlayerAnimations.Walk && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("Walk") && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("WalkWeapon") && (!_Animator.IsInTransition(1) || _Animator.GetNextAnimatorStateInfo(1).IsName("Idle") || _Animator.GetNextAnimatorStateInfo(1).IsName("IdleWeapon")))
            {
                ChangeAnimation("Walk");
            }
            else if (_playerAnimState is PlayerAnimations.Run && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("Run") && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("RunWeapon") && (!_Animator.IsInTransition(1) || _Animator.GetNextAnimatorStateInfo(1).IsName("Idle") || _Animator.GetNextAnimatorStateInfo(1).IsName("IdleWeapon")))
            {
                ChangeAnimation("Run");
            }
        }
    }
    private void Awake()
    {
        _instance = this;
        isCreateTeleportAvailable = true;
        isUseTeleportAvailable = false;
        isInvertedMirrorAvailable = true;
        SmokeTriggers = new List<GameObject>();
        _jumpBufferTime = 0.3f;
        _attackBufferTime = 0.45f;
        _throwBufferTime = 0.35f;
        _coyoteTime = 0.2f;
        _rb = GetComponent<Rigidbody>();
        _cameraController = Camera.main.transform.parent.parent.GetComponent<CameraController>();
        _lineRenderer = GetComponentInChildren<LineRenderer>();

        EnterState(new PlayerStates.Movement());
        EnterAnimState(new PlayerAnimations.Idle());

    }
    void Update()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isOnCutscene || GameManager._instance.isPlayerDead) return;
        _playerState.DoState(_rb);
        _playerAnimState.DoState(_rb);
        ArrangeAttackColliderYPosition();
        ArrangeAnimStateParameter();
        CheckForCanDoWallMovement();
        ArrangeIsInSmoke();

        UIArrangements();
        ArrangeAttackBlockedCounter();
        CheckForLoopSounds();
        CheckForPlayerFallsFromMap();
            
    }
    private void ArrangeAttackColliderYPosition()
    {
        if (GameManager._instance.MainCamera.transform.eulerAngles.x < 180f)
        {
            Vector3 pos = PlayerCombat._instance.AttackCollider.transform.localPosition;
            PlayerCombat._instance.AttackCollider.transform.localPosition = new Vector3(pos.x, 0.3f, 0.6f);
        }
        else
        {
            Vector3 pos = PlayerCombat._instance.AttackCollider.transform.localPosition;
            PlayerCombat._instance.AttackCollider.transform.localPosition = new Vector3(pos.x, 0.7f, 0.7f);
        }
    }
    private void CheckForPlayerFallsFromMap()
    {
        if (transform.position.y < -25f || transform.position.y > 100)
        {
            GameManager._instance.Die();
        }
    }
    private void CheckForCanDoWallMovement()
    {
        if (InputHandler.GetButtonDown("LeaveWall"))
        {
            PlayerMovement._instance._canDoWallMovement = !PlayerMovement._instance._canDoWallMovement;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.gameObject != null)
        {
            if (other.CompareTag("Smoke") && !SmokeTriggers.Contains(other.gameObject))
            {
                SmokeTriggers.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != null && other.gameObject != null)
        {
            if (other.CompareTag("Smoke") && SmokeTriggers.Contains(other.gameObject))
            {
                SmokeTriggers.Remove(other.gameObject);
            }
        }
    }
    private void ArrangeIsInSmoke()
    {
        for (int i = 0; i < SmokeTriggers.Count; i++)
        {
            if (SmokeTriggers[i] == null)
            {
                SmokeTriggers.Remove(SmokeTriggers[i]);
                i--;
            }
        }

        if (SmokeTriggers.Count == 0)
            GameManager._instance.IsPlayerInSmoke = false;
        else
            GameManager._instance.IsPlayerInSmoke = true;
    }
    private void ArrangeAttackBlockedCounter()
    {
        if (PlayerCombat._instance._AttackBlockedCounter > 0f)
            PlayerCombat._instance._AttackBlockedCounter -= Time.deltaTime;
    }
    private void UIArrangements()
    {
        IThrowableItem next;
        IThrowableItem before;
        if (PlayerCombat._instance._ThrowableInventory.Count == 0)
        {
            next = null;
            before = null;
        }
        else
        {
            next = PlayerCombat._instance._ThrowableInventory[(PlayerCombat._instance._ThrowableInventory.IndexOf(PlayerCombat._instance._CurrentThrowableItem) + 1) % PlayerCombat._instance._ThrowableInventory.Count];
            if (PlayerCombat._instance._ThrowableInventory.IndexOf(PlayerCombat._instance._CurrentThrowableItem) == 0)
            {
                before = PlayerCombat._instance._ThrowableInventory[PlayerCombat._instance._ThrowableInventory.Count - 1];
            }
            else
            {
                before = PlayerCombat._instance._ThrowableInventory[(PlayerCombat._instance._ThrowableInventory.IndexOf(PlayerCombat._instance._CurrentThrowableItem) - 1) % PlayerCombat._instance._ThrowableInventory.Count];
            }
        }

        GameManager._instance.ArrangeUI(PlayerMovement._instance._Stamina, PlayerMovement._instance._MaxStamina, false, PlayerCombat._instance._CurrentThrowableItem, next, before);
    }
    void LateUpdate()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isOnCutscene || GameManager._instance.isPlayerDead) return;

        _playerState.DoStateLateUpdate(_rb);
        _playerAnimState.DoStateLateUpdate(_rb);

        float speedFovAdditionMultiplier = 3.35f;
        Vector3 velWithoutY = _rb.velocity;
        velWithoutY.y = 0f;
        _cameraController.ArrangeFOV(Options._instance.FOV - (speedFovAdditionMultiplier * PlayerMovement._instance._MoveSpeed) + Mathf.Clamp(velWithoutY.magnitude, PlayerMovement._instance._MoveSpeed, 16f) * speedFovAdditionMultiplier);
    }
    void FixedUpdate()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isOnCutscene || GameManager._instance.isPlayerDead) return;

        _playerState.DoStateFixedUpdate(_rb);
        _playerAnimState.DoStateFixedUpdate(_rb);
    }


    public IEnumerator JumpBuffer()
    {
        if (_jumpCoroutine != null)
            StopCoroutine(_jumpCoroutine);
        _jumpBuffer = true;
        yield return new WaitForSeconds(_jumpBufferTime);
        _jumpBuffer = false;
    }


    public IEnumerator AttackBuffer()
    {
        if (_attackCoroutine != null)
            StopCoroutine(_attackCoroutine);
        _attackBuffer = true;
        yield return new WaitForSeconds(_attackBufferTime);
        _attackBuffer = false;
    }
    
    public IEnumerator ThrowBuffer()
    {
        if (_throwCoroutine != null)
            StopCoroutine(_throwCoroutine);
        _throwBuffer = true;
        yield return new WaitForSeconds(_throwBufferTime);
        _throwBuffer = false;
    }
    public IEnumerator IsGroundedCoyoteTime()
    {
        if (_isGroundedCoroutine != null)
            StopCoroutine(_isGroundedCoroutine);
        _isGroundedBuffer = true;
        yield return new WaitForSeconds(_coyoteTime);
        _isGroundedBuffer = false;
    }
    public IEnumerator IsTouchingCoyoteTime()
    {
        if (_isTouchingCoroutine != null)
            StopCoroutine(_isTouchingCoroutine);
        _isTouchingBuffer = true;
        yield return new WaitForSeconds(_coyoteTime);
        _isTouchingBuffer = false;
    }

    public static void CheckForBlock()
    {
        if (InputHandler.GetButton("Block") && !IsUsingSpear() && !PlayerCombat._instance.IsDodgingGetter && PlayerCombat._instance._isAllowedToBlock && !PlayerCombat._instance._IsStunned && !PlayerCombat._instance._IsAttacking && PlayerMovement._instance._Stamina >= PlayerMovement._instance._blockedStaminaUse && !(PlayerStateController._instance._playerAnimState is PlayerAnimations.Wait))
        {
            if (!PlayerCombat._instance._IsBlocking)
            {
                SoundManager._instance.PlaySound(SoundManager._instance.BladeSlide, PlayerStateController._instance.transform.position, 0.035f, false, UnityEngine.Random.Range(0.75f, 0.85f));
                SoundManager._instance.PlaySound(SoundManager._instance.GetAttackSoundForPlayer(PlayerCombat._instance.MeleeWeapon), PlayerStateController._instance.transform.position, 0.05f, false, UnityEngine.Random.Range(1.35f, 1.5f));
            }
            PlayerCombat._instance._IsBlocking = true;
        }
        else
        {
            if (PlayerCombat._instance._IsBlocking)
            {
                SoundManager._instance.PlaySound(SoundManager._instance.BladeSlide, PlayerStateController._instance.transform.position, 0.035f, false, UnityEngine.Random.Range(0.75f, 0.85f));
                SoundManager._instance.PlaySound(SoundManager._instance.GetAttackSoundForPlayer(PlayerCombat._instance.MeleeWeapon), PlayerStateController._instance.transform.position, 0.05f, false, UnityEngine.Random.Range(1.35f, 1.5f));
            }
            PlayerCombat._instance._IsBlocking = false;
        }

    }
    public static bool CheckForDodge()
    {
        if (InputHandler.GetButtonDown("Dodge") && !PlayerCombat._instance.IsBlockingGetter && !IsRunning() && PlayerCombat._instance._isAllowedToDodge && PlayerMovement._instance._isDodgeAllowedForAir && !PlayerCombat._instance._IsDodging && PlayerMovement._instance._Stamina >= PlayerMovement._instance._dodgeStaminaUse && !(PlayerStateController._instance._playerAnimState is PlayerAnimations.Wait))
        {
            return true;
        }
        return false;

    }
    public static bool CheckForOnWall()
    {
        if (PlayerMovement._instance._touchingWallColliders.Count == 0) return false;
        if (PlayerStateController._instance._playerAnimState is PlayerAnimations.Wait) return false;
        if (PlayerCombat._instance.MeleeWeapon != null) return false;

        
        bool _isWallOnLeftSide = false;
        Vector3 playerForward = PlayerMovement._instance.GetForwardDirectionForWall(PlayerStateController._instance._rb.transform, 1f);
        Vector3 playerLeftVector = Quaternion.AngleAxis(-90f, Vector3.up) * playerForward / 10f;
        Vector3 a = PlayerMovement._instance._touchingWallColliders[PlayerMovement._instance._touchingWallColliders.Count - 1].transform.position - PlayerStateController._instance._rb.transform.position;
        a.y = 0f;
        if (Vector3.Dot(a.normalized, playerLeftVector) > 0)
            _isWallOnLeftSide = true;
        else
            _isWallOnLeftSide = false;

        float angleForLookingAtWall = Vector3.Angle(-PlayerMovement._instance._touchingWallColliders[PlayerMovement._instance._touchingWallColliders.Count - 1].transform.right, PlayerStateController._instance.transform.forward);
        if (!(PlayerStateController._instance._playerState is PlayerStates.OnWall) && angleForLookingAtWall < 32f) return false;

        bool isPressingHorizontalForLeave = _isWallOnLeftSide ? InputHandler.GetAxis("Horizontal") > 0f : InputHandler.GetAxis("Horizontal") < 0f;
        bool onWall = !PlayerMovement._instance.IsGrounded() && PlayerMovement._instance.IsTouchingAnyWall() && !PlayerMovement._instance._isJumped && PlayerMovement._instance._canDoWallMovement && !PlayerCombat._instance._IsBlocking && PlayerMovement._instance._lastExitWallTime + 0.35f < Time.time;
        return onWall && !isPressingHorizontalForLeave;
    }

    public static void CheckForBuffers()
    {
        if (InputHandler.GetButtonDown("Fire1") && !InputHandler.GetButton("Block"))
            PlayerStateController._instance._attackCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.AttackBuffer());

        if (InputHandler.GetButtonDown("Throw"))
            PlayerStateController._instance._throwCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.ThrowBuffer());
        if (InputHandler.GetButtonDown("Jump"))
            PlayerStateController._instance._jumpCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.JumpBuffer());
        if (PlayerMovement._instance.IsGrounded() && !PlayerMovement._instance._isJumped)
            PlayerStateController._instance._isGroundedCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.IsGroundedCoyoteTime());
        if (PlayerMovement._instance.IsTouchingAnyWall())
            PlayerStateController._instance._isTouchingCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.IsTouchingCoyoteTime());
    }
    public static void CheckForWeaponChange()
    {
        if (InputHandler.GetButtonDown("WeaponChange") && !PlayerCombat._instance._IsAttacking && !PlayerCombat._instance._IsDodging && !PlayerCombat._instance.IsBlockingGetter)
        {
            if (PlayerCombat._instance.MeleeWeapon != null)
            {
                PlayerCombat._instance.DropWeaponStart();
            }
            else
            {
                PlayerCombat._instance.TakeWeapon();
            }
        }
    }
    public static bool CheckForAttack()
    {
        return !PlayerCombat._instance._IsAttacking && !IsUsingSpear() && PlayerStateController._instance._attackBuffer && PlayerCombat._instance._isAllowedToAttack && PlayerMovement._instance._Stamina >= PlayerMovement._instance._attackStaminaUse && !(PlayerStateController._instance._playerAnimState is PlayerAnimations.Wait);
    }
    public static bool CheckForDashAttack()
    {
        return !PlayerCombat._instance._IsAttacking  && !IsUsingSpear() && InputHandler.GetButtonDown("MiddleMouse") && PlayerCombat._instance._isAllowedToAttack && PlayerMovement._instance._Stamina >= PlayerMovement._instance._dashAttackStaminaUse && !(PlayerStateController._instance._playerAnimState is PlayerAnimations.Wait);
    }
    public static bool CheckForThrow()
    {
        return PlayerStateController._instance._throwBuffer && PlayerCombat._instance.MeleeWeapon == null && PlayerCombat._instance._isAllowedToThrow && !PlayerCombat._instance._IsDodging && PlayerCombat._instance._CurrentThrowableItem != null && !(PlayerStateController._instance._playerAnimState is PlayerAnimations.Wait);
    }
    public static bool CheckForThrowableChange()
    {
        return InputHandler.GetScrollForItems() != 0f && PlayerCombat._instance._ThrowableInventory.Count > 1;
    }
    
    public void CheckForLoopSounds()
    {

        if (PlayerStateController._instance.SlidingSoundObject != null)
        {
            PlayerStateController._instance.SlidingSoundObject.GetComponent<AudioSource>().volume -= Time.deltaTime * PlayerStateController._instance.SlidingSoundObject.GetComponent<AudioSource>().volume;
            PlayerStateController._instance.SlidingSoundObject.transform.position = transform.position;
        }
        if (PlayerStateController._instance.BladeSpinSoundObject != null)
        {
            if (_isSpinEnding)
            {
                AudioSource spinSource = PlayerStateController._instance.BladeSpinSoundObject.GetComponent<AudioSource>();
                spinSource.pitch = Mathf.Lerp(spinSource.pitch, 0f, Time.deltaTime * 6f);
                PlayerStateController._instance.BladeSpinSoundObject.transform.position = transform.position;
            }
            else
            {
                float bladeSpeed = GameManager._instance.BladeSpeed;
                AudioSource spinSource = PlayerStateController._instance.BladeSpinSoundObject.GetComponent<AudioSource>();
                spinSource.pitch = Mathf.Clamp(bladeSpeed / 2300f, 0.55f, 1.15f);
                PlayerStateController._instance.BladeSpinSoundObject.transform.position = transform.position;
            }
        }
        if (PlayerStateController._instance.StaminaRegenSoundObject != null)
        {
            PlayerStateController._instance.StaminaRegenSoundObject.transform.position = transform.position;
        }

        if ((!(_playerState is Movement) || !(_playerState as Movement)._isCrouching || _rb.velocity.magnitude < 1.5f) && PlayerStateController._instance.SlidingSoundObject != null)
            Destroy(PlayerStateController._instance.SlidingSoundObject);
    }

    public static bool CheckForManualStaminaIncrease()
    {
        if (InputHandler.GetButton("Stamina") && !IsUsingSpear() && PlayerMovement._instance._Stamina < PlayerMovement._instance._MaxStamina && PlayerMovement._instance.IsGrounded() && !PlayerCombat._instance._IsBlocking && !PlayerCombat._instance._IsDodging && !PlayerCombat._instance._IsAttacking && !(PlayerStateController._instance._playerAnimState is PlayerAnimations.Wait))
        {
            if (!PlayerStateController._instance._isStaminaManual)
            {
                PlayerStateController._instance.StaminaRegenSoundObject = SoundManager._instance.PlaySound(SoundManager._instance.StaminaReload, PlayerStateController._instance.transform.position, 0.3f, true, UnityEngine.Random.Range(0.93f, 1.07f));
            }
            PlayerStateController._instance._isStaminaManual = true;
            PlayerStateController._instance._staminaManualCounter += Time.deltaTime;
            return true;
        }
        if (PlayerStateController._instance._isStaminaManual && PlayerStateController._instance.StaminaRegenSoundObject != null)
        {
            Destroy(PlayerStateController._instance.StaminaRegenSoundObject);
        }
        PlayerStateController._instance._isStaminaManual = false;
        PlayerStateController._instance._staminaManualCounter = 0f;
        return false;
    }
    public static bool CheckForJump()
    {
        return PlayerStateController._instance._jumpBuffer && PlayerStateController._instance._isGroundedBuffer && PlayerMovement._instance._Stamina >= PlayerMovement._instance._needStaminaForJump;
    }
    public static bool CheckForWallJump()
    {
        return PlayerStateController._instance._jumpBuffer && PlayerStateController._instance._isTouchingBuffer && PlayerMovement._instance._Stamina >= PlayerMovement._instance._needStaminaForJump;
    }
    public static bool CheckForCrouch()
    {
        if ((PlayerStateController._instance._playerState as Movement)._isCrouching) return false;
        return InputHandler.GetButton("Crouch") && PlayerCombat._instance.MeleeWeapon == null && !PlayerMovement._instance._isJumped && PlayerMovement._instance.IsGrounded() && PlayerStateController._instance._rb.velocity.magnitude > PlayerMovement._instance._MoveSpeed - 1.5f;
    }
    public static bool CheckForSlideWall()
    {
        return InputHandler.GetButton("Crouch");
    }
    public static bool CheckForFastLandingInAir()
    {
        return InputHandler.GetButton("Crouch");
    }
    public static void CheckForDisableCrouch()
    {
        if (!(PlayerStateController._instance._playerState is PlayerStates.Movement)) return;
        if (!((PlayerStateController._instance._playerState as PlayerStates.Movement)._isCrouching)) return;

        if (PlayerCombat._instance.MeleeWeapon != null)
        {
            DisableCrouch();
            return;
        }

        Vector3 frontVector = PlayerStateController._instance._rb.transform.forward;
        frontVector.y = 0f;
        frontVector = frontVector.normalized;

        bool isThereColliderOnTheTop = Physics.Raycast(PlayerStateController._instance._rb.position, Vector3.up, 1.75f);
        bool isThereColliderOnTheTopFromBack = Physics.Raycast(PlayerStateController._instance._rb.position - frontVector, Vector3.up, 1.75f);
        bool isThereColliderOnTheTopFromFront = Physics.Raycast(PlayerStateController._instance._rb.position + frontVector, Vector3.up, 1.75f);
        if (!InputHandler.GetButton("Crouch") && !isThereColliderOnTheTop && !isThereColliderOnTheTopFromBack && !isThereColliderOnTheTopFromFront)
        {
            DisableCrouch();
        }
    }
    public static void DisableCrouch()
    {
        if (PlayerMovement._instance._crouchCoroutine != null)
            PlayerMovement._instance.StopCoroutine(PlayerMovement._instance._crouchCoroutine);
        PlayerStateController._instance.transform.localScale = new Vector3(PlayerStateController._instance.transform.localScale.x, PlayerMovement.normalYScale, PlayerStateController._instance.transform.localScale.z);
        PlayerMovement._instance._distToGround = 1f;

        var moveState = PlayerStateController._instance._playerState as PlayerStates.Movement;
        if (moveState != null)
        {
            moveState._isCrouching = false;
        }
    }

    public static bool IsRunning()
    {
        return InputHandler.GetAxis("Vertical") > 0f && InputHandler.GetButton("Run") && !PlayerCombat._instance._isTakeOrDropWeapon && !PlayerCombat._instance._IsAttacking && !PlayerCombat._instance._IsBlocking && PlayerMovement._instance._canRunWithStamina && IsRunningStateCheck();
    }
    private static bool IsRunningStateCheck()
    {
        return (PlayerStateController._instance._playerState is PlayerStates.OnWall && PlayerMovement._instance._isAllowedToWallRun) || (PlayerStateController._instance._playerState is PlayerStates.Movement && !(PlayerStateController._instance._playerState as PlayerStates.Movement)._isCrouching);
    }
    private static bool IsUsingSpear()
    {
        if (PlayerCombat._instance.MeleeWeapon != null && PlayerCombat._instance.MeleeWeapon.GetComponent<MeleeWeaponForPlayer>().WeaponType == MeleeWeaponType.Spear)
            return true;
        return false;
    }
}
