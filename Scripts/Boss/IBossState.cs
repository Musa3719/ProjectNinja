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
        private float _startTime;
        private float _waitTime;

        public void Enter(Rigidbody rb, IBossState oldState)
        {
            _startTime = Time.time;
            _waitTime = Random.Range(1.5f, 4.5f);
            _bossStateController = rb.GetComponent<BossStateController>();
            _bossStateController.EnableHeadAim();
            _bossStateController._animator.SetFloat("LocomotionSpeedMultiplier", 0.3f + (_bossStateController._bossMovement._runSpeed - 10.5f) / 10f);
            _targetPos = _bossStateController._bossAI.GetIdleMovementPosition(_bossStateController._agent);
        }
        public void Exit(Rigidbody rb, IBossState newState)
        {
            _bossStateController._animator.SetFloat("LocomotionSpeedMultiplier", 1f + (_bossStateController._bossMovement._runSpeed - 10.5f) / 10f);
        }
        public void DoState(Rigidbody rb)
        {
            if (_startTime + _waitTime < Time.time || _bossStateController._bossAI.CheckForExitIdle() || (_targetPos - rb.transform.position).magnitude < 0.5f)
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
                _bossStateController._bossMovement.MoveToPosition(_targetPos, GameManager._instance.PlayerRb.transform.position, 2f, 2f);
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
        private float _randomDirection;

        public void Enter(Rigidbody rb, IBossState oldState)
        {
            _bossStateController = rb.GetComponent<BossStateController>();
            _bossStateController.EnableHeadAim();
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

            if (Random.Range(0, 2) == 0)
                _randomDirection = 1f;
            else
                _randomDirection = -1f;

            //rb.GetComponent<NavMeshAgent>().stoppingDistance = _bossStateController._bossMovement._ChaseStoppingDistance;
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
                if (_bossStateController._bossAI._BossNumber == 1)
                {
                    _bossStateController.EnterState(new RetreatBoss1());
                }
                else if (_bossStateController._bossAI._BossNumber == 2)
                {
                    _bossStateController.EnterState(new FastAttackBoss2());
                }
                else if (_bossStateController._bossAI._BossNumber == 3)
                {
                    _bossStateController.EnterState(new TeleportBoss3());
                }
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

            if (!_bossStateController._bossCombat._IsDodging && !_bossStateController._bossCombat._IsInAttackPattern && !_bossStateController._bossCombat._IsBlocking)
                if (_bossStateController._bossAI._BossNumber == 2)
                {
                    if (GameManager._instance.BossPhaseCounterBetweenScenes.transform.position.x == 1)
                    {
                        _bossStateController._bossMovement.MoveToPosition(_playerTransform.position + (_bossStateController.transform.position - _playerTransform.position).normalized * 2f, GameManager._instance.PlayerRb.transform.position, speedMultiplier: 0.6f);
                        _bossStateController._animator.SetFloat("LocomotionSpeedMultiplier", 0.7f);
                    }
                    else
                    {
                        _bossStateController._bossMovement.MoveToPosition(_playerTransform.position + (_bossStateController.transform.position - _playerTransform.position).normalized * 2f, GameManager._instance.PlayerRb.transform.position);
                        _bossStateController._animator.SetFloat("LocomotionSpeedMultiplier", 1f);
                    }
                }
                else if ((_playerTransform.position - _bossStateController.transform.position).magnitude < _bossStateController._bossCombat.AttackRange * 0.8f)
                {
                    _bossStateController._bossMovement.MoveToPosition(_bossStateController.transform.position + _bossStateController.transform.right, _playerTransform.position, speedMultiplier: 0.55f);
                    _bossStateController._animator.SetFloat("LocomotionSpeedMultiplier", 0.7f);
                }
                else
                {
                    _bossStateController._bossMovement.MoveToPosition(_playerTransform.position, GameManager._instance.PlayerRb.transform.position);
                    _bossStateController._animator.SetFloat("LocomotionSpeedMultiplier", 1f);
                }


        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }

    public class RetreatBoss1 : IBossState
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
            if (!(newState is SpecialActionBoss1))
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
                _bossStateController.EnterState(new SpecialActionBoss1());
            }
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }
    public class SpecialActionBoss1 : IBossState
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

            if (_bossStateController._bossAI._BossNumber == 1)
                _bossStateController._bossAI.SpecialActionMovementBoss1(_bossStateController._rb, 15f);
        }
        public void Exit(Rigidbody rb, IBossState newState)
        {
            if (!(newState is RetreatBoss1))
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

    public class FastAttackBoss2 : IBossState
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

            _bossStateController._bossMovement.FastAttack();
        }
        public void Exit(Rigidbody rb, IBossState newState)
        {
            _bossStateController.EnterAnimState(new BossAnimations.Walk());
        }
        public void DoState(Rigidbody rb)
        {
            if (_bossStateController._bossAI.CheckForDodgeOrBlock())
            {
                bool isDodgingToRight = _bossStateController._bossMovement.Dodge();
                _bossStateController._bossCombat.Dodge(isDodgingToRight);
            }
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }
    public class TeleportBoss3 : IBossState
    {
        public BossStateController _bossStateController { get; set; }

        public Transform _playerTransform;
        private float _enterTime;

        public void Enter(Rigidbody rb, IBossState oldState)
        {
            _enterTime = Time.time;
            _bossStateController = rb.GetComponent<BossStateController>();
            _bossStateController.EnterAnimState(new BossAnimations.Retreat());
            _bossStateController._bossMovement.StopDodgeOrBlockMovement();
            _bossStateController.EnableHeadAim();
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

            _bossStateController._bossMovement.Teleport();
            if (Random.Range(0, 100) < 75)
            {
                int random = Random.Range(0, 3);
                if (random == 0)
                    _bossStateController._bossCombat.GunAttack();
                else if (random == 1)
                    _bossStateController._bossCombat.LaserAttack();
                else
                    _bossStateController._bossCombat.Throw();
            }
            else
            {
                GameManager._instance.CallForAction(() => _bossStateController.EnterState(new BossStates.Chase()), 1f);
            }
        }
        public void Exit(Rigidbody rb, IBossState newState)
        {
            if (_bossStateController._bossCombat.Boss3ExtraWeapon != null)
                GameObject.Destroy(_bossStateController._bossCombat.Boss3ExtraWeapon);
            _bossStateController.EnterAnimState(new BossAnimations.Walk());
        }
        public void DoState(Rigidbody rb)
        {
            if (_bossStateController._bossAI.CheckForDodgeOrBlock() && _enterTime >= 1f)
            {
                bool isDodgingToRight = _bossStateController._bossMovement.Dodge();
                _bossStateController._bossCombat.Dodge(isDodgingToRight);
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