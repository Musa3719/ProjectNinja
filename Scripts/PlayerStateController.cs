using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PlayerState
{
    void Enter(Rigidbody rb);
    void Exit(Rigidbody rb);
    void DoState(Rigidbody rb);
    void DoStateLateUpdate(Rigidbody rb);
    void DoStateFixedUpdate(Rigidbody rb);
}


public class PlayerStateController : MonoBehaviour
{
    public static PlayerStateController _instance;
    public PlayerState _playerState { get; private set; }

    public Rigidbody _rb { get; private set; }
    public CameraController _cameraController { get; private set; }

    private float _bufferTime;

    public bool _jumpBuffer;
    public bool _crouchBuffer;

    public Coroutine _jumpCoroutine;
    public Coroutine _crouchCoroutine;
    public Coroutine _isGroundedCoroutine;
    public Coroutine _isTouchingCoroutine;


    private float _coyoteTime;

    public bool _isGroundedBuffer;
    public bool _isTouchingBuffer;
    public CameraShake.BounceShake.Params smallShake;
   
    public void EnterState(PlayerState newState)
    {
        if (_playerState != null)
            _playerState.Exit(_rb);
        _playerState = newState;
        _playerState.Enter(_rb);
    }
    private void Awake()
    {
        _instance = this;
        _bufferTime = 0.25f;
        _coyoteTime = 0.2f;
        //Debug.Log(UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.name);
        _rb = GetComponent<Rigidbody>();
        _cameraController = Camera.main.transform.parent.GetComponent<CameraController>();

        EnterState(new Movement());
    }

    void Update()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isPlayerDead) return;
        
        _playerState.DoState(_rb);
    }
    void LateUpdate()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isPlayerDead) return;

        _playerState.DoStateLateUpdate(_rb);
    }
    void FixedUpdate()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isPlayerDead) return;

        _playerState.DoStateFixedUpdate(_rb);
    }


    public IEnumerator JumpBuffer()
    {
        if (_jumpCoroutine != null)
            StopCoroutine(_jumpCoroutine);
        _jumpBuffer = true;
        yield return new WaitForSeconds(_bufferTime);
        _jumpBuffer = false;
    }
    public IEnumerator CrouchBuffer()
    {
        if (_crouchCoroutine != null)
            StopCoroutine(_crouchCoroutine);
        _crouchBuffer = true;
        yield return new WaitForSeconds(_bufferTime);
        _crouchBuffer = false;
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

    public static bool CheckForOnWall()
    {
        if(!PlayerMovement._instance.IsGrounded() && PlayerMovement._instance.IsTouching() && !PlayerMovement._instance.isJumpedFromWall)
        {
            return true;
        }
        return false;
    }
    public static void CheckForBuffers()
    {
        if (Input.GetButtonDown("Jump"))
            PlayerStateController._instance._jumpCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.JumpBuffer());
        if (Input.GetButtonDown("Crouch"))
            PlayerStateController._instance._crouchCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.CrouchBuffer());
        if (PlayerMovement._instance.IsGrounded() && !PlayerMovement._instance.isJumped)
            PlayerStateController._instance._isGroundedCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.IsGroundedCoyoteTime());
        if (PlayerMovement._instance.IsTouching())
            PlayerStateController._instance._isTouchingCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.IsTouchingCoyoteTime());
    }


    public static bool CheckForJump()
    {
        return PlayerStateController._instance._jumpBuffer && PlayerStateController._instance._isGroundedBuffer && PlayerMovement._instance._Stamina >= PlayerMovement._instance._needStaminaForJump;
    }
    public static bool CheckForCrouch()
    {
        return PlayerStateController._instance._crouchBuffer && PlayerStateController._instance._isGroundedBuffer;
    }

    public static bool CheckForWallJump()
    {
        return Input.GetButtonUp("Jump") && PlayerStateController._instance._isTouchingBuffer;
    }
    public static bool CheckForWallJumpCounter()
    {
        return Input.GetButton("Jump");
    }
    public static bool CheckForSlideWall()
    {
        return Input.GetButton("Crouch") && PlayerStateController._instance._isTouchingBuffer;
    }
    public static void CheckForDisableCrouch()
    {
        if(Input.GetButtonUp("Crouch") && PlayerStateController._instance.transform.localScale.y != PlayerMovement.normalYScale)
        {
            if (PlayerMovement._instance.CrouchCoroutine != null)
                PlayerMovement._instance.StopCoroutine(PlayerMovement._instance.CrouchCoroutine);
            PlayerStateController._instance.transform.localScale = new Vector3(PlayerStateController._instance.transform.localScale.x, PlayerMovement.normalYScale, PlayerStateController._instance.transform.localScale.z);
            PlayerMovement._instance._distToGround = 1f;
        }
    }


    public static bool IsRunning()
    {
        return Input.GetAxis("Vertical") > 0f && Input.GetKey(KeyCode.LeftShift) && PlayerMovement._instance._canRunWithStamina && !IsCrouching();
    }
    public static bool IsCrouching()
    {
        return PlayerMovement._instance._distToGround == PlayerMovement.crouchedYScale;
    }
}

public class OnWall : PlayerState
{
    private float _jumpCounter;
    public void Enter(Rigidbody rb)
    {

    }
    public void Exit(Rigidbody rb)
    {
        
    }
    public void DoState(Rigidbody rb)
    {
        PlayerStateController.CheckForBuffers();

        PlayerStateController.CheckForDisableCrouch();

        if (!PlayerStateController.CheckForOnWall())
        {
            PlayerStateController._instance.EnterState(new Movement());
        }
        else if (PlayerStateController.CheckForWallJumpCounter())
        {
            _jumpCounter += Time.deltaTime;
        }
        else if (PlayerStateController.CheckForWallJump())
        {
            PlayerStateController._instance.StopCoroutine(PlayerStateController._instance._isTouchingCoroutine);
            PlayerStateController._instance._isTouchingBuffer = false;

            PlayerMovement._instance.JumpFromWall(rb, _jumpCounter);
        }
        else if (PlayerStateController.CheckForSlideWall())
        {
            PlayerStateController._instance.StopCoroutine(PlayerStateController._instance._isTouchingCoroutine);
            PlayerStateController._instance._isTouchingBuffer = false;

            PlayerMovement._instance.SlideFromWall(rb);
        }
        else if (PlayerMovement._instance.IsTouching())
        {
            if (PlayerStateController.IsRunning())
            {
                PlayerMovement._instance.WallRun(rb);
            }
            else
            {
                rb.GetComponent<PlayerMovement>().WallWalk(rb);
            }
        }

        CameraController._instance.LookAround(false);

    }

    public void DoStateFixedUpdate(Rigidbody rb)
    {

    }

    public void DoStateLateUpdate(Rigidbody rb)
    {

    }
}

public class Movement : PlayerState
{
    public void Enter(Rigidbody rb)
    {
    }
    public void Exit(Rigidbody rb)
    {
        
    }
    public void DoState(Rigidbody rb)
    {
        PlayerStateController.CheckForBuffers();

        PlayerStateController.CheckForDisableCrouch();

        if (PlayerStateController.CheckForOnWall())
        {
            PlayerStateController._instance.EnterState(new OnWall());
        }
        /*else if (Input.GetKeyDown(KeyCode.G))
        {
            CameraShake.CameraShaker.Shake(new CameraShake.BounceShake(PlayerStateController._instance.smallShake));
        }*/
        else if (PlayerStateController.CheckForJump())
        {
            PlayerStateController._instance.StopCoroutine(PlayerStateController._instance._jumpCoroutine);
            PlayerStateController._instance._jumpBuffer = false;
            PlayerStateController._instance.StopCoroutine(PlayerStateController._instance._isGroundedCoroutine);
            PlayerStateController._instance._isGroundedBuffer = false;

            PlayerMovement._instance.Jump(rb);
        }
        
        else if (PlayerStateController.CheckForCrouch())
        {
            PlayerStateController._instance.StopCoroutine(PlayerStateController._instance._crouchCoroutine);
            PlayerStateController._instance._crouchBuffer = false;
            PlayerStateController._instance.StopCoroutine(PlayerStateController._instance._isGroundedCoroutine);
            PlayerStateController._instance._isGroundedBuffer = false;

            PlayerMovement._instance.Crouch(rb);
        }
        else if (PlayerMovement._instance.IsGrounded())
        {
            if (PlayerStateController.IsRunning())
            {
                PlayerMovement._instance.Run(rb);
            }
            else
            {
                rb.GetComponent<PlayerMovement>().Walk(rb);
            }
        }

        CameraController._instance.LookAround(true);

    }

    public void DoStateFixedUpdate(Rigidbody rb)
    {

    }

    public void DoStateLateUpdate(Rigidbody rb)
    {

    }
}
/*
public class Interact : PlayerState
{
    Interactables interactable;

    public Interact(Interactables interactable)
    {
        this.interactable = interactable;
    }

    public void Enter(Rigidbody rb)
    {
        interactable.Interact();
    }
    public void Exit(Rigidbody rb)
    {
        interactable.CloseInteract();
    }
    public void DoState(Rigidbody rb)
    {

    }

    public void DoStateFixedUpdate(Rigidbody rb)
    {

    }

    public void DoStateLateUpdate(Rigidbody rb)
    {

    }
}*/