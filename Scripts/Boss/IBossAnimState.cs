using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BossAnimations
{
    public interface IBossAnimState : IState
    {
        BossStateController _bossStateController { get; set; }
        void Enter(Rigidbody rb, IBossAnimState oldState);
        void Exit(Rigidbody rb, IBossAnimState newState);
    }


    public class Idle : IBossAnimState
    {
        public BossStateController _bossStateController { get; set; }
        public void Enter(Rigidbody rb, IBossAnimState oldState)
        {
            _bossStateController = rb.GetComponent<BossStateController>();
        }

        public void Exit(Rigidbody rb, IBossAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            if (!_bossStateController._bossMovement.IsGrounded())
            {
                _bossStateController.EnterAnimState(new BossAnimations.InAir());
            }
            else if (_bossStateController._agent.velocity.magnitude > 0.25f)
            {
                _bossStateController.EnterAnimState(new BossAnimations.Walk());
            }
            else
            {
                if (_bossStateController._agent.enabled)
                {
                    _bossStateController.BlendAnimationLocalPositions(0f, 0f);
                }
                else
                {
                    Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);
                    localVelocity = localVelocity / _bossStateController._bossMovement._moveSpeed * 2f;
                    localVelocity = new Vector3(Mathf.Clamp(localVelocity.x * 3f, -1f, 1f), localVelocity.y, Mathf.Clamp(localVelocity.z, 0f, 0.85f));
                    _bossStateController.BlendAnimationLocalPositions(localVelocity.x, localVelocity.z);
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

    public class Walk : IBossAnimState
    {

        public BossStateController _bossStateController { get; set; }
        public void Enter(Rigidbody rb, IBossAnimState oldState)
        {
            _bossStateController = rb.GetComponent<BossStateController>();
        }

        public void Exit(Rigidbody rb, IBossAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            if (!_bossStateController._bossMovement.IsGrounded())
            {
                _bossStateController.EnterAnimState(new BossAnimations.InAir());
            }
            else if (_bossStateController._agent.velocity.magnitude <= 0.25f)
            {
                _bossStateController.EnterAnimState(new BossAnimations.Idle());
            }
            else if (_bossStateController._agent.velocity.magnitude > _bossStateController._bossMovement._moveSpeed)
            {
                _bossStateController.EnterAnimState(new BossAnimations.Run());
            }
            else
            {
                Vector3 localVelocity = rb.transform.InverseTransformDirection(_bossStateController._agent.velocity);
                localVelocity = localVelocity / _bossStateController._bossMovement._moveSpeed / 2f * 0.5f / 0.33f;
                localVelocity = new Vector3(localVelocity.x * 2f, localVelocity.y, Mathf.Clamp(localVelocity.z, -0.3f, 0.5f));
                _bossStateController.BlendAnimationLocalPositions(localVelocity.x, localVelocity.z);
            }
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }



    public class Run : IBossAnimState
    {

        public BossStateController _bossStateController { get; set; }
        public void Enter(Rigidbody rb, IBossAnimState oldState)
        {
            _bossStateController = rb.GetComponent<BossStateController>();
        }

        public void Exit(Rigidbody rb, IBossAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            if (!_bossStateController._bossMovement.IsGrounded())
            {
                _bossStateController.EnterAnimState(new BossAnimations.InAir());
            }
            else if (_bossStateController._agent.velocity.magnitude + 0.25f < _bossStateController._bossMovement._moveSpeed)
            {
                _bossStateController.EnterAnimState(new BossAnimations.Walk());
            }
            else
            {
                Vector3 localVelocity = rb.transform.InverseTransformDirection(_bossStateController._agent.velocity);
                localVelocity = localVelocity / _bossStateController._bossMovement._runSpeed;
                localVelocity = new Vector3(localVelocity.x * 2f, localVelocity.y, Mathf.Clamp(localVelocity.z, 0.5f, 1f));
                _bossStateController.BlendAnimationLocalPositions(localVelocity.x, localVelocity.z);
            }
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }
    public class Jump : IBossAnimState
    {

        public BossStateController _bossStateController { get; set; }
        private float _jumpTime;
        public void Enter(Rigidbody rb, IBossAnimState oldState)
        {
            _jumpTime = 0.5f;
            _bossStateController = rb.GetComponent<BossStateController>();
        }

        public void Exit(Rigidbody rb, IBossAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            _jumpTime -= Time.deltaTime;
            if (_jumpTime <= 0)
            {
                if (_bossStateController._bossMovement.IsGrounded())
                {
                    _bossStateController.ChangeAnimation("HitGround");
                    GameManager._instance.CallForAction(() => _bossStateController.EnterAnimState(new BossAnimations.Walk()), 0.2f);
                }
                else
                {
                    _bossStateController.EnterAnimState(new BossAnimations.InAir());
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
    public class Dodge : IBossAnimState
    {

        public BossStateController _bossStateController { get; set; }
        private float _dodgeTime;
        public void Enter(Rigidbody rb, IBossAnimState oldState)
        {
            _bossStateController = rb.GetComponent<BossStateController>();
            _dodgeTime = _bossStateController._bossCombat._DodgeTime;
        }

        public void Exit(Rigidbody rb, IBossAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            _dodgeTime -= Time.deltaTime;
            if (_dodgeTime <= 0)
            {
                if (_bossStateController._bossMovement.IsGrounded())
                {
                    _bossStateController.EnterAnimState(new BossAnimations.Walk());
                }
                else
                {
                    _bossStateController.EnterAnimState(new BossAnimations.InAir());
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
    public class InAir : IBossAnimState
    {

        public BossStateController _bossStateController { get; set; }
        public void Enter(Rigidbody rb, IBossAnimState oldState)
        {
            _bossStateController = rb.GetComponent<BossStateController>();
        }

        public void Exit(Rigidbody rb, IBossAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            if (_bossStateController._bossMovement.IsGrounded())
            {
                _bossStateController.ChangeAnimation("HitGround");
                GameManager._instance.CallForAction(() => _bossStateController.EnterAnimState(new BossAnimations.Walk()), 0.2f);
            }
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }
    
    public class Retreat : IBossAnimState
    {

        public BossStateController _bossStateController { get; set; }
        public void Enter(Rigidbody rb, IBossAnimState oldState)
        {
            _bossStateController = rb.GetComponent<BossStateController>();
        }

        public void Exit(Rigidbody rb, IBossAnimState newState)
        {

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
    }
    
    public class SpecialAction : IBossAnimState
    {

        public BossStateController _bossStateController { get; set; }
        public void Enter(Rigidbody rb, IBossAnimState oldState)
        {
            _bossStateController = rb.GetComponent<BossStateController>();
        }

        public void Exit(Rigidbody rb, IBossAnimState newState)
        {

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
    }
    public class Die : IBossAnimState
    {

        public BossStateController _bossStateController { get; set; }
        public void Enter(Rigidbody rb, IBossAnimState oldState)
        {
            _bossStateController = rb.GetComponent<BossStateController>();
        }

        public void Exit(Rigidbody rb, IBossAnimState newState)
        {

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
    }
}
