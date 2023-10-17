using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public Rigidbody _rb { get; private set; }
    public NavMeshAgent _navMeshAgent { get; private set; }
    public EnemyStateController _enemyStateController { get; private set; }

    public float _moveSpeed { get; set; }
    public float _dodgeSpeed { get; private set; }
    public float _blockSpeed { get; private set; }
    public float _jumpPower { get; private set; }
    public float _runSpeed { get; set; }


    private float _distToGround;
    private float _xBound;
    private float _zBound;

    private float _groundCounter;
    private float _moveAfterAttackToGroundSpeedCounter;

    public Transform _footTransform;

    private Coroutine _attackOrBlockRotationCoroutine;
    private Coroutine _moveAfterAttackCoroutine;
    private Coroutine _blockMovementCoroutine;

    public float _ChaseStoppingDistance { get; set; }

    private float _walkSoundCounter;
    private float _isOnNavMeshCounter;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _enemyStateController = GetComponent<EnemyStateController>();
        _distToGround = GetComponent<Collider>().bounds.extents.y;
        _xBound = GetComponent<Collider>().bounds.extents.x;
        _zBound = GetComponent<Collider>().bounds.extents.z;
        _jumpPower = 7f;
        _dodgeSpeed = 14f;
        _blockSpeed = 7.5f;
        _ChaseStoppingDistance = 3.25f;
    }
    public bool IsGrounded(float groundCounterLimit = 0.5f)
    {
        if (_enemyStateController._agent.enabled)
        {
            if (_enemyStateController._isOnOffMeshLinkPath)
                return false;
            return _navMeshAgent.isOnNavMesh;
        }
        else
        {
            if (!IsTouchingGround() && !ToGroundRaycast())
            {
                _groundCounter += Time.deltaTime;
                if (_groundCounter >= groundCounterLimit)
                {
                    return false;
                }
                return true;
            }
            _groundCounter = 0f;
            return true;
        }
        
    }
    private void Update()
    {
        if (GameManager._instance.isGameStopped || _enemyStateController._enemyCombat.IsDead || GameManager._instance.isPlayerDead) return;

        ArrangePlaneSound();

        CheckForWarp();
    }
    private void CheckForWarp()
    {
        if (_enemyStateController._agent.enabled && !_enemyStateController._isOnOffMeshLinkPath && !_enemyStateController._agent.isOnNavMesh)
        {
            _isOnNavMeshCounter += Time.deltaTime;
            if (_isOnNavMeshCounter > 0.5f)
            {
                Debug.LogError("Warped");
                //_enemyStateController._agent.FindClosestEdge(out NavMeshHit hit);
                //_enemyStateController._agent.Warp(hit.position);
            }
        }
    }
    private void ArrangePlaneSound()
    {
        if (!_enemyStateController._agent.enabled || _enemyStateController._isOnOffMeshLinkPath || _enemyStateController.TouchingGrounds.Count == 0) return;

        float speed = _enemyStateController._agent.velocity.magnitude;
        GameObject groundCollider = _enemyStateController.TouchingGrounds[_enemyStateController.TouchingGrounds.Count - 1];
        if (groundCollider != null && groundCollider.GetComponent<PlaneSound>() != null)
        {
            while (_walkSoundCounter <= 0f)//not using if because it could be lower than -1 when frame drops etc
            {
                SoundManager._instance.PlayPlaneOrWallSound(groundCollider.GetComponent<PlaneSound>().PlaneSoundType, speed);
                SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.ArmorWalkSounds), transform.position, 0.02f, false, UnityEngine.Random.Range(0.93f, 1.07f));
                _walkSoundCounter += 1f;
            }
            _walkSoundCounter -= Time.deltaTime * speed / 4f;
        }
    }
    private bool ToGroundRaycast()
    {
        Physics.Raycast(_footTransform.position, -Vector3.up, out RaycastHit hit, 0.7f);
        if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Grounds"))
            return true;
        return false;
    }
    public void BlockMovement(Vector3 targetPos)
    {
        Vector3 direction = transform.position - targetPos;
        direction.y = 0f;
        direction = direction.normalized;

        Vector3 targetVel = direction * _blockSpeed;

        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;

        if (_moveAfterAttackCoroutine != null)
            StopCoroutine(_moveAfterAttackCoroutine);
        if (_attackOrBlockRotationCoroutine != null)
            StopCoroutine(_attackOrBlockRotationCoroutine);

        if (_blockMovementCoroutine != null)
            StopCoroutine(_blockMovementCoroutine);
        _blockMovementCoroutine = StartCoroutine(BlockMovementCoroutine(targetVel, _enemyStateController._enemyCombat._BlockMoveTime));
    }
    private IEnumerator BlockMovementCoroutine(Vector3 targetVel, float time)
    {
        float startTime = Time.time;
        while (Time.time < startTime + time / 8f)
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, targetVel, Time.deltaTime * 6f);
            yield return null;
        }
        _rb.velocity = targetVel;
        while (Time.time < startTime + time)
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, Vector3.zero, Time.deltaTime * 1.5f);
            yield return null;
        }
        _rb.velocity = Vector3.zero;

        if (!_enemyStateController._enemyCombat.IsDead)
        {
            _navMeshAgent.enabled = true; _rb.isKinematic = true;
        }
    }
    public void MoveAfterAttack(bool isFirstAttack)
    {
        Vector3 direction = (GameManager._instance.PlayerRb.position + Random.Range(-0.25f, 0.25f) * GameManager._instance.PlayerRb.transform.right - transform.position).normalized;

        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;
        _enemyStateController._animator.SetInteger("MoveAfterAttackNumber", Random.Range(1, 3));
        _enemyStateController._animator.SetTrigger("MoveAfterAttack");
        GameManager._instance.CallForAction(() => { if (_enemyStateController._enemyCombat.IsDead) return; _navMeshAgent.enabled = true; _rb.isKinematic = true; }, 0.65f);

        float firstMultiplier = isFirstAttack ? 0.6f : 0.85f;
        float subtranctByDistance = (6.5f - (GameManager._instance.PlayerRb.position - transform.position).magnitude);
        if (subtranctByDistance < 0) subtranctByDistance = subtranctByDistance / 3f;
        else subtranctByDistance = 0f;
        float distanceMultiplier = (GameManager._instance.PlayerRb.position - transform.position).magnitude;// - subtranctByDistance;

        //if (subtranctByDistance < 0f) distanceMultiplier /= 1.6f;

        Vector3 targetVel = direction * 20f * firstMultiplier * Mathf.Clamp(distanceMultiplier, 1.5f, 5f) / 4.85f;
        if (_moveAfterAttackCoroutine != null)
            StopCoroutine(_moveAfterAttackCoroutine);
        _moveAfterAttackCoroutine = StartCoroutine(MoveAfterAttackCoroutine(targetVel));
    }
    private IEnumerator MoveAfterAttackCoroutine(Vector3 targetVel)
    {
        float startTime = Time.time;
        float firstMoveTime = 0.1f;
        float secondMoveTime = 0.1f;

        _rb.velocity = targetVel / 1.6f;
        while (Time.time < startTime + firstMoveTime)
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, targetVel, Time.deltaTime * 20f);
            _rb.transform.forward = Vector3.Lerp(_rb.transform.forward, new Vector3(targetVel.normalized.x, 0f, targetVel.normalized.z), Time.deltaTime * 5f);
            ArrangeMoveAfterAttackGrounded();
            yield return null;
        }
        //_rb.velocity = targetVel;
        yield return new WaitForSeconds(0.05f);
        float newTime = Time.time;
        while (Time.time < newTime + secondMoveTime)
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, Vector3.zero, Time.deltaTime * 8f);
            _rb.transform.forward = Vector3.Lerp(_rb.transform.forward, new Vector3(targetVel.normalized.x, 0f, targetVel.normalized.z), Time.deltaTime * 5f);
            ArrangeMoveAfterAttackGrounded();
            yield return null;
        }
        _rb.velocity = Vector3.zero;
    }
    private void ArrangeMoveAfterAttackGrounded()
    {
        if (_enemyStateController._isDead) return;//temp bugfix

        int c = 0;
        float timer = 0;
        bool isVFXThisFrame = false;
        float decreaseAmountForFrame = 0.01f;
        while (!IsGrounded(0.03f))
        {
            c++;
            if (c > 1000)
            {
                Debug.LogError("MoveAfterAttackTeleportWhileError");
                break;
            }
            _rb.transform.Translate(-Vector3.up * decreaseAmountForFrame);
            timer += decreaseAmountForFrame;
            if (timer > 0.35f && !isVFXThisFrame)
            {
                _enemyStateController.TeleportAndDissolve(true);
                isVFXThisFrame = true;
            }
        }
    }
    public void AttackOrBlockRotation(bool isAttack)
    {
        if (_attackOrBlockRotationCoroutine != null)
            StopCoroutine(_attackOrBlockRotationCoroutine);

        if (isAttack)
        {
            _attackOrBlockRotationCoroutine = StartCoroutine(AttackRotationCoroutine());
        }
        else
        {
            _attackOrBlockRotationCoroutine = StartCoroutine(BlockRotationCoroutine());
        }
        
    }
    private IEnumerator AttackRotationCoroutine()
    {
        while (_enemyStateController._enemyCombat._isAttacking)
        {
            if (_enemyStateController._agent.enabled && !_enemyStateController._isOnOffMeshLinkPath && _navMeshAgent.isOnNavMesh)
                MoveToPosition(transform.position, GameManager._instance.PlayerRb.transform.position, 8f, 0f);
            yield return null;
        }
    }
    private IEnumerator BlockRotationCoroutine()
    {
        while (_enemyStateController._enemyCombat._IsBlocking)
        {
            if (_enemyStateController._agent.enabled && !_enemyStateController._isOnOffMeshLinkPath && _navMeshAgent.isOnNavMesh)
                MoveToPosition(transform.position, GameManager._instance.PlayerRb.transform.position, 8f, 0f);
            yield return null;
        }
    }

    /// <returns>isDodgingToRight</returns>
    public bool Dodge()
    {
        Vector3 direction = _enemyStateController._enemyAI.GetDodgeDirection();
        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;
        GameManager._instance.CallForAction(() => { if (_enemyStateController._enemyCombat.IsDead) return; _navMeshAgent.enabled = true; _rb.isKinematic = true; }, _enemyStateController._enemyCombat._DodgeTime);
        Vector3 targetVel = direction * _dodgeSpeed;
        StartCoroutine(DodgeCoroutine(targetVel));

        return direction.x >= 0 ? true : false;
    }
    private IEnumerator DodgeCoroutine(Vector3 targetVel)
    {
        float startTime = Time.time;
        while (Time.time < startTime + _enemyStateController._enemyCombat._DodgeTime / 8f)
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, targetVel, Time.deltaTime * 6f);
            yield return null;
        }
        _rb.velocity = targetVel;
        while (Time.time < startTime + _enemyStateController._enemyCombat._DodgeTime)
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, Vector3.zero, Time.deltaTime * 1.5f);
            yield return null;
        }
        _rb.velocity = Vector3.zero;
    }
    public void StepBack(Vector3 position, Vector3 lookAtPos)
    {
        if (!_navMeshAgent.enabled) return;

        _navMeshAgent.speed = _moveSpeed * 1.5f;
        _navMeshAgent.acceleration = 2.5f;

        //float lerpSpeed = 2.5f;
        float rotationLerpSpeed = 4.5f;
        //_rb.velocity = Vector3.Lerp(_rb.velocity, position * _moveSpeed, Time.deltaTime * lerpSpeed);
        Vector3 direction = (lookAtPos - _rb.transform.position).normalized;
        _rb.transform.forward = Vector3.Lerp(_rb.transform.forward, new Vector3(direction.x, 0f, direction.z), Time.deltaTime * rotationLerpSpeed);

        if (_navMeshAgent.destination != position)
        {
            _navMeshAgent.SetDestination(position);
        }
    }
    public bool IsTouchingGround()
    {
        return _enemyStateController.TouchingGrounds.Count > 0;
    }
    public void MoveToPosition(Vector3 position, Vector3 lookAtPos, float rotationLerpSpeed = 10f, float? speed = null, float speedMultiplier = 1f)
    {
        if (!_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh || _enemyStateController._isOnOffMeshLinkPath) return;

        _navMeshAgent.acceleration = 2.5f;

        if (speed != null)
        {
            _navMeshAgent.speed = speed.Value;
        }
        else if ((position - transform.position).magnitude > 8.5f)
        {
            _navMeshAgent.speed = _runSpeed;
        }
        else
        {
            _navMeshAgent.speed = _moveSpeed;
        }
        _navMeshAgent.speed *= speedMultiplier;

        //float lerpSpeed = 2.5f;
        //_rb.velocity = Vector3.Lerp(_rb.velocity, position * _moveSpeed, Time.deltaTime * lerpSpeed);
        Vector3 direction = (lookAtPos - _rb.transform.position).normalized;
        if (direction.x != 0 || direction.z != 0)
            _rb.transform.forward = Vector3.Lerp(_rb.transform.forward, new Vector3(direction.x, 0f, direction.z), Time.deltaTime * rotationLerpSpeed);

        if (_navMeshAgent.destination != position)
        {
            _navMeshAgent.SetDestination(position);
        }
    }
}
