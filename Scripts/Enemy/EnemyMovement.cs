using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public Rigidbody _rb { get; private set; }
    public NavMeshAgent _navMeshAgent { get; private set; }
    public EnemyStateController _enemyStateController { get; private set; }

    public float _moveSpeed { get; private set; }
    public float _dodgeSpeed { get; private set; }
    public float _blockSpeed { get; private set; }
    public float _jumpPower { get; private set; }
    public float _runSpeed { get; private set; }


    private float _distToGround;
    private float _xBound;
    private float _zBound;

    private float _groundCounter;
    private float _moveAfterAttackToGroundSpeedCounter;

    public Transform _footTransform;
    public Transform _testTransformForMove;

    private Coroutine _attackOrBlockRotationCoroutine;
    private Coroutine _moveAfterAttackCoroutine;
    private Coroutine _blockMovementCoroutine;

    public float _ChaseStoppingDistance { get; set; }

    private float _walkSoundCounter;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _enemyStateController = GetComponent<EnemyStateController>();
        _distToGround = GetComponent<Collider>().bounds.extents.y;
        _xBound = GetComponent<Collider>().bounds.extents.x;
        _zBound = GetComponent<Collider>().bounds.extents.z;
        _moveSpeed = 4f;
        _jumpPower = 7f;
        _runSpeed = 11.5f;
        _dodgeSpeed = 20f;
        _blockSpeed = 10f;
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
        Vector3 direction = (GameManager._instance.PlayerRb.position + Random.Range(-0.6f, 0.6f) * GameManager._instance.PlayerRb.transform.right - transform.position).normalized;

        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;
        GameManager._instance.CallForAction(() => { if (_enemyStateController._enemyCombat.IsDead) return; _navMeshAgent.enabled = true; _rb.isKinematic = true; }, 0.65f);

        float firstMultiplier = isFirstAttack ? 0.95f : 1f;
        float subtranctByDistance = (6.5f - (GameManager._instance.PlayerRb.position - transform.position).magnitude);
        if (subtranctByDistance > 0) subtranctByDistance = subtranctByDistance / 1.5f;
        float distanceMultiplier = (GameManager._instance.PlayerRb.position - transform.position).magnitude;// - subtranctByDistance;

        if (distanceMultiplier < 5f) distanceMultiplier /= 1.6f;

        Vector3 targetVel = direction * 18f * firstMultiplier * Mathf.Clamp(distanceMultiplier, 0f, 7.5f) / 5f;
        if (_moveAfterAttackCoroutine != null)
            StopCoroutine(_moveAfterAttackCoroutine);
        _moveAfterAttackCoroutine = StartCoroutine(MoveAfterAttackCoroutine(targetVel));
    }
    private IEnumerator MoveAfterAttackCoroutine(Vector3 targetVel)
    {
        float startTime = Time.time;
        float dividerOfTime = 1.5f;
        while (Time.time < startTime + 0.65f / dividerOfTime)
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, targetVel, (Time.time - startTime) / (0.65f / dividerOfTime));
            _rb.transform.forward = Vector3.Lerp(_rb.transform.forward, new Vector3(targetVel.normalized.x, 0f, targetVel.normalized.z), Time.deltaTime * 6f);
            ArrangeMoveAfterAttackGrounded();
            yield return null;
        }
        _rb.velocity = targetVel;
        float newTime = Time.time;
        while (Time.time < startTime + 0.65f)
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, Vector3.zero, (Time.time - newTime) / (0.65f - (newTime - startTime)));
            _rb.transform.forward = Vector3.Lerp(_rb.transform.forward, new Vector3(targetVel.normalized.x, 0f, targetVel.normalized.z), Time.deltaTime * 8f);
            ArrangeMoveAfterAttackGrounded();
            yield return null;
        }
        _rb.velocity = Vector3.zero;
    }
    private void ArrangeMoveAfterAttackGrounded()
    {
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
                MoveToPosition(transform.position, GameManager._instance.PlayerRb.transform.position, 5f, 0f);
            yield return null;
        }
    }
    private IEnumerator BlockRotationCoroutine()
    {
        while (_enemyStateController._enemyCombat._IsBlocking)
        {
            if (_enemyStateController._agent.enabled && !_enemyStateController._isOnOffMeshLinkPath && _navMeshAgent.isOnNavMesh)
                MoveToPosition(transform.position, GameManager._instance.PlayerRb.transform.position, 5f, 0f);
            yield return null;
        }
    }

    /// <returns>isDodgingToRight</returns>
    public bool Dodge()
    {
        Vector3 direction = _enemyStateController._enemyAI.GetDodgeDirection();
        Vector3 tempForward = _rb.transform.forward;
        tempForward.y = 0f;
        float angle = -Vector3.SignedAngle(tempForward, Vector3.forward, Vector3.up);
        Vector3 rotatedDirection = Quaternion.AngleAxis(angle, Vector3.up) * direction;
        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;
        GameManager._instance.CallForAction(() => { if (_enemyStateController._enemyCombat.IsDead) return; _navMeshAgent.enabled = true; _rb.isKinematic = true; }, _enemyStateController._enemyCombat._DodgeTime);
        Vector3 targetVel = rotatedDirection * _dodgeSpeed;
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
        _navMeshAgent.acceleration = 5f;

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
    public void MoveToPosition(Vector3 position, Vector3 lookAtPos, float rotationLerpSpeed = 6.5f, float? speed = null)
    {
        if (!_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh || _navMeshAgent.isOnOffMeshLink) return;

        _navMeshAgent.acceleration = 5f;

        if (speed != null)
        {
            _navMeshAgent.speed = speed.Value;
        }
        else if ((position - transform.position).magnitude > 12f)
        {
            _navMeshAgent.speed = _runSpeed;
        }
        else
        {
            _navMeshAgent.speed = _moveSpeed;
        }

        //float lerpSpeed = 2.5f;
        //_rb.velocity = Vector3.Lerp(_rb.velocity, position * _moveSpeed, Time.deltaTime * lerpSpeed);
        Vector3 direction = (lookAtPos - _rb.transform.position).normalized;
        _rb.transform.forward = Vector3.Lerp(_rb.transform.forward, new Vector3(direction.x, 0f, direction.z), Time.deltaTime * rotationLerpSpeed);

        if (_navMeshAgent.destination != position)
        {
            _navMeshAgent.SetDestination(position);
        }
    }
}
