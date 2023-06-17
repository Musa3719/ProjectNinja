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
    public bool _forwardLeapBuffer { get; set; }
    public bool _throwBuffer { get; set; }
    public bool _hookBuffer { get; set; }
    public bool _upHookBuffer { get; set; }
    public bool _isGroundedBuffer { get; set; }
    public bool _isTouchingBuffer { get; set; }


    public Coroutine _jumpCoroutine;
    public Coroutine _attackCoroutine;
    public Coroutine _forwardLeapCoroutine;
    public Coroutine _throwCoroutine;
    public Coroutine _hookCoroutine;
    public Coroutine _upHookCoroutine;
    public Coroutine _isGroundedCoroutine;
    public Coroutine _isTouchingCoroutine;


    private float _jumpBufferTime;
    private float _attackBufferTime;
    private float _forwardLeapBufferTime;
    private float _throwBufferTime;
    private float _hookBufferTime;
    private float _coyoteTime;

    private GameObject _CurrentTeleportSpot;
    public GameObject _hookAnchor;

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
        //_animator.SetFloat("LocalX", localX);
        //_animator.SetFloat("LocalZ", localZ);
    }
    public void ChangeAnimation(string name, float fadeTime = 0.2f)
    {
        _Animator.CrossFadeInFixedTime(name, fadeTime);

        /*if (name.Equals("Jump"))
        {
            PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Jump());
        }*/
    }
    private void ArrangeAnimStateParameter()
    {
        _Animator.SetBool("IsMoving", _rb.velocity.magnitude >= 1f);
        _Animator.SetBool("IsBlocking", PlayerCombat._instance._IsBlocking);
        _Animator.SetBool("IsBlockedOrDeflected", PlayerCombat._instance._IsBlockedOrDeflected);
        _Animator.SetBool("IsStaminaReload", _isStaminaManual);
        _Animator.SetFloat("RunAnimSpeedMultiplier", _rb.velocity.magnitude / PlayerMovement._instance._MoveSpeed * 0.75f);

        if (_playerAnimState is PlayerAnimations.InAir && !_Animator.IsInTransition(1) && !PlayerCombat._instance._IsBlocking)
        {
            if (PlayerMovement._instance._lastTimeFastLanded + 0.1f >= Time.time && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("FastLand"))
            {
                ChangeAnimation("FastLand");
            }
            else if(PlayerMovement._instance._lastTimeFastLanded + 0.1f < Time.time && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("InAir"))
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
                else if(PlayerMovement._instance._lastTimeOnWallFastLanded + 0.1f < Time.time && _Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWallFastLand"))
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
        else if((_playerAnimState is PlayerAnimations.Idle || _playerAnimState is PlayerAnimations.Walk || _playerAnimState is PlayerAnimations.Run) && !_isStaminaManual && !PlayerCombat._instance._IsBlocking && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("BlockingReverse") && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("Blocking"))
        {
            if (_playerAnimState is PlayerAnimations.Idle && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("Idle") && !_Animator.IsInTransition(1))
            {
                ChangeAnimation("Idle", 0.4f);
            }
            else if (_playerAnimState is PlayerAnimations.Walk && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("Walk") && (!_Animator.IsInTransition(1) || _Animator.GetNextAnimatorStateInfo(1).IsName("Idle")))
            {
                ChangeAnimation("Walk");
            }
            else if (_playerAnimState is PlayerAnimations.Run && !_Animator.GetCurrentAnimatorStateInfo(1).IsName("Run") && (!_Animator.IsInTransition(1) || _Animator.GetNextAnimatorStateInfo(1).IsName("Idle")))
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
        _hookBufferTime = 0.2f;
        _attackBufferTime = 0.45f;
        _forwardLeapBufferTime = 0.4f;
        _throwBufferTime = 0.35f;
        _coyoteTime = 0.2f;
        //Debug.Log(UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.name);
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
        ArrangeAnimStateParameter();
        CheckForCanDoWallMovement();
        ArrangeIsInSmoke();

        UIArrangements();
        ArrangeAttackBlockedCounter();
        CheckForLoopSounds();
    }
    private void CheckForCanDoWallMovement()
    {
        if (InputHandler.GetButtonDown("LeaveWall"))
        {
            PlayerMovement._instance._canDoWallMovement = !PlayerMovement._instance._canDoWallMovement;
            GameManager._instance.CanDoWallMovementUI.SetActive(!GameManager._instance.CanDoWallMovementUI.activeInHierarchy);
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
            GameManager._instance._isPlayerInSmoke = false;
        else
            GameManager._instance._isPlayerInSmoke = true;
    }
    private void ArrangeAttackBlockedCounter()
    {
        if (PlayerCombat._instance._AttackBlockedCounter > 0f)
            PlayerCombat._instance._AttackBlockedCounter -= Time.deltaTime;
    }
    private void UIArrangements()
    {
        bool isHookAvailable = PlayerMovement._instance._isHookAllowed && PlayerMovement._instance._isHookAllowedForAir;

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

        GameManager._instance.ArrangeUI(PlayerMovement._instance._Stamina, _playerState.ToString(), isHookAvailable, PlayerCombat._instance._CurrentThrowableItem, next, before);
    }
    void LateUpdate()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isOnCutscene || GameManager._instance.isPlayerDead) return;

        _playerState.DoStateLateUpdate(_rb);
        _playerAnimState.DoStateLateUpdate(_rb);

        _cameraController.ArrangeFOV(50f + Mathf.Clamp(_rb.velocity.magnitude, PlayerMovement._instance._MoveSpeed, 16f) * 1f);
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
    public IEnumerator ForwardLeapBuffer()
    {
        if (_forwardLeapCoroutine != null)
            StopCoroutine(_forwardLeapCoroutine);
        _forwardLeapBuffer = true;
        yield return new WaitForSeconds(_forwardLeapBufferTime);
        _forwardLeapBuffer = false;
    }
    public IEnumerator ThrowBuffer()
    {
        if (_throwCoroutine != null)
            StopCoroutine(_throwCoroutine);
        _throwBuffer = true;
        yield return new WaitForSeconds(_throwBufferTime);
        _throwBuffer = false;
    }
    public IEnumerator HookBuffer()
    {
        if (_hookCoroutine != null)
            StopCoroutine(_hookCoroutine);
        _hookBuffer = true;
        yield return new WaitForSeconds(_hookBufferTime);
        _hookBuffer = false;
    }
    public IEnumerator UpHookBuffer()
    {
        if (_upHookCoroutine != null)
            StopCoroutine(_upHookCoroutine);
        _upHookBuffer = true;
        yield return new WaitForSeconds(_hookBufferTime);
        _upHookBuffer = false;
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
        if (InputHandler.GetButton("Block") && PlayerCombat._instance._isAllowedToBlock && !PlayerCombat._instance._IsStunned && !PlayerCombat._instance._IsAttacking && PlayerMovement._instance._Stamina >= PlayerMovement._instance._blockedStaminaUse)
        {
            if (!PlayerCombat._instance._IsBlocking)
                SoundManager._instance.PlaySound(SoundManager._instance.BladeSlide, PlayerStateController._instance.transform.position, 0.07f, false, UnityEngine.Random.Range(0.75f, 0.85f));
            PlayerCombat._instance._IsBlocking = true;
        }
        else
        {
            if (PlayerCombat._instance._IsBlocking)
                SoundManager._instance.PlaySound(SoundManager._instance.BladeSlide, PlayerStateController._instance.transform.position, 0.07f, false, UnityEngine.Random.Range(0.75f, 0.85f));
            PlayerCombat._instance._IsBlocking = false;
        }

    }
    public static bool CheckForDodge()
    {
        if (InputHandler.GetButtonDown("Dodge") && PlayerCombat._instance._isAllowedToDodge && PlayerMovement._instance._isDodgeAllowedForAir && !PlayerCombat._instance._IsDodgingOrForwardLeap && PlayerMovement._instance._Stamina >= PlayerMovement._instance._dodgeStaminaUse)
        {
            return true;
        }
        return false;

    }
    public static bool CheckForOnWall()
    {
        bool onWall = !PlayerMovement._instance.IsGrounded() && PlayerMovement._instance.IsTouching() && !PlayerMovement._instance._isJumped && PlayerMovement._instance._canDoWallMovement && !PlayerCombat._instance._IsBlocking && PlayerMovement._instance._lastExitWallTime + 0.35f < Time.time;
        return onWall;
    }
    public static bool CheckForHookTrigger()
    {
        if (!PlayerStateController._instance._hookBuffer) return false;

        RaycastHit hit = PlayerMovement._instance.RaycastForHook();
        if (hit.collider == null)
        {
            if (PlayerMovement._instance._lastHookNotReadyTime + 0.5f < Time.time)
            {
                SoundManager._instance.PlaySound(SoundManager._instance.HookNotReady, PlayerMovement._instance.transform.position, 0.15f, false, UnityEngine.Random.Range(0.93f, 1.07f));
                PlayerMovement._instance._lastHookNotReadyTime = Time.time;
            }
            return false;
        }
        bool isRayHit = hit.collider.gameObject.layer == LayerMask.NameToLayer("Grounds") || hit.collider.CompareTag("Wall");

        if ((hit.point - PlayerStateController._instance.transform.position).magnitude < 3f)
        {
            if (PlayerMovement._instance._lastHookNotReadyTime + 0.5f < Time.time)
            {
                SoundManager._instance.PlaySound(SoundManager._instance.HookNotReady, PlayerMovement._instance.transform.position, 0.15f, false, UnityEngine.Random.Range(0.93f, 1.07f));
                PlayerMovement._instance._lastHookNotReadyTime = Time.time;
            }
            return false;
        }

        if (!(PlayerMovement._instance._isHookAllowed && PlayerMovement._instance._isHookAllowedForAir && isRayHit && PlayerMovement._instance._Stamina >= PlayerMovement._instance._hookStaminaUse) && PlayerMovement._instance._lastHookTime + 0.5 < Time.time)
        {
            if (PlayerMovement._instance._lastHookNotReadyTime + 0.5f < Time.time)
            {
                SoundManager._instance.PlaySound(SoundManager._instance.HookNotReady, PlayerMovement._instance.transform.position, 0.15f, false, UnityEngine.Random.Range(0.93f, 1.07f));
                PlayerMovement._instance._lastHookNotReadyTime = Time.time;
            }
        }

        return PlayerMovement._instance._isHookAllowed && PlayerMovement._instance._isHookAllowedForAir && isRayHit && PlayerMovement._instance._Stamina >= PlayerMovement._instance._hookStaminaUse;
    }
    public static bool CheckForUpHookTrigger()
    {
        if (!PlayerStateController._instance._upHookBuffer) return false;

        RaycastHit hit = PlayerMovement._instance.RaycastForUpHook();
        if (hit.collider == null)
        {
            if (PlayerMovement._instance._lastHookNotReadyTime + 0.5f < Time.time)
            {
                SoundManager._instance.PlaySound(SoundManager._instance.HookNotReady, PlayerMovement._instance.transform.position, 0.15f, false, UnityEngine.Random.Range(0.93f, 1.07f));
                PlayerMovement._instance._lastHookNotReadyTime = Time.time;
            }
            return false;
        }
        bool isRayHit = hit.collider.gameObject.layer == LayerMask.NameToLayer("Grounds") || hit.collider.CompareTag("Wall");

        if (!(PlayerMovement._instance._isHookAllowed && PlayerMovement._instance._isHookAllowedForAir && isRayHit && PlayerMovement._instance._Stamina >= PlayerMovement._instance._hookStaminaUse) && PlayerMovement._instance._lastHookTime + 0.5 < Time.time)
        {
            if (PlayerMovement._instance._lastHookNotReadyTime + 0.5f < Time.time)
            {
                SoundManager._instance.PlaySound(SoundManager._instance.HookNotReady, PlayerMovement._instance.transform.position, 0.15f, false, UnityEngine.Random.Range(0.93f, 1.07f));
                PlayerMovement._instance._lastHookNotReadyTime = Time.time;
            }
        }

        return PlayerMovement._instance._isHookAllowed && PlayerMovement._instance._isHookAllowedForAir && isRayHit && PlayerMovement._instance._Stamina >= PlayerMovement._instance._hookStaminaUse;
    }


    public static void CheckForBuffers()
    {
        //if (Input.GetButtonDown("Fire1") && Input.GetKey(KeyCode.LeftShift))
        //else ifPlayerStateController._instance._forwardLeapCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.ForwardLeapBuffer());
        if (InputHandler.GetButtonDown("Fire1") && !InputHandler.GetButton("Block"))
            PlayerStateController._instance._attackCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.AttackBuffer());

        if (InputHandler.GetButtonDown("Throw"))
            PlayerStateController._instance._throwCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.ThrowBuffer());
        if (InputHandler.GetButtonDown("Jump"))
            PlayerStateController._instance._jumpCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.JumpBuffer());
        if (InputHandler.GetButtonDown("Hook"))
            PlayerStateController._instance._hookCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.HookBuffer());
        if (InputHandler.GetButtonDown("UpHook"))
            PlayerStateController._instance._upHookCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.UpHookBuffer());
        if (PlayerMovement._instance.IsGrounded() && !PlayerMovement._instance._isJumped)
            PlayerStateController._instance._isGroundedCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.IsGroundedCoyoteTime());
        if (PlayerMovement._instance.IsTouching())
            PlayerStateController._instance._isTouchingCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.IsTouchingCoyoteTime());
    }
    public static bool CheckForAttack()
    {
        return PlayerStateController._instance._attackBuffer && PlayerCombat._instance._isAllowedToAttack && !PlayerCombat._instance._IsDodgingOrForwardLeap && PlayerMovement._instance._Stamina >= PlayerMovement._instance._attackStaminaUse;
    }
    public static bool CheckForForwardLeap()
    {
        return PlayerStateController._instance._forwardLeapBuffer && PlayerCombat._instance._isAllowedToForwardLeap && PlayerMovement._instance.IsGrounded() && !PlayerCombat._instance._IsDodgingOrForwardLeap && PlayerMovement._instance._Stamina >= PlayerMovement._instance._forwardLeapStaminaUse;
    }
    public static bool CheckForThrow()
    {
        return PlayerStateController._instance._throwBuffer && PlayerCombat._instance._isAllowedToThrow && !PlayerCombat._instance._IsDodgingOrForwardLeap && PlayerCombat._instance._CurrentThrowableItem != null;
    }
    public static bool CheckForThrowableChange()
    {
        return InputHandler.GetScrollForItems() != 0f && PlayerCombat._instance._ThrowableInventory.Count > 1;
    }
    public void CheckForTeleport()
    {
        if (!GameManager._instance.isTeleportSkillOpen) return;

        if (InputHandler.GetButtonDown("Teleport") && isCreateTeleportAvailable)
        {
            ChangeAnimation("SkillUse");
            EnterAnimState(new PlayerAnimations.WaitForOneAnim(0.4f));
            GameManager._instance.TeleportSkillUIGetter.GetComponent<SkillUI>().StartCountdown();
            _CurrentTeleportSpot = Instantiate(GameManager._instance.TeleportSpotPrefab, _rb.transform.position, Quaternion.identity);
            _CurrentTeleportSpot.transform.Find("Hologram").transform.localEulerAngles = new Vector3(_CurrentTeleportSpot.transform.Find("Hologram").transform.localEulerAngles.x, _rb.transform.localEulerAngles.y, _CurrentTeleportSpot.transform.Find("Hologram").transform.localEulerAngles.z);
            _CurrentTeleportSpot.transform.Find("RotationInfo").transform.rotation = GameManager._instance.MainCamera.transform.parent.transform.rotation;
            GameManager._instance.ArrangeSkillUI(GameManager._instance.TeleportSkillUIGetter, GameManager._instance._InUseWaitingColor);
            isCreateTeleportAvailable = false;

            Action OpenIsUseTeleportAvailable = () => {
                isUseTeleportAvailable = true;
                GameManager._instance.ArrangeSkillUI(GameManager._instance.TeleportSkillUIGetter, GameManager._instance._InUseColor);
            };
            GameManager._instance.CallForAction(OpenIsUseTeleportAvailable, GameManager._instance.TeleportAvailableTimeAfterHologram);
        }
        else if (InputHandler.GetButtonDown("Teleport") && isUseTeleportAvailable)
        {
            if (_CurrentTeleportSpot == null) return;

            ChangeAnimation("SkillUse");
            EnterAnimState(new PlayerAnimations.WaitForOneAnim(0.4f));
            GameManager._instance.ArrangeSkillUI(GameManager._instance.TeleportSkillUIGetter, GameManager._instance._NotAvailableColor);
            isUseTeleportAvailable = false;
            _rb.transform.position = _CurrentTeleportSpot.transform.position;
            GameManager._instance.MainCamera.transform.parent.transform.rotation = _CurrentTeleportSpot.transform.Find("RotationInfo").transform.rotation;
            _rb.transform.localEulerAngles = new Vector3(_rb.transform.localEulerAngles.x, GameManager._instance.MainCamera.transform.parent.transform.localEulerAngles.y, _rb.transform.localEulerAngles.z);
            Vector3 speedWithoutY = _rb.velocity;
            speedWithoutY.y = 0f;
            float yVelocity = _rb.velocity.y;
            _rb.velocity = speedWithoutY.magnitude * _rb.transform.forward + Vector3.up * yVelocity;
            Destroy(_CurrentTeleportSpot);

            Action OpenIsCreateTeleportAvailable = () => {
                isCreateTeleportAvailable = true;
                GameManager._instance.ArrangeSkillUI(GameManager._instance.TeleportSkillUIGetter, GameManager._instance._AvailableColor);
            };
            GameManager._instance.CallForAction(OpenIsCreateTeleportAvailable, 10f);
        }
    }
    
    public void CheckForInvertedMirror()
    {
        if (!GameManager._instance.isInvertedMirrorSkillOpen) return;

        if (InputHandler.GetButtonDown("InvertedMirror") && isInvertedMirrorAvailable)
        {
            RaycastHit hit;
            if (Physics.Raycast(_rb.transform.position, GameManager._instance.MainCamera.transform.parent.transform.forward, out hit, 100f, GameManager._instance.MirrorRayLayer))
            {
                if (hit.normal.y != 0f || !hit.collider.CompareTag("Wall")) return;
                GameManager._instance.CallForAction(() => InvertedMirrorActivate(hit), 0.35f);
                ChangeAnimation(PlayerCombat._instance.GetThrowName());
            }
        }
    }
    private void InvertedMirrorActivate(RaycastHit hit)
    {
        GameManager._instance.InvertedMirrorSkillUIGetter.GetComponent<SkillUI>().StartCountdown();
        Destroy(Instantiate(GameManager._instance.InvertedMirrorPrefab, hit.point + hit.normal * 0.25f, Quaternion.LookRotation(hit.normal)), GameManager._instance.InvertedMirrorFunctionalTime);
        GameManager._instance.ArrangeSkillUI(GameManager._instance.InvertedMirrorSkillUIGetter, GameManager._instance._NotAvailableColor);
        isInvertedMirrorAvailable = false;

        Action OpenisInvertedMirrorAvailable = () => {
            GameManager._instance.ArrangeSkillUI(GameManager._instance.InvertedMirrorSkillUIGetter, GameManager._instance._AvailableColor);
            isInvertedMirrorAvailable = true;
        };
        GameManager._instance.CallForAction(OpenisInvertedMirrorAvailable, 10f);
    }

    public void CheckForIce()
    {
        if (!GameManager._instance.isIceSkillOpen) return;

        if (InputHandler.GetButtonDown("IceSkill") && isIceSkillAvailable)
        {
            GameManager._instance.CallForAction(() => IceSkillActivate(), 0.2f);
            ChangeAnimation(PlayerCombat._instance.GetThrowName());
        }
    }
    private void IceSkillActivate()
    {
        //throw
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
        if (InputHandler.GetButton("Stamina") && PlayerMovement._instance._Stamina < PlayerMovement._instance._MaxStamina && PlayerMovement._instance.IsGrounded() && !PlayerCombat._instance._IsBlocking && !PlayerCombat._instance._IsDodgingOrForwardLeap && !PlayerCombat._instance._IsAttacking)
        {
            if(!PlayerStateController._instance._isStaminaManual)
                PlayerStateController._instance.StaminaRegenSoundObject = SoundManager._instance.PlaySound(SoundManager._instance.StaminaReload, PlayerStateController._instance.transform.position, 0.3f, true, UnityEngine.Random.Range(0.93f, 1.07f));
            PlayerStateController._instance._isStaminaManual = true;
            PlayerStateController._instance._staminaManualCounter += Time.deltaTime;
            return true;
        }
        if (PlayerStateController._instance._isStaminaManual && PlayerStateController._instance.StaminaRegenSoundObject != null)
            Destroy(PlayerStateController._instance.StaminaRegenSoundObject);
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
        return InputHandler.GetButton("Crouch") && !PlayerMovement._instance._isJumped && PlayerMovement._instance.IsGrounded() && PlayerStateController._instance._rb.velocity.magnitude > PlayerMovement._instance._MoveSpeed - 1.5f;
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
        return InputHandler.GetAxis("Vertical") > 0f && InputHandler.GetButton("Run") && !PlayerCombat._instance._IsAttacking && !PlayerCombat._instance._IsBlocking && PlayerMovement._instance._canRunWithStamina && IsRunningStateCheck();
    }
    private static bool IsRunningStateCheck()
    {
        return (PlayerStateController._instance._playerState is PlayerStates.OnWall && PlayerMovement._instance._isAllowedToWallRun) || (PlayerStateController._instance._playerState is PlayerStates.Movement && !(PlayerStateController._instance._playerState as PlayerStates.Movement)._isCrouching);
    }
}
