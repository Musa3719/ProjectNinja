using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using EnemyAnimations;
using EnemyStates;
using UnityEngine.Animations.Rigging;

public class EnemyStateController : MonoBehaviour
{

    public EnemyStates.IEnemyState _enemyState { get; private set; }
    public IEnemyAnimState _enemyAnimState { get; private set; }
    public Rigidbody _rb { get; private set; }
    public NavMeshAgent _agent { get; private set; }
    public Animator _animator { get; private set; }
    public EnemyMovement _enemyMovement { get; private set; }
    public EnemyCombat _enemyCombat { get; private set; }
    public EnemyAI _enemyAI { get; private set; }
    public Transform _playerTransform { get; private set; }
    public Rig _rig { get; private set; }
    [SerializeField]
    private int _enemyNumber;

    public int EnemyNumber => _enemyNumber;

    [SerializeField]
    private MultiAimConstraint headAimConstraint;

    [SerializeField]
    private Material _normalMaterial;
    [SerializeField]
    private Material _dissolveMaterial;

    public SkinnedMeshRenderer _mesh { get; private set; }
    public SkinnedMeshRenderer[] _meshes { get; private set; }
    public bool _isDead { get; set; }
    public bool _isOnOffMeshLinkPath { get; set; }
    public bool _isInSmoke { get; set; }

    public Vector3 _searchingPositionBuffer { get; set; }

    public float _searchStartTime { get; set; }

    [HideInInspector] public Vector3 _BlockedEnemyPosition;

    private float _searchingTime;
    private float _seeAngleHalf;

    private float _checkDistance;
    private float _checkDistanceWhileInSmoke;

    private float _pushCounter;

    [HideInInspector] public bool _isHearTriggered;
    [HideInInspector] public float _lastTimeCheckForPlayerInSeenCalled;

    private List<GameObject> SmokeTriggers;

    public List<GameObject> TouchingGrounds { get; private set; }

    private Coroutine _blockCoroutine;

    private Coroutine _enableHeadAimCoroutine;
    private Coroutine _disableHeadAimCoroutine;
    private Coroutine _halfHeadAimCoroutine;
    private Coroutine _pushCoroutine;

    private Coroutine _dissolveCoroutine;

    
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();
        _enemyMovement = GetComponent<EnemyMovement>();
        _enemyAI = GetComponent<EnemyAI>();
        _enemyCombat = GetComponent<EnemyCombat>();
        _animator = GetComponentInChildren<Animator>();
        SmokeTriggers = new List<GameObject>();
        TouchingGrounds = new List<GameObject>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _rig = GetComponentInChildren<Rig>();
        _mesh = GetComponentInChildren<SkinnedMeshRenderer>();
        _searchingTime = 5f;
        _seeAngleHalf = 110f;
        _checkDistance = 25f;
        _checkDistanceWhileInSmoke = 2.75f;
        float yScaleMultiplier = Random.Range(0.975f, 1.04f);
        Transform model = transform.Find("Model");
        model.localScale = new Vector3(model.localScale.x, model.localScale.y * yScaleMultiplier, model.localScale.z);
        _meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        //_agent.baseOffset = _agent.baseOffset + (_agent.baseOffset - yScaleMultiplier * _agent.baseOffset);
    }
    private void Start()
    {
        GameManager._instance.allEnemies.Add(gameObject);
        EnterState(new EnemyStates.Idle());
        EnterAnimState(new EnemyAnimations.Idle());
    }
    public void TeleportAndDissolve(bool hasMovedAlready = false)
    {
        if (_dissolveCoroutine != null)
            StopCoroutine(_dissolveCoroutine);
        _dissolveCoroutine = StartCoroutine(DissolveCoroutine(hasMovedAlready));
    }
    private IEnumerator DissolveCoroutine(bool hasMovedAlready)
    {
        _mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _mesh.material = _dissolveMaterial;

        List<GameObject> weapons = GetComponentInChildren<RagdollForWeapon>()._Weapons;
        List<MeshRenderer> renderers = new List<MeshRenderer>();
        foreach (var weapon in weapons)
        {
            renderers.Add(weapon.GetComponent<MeshRenderer>());
            renderers[renderers.Count - 1].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderers[renderers.Count - 1].material = weapon.GetComponent<PlaySoundOnCollision>()._dissolveMaterial;
        }

        while (_mesh.material.GetFloat("_CutoffHeight") > 0f)
        {
            foreach (var renderer in renderers)
            {
                renderer.material.SetFloat("_CutoffHeight", _mesh.material.GetFloat("_CutoffHeight") - 24f * Time.deltaTime);
            }
            _mesh.material.SetFloat("_CutoffHeight", _mesh.material.GetFloat("_CutoffHeight") - 18f * Time.deltaTime);
            yield return null;
        }

        if (!hasMovedAlready)
        {
            Vector3 direction = (GameManager._instance.PlayerRb.transform.position - _rb.transform.position).normalized;
            direction.y = 0f;
            _rb.transform.position = _rb.transform.position - (direction * 6f);
        }

        _mesh.material.SetFloat("_CutoffHeight", 0f);
        foreach (var renderer in renderers)
        {
            renderer.material.SetFloat("_CutoffHeight", 0f);
        }

        while (_mesh.material.GetFloat("_CutoffHeight") < 3f)
        {
            foreach (var renderer in renderers)
            {
                renderer.material.SetFloat("_CutoffHeight", _mesh.material.GetFloat("_CutoffHeight") + 12f * Time.deltaTime);
            }
            _mesh.material.SetFloat("_CutoffHeight", _mesh.material.GetFloat("_CutoffHeight") + 12f * Time.deltaTime);
            yield return null;
        }

        _mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        _mesh.material = _normalMaterial;

        foreach (var weapon in weapons)
        {
            weapon.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            weapon.GetComponent<MeshRenderer>().material = weapon.GetComponent<PlaySoundOnCollision>()._normalMaterial;
        }
    }
    private void ArrangeIsInSmoke()
    {
        for (int i = 0; i < SmokeTriggers.Count; i++)
        {
            if (SmokeTriggers[i] == null)
            {
                SmokeTriggers.Remove(SmokeTriggers[i]);
                i--;
            }
        }

        if (SmokeTriggers.Count == 0)
            _isInSmoke = false;
        else
            _isInSmoke = true;
    }
    void Update()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isOnCutscene || GameManager._instance.isPlayerDead || _isDead) return;

        ArrangeIsInSmoke();
        _enemyState.DoState(_rb);
        _enemyAnimState.DoState(_rb);
        ArrangeAnimStateParameter();
        CheckForPush();
    }
    void LateUpdate()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isOnCutscene || GameManager._instance.isPlayerDead || _isDead) return;

        _enemyState.DoStateLateUpdate(_rb);
        _enemyAnimState.DoStateLateUpdate(_rb);
    }
    void FixedUpdate()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isOnCutscene || GameManager._instance.isPlayerDead || _isDead) return;

        _enemyState.DoStateFixedUpdate(_rb);
        _enemyAnimState.DoStateFixedUpdate(_rb);
    }
    public void EnterState(EnemyStates.IEnemyState newState)
    {
        if (_enemyState != null)
            _enemyState.Exit(_rb, newState);
        EnemyStates.IEnemyState oldState = _enemyState;
        _enemyState = newState;
        _enemyState.Enter(_rb, oldState);
    }
    public void EnterAnimState(IEnemyAnimState newState)
    {
        if (_enemyAnimState != null)
            _enemyAnimState.Exit(_rb, newState);
        IEnemyAnimState oldState = _enemyAnimState;
        _enemyAnimState = newState;
        _enemyAnimState.Enter(_rb, oldState);
    }

    public void BlendAnimationLocalPositions(float localX, float localZ)
    {
        _animator.SetFloat("LocalX", localX);
        _animator.SetFloat("LocalZ", localZ);
    }
    private void CheckForPush()
    {
        if (_pushCounter > 1f && !_enemyCombat._isAttacking && !_enemyCombat._IsDodging && !_enemyCombat._IsBlocking)
        {
            _pushCounter = 0f;
            _enemyCombat.Push();
        }
        else if ((GameManager._instance.PlayerRb.transform.position - transform.position).magnitude < 1.25f)
        {
            _pushCounter += Time.deltaTime;
        }
        else
        {
            _pushCounter = 0f;
        }
    }
    /// <returns>true if plays hand idle anim</returns>
    public bool ChangeAnimation(string name, float fadeTime = 0.2f, bool isInstantPlay = false)
    {
        if (name.Equals("Jump"))
        {
            EnterAnimState(new EnemyAnimations.Jump());
            _animator.CrossFadeInFixedTime(name, fadeTime);
            return false;
        }
        else if (AnimNameWithoutNumber(name).Equals("Dodge"))
        {
            EnterAnimState(new EnemyAnimations.Dodge());
            _animator.CrossFadeInFixedTime(name, fadeTime);
            return false;
        }
        else if (name.Equals("Die"))
        {
            EnterAnimState(new EnemyAnimations.Die());
            _animator.CrossFadeInFixedTime(name, fadeTime);
            return false;
        }

        if (isInstantPlay)
            _animator.Play(name);
        else
            _animator.CrossFadeInFixedTime(name, fadeTime);

        if(name.Equals("Empty"))
            return true;
        return false;
    }
    private string AnimNameWithoutNumber(string name)
    {
        char[] array = name.ToCharArray();
        char[] newArray = new char[array.Length - 1];
        for (int i = 0; i < array.Length - 1; i++)
        {
            newArray[i] = array[i];
        }
        return new string(newArray);
    }
    
    private void ArrangeAnimStateParameter()
    {
        bool isStanding = _agent.enabled ? _agent.velocity.magnitude < 0.1f : _rb.velocity.magnitude < 0.1f;
        _animator.SetBool("IsStanding", isStanding);
        if (_enemyAnimState is EnemyAnimations.InAir && !_animator.GetCurrentAnimatorStateInfo(6).IsName("InAir") && !_animator.IsInTransition(6))
        {
            ChangeAnimation("InAir", 0.5f);
        }
        else if (_enemyAnimState is EnemyAnimations.Jump && !_animator.GetCurrentAnimatorStateInfo(6).IsName("Jump") && !_animator.IsInTransition(6))
        {
            ChangeAnimation("Jump");
        }
        else if (_enemyAnimState is EnemyAnimations.Idle || _enemyAnimState is EnemyAnimations.Walk || _enemyAnimState is EnemyAnimations.Run)
        {
            if (!_animator.GetCurrentAnimatorStateInfo(6).IsName("EmptyBody") && !_animator.IsInTransition(6) && !_enemyCombat._isInAttackPattern)
            {
                ChangeAnimation("EmptyBody");
            }
        }

        if (_enemyCombat.WeaponType == WeaponTypeEnum.Katana)
        {
            if (_animator.GetCurrentAnimatorStateInfo(1).IsName("Empty"))
            {
                if (_agent.velocity.magnitude < 0.1f)
                {
                    _animator.SetLayerWeight(4, Mathf.Lerp(_animator.GetLayerWeight(4), 0f, Time.deltaTime * 3f));
                    _animator.SetLayerWeight(5, Mathf.Lerp(_animator.GetLayerWeight(5), 0f, Time.deltaTime * 3f));
                }
                else
                {
                    _animator.SetLayerWeight(4, Mathf.Lerp(_animator.GetLayerWeight(4), 0f, Time.deltaTime * 3f));
                    _animator.SetLayerWeight(5, Mathf.Lerp(_animator.GetLayerWeight(5), 1f, Time.deltaTime * 3f));
                }
            }
            else
            {
                _animator.SetLayerWeight(4, Mathf.Lerp(_animator.GetLayerWeight(4), 0f, Time.deltaTime * 3f));
                _animator.SetLayerWeight(5, Mathf.Lerp(_animator.GetLayerWeight(5), 0f, Time.deltaTime * 3f));
            }
        }
        else
        {
            if (_animator.GetCurrentAnimatorStateInfo(1).IsName("Empty"))
            {
                if (_agent.velocity.magnitude < 0.1f)
                {
                    _animator.SetLayerWeight(4, Mathf.Lerp(_animator.GetLayerWeight(4), 0f, Time.deltaTime * 3f));
                    _animator.SetLayerWeight(5, Mathf.Lerp(_animator.GetLayerWeight(5), 0f, Time.deltaTime * 3f));
                }
                else
                {
                    _animator.SetLayerWeight(4, Mathf.Lerp(_animator.GetLayerWeight(4), 0f, Time.deltaTime * 3f));
                    _animator.SetLayerWeight(5, Mathf.Lerp(_animator.GetLayerWeight(5), 1f, Time.deltaTime * 3f));
                }
            }
            else
            {
                _animator.SetLayerWeight(4, Mathf.Lerp(_animator.GetLayerWeight(4), 0.75f, Time.deltaTime * 3f));
                _animator.SetLayerWeight(5, Mathf.Lerp(_animator.GetLayerWeight(5), 0f, Time.deltaTime * 3f));
            }
        }
        
    }
    public void EnableHeadAim()
    {
        if (_enableHeadAimCoroutine != null)
            StopCoroutine(_enableHeadAimCoroutine);
        if (_disableHeadAimCoroutine != null)
            StopCoroutine(_disableHeadAimCoroutine);
        if (_halfHeadAimCoroutine != null)
            StopCoroutine(_halfHeadAimCoroutine);

        _enableHeadAimCoroutine = StartCoroutine(EnableHeadAimCoroutine());
    }
    private IEnumerator EnableHeadAimCoroutine()
    {
        while (headAimConstraint.weight < 0.9f)
        {
            headAimConstraint.weight = Mathf.Lerp(headAimConstraint.weight, 1f, Time.deltaTime * 4f);
            yield return null;
        }
        headAimConstraint.weight = 1f;
    }
    public void DisableHeadAim()
    {
        if (_enableHeadAimCoroutine != null)
            StopCoroutine(_enableHeadAimCoroutine);
        if (_disableHeadAimCoroutine != null)
            StopCoroutine(_disableHeadAimCoroutine);
        if (_halfHeadAimCoroutine != null)
            StopCoroutine(_halfHeadAimCoroutine);

        _disableHeadAimCoroutine = StartCoroutine(DisableHeadAimCoroutine());
    }
    private IEnumerator DisableHeadAimCoroutine()
    {
        while (headAimConstraint.weight > 0.1f)
        {
            headAimConstraint.weight = Mathf.Lerp(headAimConstraint.weight, 0f, Time.deltaTime * 4f);
            yield return null;
        }
        headAimConstraint.weight = 0f;
    }
    public void HalfHeadAim()
    {
        if (_enableHeadAimCoroutine != null)
            StopCoroutine(_enableHeadAimCoroutine);
        if (_disableHeadAimCoroutine != null)
            StopCoroutine(_disableHeadAimCoroutine);
        if (_halfHeadAimCoroutine != null)
            StopCoroutine(_halfHeadAimCoroutine);

        _halfHeadAimCoroutine = StartCoroutine(HalfHeadAimCoroutine());
    }
    private IEnumerator HalfHeadAimCoroutine()
    {
        float aimAmount = 0.75f;
        while (Mathf.Abs(headAimConstraint.weight - aimAmount) > 0.1f)
        {
            headAimConstraint.weight = Mathf.Lerp(headAimConstraint.weight, aimAmount, Time.deltaTime * 4f);
            yield return null;
        }
        headAimConstraint.weight = aimAmount;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.gameObject != null)
        {
            if (other.CompareTag("Smoke") && !SmokeTriggers.Contains(other.gameObject))
            {
                SmokeTriggers.Add(other.gameObject);
            }
        }
    }
   
    private void OnTriggerExit(Collider other)
    {
        if (other != null && other.gameObject != null)
        {
            if (other.CompareTag("Smoke") && SmokeTriggers.Contains(other.gameObject))
            {
                 SmokeTriggers.Remove(other.gameObject);
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null && collision.collider != null)
        {
            if (!TouchingGrounds.Contains(collision.collider.gameObject) && collision.collider.gameObject.layer == LayerMask.NameToLayer("Grounds"))
            {
                TouchingGrounds.Add(collision.collider.gameObject);
            }
            if (collision.collider.CompareTag("Player") && GameManager._instance.PlayerLastSpeed > 13.5f && !GameManager._instance.isPlayerAttacking)
            {
                ChangeAnimation("Stun");
                SoundManager._instance.PlaySound(SoundManager._instance.SmallCrash, transform.position, 0.3f, false, UnityEngine.Random.Range(0.93f, 1.07f));
                if (_pushCoroutine != null)
                    StopCoroutine(_pushCoroutine);
                _pushCoroutine = StartCoroutine(PushCoroutine());
            }
        }
    }
    private IEnumerator PushCoroutine()
    {
        _agent.enabled = false;
        _rb.isKinematic = false;
        Vector3 direction = (transform.position - GameManager._instance.PlayerRb.transform.position).normalized;
        float startTime = Time.time;
        while (startTime + 0.15f > Time.time)
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, (direction * GameManager._instance.PlayerLastSpeed * 2f), Time.deltaTime * 15f);
            yield return null;
        }
        _rb.velocity = (direction * 2f * GameManager._instance.PlayerLastSpeed);
        while (startTime + 0.65f > Time.time)
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, Vector3.zero, Time.deltaTime * 2.5f);
            yield return null;
        }
        _agent.enabled = true;
        _rb.isKinematic = true;
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision != null && collision.collider != null)
        {
            if (TouchingGrounds.Contains(collision.collider.gameObject) && collision.collider.gameObject.layer == LayerMask.NameToLayer("Grounds"))
            {
                TouchingGrounds.Remove(collision.collider.gameObject);
            }
        }
    }

    public void SearchingFromChasing()
    {
        _searchingPositionBuffer = _playerTransform.position + _playerTransform.GetComponent<Rigidbody>().velocity.normalized * 4f;
    }


    /// <summary>
    /// Checks for player in front of enemy
    /// </summary>
    /// <returns></returns>
    public bool CheckForPlayerInSeen()
    {
        bool isSeeingPlayer = false;
        bool isPlayerInSphere = false;
        Transform playerTransform = null;

        Collider[] hits = Physics.OverlapSphere(_rb.transform.position, 30f);

        foreach (var hit in hits)
        {
            if (hit != null && hit.gameObject.CompareTag("Player"))
            {
                isPlayerInSphere = true;
                playerTransform = hit.transform;
            }
        }

        if (isPlayerInSphere)
        {
            float localCheckDistance;
            if (_isInSmoke)
                localCheckDistance = _checkDistanceWhileInSmoke;
            else
            {
                localCheckDistance = _checkDistance;
                if (GameManager._instance.IsPlayerInSmoke)
                {
                    return false;
                }
            }

            RaycastHit hit;
            Physics.Raycast(_rb.transform.position, (playerTransform.position - _rb.transform.position).normalized, out hit, localCheckDistance, GameManager._instance.LayerMaskForVisible);

            if (hit.collider != null && hit.collider.gameObject.CompareTag("Player"))
            {
                Vector3 firstAngle = (playerTransform.position - _rb.transform.position).normalized;

                Vector3 secondAngle = _rb.transform.forward;

                //Debug.Log(Vector3.Angle(firstAngle, secondAngle));

                bool isInAngle = Vector3.Angle(firstAngle, secondAngle) <= _seeAngleHalf ? true : false;

                //Debug.Log(isInAngle);

                if(isInAngle || (playerTransform.position - _rb.transform.position).magnitude < 4f)
                    isSeeingPlayer = true;
            }
        }


        if (isSeeingPlayer)
        {
            return true;
        }

        return false;
    }

    public bool CheckForStillSearching()
    {
        if (_searchStartTime == 0f) return false;

        if (_searchStartTime + _searchingTime > Time.time)
        {
            return true;
        }

        return false;
    }

    public void BlockOpen(Vector3 blockedEnemyPosition)
    {
        if (_blockCoroutine != null)
            StopCoroutine(_blockCoroutine);
        _blockCoroutine = StartCoroutine(BlockCoroutine());

        ChangeAnimation("Blocking");

        if (_enemyCombat._isInAttackPattern)
        {
            _enemyCombat.StopAttackInstantly();
        }

        _enemyCombat._IsBlocking = true;
        _enemyMovement.AttackOrBlockRotation(false);
        _BlockedEnemyPosition = blockedEnemyPosition;
    }
    private IEnumerator BlockCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        _enemyCombat._IsBlocking = false;
    }
}
