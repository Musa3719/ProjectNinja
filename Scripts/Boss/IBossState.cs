using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BossStates
{
    public interface IBossState : IState
    {
        BossStateController _bossStateController { get; set; }
        void Enter(Rigidbody rb, IBossState oldState);
        void Exit(Rigidbody rb, IBossState newState);
    }


    public class IdleMove : IBossState
    {
        public BossStateController _bossStateController { get; set; }
        private Vector3 _targetPos;

        public void Enter(Rigidbody rb, IBossState oldState)
        {
            _bossStateController = rb.GetComponent<BossStateController>();
            _bossStateController.EnableHeadAim();
            _bossStateController._animator.SetFloat("LocomotionSpeedMultiplier", 0.5f);
            _targetPos = _bossStateController._bossAI.GetIdleMovementPosition(_bossStateController._agent);
        }
        public void Exit(Rigidbody rb, IBossState newState)
        {
            _bossStateController._animator.SetFloat("LocomotionSpeedMultiplier", 1f);
        }
        public void DoState(Rigidbody rb)
        {
            _bossStateController._bossAI._idleTimer -= Time.deltaTime;

            if (_bossStateController._bossAI.CheckForExitIdle() || (_targetPos - rb.transform.position).magnitude < 0.5f)
            {
                _bossStateController.EnterState(new Chase());
            }

            else if (_bossStateController._bossAI.CheckForDodgeOrBlock())
            {
                if (_bossStateController._bossAI.CheckForDodge())
                {
                    bool isDodgingToRight = _bossStateController._bossMovement.Dodge();
                    _bossStateController._bossCombat.Dodge(isDodgingToRight);
                }
                else
                {
                    _bossStateController.BlockOpen(_bossStateController._bossAI._attackPosition);
                }
            }

            if (_bossStateController._bossCombat._IsStunned) return;

            if (!_bossStateController._bossCombat._IsDodging && !_bossStateController._bossCombat._IsInAttackPattern && !_bossStateController._bossCombat._IsBlocking)
                _bossStateController._bossMovement.MoveToPosition(_targetPos, GameManager._instance.PlayerRb.transform.position, 6.5f, 2.5f);
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }


    public class Chase : IBossState
    {
        public BossStateController _bossStateController { get; set; }

        public Transform _playerTransform;

        public void Enter(Rigidbody rb, IBossState oldState)
        {
            _bossStateController = rb.GetComponent<BossStateController>();
            _bossStateController.EnableHeadAim();
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

            rb.GetComponent<NavMeshAgent>().stoppingDistance = _bossStateController._bossMovement._ChaseStoppingDistance;
        }
        public void Exit(Rigidbody rb, IBossState newState)
        {
            rb.GetComponent<NavMeshAgent>().stoppingDistance = 0.05f;
        }
        public void DoState(Rigidbody rb)
        {
            if (_bossStateController._bossCombat._IsStunned)
            {
                if (_bossStateController._bossAI.CheckForDodgeOrBlock())
                {
                    _bossStateController.BlockOpen(_bossStateController._bossAI._attackPosition);
                }
                return;
            }

            if (_bossStateController._bossAI.CheckForIdle())
            {
                _bossStateController.EnterState(new IdleMove());
            }
            else if (_bossStateController._bossAI.CheckForRetreat())
            {
                _bossStateController.EnterState(new Retreat());
            }
            else if (_bossStateController._bossAI.CheckForDodgeOrBlock())
            {
                if (_bossStateController._bossAI.CheckForDodge())
                {
                    bool isDodgingToRight = _bossStateController._bossMovement.Dodge();
                    _bossStateController._bossCombat.Dodge(isDodgingToRight);
                }
                else
                {
                    _bossStateController.BlockOpen(_bossStateController._bossAI._attackPosition);
                }
            }
            
            else if (_bossStateController._bossAI.CheckForAttack())
            {
                _bossStateController._bossCombat.AttackWithPattern();
            }

            if(!_bossStateController._bossCombat._IsDodging && !_bossStateController._bossCombat._IsInAttackPattern && !_bossStateController._bossCombat._IsBlocking)
                _bossStateController._bossMovement.MoveToPosition(_playerTransform.position, GameManager._instance.PlayerRb.transform.position);


        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }

    public class Retreat : IBossState
    {
        public BossStateController _bossStateController { get; set; }

        public Transform _playerTransform;

        public void Enter(Rigidbody rb, IBossState oldState)
        {
            _bossStateController = rb.GetComponent<BossStateController>();
            _bossStateController.EnterAnimState(new BossAnimations.Retreat());
            _bossStateController._bossMovement.StopDodgeOrBlockMovement();
            _bossStateController.EnableHeadAim();
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

            _bossStateController._bossMovement.Retreat();
        }
        public void Exit(Rigidbody rb, IBossState newState)
        {
            if (!(newState is SpecialAction))
            {
                _bossStateController.EnterAnimState(new BossAnimations.Walk());
            }
            if (newState is Chase)
            {
                _bossStateController._bossMovement.ToGroundAfterRetreat();
            }
        }
        public void DoState(Rigidbody rb)
        {
            if (_bossStateController._bossAI.CheckForRetreatToSpecialAction())
            {
                _bossStateController.EnterState(new SpecialAction());
            }
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }
    public class SpecialAction : IBossState
    {
        public BossStateController _bossStateController { get; set; }

        public Transform _playerTransform;


        public void Enter(Rigidbody rb, IBossState oldState)
        {
            _bossStateController = rb.GetComponent<BossStateController>();
            _bossStateController.EnterAnimState(new BossAnimations.SpecialAction());
            _bossStateController._bossMovement.StopDodgeOrBlockMovement();
            _bossStateController.DisableHeadAim();
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

            _bossStateController._bossAI.SpecialActionMovement(_bossStateController._rb, 15f);
        }
        public void Exit(Rigidbody rb, IBossState newState)
        {
            if (!(newState is Retreat))
            {
                _bossStateController.EnterAnimState(new BossAnimations.Walk());
            }

            if (newState is Chase)
            {
                _bossStateController._bossMovement.ToGroundAfterRetreat();
            }
        }
        public void DoState(Rigidbody rb)
        {
            
            //Exit handled in ai
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }

}