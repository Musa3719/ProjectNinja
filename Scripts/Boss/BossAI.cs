using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
    public Rigidbody _rb { get; private set; }
    public BossSpecial _bossSpecial;

    public bool _isAttackWarned;
    public Vector3 _attackPosition;

    [SerializeField]
    private int _bossNumber;
    public int _BossNumber => _bossNumber;

    private Vector3 _targetIdlePosition;
    public float _idleTimer;
    private NavMeshAgent _agent;
    private BossStateController _controller;

    private float _AgressiveValue;
    private float _DodgeOverBlockValue;
    private float _RetreatValue;
    private float _IdleValue;

    private Coroutine _specialActionMovementCoroutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();
        _controller = GetComponent<BossStateController>();
        _targetIdlePosition = Vector3.zero;
    }
    private void Start()
    {
        ArrangeBossNumber();
    }
    private void ArrangeBossNumber()
    {
        switch (_bossNumber)
        {
            case 1:
                _bossSpecial = new Boss1Special(_controller);
                _AgressiveValue = 0.8f;
                _DodgeOverBlockValue = 0.1f;
                _RetreatValue = 0.4f;
                _IdleValue = 0.5f;
                _controller._bossCombat._attackNameToPrepareName = GameManager._instance.AttackNameToPrepareNameBoss;
                _controller._bossCombat._attackNameToHitOpenTime = GameManager._instance.NameToHitOpenTimeBoss1;
                _controller._animatorController = GameManager._instance.Boss1AnimatorGetter;
                break;
            case 2:
                _bossSpecial = new Boss2Special(_controller);
                _AgressiveValue = 0.8f;
                _DodgeOverBlockValue = 0f;
                _RetreatValue = 0.4f;
                _IdleValue = 0.5f;
                _controller._bossCombat._attackNameToPrepareName = GameManager._instance.AttackNameToPrepareNameBoss;
                _controller._bossCombat._attackNameToHitOpenTime = GameManager._instance.NameToHitOpenTimeBoss2;
                _controller._animatorController = GameManager._instance.Boss2AnimatorGetter;
                break;
            case 3:
                _bossSpecial = new Boss3Special(_controller);
                _AgressiveValue = 0.8f;
                _DodgeOverBlockValue = 0.2f;
                _RetreatValue = 0.4f;
                _IdleValue = 0.5f;
                _controller._bossCombat._attackNameToPrepareName = GameManager._instance.AttackNameToPrepareNameBoss;
                _controller._bossCombat._attackNameToHitOpenTime = GameManager._instance.NameToHitOpenTimeBoss3;
                _controller._animatorController = GameManager._instance.Boss3AnimatorGetter;
                break;
            default:
                break;
        }
    }
    
    private void Update()
    {
        if (GameManager._instance.isGameStopped || _controller._isDead) return;

        if (_agent.isOnOffMeshLink)
        {
            StartCoroutine(Parabola(_agent, 1.5f, 1f));
            _agent.CompleteOffMeshLink();
            _controller.ChangeAnimation("Jump");
            SoundManager._instance.PlaySound(SoundManager._instance.Jump, transform.position, 0.1f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        }
    }
    #region SpecialActionMovement
    public void SpecialActionMovement(Rigidbody rb, float speed)
    {
        if (_specialActionMovementCoroutine != null)
            StopCoroutine(_specialActionMovementCoroutine);
        _specialActionMovementCoroutine = StartCoroutine(SpecialActionMovementCoroutine(rb, speed));
    }
    private IEnumerator SpecialActionMovementCoroutine(Rigidbody rb, float speed)
    {
        Coroutine velocityLerperCoroutine = null;
        
        yield return new WaitForSeconds(0.5f);
        Vector3[] array = GetSpecialActionWallToWallDirection(rb);
        Vector3 toWallDirection = array[0];
        Vector3 wallNormal = array[1];
        Vector3 wallHitPosition = array[2];
        _controller.ChangeAnimation("InAir", 0.3f);

        Vector3 wallToWallSpeed = (toWallDirection + Vector3.up * Random.Range(0.5f, 0.8f)) * speed;
        if (velocityLerperCoroutine != null)
            StopCoroutine(velocityLerperCoroutine);
        velocityLerperCoroutine = StartCoroutine(VelocityLerper(rb, wallToWallSpeed, 0.5f));

        float startYSpeed = wallToWallSpeed.y;
        yield return new WaitForSeconds(0.2f);

        float waitTime = Random.Range(0.4f, 0.8f);
        float timeCounter = 0f;
        while (timeCounter < waitTime && (wallHitPosition - transform.position).magnitude / speed > 1.1f && !_controller._bossMovement.IsTouchingAnyWalls() && !_controller._bossMovement.IsTouchingGround() && !_controller._bossMovement.IsTouchingRoof())
        {
            timeCounter += Time.deltaTime;
            Vector3 forwardToPlayerDirection = (GameManager._instance.PlayerRb.transform.position - rb.transform.position).normalized;
            forwardToPlayerDirection.y = 0f;
            rb.transform.forward = Vector3.Lerp(rb.transform.forward, forwardToPlayerDirection, Time.deltaTime * 15f);
            ArrangeYSpeed(rb, startYSpeed);
            yield return null;
        }

        if (_bossSpecial.DoRandomWallAction() == -1f)
        {
            yield break;
        }

        float c = 0f;
        while (!_controller._bossMovement.IsTouchingAnyWalls() && !_controller._bossMovement.IsTouchingGround() && !_controller._bossMovement.IsTouchingRoof())
        {
            c += Time.deltaTime;
            if (c > 5f || rb.velocity.magnitude < 0.5f)
            {
                Debug.LogError("SpecialActionFlyTargetNotReached..");
                SpecialMoveExitFromError();
                yield break;
            }

            Vector3 forwardToPlayerDirection = (GameManager._instance.PlayerRb.transform.position - rb.transform.position).normalized;
            rb.transform.forward = Vector3.Lerp(rb.transform.forward, forwardToPlayerDirection, Time.deltaTime * 15f);
            ArrangeYSpeed(rb, startYSpeed);
            yield return null;
        }

        if (velocityLerperCoroutine != null)
            StopCoroutine(velocityLerperCoroutine);
        velocityLerperCoroutine = StartCoroutine(VelocityLerper(rb, Vector3.zero, 0.3f));

        _controller.ChangeAnimation("OnWall", 0.3f);

        timeCounter = 0f;
        while (timeCounter < 0.3f)
        {
            timeCounter += Time.deltaTime;
            rb.transform.forward = Vector3.Lerp(rb.transform.forward, wallNormal, Time.deltaTime * 5f);
            yield return null;
        }
        rb.transform.forward = wallNormal;

        yield return new WaitForSeconds(Random.Range(0.3f, 0.7f));

        _controller.ChangeAnimation("InAir", 0.5f);

        if (velocityLerperCoroutine != null)
            StopCoroutine(velocityLerperCoroutine);
        velocityLerperCoroutine = StartCoroutine(VelocityLerper(rb, (wallNormal * 0.3f - Vector3.up * 1f).normalized * speed / 1f, 0.25f));

        _rb.useGravity = true;

        c = 0f;
        while (!_controller._bossMovement.IsGrounded())
        {
            c += Time.deltaTime;
            if (c > 5f)
            {
                Debug.LogError("SpecialActionGroundNotReached..");
                SpecialMoveExitFromError();
                yield break;
            }
            Vector3 forwardToPlayerDirection = (GameManager._instance.PlayerRb.transform.position - rb.transform.position).normalized;
            forwardToPlayerDirection.y = 0f;
            rb.transform.forward = Vector3.Lerp(rb.transform.forward, forwardToPlayerDirection, Time.deltaTime * 15f);
            yield return null;
        }

        if (velocityLerperCoroutine != null)
            StopCoroutine(velocityLerperCoroutine);
        velocityLerperCoroutine = StartCoroutine(VelocityLerper(rb, Vector3.zero, 0.3f));
        _controller.ChangeAnimation("HitGround");

        yield return new WaitForSeconds(0.5f);

        int random = Random.Range(0, 3);
        if (random == 0 || random == 1)
        {
            float groundActionTime = _bossSpecial.DoRandomGroundAction();
            if (groundActionTime == -1f) yield break;
            yield return groundActionTime;
        }
        _controller.EnterState(new BossStates.Chase());
    }/*
    private Vector3[] GetSpecialActionRoofToWallDirection(Rigidbody rb)
    {
        float minDistance = 100f;
        Vector3 minDistanceDirection = Vector3.right;
        Vector3 wallNormal = Vector3.forward;
        Vector3 wallHitPosition = Vector3.forward;

        Vector3 downDirection = -Vector3.up * Random.Range(0.2f, 0.5f);
        RayTo4Direction(rb, (rb.transform.forward+ downDirection).normalized, ref minDistance, ref minDistanceDirection, ref wallNormal, ref wallHitPosition);
        RayTo4Direction(rb, (rb.transform.right + downDirection).normalized, ref minDistance, ref minDistanceDirection, ref wallNormal, ref wallHitPosition);
        RayTo4Direction(rb, (-rb.transform.right + downDirection).normalized, ref minDistance, ref minDistanceDirection, ref wallNormal, ref wallHitPosition);
        RayTo4Direction(rb, (-rb.transform.forward + downDirection).normalized, ref minDistance, ref minDistanceDirection, ref wallNormal, ref wallHitPosition);

        if (minDistance == 100f)
        {
            Debug.LogError("Every RoofToWall ray didn't hit a wall...");
        }

        return new Vector3[] { minDistanceDirection.normalized, wallNormal, wallHitPosition };
    }*/
    private void SpecialMoveExitFromError()
    {
        _controller.EnterState(new BossStates.Chase());
    }
    private void ArrangeYSpeed(Rigidbody rb, float startYSpeed)
    {
        if(rb.velocity.y > -startYSpeed)
        {
            rb.velocity -= Vector3.up * Time.deltaTime * 20f;
        }
    }
    private void RayTo4Direction(Rigidbody rb, Vector3 dir, ref float minDistance, ref Vector3 minDistanceDirection, ref Vector3 wallNormal, ref Vector3 wallHitPosition)
    {
        Physics.Raycast(rb.transform.position + dir, dir, out RaycastHit hit, 100f);
        if (hit.collider == null) return;
        //if (!hit.collider.CompareTag("Wall"))
            //Debug.LogError("WallToWall ray didn't hit any wall..");

        if ((hit.transform.position - rb.transform.position).magnitude < minDistance)
        {
            minDistance = (hit.transform.position - rb.transform.position).magnitude;
            minDistanceDirection = dir;
            wallNormal = hit.normal;
            wallHitPosition = hit.point;
        }
    }

    private Vector3[] GetSpecialActionWallToWallDirection(Rigidbody rb)
    {
        float minDistance = 100f;
        Vector3 minDistanceDirection = rb.transform.forward;
        Vector3 wallNormal = Vector3.forward;
        Vector3 wallHitPosition = Vector3.forward;

        Vector3 forwardDirection = rb.transform.forward * Random.Range(1.7f, 3f);
        RayTo4Direction(rb, (rb.transform.right + forwardDirection).normalized, ref minDistance, ref minDistanceDirection, ref wallNormal, ref wallHitPosition);
        RayTo4Direction(rb, (-rb.transform.right + forwardDirection).normalized, ref minDistance, ref minDistanceDirection, ref wallNormal, ref wallHitPosition);
        RayTo4Direction(rb, rb.transform.forward, ref minDistance, ref minDistanceDirection, ref wallNormal, ref wallHitPosition);

        if (minDistance == 100f)
        {
            Debug.LogError("Every WallToWall ray didn't hit a wall...");
        }

        return new Vector3[] { minDistanceDirection.normalized, wallNormal, wallHitPosition };
    }
    private IEnumerator VelocityLerper(Rigidbody rb, Vector3 targetVelocity, float fullLerpTime)
    {
        Vector3 startVelocity = rb.velocity;
        float startTime = Time.time;
        while ((Time.time - startTime) / fullLerpTime < 1f)
        {
            rb.velocity = Vector3.Lerp(startVelocity, targetVelocity, (Time.time - startTime) / fullLerpTime);
            yield return null;
        }
        rb.velocity = targetVelocity;
        
    }

#endregion
    public List<string> ChooseAttackPattern()
    {
        int attackAnimCount = _controller._bossCombat._AttackAnimCount;
        List<string> selectedAnimNames = new List<string>();

        //selectedAnimNames.Add("Attack2");
        //return selectedAnimNames;

        int lastSelectedIndex = -2;
        for (int i = 0; i < attackAnimCount; i++)
        {
            if (lastSelectedIndex + 1 == i)
            {
                int random = Random.Range(0, 3);
                if (random == 0 || random == 1)
                {
                    lastSelectedIndex = AddAnimToPattern(selectedAnimNames, i);
                }
            }
            else if (Random.Range(0, 5) == 0)
            {
                lastSelectedIndex = AddAnimToPattern(selectedAnimNames, i);
            }
        }

        if (selectedAnimNames.Count == 1)
        {
            if (lastSelectedIndex + 1 < attackAnimCount)
                selectedAnimNames.Add("Attack" + (lastSelectedIndex + 2));
        }
        if (selectedAnimNames.Count == 0)
        {
            selectedAnimNames.Add("Attack" + (Random.Range(0, attackAnimCount) + 1).ToString());
        }


        return selectedAnimNames;
    }
    private int AddAnimToPattern(List<string> selectedAnimNames, int i)
    {
        selectedAnimNames.Add("Attack" + (i + 1).ToString());
        return i;
    }
    IEnumerator Parabola(NavMeshAgent agent, float height, float duration)
    {
        _controller._isOnOffMeshLinkPath = true;

        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = height * 4.0f * (normalizedTime - normalizedTime * normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }

        _controller._isOnOffMeshLinkPath = false;
    }

    public Vector3 GetIdleMovementPosition(NavMeshAgent agent)
    {
        if (_targetIdlePosition == Vector3.zero || (_targetIdlePosition - _rb.position).magnitude < 1f)
        {
            GetRandomWalkablePosition(agent);
            _idleTimer = 2.5f;
        }
        else if (_idleTimer <= 0f)
        {
            GetRandomWalkablePosition(agent);
            _idleTimer += 2.5f;
        }
        return _targetIdlePosition;
    }
    private void GetRandomWalkablePosition(NavMeshAgent agent)
    {
        int i = 0;
        while (i < 6)
        {
            _targetIdlePosition = _rb.transform.position + (new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) * 1.5f);
            i++;
            if(agent.enabled && agent.isOnNavMesh && !agent.isOnOffMeshLink)
                agent.SetDestination(_targetIdlePosition);
            if (agent.pathStatus == NavMeshPathStatus.PathComplete)
                break;
        }
    }
    public Vector3 GetDodgeDirection()
    {
        int random = Random.Range(0, 3);
        float xTemp, extraZ = 0f;
        switch (random)
        {
            case 0:
                xTemp = 0;
                extraZ = -0.2f;
                break;
            case 1:
                xTemp = -0.3f;
                break;
            case 2:
                xTemp = 0.3f;
                break;
            default:
                xTemp = 0;
                break;
        }
        return transform.right * xTemp + transform.forward * (Random.Range(-0.8f, -0.6f) + extraZ);
    }
    public bool CheckForIdle()
    {
        if (_controller._bossCombat._IsDodging || _controller._bossCombat._IsBlocking || _controller._bossCombat._IsInAttackPattern) return false;

        if ((transform.position - _controller._playerTransform.position).magnitude > 6f)
        {
            return _IdleValue * 3.5f * Time.deltaTime * 60f > UnityEngine.Random.Range(0, 1000);
        }
        else if ((transform.position - _controller._playerTransform.position).magnitude > 12f)
        {
            return _IdleValue * 5.5f * Time.deltaTime * 60f > UnityEngine.Random.Range(0, 1000);
        }
        return false;
    }
    public bool CheckForExitIdle()
    {
        if (_controller._bossCombat._IsDodging || _controller._bossCombat._IsBlocking) return false;

        if ((transform.position - _controller._playerTransform.position).magnitude > 8f)
        {
            return UnityEngine.Random.Range(0, 1000) < Time.deltaTime * 60f * 5f * (1f - _IdleValue);
        }
        return UnityEngine.Random.Range(0, 1000) < Time.deltaTime * 60f * 15f * (1f - _IdleValue);
    }
    public bool CheckForRetreatToSpecialAction()
    {
        if (_controller._bossMovement._isRetreatToSpecialAction)
        {
            _controller._bossMovement._isRetreatToSpecialAction = false;
            return true;
        }
        return false;
    }
    public bool CheckForRetreat()
    {
        if (_controller._bossCombat._IsDodging || _controller._bossCombat._IsBlocking || _controller._bossCombat._IsInAttackPattern) return false;
        if (_controller._bossCombat._LastBlockOrDodgeTime + 1.5f > Time.time) return false;

        if (!IsRaycastHittingForRetreat()) return false;

        if ((_controller._playerTransform.position - _controller.transform.position).magnitude < 7f)
            return Random.Range(0, 1000) < Time.deltaTime * 60f * 2f * _RetreatValue;
        return Random.Range(0, 1000) < Time.deltaTime * 60f * 4f * _RetreatValue;
    }
    private bool IsRaycastHittingForRetreat()
    {
        Vector3 dir = (-transform.forward + Vector3.up * 0.35f).normalized;
        Physics.Raycast(transform.position + dir, dir, out RaycastHit hit, 50f);
        if (hit.collider != null)
            return true;
        return false;
    }
    public bool CheckForDodgeOrBlock()
    {
        if (IsAttackComing())
        {
            if (_controller._bossCombat._CombatStamina - _controller._bossCombat._DodgeOrBlockStaminaUse <= 0f)
            {
                return false;
            }
            return true;
        }
        return false;
    }
    /// <summary>
    /// Checks for dodge over Block. If it returns false; it means humanoid will choose block, not dodge.
    /// </summary>
    public bool CheckForDodge()
    {
        if (_DodgeOverBlockValue * 1000 >= Random.Range(0, 1000))
            return true;
        return false;
    }
    private bool IsAttackComing()
    {
        if (_isAttackWarned)
        {
            _isAttackWarned = false;
            return true;
        }
        return false;
    }
    public bool CheckForAttack()
    {
        if (_controller._bossCombat._IsDodging || _controller._bossCombat._IsBlocking || _controller._bossCombat._IsInAttackPattern) return false;

        float attackRange = _controller._bossCombat.AttackRange / Mathf.Clamp(Mathf.Abs((_controller._playerTransform.position - _controller._rb.transform.position).y), 1f, 4f);

        float chanceMultiplier = 1f;
        if (_controller._bossCombat._IsDeflectedLately)
        {
            attackRange *= 1.5f;
            chanceMultiplier = 10f;
        }
        else if (_controller._bossCombat._IsDodgedLately)
        {
            attackRange *= 2.25f;
            chanceMultiplier = 10f;
        }

        if ((_controller._playerTransform.position - _controller._rb.transform.position).magnitude < attackRange && _controller._bossCombat._IsAllowedToAttack)
        {
            if (_AgressiveValue * chanceMultiplier * 50f * Time.deltaTime * 60f > Random.Range(0, 1000))
            {
                return true;
            }
            return false;
        }
        return false;
    }
}

