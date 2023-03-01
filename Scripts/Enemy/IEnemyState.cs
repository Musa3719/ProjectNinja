using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyStates
{
    public interface IEnemyState : IState
    {
        EnemyStateController _enemyStateController { get; set; }
        void Enter(Rigidbody rb, IEnemyState oldState);
        void Exit(Rigidbody rb, IEnemyState newState);
    }


    public class Idle : IEnemyState
    {
        public EnemyStateController _enemyStateController { get; set; }
        private Vector3 targetPos;

        public void Enter(Rigidbody rb, IEnemyState oldState)
        {
            _enemyStateController = rb.GetComponent<EnemyStateController>();
            _enemyStateController.HalfHeadAim();
            _enemyStateController._animator.SetFloat("LocomotionSpeedMultiplier", 0.5f);
        }
        public void Exit(Rigidbody rb, IEnemyState newState)
        {
            _enemyStateController._animator.SetFloat("LocomotionSpeedMultiplier", 1f);
        }
        public void DoState(Rigidbody rb)
        {
            if (_enemyStateController._enemyCombat._isStunned)
            {
                if (_enemyStateController._enemyAI.CheckForDodgeOrBlockWhenStunned())
                {
                    if (_enemyStateController._enemyAI.CheckForDodge())
                    {
                        bool isDodgingToRight = _enemyStateController._enemyMovement.Dodge();
                        _enemyStateController._enemyCombat.Dodge(isDodgingToRight);
                    }
                    else
                        _enemyStateController.BlockOpen(_enemyStateController._enemyAI._attackPosition);
                }
                return;
            }

            if (_enemyStateController.CheckForPlayerInSeen())
            {
                _enemyStateController.EnterState(new Chasing());
            }
            else if (_enemyStateController._isHearTriggered)
            {
                _enemyStateController._isHearTriggered = false;
                _enemyStateController.EnterState(new Searching());
            }

            if (!_enemyStateController._enemyCombat._IsDodging && !_enemyStateController._enemyCombat._isInAttackPattern && !_enemyStateController._enemyCombat._IsBlocking)
            {
                targetPos = _enemyStateController._enemyAI.GetIdleMovementPosition(_enemyStateController._agent);
                _enemyStateController._enemyMovement.MoveToPosition(targetPos, targetPos);
            }
            
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }


    public class Searching : IEnemyState
    {
        public EnemyStateController _enemyStateController { get; set; }

        public Vector3 _searchingPosition { get; private set; }

        public void Enter(Rigidbody rb, IEnemyState oldState)
        {
            _enemyStateController = rb.GetComponent<EnemyStateController>();

            _searchingPosition = _enemyStateController._searchingPositionBuffer;
            _enemyStateController._searchStartTime = Time.time;
            _enemyStateController.DisableHeadAim();
        }
        public void Exit(Rigidbody rb, IEnemyState newState)
        {

        }
        public void DoState(Rigidbody rb)
        {
            if (_enemyStateController._enemyCombat._isStunned)
            {
                if (_enemyStateController._enemyAI.CheckForDodgeOrBlockWhenStunned())
                {
                    if (_enemyStateController._enemyAI.CheckForDodge())
                    {
                        bool isDodgingToRight = _enemyStateController._enemyMovement.Dodge();
                        _enemyStateController._enemyCombat.Dodge(isDodgingToRight);
                    }
                    else
                        _enemyStateController.BlockOpen(_enemyStateController._enemyAI._attackPosition);
                }
                return;
            }

            if (_enemyStateController.CheckForPlayerInSeen())
            {
                _enemyStateController.EnterState(new Chasing());
            }
            else if (_enemyStateController._isHearTriggered)
            {
                _enemyStateController._isHearTriggered = false;
                _enemyStateController.EnterState(new Searching());
            }
            else if (!_enemyStateController.CheckForStillSearching())
            {
                _enemyStateController.EnterState(new Idle());
            }

            if (!_enemyStateController._enemyCombat._IsDodging && !_enemyStateController._enemyCombat._isInAttackPattern && !_enemyStateController._enemyCombat._IsBlocking)
                _enemyStateController._enemyMovement.MoveToPosition(_searchingPosition, _searchingPosition);
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }


    public class Chasing : IEnemyState
    {
        public EnemyStateController _enemyStateController { get; set; }

        public Transform _playerTransform;

        public void Enter(Rigidbody rb, IEnemyState oldState)
        {
            _enemyStateController = rb.GetComponent<EnemyStateController>();
            _playerTransform = GameManager._instance.PlayerRb.transform;

            _enemyStateController.EnableHeadAim();

            rb.GetComponent<NavMeshAgent>().stoppingDistance = _enemyStateController._enemyMovement._ChaseStoppingDistance;
        }
        public void Exit(Rigidbody rb, IEnemyState newState)
        {
            rb.GetComponent<NavMeshAgent>().stoppingDistance = 0.1f;
        }
        public void DoState(Rigidbody rb)
        {
            if (_enemyStateController._enemyCombat._isStunned)
            {
                if (_enemyStateController._enemyAI.CheckForDodgeOrBlockWhenStunned())
                {
                    if (_enemyStateController._enemyAI.CheckForDodge())
                    {
                        bool isDodgingToRight = _enemyStateController._enemyMovement.Dodge();
                        _enemyStateController._enemyCombat.Dodge(isDodgingToRight);
                    }
                    else
                        _enemyStateController.BlockOpen(_enemyStateController._enemyAI._attackPosition);
                }
                return;
            }

            if (!_enemyStateController.CheckForPlayerInSeen())
            {
                _enemyStateController.SearchingFromChasing();
                _enemyStateController.EnterState(new Searching());
            }
            else if (_enemyStateController._enemyAI.CheckForStepBack())
            {
                _enemyStateController.EnterState(new StepBack());
            }
            else if (_enemyStateController._enemyAI.CheckForDodgeOrBlock())
            {
                if (_enemyStateController._enemyAI.CheckForDodge())
                {
                    bool isDodgingToRight = _enemyStateController._enemyMovement.Dodge();
                    _enemyStateController._enemyCombat.Dodge(isDodgingToRight);
                }
                else
                    _enemyStateController.BlockOpen(_enemyStateController._enemyAI._attackPosition);
            }
            else if (_enemyStateController._enemyAI.CheckForAttack())
            {
                _enemyStateController._enemyCombat.AttackWithPattern();
            }
            else if (_enemyStateController._enemyAI.CheckForThrow())
            {
                _enemyStateController._enemyCombat._rangedWarning.SetActive(true);
                GameManager._instance.CallForAction(() => _enemyStateController._enemyCombat.ThrowKillObject(), 0.5f); 
            }

            Vector3 lookAtPos = _enemyStateController._enemyCombat._IsRanged ? _playerTransform.position + GameManager._instance.PlayerRb.velocity * 0.6f * 0.075f * (_playerTransform.position - _enemyStateController.transform.position).magnitude : _playerTransform.position;
            if (_enemyStateController._enemyCombat._IsRanged)
            {
                Vector3 normalDir = (_playerTransform.position - _enemyStateController.transform.position).normalized;
                Vector3 futureDir = (_playerTransform.position + GameManager._instance.PlayerRb.velocity * 0.6f * 0.075f * (_playerTransform.position - _enemyStateController.transform.position).magnitude - _enemyStateController.transform.position).normalized;
                float angle = Vector3.Angle(normalDir, futureDir);
                if (angle > 30f)
                {
                    if (Vector3.Angle(Quaternion.AngleAxis(30f, Vector3.up) * normalDir, futureDir) < angle)
                        lookAtPos = Quaternion.AngleAxis(30f, Vector3.up) * normalDir;
                    else
                        lookAtPos = Quaternion.AngleAxis(-30f, Vector3.up) * normalDir;
                }
            }

            if (!_enemyStateController._enemyCombat._IsDodging && !_enemyStateController._enemyCombat._isInAttackPattern && !_enemyStateController._enemyCombat._IsBlocking)
                if (_enemyStateController._enemyCombat._IsRanged && (_playerTransform.position - _enemyStateController.transform.position).magnitude < _enemyStateController._enemyCombat.AttackRange * 0.75f)
                    _enemyStateController._enemyMovement.MoveToPosition(_enemyStateController.transform.position, lookAtPos);
                else if (!_enemyStateController._enemyCombat._IsRanged && (_playerTransform.position - _enemyStateController.transform.position).magnitude < _enemyStateController._enemyCombat.AttackRange * 0.5f)
                    _enemyStateController._enemyMovement.MoveToPosition(_enemyStateController.transform.position - _enemyStateController.transform.forward, lookAtPos);
                else
                    _enemyStateController._enemyMovement.MoveToPosition(_playerTransform.position, lookAtPos);
        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }
    public class StepBack : IEnemyState
    {
        public EnemyStateController _enemyStateController { get; set; }

        public Transform _playerTransform;
        private float stepBackTimer;
        private Vector3 targetPos;

        public void Enter(Rigidbody rb, IEnemyState oldState)
        {
            _enemyStateController = rb.GetComponent<EnemyStateController>();
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            stepBackTimer = Random.Range(2.5f, 5f);
            targetPos = _enemyStateController._enemyAI.GetStepBackPosition(_enemyStateController._agent);
            _enemyStateController.EnableHeadAim();
        }
        public void Exit(Rigidbody rb, IEnemyState newState)
        {

        }
        public void DoState(Rigidbody rb)
        {
            stepBackTimer -= Time.deltaTime;
            if (_enemyStateController._enemyCombat._isStunned)
            {
                if (_enemyStateController._enemyAI.CheckForDodgeOrBlockWhenStunned())
                {
                    if (_enemyStateController._enemyAI.CheckForDodge())
                    {
                        bool isDodgingToRight = _enemyStateController._enemyMovement.Dodge();
                        _enemyStateController._enemyCombat.Dodge(isDodgingToRight);
                    }
                    else
                        _enemyStateController.BlockOpen(_enemyStateController._enemyAI._attackPosition);
                }
                return;
            }

            if (stepBackTimer <= 0f)
                _enemyStateController.EnterState(new Searching());
            else if (_enemyStateController._enemyAI.CheckForDodgeOrBlock())
            {
                if (_enemyStateController._enemyAI.CheckForDodge())
                {
                    bool isDodgingToRight = _enemyStateController._enemyMovement.Dodge();
                    _enemyStateController._enemyCombat.Dodge(isDodgingToRight);
                }
                else
                    _enemyStateController.BlockOpen(_enemyStateController._enemyAI._attackPosition);
            }
            else if (_enemyStateController._enemyAI.CheckForAttack())
            {
                _enemyStateController.EnterState(new Chasing());
                _enemyStateController._enemyCombat.AttackWithPattern();
            }
            else if (_enemyStateController._enemyAI.CheckForThrow())
            {
                _enemyStateController._enemyCombat._rangedWarning.SetActive(true);
                GameManager._instance.CallForAction(() => _enemyStateController._enemyCombat.ThrowKillObject(), 0.5f);
            }

            if (!_enemyStateController._enemyCombat._IsDodging && !_enemyStateController._enemyCombat._isInAttackPattern && !_enemyStateController._enemyCombat._IsBlocking)
                _enemyStateController._enemyMovement.StepBack(targetPos, _playerTransform.position);

        }

        public void DoStateFixedUpdate(Rigidbody rb)
        {

        }

        public void DoStateLateUpdate(Rigidbody rb)
        {

        }
    }


}