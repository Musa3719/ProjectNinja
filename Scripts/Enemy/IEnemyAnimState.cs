using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyAnimations
{
    public interface IEnemyAnimState : IState
    {
        EnemyStateController _enemyStateController { get; set; }
        void Enter(Rigidbody rb, IEnemyAnimState oldState);
        void Exit(Rigidbody rb, IEnemyAnimState newState);
    }


    public class Idle : IEnemyAnimState
    {
        public EnemyStateController _enemyStateController { get; set; }
        public void Enter(Rigidbody rb, IEnemyAnimState oldState)
        {
            _enemyStateController = rb.GetComponent<EnemyStateController>();
        }

        public void Exit(Rigidbody rb, IEnemyAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            if (!_enemyStateController._enemyMovement.IsGrounded())
            {
                _enemyStateController.EnterAnimState(new EnemyAnimations.InAir());
            }
            else if (_enemyStateController._agent.velocity.magnitude > 0.25f)
            {
                _enemyStateController.EnterAnimState(new EnemyAnimations.Walk());
            }
            else
            {
                if (_enemyStateController._agent.enabled)
                {
                    _enemyStateController.BlendAnimationLocalPositions(0f, 0f);
                }
                else
                {
                    Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);
                    localVelocity = localVelocity / _enemyStateController._enemyMovement._moveSpeed * 2f;
                    localVelocity = new Vector3(Mathf.Clamp(localVelocity.x * 3f, -1f, 1f), localVelocity.y, Mathf.Clamp(localVelocity.z, 0f, 0.8f));
                    _enemyStateController.BlendAnimationLocalPositions(localVelocity.x, localVelocity.z);
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

    public class Walk : IEnemyAnimState
    {

        public EnemyStateController _enemyStateController { get; set; }
        public void Enter(Rigidbody rb, IEnemyAnimState oldState)
        {
            _enemyStateController = rb.GetComponent<EnemyStateController>();
        }

        public void Exit(Rigidbody rb, IEnemyAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            if (!_enemyStateController._enemyMovement.IsGrounded())
            {
                _enemyStateController.EnterAnimState(new EnemyAnimations.InAir());
            }
            else if (_enemyStateController._agent.velocity.magnitude <= 0.25f)
            {
                _enemyStateController.EnterAnimState(new EnemyAnimations.Idle());
            }
            else if (_enemyStateController._agent.velocity.magnitude > _enemyStateController._enemyMovement._moveSpeed)
            {
                _enemyStateController.EnterAnimState(new EnemyAnimations.Run());
            }
            else
            {
                Vector3 localVelocity = rb.transform.InverseTransformDirection(_enemyStateController._agent.velocity);
                localVelocity = localVelocity / _enemyStateController._enemyMovement._moveSpeed / 2f * 0.5f / 0.33f;
                localVelocity = new Vector3(localVelocity.x * 2f, localVelocity.y, Mathf.Clamp(localVelocity.z, -0.3f, 0.5f));
                _enemyStateController.BlendAnimationLocalPositions(localVelocity.x, localVelocity.z);
            }
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }



    public class Run : IEnemyAnimState
    {

        public EnemyStateController _enemyStateController { get; set; }
        public void Enter(Rigidbody rb, IEnemyAnimState oldState)
        {
            _enemyStateController = rb.GetComponent<EnemyStateController>();
        }

        public void Exit(Rigidbody rb, IEnemyAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            if (!_enemyStateController._enemyMovement.IsGrounded())
            {
                _enemyStateController.EnterAnimState(new EnemyAnimations.InAir());
            }
            else if (_enemyStateController._agent.velocity.magnitude + 0.25f < _enemyStateController._enemyMovement._moveSpeed)
            {
                _enemyStateController.EnterAnimState(new EnemyAnimations.Walk());
            }
            else
            {
                Vector3 localVelocity = rb.transform.InverseTransformDirection(_enemyStateController._agent.velocity);
                localVelocity = localVelocity / _enemyStateController._enemyMovement._runSpeed;
                localVelocity = new Vector3(localVelocity.x * 2f, localVelocity.y, Mathf.Clamp(localVelocity.z, 0.5f, 1f));
                _enemyStateController.BlendAnimationLocalPositions(localVelocity.x, localVelocity.z);
            }
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }
    public class Jump : IEnemyAnimState
    {

        public EnemyStateController _enemyStateController { get; set; }
        private float _jumpTime;
        public void Enter(Rigidbody rb, IEnemyAnimState oldState)
        {
            _jumpTime = 0.5f;
            _enemyStateController = rb.GetComponent<EnemyStateController>();
        }

        public void Exit(Rigidbody rb, IEnemyAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            _jumpTime -= Time.deltaTime;
            if (_jumpTime <= 0)
            {
                if (_enemyStateController._enemyMovement.IsGrounded())
                {
                    _enemyStateController.ChangeAnimation("HitGround");
                    GameManager._instance.CallForAction(() => _enemyStateController.EnterAnimState(new EnemyAnimations.Walk()), 0.5f);
                    
                }
                else
                {
                    _enemyStateController.EnterAnimState(new EnemyAnimations.InAir());
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
    public class Dodge : IEnemyAnimState
    {

        public EnemyStateController _enemyStateController { get; set; }
        private float _dodgeTime;
        public void Enter(Rigidbody rb, IEnemyAnimState oldState)
        {
            _enemyStateController = rb.GetComponent<EnemyStateController>();
            _dodgeTime = _enemyStateController._enemyCombat._DodgeTime;
        }

        public void Exit(Rigidbody rb, IEnemyAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            _dodgeTime -= Time.deltaTime;
            if (_dodgeTime <= 0)
            {
                if (_enemyStateController._enemyMovement.IsGrounded())
                {
                    _enemyStateController.EnterAnimState(new EnemyAnimations.Walk());
                }
                else
                {
                    _enemyStateController.EnterAnimState(new EnemyAnimations.InAir());
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
    public class InAir : IEnemyAnimState
    {

        public EnemyStateController _enemyStateController { get; set; }
        public void Enter(Rigidbody rb, IEnemyAnimState oldState)
        {
            _enemyStateController = rb.GetComponent<EnemyStateController>();
        }

        public void Exit(Rigidbody rb, IEnemyAnimState newState)
        {

        }

        public void DoState(Rigidbody rb)
        {
            if (_enemyStateController._enemyMovement.IsGrounded())
            {
                _enemyStateController.ChangeAnimation("HitGround");
                GameManager._instance.CallForAction(() => _enemyStateController.EnterAnimState(new EnemyAnimations.Walk()), 0.5f);
            }
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }
    
    public class Die : IEnemyAnimState
    {

        public EnemyStateController _enemyStateController { get; set; }
        public void Enter(Rigidbody rb, IEnemyAnimState oldState)
        {
            _enemyStateController = rb.GetComponent<EnemyStateController>();
        }

        public void Exit(Rigidbody rb, IEnemyAnimState newState)
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
