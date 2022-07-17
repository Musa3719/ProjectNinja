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
    public PlayerState playerState { get; private set; }

    public Rigidbody _rb { get; private set; }

    private float bufferTime;

    public bool jumpBuffer;
    public bool crouchBuffer;

    public Coroutine jumpCoroutine;
    public Coroutine crouchCoroutine;

    private void EnterState(PlayerState newState)
    {
        if (playerState != null)
            playerState.Exit(_rb);
        playerState = newState;
        playerState.Enter(_rb);
    }
    private void Awake()
    {
        _instance = this;
        bufferTime = 0.3f;
        //Debug.Log(UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.name);
        _rb = GetComponent<Rigidbody>();

        EnterState(new Movement());
    }

    void Update()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isPlayerDead) return;

        playerState.DoState(_rb);
    }
    void LateUpdate()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isPlayerDead) return;

        playerState.DoStateLateUpdate(_rb);
    }
    void FixedUpdate()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isPlayerDead) return;

        playerState.DoStateFixedUpdate(_rb);
    }
    public IEnumerator JumpBuffer()
    {
        if (jumpCoroutine != null)
            StopCoroutine(jumpCoroutine);
        jumpBuffer = true;
        yield return new WaitForSeconds(bufferTime);
        jumpBuffer = false;
    }
    public IEnumerator CrouchBuffer()
    {
        if (crouchCoroutine != null)
            StopCoroutine(crouchCoroutine);
        crouchBuffer = true;
        yield return new WaitForSeconds(bufferTime);
        crouchBuffer = false;
    }
    public static void CheckForBuffers()
    {
        if (Input.GetButtonDown("Jump"))
            PlayerStateController._instance.jumpCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.JumpBuffer());
        if (Input.GetButtonDown("Crouch"))
            PlayerStateController._instance.crouchCoroutine = PlayerStateController._instance.StartCoroutine(PlayerStateController._instance.CrouchBuffer());
    }
    public static bool CheckForJump()
    {
        return PlayerStateController._instance.jumpBuffer && PlayerMovement._instance.IsGrounded() && PlayerMovement._instance._Stamina >= PlayerMovement._instance._needStaminaForJump;
    }
    public static bool CheckForCrouch()
    {
        return PlayerStateController._instance.crouchBuffer && PlayerMovement._instance.IsGrounded();
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
        return Input.GetKey(KeyCode.LeftShift) && PlayerMovement._instance._canRunWithStamina && PlayerMovement._instance._distToGround != PlayerMovement.crouchedYScale;
    }
}

public class Movement : PlayerState
{
    public void Enter(Rigidbody rb)
    {
    }
    public void Exit(Rigidbody rb)
    {
        rb.velocity = Vector3.zero;
    }
    public void DoState(Rigidbody rb)
    {
        PlayerStateController.CheckForBuffers();

        PlayerStateController.CheckForDisableCrouch();

        if (PlayerStateController.CheckForJump())
        {
            PlayerStateController._instance.StopCoroutine(PlayerStateController._instance.jumpCoroutine);
            PlayerStateController._instance.jumpBuffer = false;

            PlayerMovement._instance.Jump(rb);
        }
        else if (PlayerStateController.CheckForCrouch())
        {
            PlayerStateController._instance.StopCoroutine(PlayerStateController._instance.crouchCoroutine);
            PlayerStateController._instance.crouchBuffer = false;

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
        
        CameraController._instance.LookAround();

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