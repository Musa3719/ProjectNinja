using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerStates;
namespace PlayerAnimations
{
    public interface IPlayerAnimState : IState
    {
        void Enter(Rigidbody rb, IPlayerAnimState oldState);
        void Exit(Rigidbody rb, IPlayerAnimState newState);
    }

    public class Idle : IPlayerAnimState
    {
        public void Enter(Rigidbody rb, IPlayerAnimState oldState)
        {
           
        }

        public void Exit(Rigidbody rb, IPlayerAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            if (PlayerStateController._instance._playerState is PlayerStates.Movement && (PlayerStateController._instance._playerState as PlayerStates.Movement)._isCrouching)
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Sliding());
            }
            else if ((!PlayerMovement._instance.IsGrounded(1.5f) && !PlayerCombat._instance._IsBlocking) || (PlayerMovement._instance._isJumped && !PlayerCombat._instance._IsBlocking))
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.InAir());
            }
            else if (rb.velocity.magnitude > 1f)
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Walk());
            }
            else if (PlayerStateController._instance._playerState is PlayerStates.OnWall)
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.OnWall());
            }
            else
            {
                PlayerStateController._instance.BlendAnimationLocalPositions(0f, 0f);
            }
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {
            
        }

        public void DoStateLateUpdate(Rigidbody rb)
        {
            if (!PlayerStateController._instance._Animator.IsInTransition(2) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyLeft");
            }
            if (!PlayerStateController._instance._Animator.IsInTransition(3) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyRight");
            }
        }
    }
    
    public class Walk : IPlayerAnimState
    {
        private float lastVelocityValueFrom;
        public void Enter(Rigidbody rb, IPlayerAnimState oldState)
        {
           
        }

        public void Exit(Rigidbody rb, IPlayerAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            if (PlayerStateController._instance._playerState is PlayerStates.Movement && (PlayerStateController._instance._playerState as PlayerStates.Movement)._isCrouching)
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Sliding());
            }
            else if ((!PlayerMovement._instance.IsGrounded(1.5f) && !PlayerCombat._instance._IsBlocking) || (PlayerMovement._instance._isJumped && !PlayerCombat._instance._IsBlocking))
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.InAir());
            }
            else if (lastVelocityValueFrom <= 0.25f)
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Idle());
            }
            else if (lastVelocityValueFrom > PlayerMovement._instance._MoveSpeed + 1.5f)
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Run());
            }
            else if (PlayerStateController._instance._playerState is PlayerStates.OnWall)
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.OnWall());
            }
            else
            {
                Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);
                localVelocity = localVelocity / PlayerMovement._instance._MoveSpeed / 2f * 0.5f / 0.33f;
                localVelocity = new Vector3(localVelocity.x, localVelocity.y, Mathf.Clamp(localVelocity.z, -0.3f, 0.5f));
                PlayerStateController._instance.BlendAnimationLocalPositions(localVelocity.x, localVelocity.z);
            }
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {
            lastVelocityValueFrom = rb.velocity.magnitude;
        }

        public void DoStateLateUpdate(Rigidbody rb)
        {
            if (!PlayerStateController._instance._Animator.IsInTransition(2) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyLeft");
            }
            if (!PlayerStateController._instance._Animator.IsInTransition(3) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyRight");
            }
        }
    }
    


    public class Run : IPlayerAnimState
    {
        private float lastVelocityValueFrom;
        public void Enter(Rigidbody rb, IPlayerAnimState oldState)
        {
            
        }

        public void Exit(Rigidbody rb, IPlayerAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            if (PlayerStateController._instance._playerState is PlayerStates.Movement && (PlayerStateController._instance._playerState as PlayerStates.Movement)._isCrouching)
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Sliding());
            }
            else if ((!PlayerMovement._instance.IsGrounded(1.5f) && !PlayerCombat._instance._IsBlocking) || (PlayerMovement._instance._isJumped && !PlayerCombat._instance._IsBlocking))
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.InAir());
            }
            else if (lastVelocityValueFrom <= PlayerMovement._instance._MoveSpeed)
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Walk());
            }
            else if (PlayerStateController._instance._playerState is PlayerStates.OnWall)
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.OnWall());
            }
            else
            {
                Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);
                localVelocity = localVelocity / PlayerMovement._instance._RunSpeed;
                localVelocity = new Vector3(localVelocity.x, localVelocity.y, Mathf.Clamp(localVelocity.z, 0.5f, 1f));
                PlayerStateController._instance.BlendAnimationLocalPositions(localVelocity.x, localVelocity.z);
            }
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {
            lastVelocityValueFrom = rb.velocity.magnitude;
        }

        public void DoStateLateUpdate(Rigidbody rb)
        {
            if (!PlayerStateController._instance._Animator.IsInTransition(2) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyLeft");
            }
            if (!PlayerStateController._instance._Animator.IsInTransition(3) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyRight");
            }
        }
    }
    public class InAir : IPlayerAnimState
    {
        public void Enter(Rigidbody rb, IPlayerAnimState oldState)
        {
            
        }

        public void Exit(Rigidbody rb, IPlayerAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            if (PlayerStateController._instance._playerState is PlayerStates.Movement && (PlayerStateController._instance._playerState as PlayerStates.Movement)._isCrouching)
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Sliding());
            }
            else if (PlayerMovement._instance.IsGrounded())
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.ToGround(0.15f));
            }
            else if (PlayerStateController._instance._playerState is PlayerStates.OnWall)
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.OnWall());
            }
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {
            if (!PlayerStateController._instance._Animator.IsInTransition(2) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyLeft");
            }
            if (!PlayerStateController._instance._Animator.IsInTransition(3) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyRight");
            }
        }
    }
    public class WaitForOneAnim : IPlayerAnimState
    {
        private float _waitTime;
        public WaitForOneAnim(float waitTime)
        {
            _waitTime = waitTime;
        }

        public void Enter(Rigidbody rb, IPlayerAnimState oldState)
        {
            
        }

        public void Exit(Rigidbody rb, IPlayerAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            _waitTime -= Time.deltaTime;
            if (_waitTime <= 0)
            {
                if (PlayerCombat._instance._IsBlocking)
                {
                    //
                }
                else if (PlayerMovement._instance.IsGrounded())
                {
                    PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Idle());
                }
                else
                {
                    PlayerStateController._instance.EnterAnimState(new PlayerAnimations.InAir());
                }
            }
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {
            if (!PlayerStateController._instance._Animator.IsInTransition(2) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyLeft");
            }
            if (!PlayerStateController._instance._Animator.IsInTransition(3) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyRight");
            }
        }
    }
    public class Hook : IPlayerAnimState
    {
        public void Enter(Rigidbody rb, IPlayerAnimState oldState)
        {

        }

        public void Exit(Rigidbody rb, IPlayerAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            if (!PlayerStateController._instance._lineRenderer.enabled)
            {
                if (PlayerMovement._instance.IsGrounded())
                {
                    PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Idle());
                }
                else
                {
                    PlayerStateController._instance.EnterAnimState(new PlayerAnimations.InAir());
                }
            }
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {
            if (!PlayerStateController._instance._Animator.IsInTransition(2) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyLeft");
            }
            if (!PlayerStateController._instance._Animator.IsInTransition(3) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyRight");
            }
        }
    }


    public class ToGround : IPlayerAnimState
    {
        private float _toGroundTime;
        public ToGround(float toGroundTime)
        {
            _toGroundTime = toGroundTime;
        }

        public void Enter(Rigidbody rb, IPlayerAnimState oldState)
        {
            if (PlayerMovement._instance._touchingGroundColliders.Count != 0 && PlayerMovement._instance._touchingGroundColliders[PlayerMovement._instance._touchingGroundColliders.Count - 1].GetComponent<PlaneSound>() != null)
            {
                SoundManager._instance.PlayPlaneOrWallSound(PlayerMovement._instance._touchingGroundColliders[PlayerMovement._instance._touchingGroundColliders.Count - 1].GetComponent<PlaneSound>().PlaneSoundType, 1f, 0.5f, 2f);
                SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.ArmorWalkSounds), PlayerMovement._instance.transform.position, 0.07f, false, UnityEngine.Random.Range(0.93f, 1.07f));
            }

            if (PlayerMovement._instance._lastTimeFastLanded + 0.5f < Time.time)
                PlayerStateController._instance.ChangeAnimation("AirToGround", 0.075f);
            else
                PlayerStateController._instance.ChangeAnimation("FastLandToGround", 0.075f);
        }

        public void Exit(Rigidbody rb, IPlayerAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            _toGroundTime -= Time.deltaTime;
            if (_toGroundTime <= 0)
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Idle());
            }
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {
            if (!PlayerStateController._instance._Animator.IsInTransition(2) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyLeft");
            }
            if (!PlayerStateController._instance._Animator.IsInTransition(3) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyRight");
            }
        }
    }
    public class Sliding : IPlayerAnimState
    {
        public void Enter(Rigidbody rb, IPlayerAnimState oldState)
        {
            PlayerStateController._instance.ChangeAnimation("Sliding");
        }

        public void Exit(Rigidbody rb, IPlayerAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            if (!(PlayerStateController._instance._playerState is PlayerStates.Movement) || !(PlayerStateController._instance._playerState as PlayerStates.Movement)._isCrouching)
            {
                PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Walk());
            }

        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {
            if (!PlayerStateController._instance._Animator.IsInTransition(2) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyLeft");
            }
            if (!PlayerStateController._instance._Animator.IsInTransition(3) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyRight");
            }
        }
    }
    public class OnWall : IPlayerAnimState
    {
        public bool isWallOnTheLeftSide { get; private set; }

        public void Enter(Rigidbody rb, IPlayerAnimState oldState)
        {
            if(PlayerStateController._instance._playerState is PlayerStates.OnWall)
            {
                isWallOnTheLeftSide = (PlayerStateController._instance._playerState as PlayerStates.OnWall)._isWallOnLeftSide;
            }
        }

        public void Exit(Rigidbody rb, IPlayerAnimState newState)
        {
            if (!PlayerStateController._instance._Animator.IsInTransition(2) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(2).IsName("LeftOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyLeft");
            }
            if (!PlayerStateController._instance._Animator.IsInTransition(3) && (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWall") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(3).IsName("RightOnWallFastLand")))
            {
                PlayerStateController._instance.ChangeAnimation("EmptyRight");
            }
        }

        public void DoState(Rigidbody rb)
        {
            if (!(PlayerStateController._instance._playerState is PlayerStates.OnWall))
            {
                if (PlayerMovement._instance.IsGrounded())
                {
                    PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Walk());
                }
                else
                {
                    PlayerStateController._instance.EnterAnimState(new PlayerAnimations.InAir());
                }
            }
            
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }
}
