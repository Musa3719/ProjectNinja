using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
    public Rigidbody _rb { get; private set; }
    public BossSpecial _bossSpecial;

    [HideInInspector] public bool _isAttackWarned;
    [HideInInspector] public Vector3 _attackPosition;

    [SerializeField]
    private int _bossNumber;
    public int _BossNumber => _bossNumber;

    private NavMeshAgent _agent;
    private BossStateController _controller;

    private float _AgressiveValue;
    private float _DodgeOverBlockValue;
    private float _RetreatValue;
    private float _IdleValue;

    private Coroutine _specialActionMovementCoroutine;

    private List<string> _selectedAnimNames;
    private void Awake()
    {
        _selectedAnimNames = new List<string>();
        _rb = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();
        _controller = GetComponent<BossStateController>();
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
                _AgressiveValue = 0.4f;
                _DodgeOverBlockValue = 0.1f;
                _RetreatValue = 0.5f;
                _IdleValue = 0.5f;
                _controller._animatorController = GameManager._instance.Boss1AnimatorGetter;
                break;
            case 2:
                _bossSpecial = new Boss2Special(_controller);
                _AgressiveValue = 2.5f;
                _DodgeOverBlockValue = 0.01f;
                _RetreatValue = 0.65f;
                _IdleValue = 0.05f;
                _controller._animatorController = GameManager._instance.Boss2AnimatorGetter;
                break;
            case 3:
                _bossSpecial = new Boss3Special(_controller);
                _AgressiveValue = 1.25f;
                _DodgeOverBlockValue = 0.175f;
                _RetreatValue = 1.25f;
                _IdleValue = 0.28f;
                _controller._animatorController = GameManager._instance.Boss3AnimatorGetter;
                break;
            default:
                break;
        }
    }

    private void Update()
    {
        if (GameManager._instance.isGameStopped || _controller._isDead || GameManager._instance.isOnCutscene) return;

        if (_agent.isOnOffMeshLink)
        {
            ToTheLink();
        }
        /*if ((transform.position - _lastFramePos).magnitude > 1.5f)
        {
            _controller._bossMovement.Teleport();
        }*/
    }
    #region SpecialActionMovement
    private void ToTheLink()
    {
        StartCoroutine(Parabola(_agent, 1.5f, 1f));
        _agent.CompleteOffMeshLink();
        _controller.ChangeAnimation("Jump");
        SoundManager._instance.PlaySound(SoundManager._instance.Jump, transform.position, 0.1f, false, UnityEngine.Random.Range(0.93f, 1.07f));
    }
    public void SpecialActionMovementBoss1(Rigidbody rb, float speed)
    {
        if (_specialActionMovementCoroutine != null)
            StopCoroutine(_specialActionMovementCoroutine);
        _specialActionMovementCoroutine = StartCoroutine(SpecialActionMovementCoroutine(rb, speed));
    }
    private IEnumerator SpecialActionMovementCoroutine(Rigidbody rb, float speed)
    {
        Coroutine velocityLerperCoroutine = null;

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
        while (timeCounter < waitTime && (wallHitPosition - transform.position).magnitude / speed > 1.1f && !_controller._bossMovement.IsTouchingAnyProp() && !_controller._bossMovement.IsTouchingAnyWall() && !_controller._bossMovement.IsTouchingGround() && !_controller._bossMovement.IsTouchingRoof())
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
        while (!_controller._bossMovement.IsTouchingAnyWall() && !_controller._bossMovement.IsTouchingAnyProp() && !_controller._bossMovement.IsTouchingGround() && !_controller._bossMovement.IsTouchingRoof())
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

        SoundManager._instance.PlaySound(SoundManager._instance.WeaponStickGround, transform.position, 0.4f, false, UnityEngine.Random.Range(0.8f, 0.85f));
        GameObject hitSmoke = Instantiate(GameManager._instance.HitSmokeVFX, transform.position + Vector3.up * 0.75f, Quaternion.identity);
        hitSmoke.GetComponentInChildren<Animator>().speed = 0.7f;
        Color color = hitSmoke.GetComponentInChildren<SpriteRenderer>().color;
        hitSmoke.GetComponentInChildren<SpriteRenderer>().color = new Color(color.r, color.g, color.b, 10f / 255f);
        hitSmoke.transform.localScale *= 10f;
        Destroy(hitSmoke, 5f);

        if (_controller._bossMovement.IsTouchingAnyWall() || _controller._bossMovement.IsTouchingAnyProp() || _controller._bossMovement.IsTouchingRoof())//do it for normal walls, not for props
        {
            if (_controller._bossMovement.IsTouchingAnyProp()) wallNormal = (GameManager._instance.PlayerRb.transform.position - transform.position).normalized;
            timeCounter = 0f;
            while (timeCounter < 0.2f)
            {
                timeCounter += Time.deltaTime;
                rb.transform.forward = Vector3.Lerp(rb.transform.forward, wallNormal, Time.deltaTime * 8f);
                yield return null;
            }
            rb.transform.forward = wallNormal;
        }


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
        SoundManager._instance.PlaySound(SoundManager._instance.WeaponStickGround, transform.position, 0.45f, false, UnityEngine.Random.Range(0.8f, 0.85f));
        hitSmoke = Instantiate(GameManager._instance.HitSmokeVFX, transform.position + Vector3.up * 0.75f, Quaternion.identity);
        hitSmoke.GetComponentInChildren<Animator>().speed = 0.7f;
        color = hitSmoke.GetComponentInChildren<SpriteRenderer>().color;
        hitSmoke.GetComponentInChildren<SpriteRenderer>().color = new Color(color.r, color.g, color.b, 10f / 255f);
        hitSmoke.transform.localScale *= 10f;
        Destroy(hitSmoke, 5f);

        yield return new WaitForSeconds(0.15f);

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
        if (rb.velocity.y > -startYSpeed)
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
        Vector3 wallNormal = rb.transform.forward;
        Vector3 wallHitPosition = Vector3.forward;

        Vector3 forwardDirection = rb.transform.forward * Random.Range(1.7f, 3f);
        RayTo4Direction(rb, (rb.transform.right + forwardDirection).normalized, ref minDistance, ref minDistanceDirection, ref wallNormal, ref wallHitPosition);
        RayTo4Direction(rb, (-rb.transform.right + forwardDirection).normalized, ref minDistance, ref minDistanceDirection, ref wallNormal, ref wallHitPosition);
        RayTo4Direction(rb, rb.transform.forward, ref minDistance, ref minDistanceDirection, ref wallNormal, ref wallHitPosition);

        if (minDistance == 100f)
        {
            Debug.LogError("Every WallToWall ray didn't hit a wall...");
        }
        if (_controller._bossMovement.IsTouchingAnyWall())
        {
            wallNormal = _controller.TouchingWalls[_controller.TouchingWalls.Count - 1].transform.right;
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
        _selectedAnimNames.Clear();

        //selectedAnimNames.Add("Attack5");
        //return selectedAnimNames;

        int lastSelectedIndex = -2;
        for (int i = 0; i < attackAnimCount; i++)
        {
            if (Random.Range(0, 8) < 4)
            {
                lastSelectedIndex = AddAnimToPattern(_selectedAnimNames, i);
            }
            /*if (lastSelectedIndex + 1 == i)
            {
                int random = Random.Range(0, 3);
                if (random == 0 || random == 1)
                {
                    lastSelectedIndex = AddAnimToPattern(selectedAnimNames, i);
                }
            }
            else if (Random.Range(0, 3) == 0)
            {
                lastSelectedIndex = AddAnimToPattern(selectedAnimNames, i);
            }*/
        }

        if (_selectedAnimNames.Count == 1 && UnityEngine.Random.Range(0, 2) == 0)
        {
            if (lastSelectedIndex + 1 < attackAnimCount)
                _selectedAnimNames.Add("Attack" + (lastSelectedIndex + 2));
        }
        if (_selectedAnimNames.Count == 0)
        {
            _selectedAnimNames.Add("Attack" + (Random.Range(0, attackAnimCount) + 1).ToString());
        }

        GameManager.Shuffle(_selectedAnimNames);

        return _selectedAnimNames;
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
        return transform.position + (transform.right * Random.Range(-1f, 1f) + transform.forward * Random.Range(-0.35f, 1f)) * 4f;
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
        if (_BossNumber == 3 && Time.time < 4.5f) return true;
        else if (_BossNumber == 2 && Time.time < 2f) return true;
        else if (_BossNumber == 1 && Time.time < 2f) return true;

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
            return UnityEngine.Random.Range(0, 1000) < Time.deltaTime * 60f * 10f * (1f - _IdleValue);
        }
        return UnityEngine.Random.Range(0, 1000) < Time.deltaTime * 60f * 25f * (1f - _IdleValue);
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
            return Random.Range(0, 1000) < Time.deltaTime * 60f * 3.5f * _RetreatValue;
        return Random.Range(0, 1000) < Time.deltaTime * 60f * 7f * _RetreatValue;
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
        bool allowAttack = false;
        if (_controller._bossCombat._IsDeflectedLately)
        {
            attackRange *= 1.5f;
            allowAttack = true;
        }
        else if (_controller._bossCombat._IsDodgedLately)
        {
            attackRange *= 1.5f;
            allowAttack = true;
        }

        if (allowAttack && (_controller._playerTransform.position - _controller._rb.transform.position).magnitude < attackRange)
        {
            return true;
        }
        else if ((_controller._playerTransform.position - _controller._rb.transform.position).magnitude < attackRange && _controller._bossCombat._IsAllowedToAttack)
        {
            if (_AgressiveValue * chanceMultiplier * 25f * Time.deltaTime * 60f > Random.Range(0, 1000))
            {
                return true;
            }
            return false;
        }
        return false;
    }
}

