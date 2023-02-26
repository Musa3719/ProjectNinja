using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PlayerStates
{
    public interface IPlayerState : IState
    {
        void Enter(Rigidbody rb, IPlayerState oldState);
        void Exit(Rigidbody rb, IPlayerState newState);
    }



    public class Movement : IPlayerState
    {
        public bool isCrouching { get; set; }
        private bool inAirFlag { get; set; }
        private bool fastLandToGroundAnimationTriggered { get; set; }

        public void Enter(Rigidbody rb, IPlayerState oldState)
        {
            GameManager._instance.MidScreenDot.color = GameManager._instance.MidScreenDotMovementColor;
        }
        public void Exit(Rigidbody rb, IPlayerState newState)
        {
            if (isCrouching)
            {
                Debug.LogError("exiting with isCrouching");
                PlayerStateController.DisableCrouch();
            }
        }
        
        public void DoState(Rigidbody rb)
        {
            if (PlayerStateController.CheckForManualStaminaIncrease())
            {
                PlayerMovement._instance.StaminaMovement(rb);
                CameraController._instance.LookAround(true);
                return;
            }

            PlayerStateController.CheckForBuffers();

            PlayerStateController.CheckForDisableCrouch();

            PlayerStateController.CheckForBlock();

            PlayerStateController._instance.CheckForTeleport();
            PlayerStateController._instance.CheckForInvertedMirror();

            if (PlayerCombat._instance._IsDodgingOrForwardLeap)
            {
                //do nothing
            }
            else if(PlayerCombat._instance._IsStunned)
            {
                //do nothing
            }

            else if (PlayerStateController.CheckForOnWall())
            {
                PlayerStateController._instance.EnterState(new OnWall());
            }
            else if (PlayerStateController.CheckForUpHookTrigger())
            {
                PlayerMovement._instance.ThrowUpHook(rb);
            }
            else if (PlayerStateController.CheckForHookTrigger())
            {
                PlayerMovement._instance.ThrowHook(rb);
            }
            else if (PlayerStateController.CheckForAttack())
            {
                PlayerStateController._instance.StopCoroutine(PlayerStateController._instance._attackCoroutine);
                PlayerStateController._instance._attackBuffer = false;

                PlayerCombat._instance.Attack();
                //PlayerMovement._instance.AttackMove(PlayerStateController._instance._rb);
            }
            else if (PlayerStateController.CheckForForwardLeap())
            {
                PlayerStateController._instance.StopCoroutine(PlayerStateController._instance._forwardLeapCoroutine);
                PlayerStateController._instance._forwardLeapBuffer = false;

                PlayerCombat._instance.ForwardLeap();
                PlayerMovement._instance.ForwardLeap(rb);
            }
            else if (PlayerStateController.CheckForThrow())
            {
                PlayerStateController._instance.StopCoroutine(PlayerStateController._instance._throwCoroutine);
                PlayerStateController._instance._throwBuffer = false;

                PlayerCombat._instance.ThrowKillObject(PlayerCombat._instance._CurrentThrowableItem);
            }
            else if (PlayerStateController.CheckForThrowableChange())
            {
                if(InputHandler.GetScrollForItems() > 0f)
                {
                    int newIndex = (PlayerCombat._instance._ThrowableInventory.IndexOf(PlayerCombat._instance._CurrentThrowableItem) + 1) % PlayerCombat._instance._ThrowableInventory.Count;
                    PlayerCombat._instance._CurrentThrowableItem = PlayerCombat._instance._ThrowableInventory[newIndex];
                }
                else if (InputHandler.GetScrollForItems() < 0f)
                {
                    if (PlayerCombat._instance._ThrowableInventory.IndexOf(PlayerCombat._instance._CurrentThrowableItem) == 0)
                    {
                        PlayerCombat._instance._CurrentThrowableItem = PlayerCombat._instance._ThrowableInventory[PlayerCombat._instance._ThrowableInventory.Count - 1];
                    }
                    else
                    {
                        int newIndex = (PlayerCombat._instance._ThrowableInventory.IndexOf(PlayerCombat._instance._CurrentThrowableItem) - 1) % PlayerCombat._instance._ThrowableInventory.Count;
                        PlayerCombat._instance._CurrentThrowableItem = PlayerCombat._instance._ThrowableInventory[newIndex];
                    }
                }
                
            }
            else if (PlayerStateController.CheckForDodge())
            {
                PlayerCombat._instance.Dodge();
                PlayerMovement._instance.Dodge(rb);
            }
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
                PlayerMovement._instance.Crouch(rb);
            }
            else if (PlayerMovement._instance.IsGrounded())
            {
                if(PlayerMovement._instance._lastTimeFastLanded +0.25f > Time.time && !fastLandToGroundAnimationTriggered)
                {
                    fastLandToGroundAnimationTriggered = true;
                    GameManager._instance.CallForAction(() => fastLandToGroundAnimationTriggered = false, 0.5f);
                    inAirFlag = false;
                    PlayerStateController._instance.ChangeAnimation("FastLandToGround");
                }
                else if(inAirFlag == true)
                {
                    inAirFlag = false;
                    PlayerStateController._instance.ChangeAnimation("AirToGround");
                }

                if (isCrouching)
                {
                    PlayerMovement._instance.CrouchMovement(rb);
                    CameraController._instance.LookAround(false);
                }
                else if (PlayerCombat._instance._AttackBlockedCounter > 0 || PlayerMovement._instance._isOnAttackOrAttackDeflectedMove)
                {
                    //wait
                }
                else if (PlayerStateController.IsRunning())
                {
                    PlayerMovement._instance.Run(rb);
                }
                else
                {
                    PlayerMovement._instance.Walk(rb);
                }
            }
            else//in the air
            {
                inAirFlag = true;
                if (isCrouching)
                {
                    Debug.LogError("crouch in the air deactivated");
                    PlayerStateController.DisableCrouch();
                }

                if (PlayerStateController.CheckForFastLandingInAir())
                {
                    PlayerMovement._instance.FastLandInAir(rb);
                }
                else
                {
                    PlayerMovement._instance.AirMovement(rb);
                }
            }

            if (!isCrouching)
                CameraController._instance.LookAround(true);

        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }
    public class OnWall : IPlayerState
    {
        private float firstLerpTime;
        public bool isWallOnLeftSide { get; private set; }
        
        
        public void Enter(Rigidbody rb, IPlayerState oldState)
        {
            PlayerStateController._instance._isSpinEnding = false;

            if (PlayerMovement._instance._touchingWallColliders.Count > 0)
            {
                Vector3 playerForward = PlayerMovement._instance.GetForwardDirectionForWall(rb.transform, 1f);
                Vector3 playerLeftVector = Quaternion.AngleAxis(-90f, Vector3.up) * playerForward / 10f;

                Vector3 a = PlayerMovement._instance._touchingWallColliders[PlayerMovement._instance._touchingWallColliders.Count - 1].transform.position - rb.transform.position;
                a.y = 0f;
                
                if (Vector3.Dot(a.normalized, playerLeftVector) > 0)
                {
                    isWallOnLeftSide = true;
                    PlayerStateController._instance.ChangeAnimation("LeftOnWall");
                }
                else
                {
                    isWallOnLeftSide = false;
                    PlayerStateController._instance.ChangeAnimation("RightOnWall");
                }
            }
            firstLerpTime = 0f;
            PlayerCombat._instance._IsBlocking = false;
            GameManager._instance.MidScreenDot.color = GameManager._instance.MidScreenDotOnWallColor;

            PlayerStateController._instance.BladeSpinSoundObject = SoundManager._instance.PlaySound(SoundManager._instance.BladeSpin, PlayerStateController._instance.transform.position, 0.25f, true, 1f);

            CreateSpark();
        }
        public void Exit(Rigidbody rb, IPlayerState newState)
        {
            PlayerStateController._instance.StopCoroutine(PlayerStateController._instance._isGroundedCoroutine);
            PlayerStateController._instance._isGroundedBuffer = false;

            GameObject.Destroy(PlayerStateController._instance.BladeSpinSoundObject, 2f);
            PlayerStateController._instance._isSpinEnding = true;
        }
        public void DoState(Rigidbody rb)
        {
            PlayerStateController.CheckForBuffers();

            PlayerStateController.CheckForDisableCrouch();

            PlayerStateController.CheckForBlock();

            PlayerStateController._instance.CheckForTeleport();
            PlayerStateController._instance.CheckForInvertedMirror();

            PlayerMovement._instance.ArrangeOnWallSound();

            if (PlayerStateController._instance.OnWallSpark == null)
            {
                CreateSpark();
            }
            else
            {
                Vector3 leftPos = GameManager._instance.PlayerLeftHandTransform.position + PlayerStateController._instance.transform.up * -0.3f + PlayerStateController._instance.transform.right * -0.3f;
                Vector3 rightPos = GameManager._instance.PlayerRightHandTransform.position + PlayerStateController._instance.transform.up * -0.3f + PlayerStateController._instance.transform.right * 0.3f;
                PlayerStateController._instance.OnWallSpark.transform.position = isWallOnLeftSide ? leftPos : rightPos;
            }
            
            if (!PlayerStateController.CheckForOnWall())
            {
                PlayerStateController._instance.EnterState(new Movement());
            }
            else if (PlayerStateController.CheckForUpHookTrigger())
            {
                PlayerStateController._instance.StopCoroutine(PlayerStateController._instance._upHookCoroutine);
                PlayerStateController._instance._upHookBuffer = false;

                PlayerMovement._instance.ThrowUpHook(rb);
            }
            else if (PlayerStateController.CheckForHookTrigger())
            {
                PlayerStateController._instance.StopCoroutine(PlayerStateController._instance._hookCoroutine);
                PlayerStateController._instance._hookBuffer = false;

                PlayerMovement._instance.ThrowHook(rb);
            }
            else if (PlayerStateController.CheckForAttack())
            {
                PlayerStateController._instance.StopCoroutine(PlayerStateController._instance._attackCoroutine);
                PlayerStateController._instance._attackBuffer = false;

                PlayerCombat._instance.Attack();
            }
            else if (PlayerStateController.CheckForThrow())
            {
                PlayerStateController._instance.StopCoroutine(PlayerStateController._instance._throwCoroutine);
                PlayerStateController._instance._throwBuffer = false;

                PlayerCombat._instance.ThrowKillObject(PlayerCombat._instance._CurrentThrowableItem);
            }
            else if (PlayerStateController.CheckForThrowableChange())
            {
                if (InputHandler.GetScrollForItems() > 0f)
                {
                    int newIndex = (PlayerCombat._instance._ThrowableInventory.IndexOf(PlayerCombat._instance._CurrentThrowableItem) + 1) % PlayerCombat._instance._ThrowableInventory.Count;
                    PlayerCombat._instance._CurrentThrowableItem = PlayerCombat._instance._ThrowableInventory[newIndex];
                }
                else if (InputHandler.GetScrollForItems() < 0f)
                {
                    if (PlayerCombat._instance._ThrowableInventory.IndexOf(PlayerCombat._instance._CurrentThrowableItem) == 0)
                    {
                        PlayerCombat._instance._CurrentThrowableItem = PlayerCombat._instance._ThrowableInventory[PlayerCombat._instance._ThrowableInventory.Count - 1];
                    }
                    else
                    {
                        int newIndex = (PlayerCombat._instance._ThrowableInventory.IndexOf(PlayerCombat._instance._CurrentThrowableItem) - 1) % PlayerCombat._instance._ThrowableInventory.Count;
                        PlayerCombat._instance._CurrentThrowableItem = PlayerCombat._instance._ThrowableInventory[newIndex];
                    }
                }

            }
            else if (PlayerStateController.CheckForWallJump())
            {
                PlayerStateController._instance.StopCoroutine(PlayerStateController._instance._jumpCoroutine);
                PlayerStateController._instance._jumpBuffer = false;
                PlayerStateController._instance.StopCoroutine(PlayerStateController._instance._isTouchingCoroutine);
                PlayerStateController._instance._isTouchingBuffer = false;

                PlayerMovement._instance.JumpFromWall(rb);
            }
            else if (firstLerpTime > 0f)
            {
                float lerpSpeed = 8f;
                Vector3 targetDirection = (PlayerMovement._instance._touchingWallColliders[PlayerMovement._instance._touchingWallColliders.Count - 1].transform.forward);
                Vector3 targetVelocity = targetDirection * rb.velocity.magnitude * 7f / 10f;
                rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.deltaTime * lerpSpeed / (targetVelocity - rb.velocity).magnitude);
                firstLerpTime -= Time.deltaTime;
            }
            else if (PlayerMovement._instance.IsTouching())
            {
                if (PlayerStateController.IsRunning())
                {
                    PlayerMovement._instance.WallRun(rb);
                }
                else
                {
                    PlayerMovement._instance.WallWalk(rb);
                }
            }

            if (PlayerStateController.CheckForSlideWall())
            {
                PlayerMovement._instance.SlideFromWall(rb);
            }

            CameraController._instance.LookAround(false);

        }
        private void CreateSpark()
        {
            if (PlayerMovement._instance._touchingWallColliders.Count == 0) return;
            Collider wallCollider = PlayerMovement._instance._touchingWallColliders[PlayerMovement._instance._touchingWallColliders.Count - 1];

            Vector3 tempCameraDirection = CameraController._instance.transform.right;
            tempCameraDirection.y = 0f;

            Vector3 tempWallDirection = wallCollider.transform.right;
            tempWallDirection.y = 0f;
            float angle = Vector3.SignedAngle(tempWallDirection, tempCameraDirection, Vector3.up);
            if (isWallOnLeftSide)
            {
                if (angle > 20f) return;
            }
            else
            {
                if (angle > 0f && angle < 160f) return;
            }

            //Vector3 pos = isWallOnLeftSide ? GameManager._instance.PlayerLeftHandTransform.position + PlayerStateController._instance.transform.right / 2f : GameManager._instance.PlayerRightHandTransform.position - PlayerStateController._instance.transform.right / 2f;
            Vector3 pos = isWallOnLeftSide ? GameManager._instance.PlayerLeftHandTransform.position : GameManager._instance.PlayerRightHandTransform.position;
            pos += isWallOnLeftSide ? PlayerStateController._instance.transform.right * 0.75f : PlayerStateController._instance.transform.right * -0.75f;
            PlayerStateController._instance.OnWallSpark = GameObject.Instantiate(GameManager._instance.SparksVFX[0], pos, Quaternion.identity);
            PlayerStateController._instance.OnWallSpark.GetComponentInChildren<Animator>().speed *= 4.5f;
            PlayerStateController._instance.OnWallSpark.transform.localScale *= 0.4f;
            GameObject.Destroy(PlayerStateController._instance.OnWallSpark, 0.14f);

            GameObject.Destroy(SoundManager._instance.PlaySound(SoundManager._instance.WeaponHitSounds[0], PlayerStateController._instance.transform.position, Random.Range(0.2f, 0.3f), false, Random.Range(0.78f, 1.02f)), 2f);

            /*GameObject decal = GameObject.Instantiate(GameManager._instance.HoleDecal, pos, Quaternion.identity);
            decal.transform.forward = wallCollider.transform.right;
            decal.transform.localEulerAngles = new Vector3(decal.transform.localEulerAngles.x, decal.transform.localEulerAngles.y, UnityEngine.Random.Range(0f, 360f));
            */
            pos = isWallOnLeftSide ? GameManager._instance.PlayerLeftHandTransform.position : GameManager._instance.PlayerRightHandTransform.position;
            //pos += isWallOnLeftSide ? PlayerStateController._instance.transform.right * 0.25f : -PlayerStateController._instance.transform.right * 0.25f;
            GameObject hitSmoke = GameObject.Instantiate(GameManager._instance.HitSmokeVFX, pos, Quaternion.identity);
            hitSmoke.transform.localScale *= 1.1f;
            hitSmoke.GetComponentInChildren<Animator>().speed = 4f;
            Color temp = hitSmoke.GetComponentInChildren<SpriteRenderer>().color;
            hitSmoke.GetComponentInChildren<SpriteRenderer>().color = new Color(temp.r, temp.g, temp.b, 25f / 255f);
            GameObject.Destroy(hitSmoke, 1f);
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
}
