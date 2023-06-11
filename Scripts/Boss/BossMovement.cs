using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossMovement : MonoBehaviour
{
    public Rigidbody _rb { get; private set; }
    public NavMeshAgent _navMeshAgent { get; private set; }
    public BossStateController _bossStateController { get; private set; }

    public float _moveSpeed { get; set; }
    public float _dodgeSpeed { get; private set; }
    public float _blockSpeed { get; private set; }
    public float _jumpPower { get; private set; }
    public float _runSpeed { get; set; }


    private float _distToGround;
    private float _xBound;
    private float _zBound;

    private float _groundCounter;

    private Coroutine _retreatCoroutine;
    private Coroutine ToGroundCoroutineHolder;

    public bool _isRetreatToSpecialAction { get; set; }

    public Transform _footTransform;
    public Transform _testTransformForMove;

    private Coroutine _attackOrBlockRotationCoroutine;
    private Coroutine _moveAfterAttackCoroutine;
    private Coroutine _blockMovementCoroutine;
    private Coroutine _dodgeCoroutine;

    public float _ChaseStoppingDistance { get; set; }

    private float _walkSoundCounter;
    private bool _isInBlockOrDodgeMovement;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _bossStateController = GetComponent<BossStateController>();
        _distToGround = GetComponent<Collider>().bounds.extents.y;
        _xBound = GetComponent<Collider>().bounds.extents.x;
        _zBound = GetComponent<Collider>().bounds.extents.z;
        _moveSpeed = 4f;
        _jumpPower = 7f;
        _runSpeed = 11.5f;
        _dodgeSpeed = 17f;
        _blockSpeed = 9f;
        _ChaseStoppingDistance = 3.5f;
    }
    public bool IsGrounded(float groundCounterLimit = 0.5f)
    {
        if (_bossStateController._agent.enabled)
        {
            if (_bossStateController._isOnOffMeshLinkPath)
                return false;
            return _navMeshAgent.isOnNavMesh;
        }
        else
        {
            if (!IsTouchingGround() && !ToGroundRaycast())
            {
                _groundCounter += Time.deltaTime;
                if(_groundCounter >= groundCounterLimit)
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
        if (GameManager._instance.isGameStopped || _bossStateController._bossCombat.IsDead || GameManager._instance.isPlayerDead) return;

        ArrangePlaneSound();
    }
    private void ArrangePlaneSound()
    {
        if (!_bossStateController._agent.enabled || _bossStateController._isOnOffMeshLinkPath || _bossStateController.TouchingGrounds.Count == 0) return;

        float speed = _bossStateController._agent.velocity.magnitude;
        GameObject groundCollider = _bossStateController.TouchingGrounds[_bossStateController.TouchingGrounds.Count - 1];
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
    public void Teleport()
    {
        _bossStateController.TeleportAndDissolve();
    }
    
    public void StopDodgeOrBlockMovement()
    {
        if (_blockMovementCoroutine != null)
            StopCoroutine(_blockMovementCoroutine);
        if (_dodgeCoroutine != null)
            StopCoroutine(_dodgeCoroutine);
        if (_attackOrBlockRotationCoroutine != null)
            StopCoroutine(_attackOrBlockRotationCoroutine);
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
        _blockMovementCoroutine = StartCoroutine(BlockMovementCoroutine(targetVel, _bossStateController._bossCombat._BlockMoveTime));
    }
    private IEnumerator BlockMovementCoroutine(Vector3 targetVel, float time)
    {
        _isInBlockOrDodgeMovement = true;
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

        if (!_bossStateController._bossCombat.IsDead && !(_bossStateController._bossState is BossStates.Retreat) && !(_bossStateController._bossState is BossStates.SpecialAction))
        {
            _navMeshAgent.enabled = true; _rb.isKinematic = true;
        }
        _isInBlockOrDodgeMovement = false;
    }
    public void MoveAfterAttack(bool isFirstAttack)
    {
        if (_isInBlockOrDodgeMovement) return;

        Vector3 direction = (GameManager._instance.PlayerRb.position + Random.Range(-0.25f, 0.25f) * GameManager._instance.PlayerRb.transform.right - transform.position).normalized;

        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;
        _bossStateController._animator.SetInteger("MoveAfterAttackNumber", Random.Range(1, 3));
        _bossStateController._animator.SetTrigger("MoveAfterAttack");
        GameManager._instance.CallForAction(() => { if (_bossStateController._bossCombat.IsDead) return; _navMeshAgent.enabled = true; _rb.isKinematic = true; }, 0.65f);

        float firstMultiplier = isFirstAttack ? 0.95f : 1f;
        float subtranctByDistance = (6.5f - (GameManager._instance.PlayerRb.position - transform.position).magnitude);
        if (subtranctByDistance < 0) subtranctByDistance = subtranctByDistance / 3f;
        else subtranctByDistance = 0f;
        float distanceMultiplier = (GameManager._instance.PlayerRb.position - transform.position).magnitude;// - subtranctByDistance;

        //if (subtranctByDistance < 0f) distanceMultiplier /= 1.6f;

        Vector3 targetVel = direction * 18f * firstMultiplier * Mathf.Clamp(distanceMultiplier, 1.5f, 7f) / 5.5f;
        if (_moveAfterAttackCoroutine != null)
            StopCoroutine(_moveAfterAttackCoroutine);
        _moveAfterAttackCoroutine = StartCoroutine(MoveAfterAttackCoroutine(targetVel));
    }
    private IEnumerator MoveAfterAttackCoroutine(Vector3 targetVel)
    {
        float startTime = Time.time;
        float firstMoveTime = 0.2f;
        float secondMoveTime = 0.5f;

        while (Time.time < startTime + firstMoveTime)
        {
            if (_bossStateController._bossCombat._IsAttackInterrupted) yield break;

            _rb.velocity = Vector3.Lerp(_rb.velocity, targetVel, Time.deltaTime * 5f);
            _rb.transform.forward = Vector3.Lerp(_rb.transform.forward, new Vector3(targetVel.normalized.x, 0f, targetVel.normalized.z), Time.deltaTime * 6f);
            ArrangeMoveAfterAttackGrounded();
            yield return null;
        }
        //_rb.velocity = targetVel;
        float newTime = Time.time;
        while (Time.time < newTime + secondMoveTime)
        {
            if (_bossStateController._bossCombat._IsAttackInterrupted) yield break;

            _rb.velocity = Vector3.Lerp(_rb.velocity, Vector3.zero, Time.deltaTime * 5f);
            _rb.transform.forward = Vector3.Lerp(_rb.transform.forward, new Vector3(targetVel.normalized.x, 0f, targetVel.normalized.z), Time.deltaTime * 8f);
            ArrangeMoveAfterAttackGrounded();
            yield return null;
        }
        _rb.velocity = Vector3.zero;
    }
    private void ArrangeMoveAfterAttackGrounded()
    {
        int c = 0;
        float decreaseAmountForFrame = 0.01f;
        float timer = 0f;
        bool isVFXThisFrame = false;
        while (!IsGrounded(0.1f))
        {
            if (IsStandingHigherFromLastGround())
            {
                c++;
                if (c > 1000)
                {
                    Debug.LogError("MoveAfterAttackTeleportWhileError");
                    break;
                }
                _rb.transform.Translate(-Vector3.up * decreaseAmountForFrame);
                
            }
            else
            {
                c++;
                if (c > 1000)
                {
                    Debug.LogError("MoveAfterAttackTeleportWhileError");
                    break;
                }
                _rb.transform.Translate(Vector3.up * decreaseAmountForFrame);
            }

            timer += decreaseAmountForFrame;
            if (timer > 0.35f && !isVFXThisFrame)
            {
                _bossStateController.TeleportAndDissolve(true);
                isVFXThisFrame = true;
            }

        }
    }
    private bool IsStandingHigherFromLastGround()
    {
        if(transform.position.y > GameManager._instance.BossArenaGroundYPosition)
        {
            return true;
        }
        return false;
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
        while (_bossStateController._bossCombat._IsAttacking)
        {
            if (_bossStateController._agent.enabled && !_bossStateController._isOnOffMeshLinkPath && _navMeshAgent.isOnNavMesh)
                MoveToPosition(transform.position, GameManager._instance.PlayerRb.transform.position, 8f, 0f);
            yield return null;
        }
    }
    private IEnumerator BlockRotationCoroutine()
    {
        while (_bossStateController._bossCombat._IsBlocking)
        {
            if (_bossStateController._agent.enabled && !_bossStateController._isOnOffMeshLinkPath && _navMeshAgent.isOnNavMesh)
                MoveToPosition(transform.position, GameManager._instance.PlayerRb.transform.position, 8f, 0f);
            yield return null;
        }
    }

    /// <returns>isDodgingToRight</returns>
    public bool Dodge()
    {
        if (_bossStateController._bossState is BossStates.Retreat || _bossStateController._bossState is BossStates.SpecialAction)
        {
            Debug.LogError("Special but dodge started..");
            return false;
        }

        Vector3 direction = _bossStateController._bossAI.GetDodgeDirection();
        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;
        GameManager._instance.CallForAction(() => { if (_bossStateController._bossCombat.IsDead || _bossStateController._bossState is BossStates.Retreat || _bossStateController._bossState is BossStates.SpecialAction) return; _navMeshAgent.enabled = true; _rb.isKinematic = true; }, _bossStateController._bossCombat._DodgeTime);
        Vector3 targetVel = direction * _dodgeSpeed;
        if (_dodgeCoroutine != null)
            StopCoroutine(_dodgeCoroutine);
        _dodgeCoroutine = StartCoroutine(DodgeCoroutine(targetVel));

        return direction.x >= 0 ? true : false;
    }
    private IEnumerator DodgeCoroutine(Vector3 targetVel)
    {
        _isInBlockOrDodgeMovement = true;
        float startTime = Time.time;
        while (Time.time < startTime + _bossStateController._bossCombat._DodgeTime / 8f)
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, targetVel, Time.deltaTime * 6f);
            yield return null;
        }
        _rb.velocity = targetVel;
        while (Time.time < startTime + _bossStateController._bossCombat._DodgeTime)
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, Vector3.zero, Time.deltaTime * 1.5f);
            yield return null;
        }
        _rb.velocity = Vector3.zero;
        _isInBlockOrDodgeMovement = false;
    }
    public void Retreat()
    {
        if (Random.Range(0, 4) == 0 && IsBackWallNotNear())
        {
            if (_retreatCoroutine != null)
                StopCoroutine(_retreatCoroutine);
            _retreatCoroutine = StartCoroutine(RetreatGroundCoroutine());
        }
        else if (Random.Range(0, 4) == 0 && (GameManager._instance.PlayerRb.transform.position - transform.position).magnitude > 13f)
        {
            if (_retreatCoroutine != null)
                StopCoroutine(_retreatCoroutine);
            _retreatCoroutine = StartCoroutine(RetreatJumpToPlayerCoroutine());
        }
        else
        {
            if (_retreatCoroutine != null)
                StopCoroutine(_retreatCoroutine);
            _retreatCoroutine = StartCoroutine(RetreatCoroutine());
        }
    }
    private bool IsBackWallNotNear()
    {
        Vector3 dir = -transform.forward;
        Physics.Raycast(transform.position + dir, dir, out RaycastHit hit, 20f);
        if (hit.collider == null) return true;
        if ((hit.point - transform.position).magnitude > 7f) return true;
        return false;
    }
    private IEnumerator RetreatJumpToPlayerCoroutine()
    {
        _rb.useGravity = false;
        _rb.isKinematic = false;
        _bossStateController._agent.enabled = false;

        _bossStateController.ChangeAnimation("HitGround");

        _rb.velocity = Vector3.zero;

        float timer = 0f;
        while (timer < 0.75f)
        {
            timer += Time.deltaTime;
            transform.forward = Vector3.Lerp(transform.forward, (GameManager._instance.PlayerRb.transform.position - transform.position).normalized, Time.deltaTime * 8f);
            transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
            yield return null;
        }

        _bossStateController._bossAI._bossSpecial.JumpToPlayer();
    }
    private IEnumerator RetreatGroundCoroutine()
    {
        _rb.useGravity = false;
        _rb.isKinematic = false;
        _bossStateController._agent.enabled = false;

        _bossStateController.ChangeAnimation("GroundRetreat");

        Vector3 retreatDir = -transform.forward;
        _rb.velocity = retreatDir * 48f;

        float groundActionTime = 0f;
        float startTime = Time.time;
        while (!IsTouchingAnyWalls())
        {
            if (startTime + 1.75f < Time.time || _rb.velocity.magnitude < 0.5f)
            {
                groundActionTime = _bossStateController._bossAI._bossSpecial.DoRandomGroundAction();
                if (groundActionTime == -1f) yield break;
                yield return new WaitForSeconds(1f);
                _rb.useGravity = true;
                _rb.isKinematic = true;
                _bossStateController._agent.enabled = true;
                _bossStateController.EnterState(new BossStates.Chase());
                yield break;
            }
            DecreaseSpeedForGroundRetreat();
            yield return null;
        }

        groundActionTime = _bossStateController._bossAI._bossSpecial.DoRandomGroundAction();
        if (groundActionTime == -1f) yield break;
        yield return new WaitForSeconds(1f);
        _rb.useGravity = true;
        _rb.isKinematic = true;
        _bossStateController._agent.enabled = true;
        _bossStateController.EnterState(new BossStates.Chase());
    }
    private void DecreaseSpeedForGroundRetreat()
    {
        Vector3 zeroTempVelocity = new Vector3(0f, _rb.velocity.y, 0f);
        if (_rb.velocity.magnitude > 0.5f)
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, zeroTempVelocity, Time.deltaTime / 3f);
        }
        else
            _rb.velocity = Vector3.zero;
    }
    private IEnumerator RetreatCoroutine()
    {
        _rb.useGravity = false;
        _rb.isKinematic = false;
        _bossStateController._agent.enabled = false;

        _bossStateController.ChangeAnimation("Retreat");

        Vector3 retreatDir = (Vector3.up * 0.55f - transform.forward * Random.Range(0.8f, 1.2f));
        _rb.velocity = retreatDir * 19f;
        float yStartVelocity = _rb.velocity.y;

        float startTime = Time.time;
        while (!IsTouchingAnyWalls())
        {
            if (startTime + 3.5f < Time.time || _rb.velocity.magnitude < 0.5f)
            {
                _bossStateController.EnterState(new BossStates.Chase());
                yield break;
            }
            Vector3 forwardToPlayerDirection = (GameManager._instance.PlayerRb.transform.position - _bossStateController._rb.transform.position).normalized;
            forwardToPlayerDirection.y = 0f;
            _bossStateController._rb.transform.forward = Vector3.Lerp(_bossStateController._rb.transform.forward, forwardToPlayerDirection, Time.deltaTime * 15f);
            DecreaseUpSpeedForRetreat(yStartVelocity);
            yield return null;
        }

        _bossStateController.ChangeAnimation("OnWall");
        //hit Wall particles
        float time = 0f;
        while (time <0.25f)
        {
            time += Time.deltaTime;
            _rb.velocity = Vector3.Lerp(_rb.velocity, Vector3.zero, Time.deltaTime * 4f);
            transform.forward = Vector3.Lerp(transform.forward, _bossStateController.TouchingWalls[0].transform.right, Time.deltaTime * 4f);
        }
        transform.forward = _bossStateController.TouchingWalls[0].transform.right;
        _rb.velocity = Vector3.zero;

        _isRetreatToSpecialAction = true;
    }
    private void DecreaseUpSpeedForRetreat(float yStartVelocity)
    {
        if (_rb.velocity.y > -yStartVelocity)
        {
            /*if (_rb.velocity.magnitude == 0f)
                _rb.velocity += retreatDir * Time.deltaTime * Mathf.Clamp((15f / 0.1f), 1f, 15f);
            else
                _rb.velocity += retreatDir * Time.deltaTime * Mathf.Clamp((15f / _rb.velocity.magnitude), 1f, 15f);*/
            _rb.velocity -= Vector3.up * Time.deltaTime * 8f;
        }
    }
    public bool IsTouchingAnyWalls()
    {
        return _bossStateController.TouchingWalls.Count > 0;
    }
    public bool IsTouchingGround()
    {
        return _bossStateController.TouchingGrounds.Count > 0;
    }
    public bool IsTouchingRoof()
    {
        return _bossStateController.TouchingRoofs.Count > 0;
    }
    public void ToGroundAfterRetreat()
    {
        if (ToGroundCoroutineHolder != null)
            StopCoroutine(ToGroundCoroutineHolder);
        ToGroundCoroutineHolder = StartCoroutine(ToGroundCoroutine());
    }
    private IEnumerator ToGroundCoroutine()
    {
        if(!IsTouchingGround())
            _rb.useGravity = true;
        while (!IsTouchingGround())
        {
            _rb.velocity -= Vector3.up * Time.deltaTime * 10f;
            yield return null;
        }
        _rb.velocity = Vector3.zero;

        if (_bossStateController._isDead) yield break;

        _bossStateController._agent.enabled = true;
        _rb.useGravity = true;
        _rb.isKinematic = true;
    }
    public void PhaseChange()
    {
        //phase changed anim sound etc
        //phase change position
        //cutscene etc
    }
    public void MoveToPosition(Vector3 position, Vector3 lookAtPos, float rotationLerpSpeed = 10f, float? speed=null)
    {
        if (!_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh || _navMeshAgent.isOnOffMeshLink) return;

        _navMeshAgent.acceleration = 5f;

        if (speed != null)
        {
            _navMeshAgent.speed = speed.Value;
        }
        else if ((position - transform.position).magnitude > 15f)
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
