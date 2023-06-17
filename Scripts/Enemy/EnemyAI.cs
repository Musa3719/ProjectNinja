using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Rigidbody _rb { get; private set; }
    public static Dictionary<GameObject, EnemyAI> enemyAIs;

    public bool _isAttackWarned;
    public bool _isAttackFast;
    public Vector3 _attackPosition;

    private Vector3 _targetIdlePosition;
    private float _idleTimer;
    private NavMeshAgent _agent;
    private EnemyStateController _controller;
    private float _hearSoundCounter;


    private float _AgressiveValue;
    private float _DodgeOrBlockEfficiencyValue;
    private float _DodgeOverBlockValue;
    private float _StepBackValue;
    private float _ThrowValue;

    private float _lastStanceAnimCounter;

    private void Awake()
    {
        if (enemyAIs == null)
            enemyAIs = new Dictionary<GameObject, EnemyAI>();

        enemyAIs.Add(gameObject, this);

        _rb = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();
        _controller = GetComponent<EnemyStateController>();
        _targetIdlePosition = Vector3.zero;
    }
    private void OnEnable()
    {
        SoundManager.ProjectileTriggeredSoundArtificial += MakeArtificialSoundForProjectileHit;
    }
    private void OnDisable()
    {
        SoundManager.ProjectileTriggeredSoundArtificial -= MakeArtificialSoundForProjectileHit;
    }
    public void SetValuesForAI(float agressive, float dodgeOrBlockEfficiency, float dodgeOverBlock, float stepBack, float throwVariable)
    {
        _AgressiveValue = agressive;
        _DodgeOrBlockEfficiencyValue = dodgeOrBlockEfficiency;
        _DodgeOverBlockValue = dodgeOverBlock;
        _StepBackValue = stepBack;
        _ThrowValue = throwVariable;
    }
    private void Update()
    {
        if (GameManager._instance.isGameStopped || _controller._isDead) return;

        if (_controller._animator.GetCurrentAnimatorStateInfo(1).IsName("Stance"))
            _lastStanceAnimCounter = 0f;
        else if (_lastStanceAnimCounter < 10f)
            _lastStanceAnimCounter += Time.deltaTime;

        if (_agent.isOnOffMeshLink)
        {
            StartCoroutine(Parabola(_agent, 1.5f, 1f));
            _agent.CompleteOffMeshLink();
            _controller.ChangeAnimation("Jump");
            SoundManager._instance.PlaySound(SoundManager._instance.Jump, transform.position, 0.1f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        }

        if (_hearSoundCounter > 0f)
            _hearSoundCounter -= Time.deltaTime;
    }
    public List<string> ChooseAttackPattern()
    {
        int attackAnimCount = 0;
        List<string> selectedAnimNames = new List<string>();

        //selectedAnimNames.Add("Attack3");
        //selectedAnimNames.Add("Attack6");
        //return selectedAnimNames;

        switch (_controller._enemyCombat.WeaponType)
        {
            case WeaponTypeEnum.Sword:
                attackAnimCount = GameManager._instance._swordAttackAnimCount;
                break;
            case WeaponTypeEnum.Axe:
                attackAnimCount = GameManager._instance._axeAttackAnimCount;
                break;
            case WeaponTypeEnum.Halberd:
                attackAnimCount = GameManager._instance._halberdAttackAnimCount;
                break;
            case WeaponTypeEnum.Mace:
                attackAnimCount = GameManager._instance._maceAttackAnimCount;
                break;
            case WeaponTypeEnum.Hammer:
                attackAnimCount = GameManager._instance._hammerAttackAnimCount;
                break;
            case WeaponTypeEnum.Katana:
                attackAnimCount = GameManager._instance._katanaAttackAnimCount;
                break;
            default:
                break;
        }

        int lastSelectedIndex = -2;
        for (int i = 0; i < attackAnimCount; i++)
        {
            if (Random.Range(0, 8) < 3)
            {
                lastSelectedIndex = AddAnimToPattern(selectedAnimNames, i);
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

        if (selectedAnimNames.Count == 1 && UnityEngine.Random.Range(0, 2) == 0)
        {
            if (lastSelectedIndex + 1 < attackAnimCount)
                selectedAnimNames.Add("Attack" + (lastSelectedIndex + 2));
        }
        if (selectedAnimNames.Count == 0)
        {
            selectedAnimNames.Add("Attack" + (Random.Range(0, attackAnimCount) + 1).ToString());
        }

        GameManager.Shuffle(selectedAnimNames);

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
    public Vector3 GetStepBackPosition(NavMeshAgent agent)
    {
        Vector3 relativePos = -_rb.transform.forward * Random.Range(1.25f, 3f) + Random.Range(-1f, 1f) * _rb.transform.right * Random.Range(2.5f, 5f);
        return relativePos + agent.transform.position;
    }
    public Vector3 GetIdleMovementPosition(NavMeshAgent agent)
    {
        if (_targetIdlePosition == Vector3.zero || (_targetIdlePosition - _rb.position).magnitude < 1f)
        {
            if(_idleTimer > 1f)
            {
                GetRandomWalkablePosition(agent);
                _idleTimer -= 1f;
            }
            _idleTimer += Time.deltaTime;
        }
        return _targetIdlePosition;
    }
    private void GetRandomWalkablePosition(NavMeshAgent agent)
    {
        int i = 0;
        while (i < 6)
        {
            _targetIdlePosition = _rb.position + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            i++;
            if (agent.enabled && agent.isOnNavMesh && !agent.isOnOffMeshLink)
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
    
    public bool CheckForStepBack()
    {
        if (_controller._enemyCombat._IsDodging || _controller._enemyCombat._IsBlocking || _controller._enemyCombat._isInAttackPattern) return false;

        if (_StepBackValue * 6f * Time.deltaTime * 60f > Random.Range(0,1000))
        {
            return true;
        }
        return false;
    }
    public bool CheckForThrow()
    {
        if (_controller._enemyCombat._ThrowableItem.CountInterface == 0) return false;
        if (_controller._enemyCombat._IsRanged || _controller._enemyCombat._IsDodging || _controller._enemyCombat._IsBlocking || _controller._enemyCombat._isInAttackPattern) return false;

        if (_ThrowValue / 4f * Time.deltaTime * 60f > Random.Range(0, 1000))
        {
            return true;
        }
        return false;
    }
    public bool CheckForDodgeOrBlockWhenStunned()
    {
        return CheckForDodgeOrBlock(0.4f);
    }
    public bool CheckForDodgeOrBlock(float chanceMultiplier = 1f)
    {
        if (_controller._enemyCombat._IsDodging || _controller._enemyCombat._IsBlocking) return false;
        
        if (IsAttackComing())
        {
            if (IsAttackFast())
                chanceMultiplier *= 0.66f;
            if (_lastStanceAnimCounter < 0.7f)
                chanceMultiplier *= 2;
            if (_DodgeOrBlockEfficiencyValue * 0.6f * chanceMultiplier * 1000 >= Random.Range(0, 1000))
                return true;
            return false;
        }
        return false;
    }
    /// <summary>
    /// Checks for dodge over Block. If it returns false; it means humanoid will choose block, not dodge.
    /// </summary>
    public bool CheckForDodge()
    {
        if (_controller._enemyCombat._IsDodging || _controller._enemyCombat._IsBlocking) return false;

        if (_controller._enemyCombat._IsRanged) return true;//always choosing dodge over block

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
    private bool IsAttackFast()
    {
        if (_isAttackFast)
        {
            _isAttackFast = false;
            return true;
        }
        return false;
    }
    private bool IsRayHitPlayer(RaycastHit hit)
    {
        if (hit.collider == null) return false;
        Transform temp = hit.collider.transform;
        while (temp.parent != null)
        {
            temp = temp.parent;
        }
        return temp.CompareTag("Player");
    }
    public bool CheckForAttack()
    {
        if (_controller._enemyCombat._IsDodging || _controller._enemyCombat._IsBlocking || _controller._enemyCombat._isInAttackPattern) return false;

        if (_controller._enemyCombat._IsRanged)
        {
            Vector3 direction = (GameManager._instance.PlayerRb.transform.position - transform.position).normalized;
            Physics.Raycast(transform.position + direction, direction, out RaycastHit hit, _controller._enemyCombat.AttackRange, GameManager._instance.LayerMaskForVisible);
            if (IsRayHitPlayer(hit) && _controller._enemyCombat._IsAllowedToAttack)
            {
                if (_AgressiveValue * 40f * Time.deltaTime * 60f > Random.Range(0, 1000) && !CheckForAttackFriendlyFire())
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        else
        {
            float attackRange = _controller._enemyCombat.AttackRange / Mathf.Clamp(Mathf.Abs((_controller._playerTransform.position - _controller._rb.transform.position).y), 1f, 4f);

            if ((_controller._playerTransform.position - _controller._rb.transform.position).magnitude < attackRange && _controller._enemyCombat._IsAllowedToAttack)
            {
                if (_AgressiveValue * 20f * Time.deltaTime * 60f > Random.Range(0, 1000))
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        
    }
    public bool CheckForAttackFriendlyFire()
    {
        Vector3 direction = (GameManager._instance.PlayerRb.transform.position - transform.position).normalized;
        Physics.Raycast(transform.position + direction, direction, out RaycastHit hit, _controller._enemyCombat.AttackRange * 1.5f, GameManager._instance.LayerMaskForVisible);
        if (hit.collider!=null && hit.collider.CompareTag("HitBox") && GetParent(hit.collider.transform).CompareTag("Enemy"))
            return true;
        return false;
    }
    private Transform GetParent(Transform tr)
    {
        Transform parentTransform = tr.transform;
        while (parentTransform.parent != null)
        {
            parentTransform = parentTransform.parent;
        }
        return parentTransform;
    }
    public void HearArtificialSound(Vector3 position)
    {
        if (_hearSoundCounter <= 0f)
        {
            _controller._searchingPositionBuffer = position;
            _controller._isHearTriggered = true;
            _hearSoundCounter = 0.25f;
        }
    }

    public static void MakeArtificialSoundForPlayer(Vector3 position, float radius)
    {
        radius *= 1.25f;
        foreach (var nearEnemy in GameManager._instance.enemiesNearPlayer)
        {
            if ((position - nearEnemy.transform.position).magnitude < radius)
            {
                RaycastHit hit;
                Vector3 dir = (position - nearEnemy.transform.position).normalized;
                Physics.Raycast(nearEnemy.transform.position, dir, out hit, GameManager._instance.LayerMaskForVisible);
                
                if (hit.collider != null && (hit.collider.CompareTag("Player") || (hit.collider.transform.parent != null && hit.collider.transform.parent.CompareTag("Player"))))
                {
                    enemyAIs[nearEnemy].HearArtificialSound(position);
                }
            }
        }
    }
    
    public void MakeArtificialSoundForProjectileHit(Vector3 position, float radius)
    {
        radius *= 1.25f;
        if((position-transform.position).magnitude <= radius)
        {
            RaycastHit hit;
            Physics.Raycast(position, (transform.position - position).normalized, out hit, GameManager._instance.LayerMaskForVisible);

            if (hit.collider != null && GetParentObject(hit.collider.gameObject) == gameObject)
                HearArtificialSound(position);
        }
    }
    private GameObject GetParentObject(GameObject obj)
    {
        Transform temp = obj.transform;
        while (temp.transform.parent != null)
            temp = temp.transform.parent;
        return temp.gameObject;
    }
}

