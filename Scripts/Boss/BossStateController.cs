using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BossAnimations;
using BossStates;
using UnityEngine.Animations.Rigging;

public class BossStateController : MonoBehaviour
{

    public BossStates.IBossState _bossState { get; private set; }
    public IBossAnimState _bossAnimState { get; private set; }
    public Rigidbody _rb { get; private set; }
    public NavMeshAgent _agent { get; private set; }
    public Animator _animator { get; private set; }
    public RuntimeAnimatorController _animatorController { get; set; }
    public BossMovement _bossMovement { get; private set; }
    public BossCombat _bossCombat { get; private set; }
    public BossAI _bossAI { get; private set; }
    public Transform _playerTransform { get; private set; }

    [SerializeField]
    private MultiAimConstraint headAimConstraint;

    [SerializeField]
    private Material _normalMaterial;
    [SerializeField]
    private Material _dissolveMaterial;

    public Rig _rig { get; private set; }
    public SkinnedMeshRenderer _mesh { get; private set; }
    public SkinnedMeshRenderer[] _meshes { get; private set; }
    public bool _isDead { get; set; }
    public bool _isOnOffMeshLinkPath { get; set; }
    public bool _isInSmoke { get; set; }
    private bool _isHeadAimEnabled;

    public Vector3 _searchingPositionBuffer { get; set; }

    public float _searchStartTime { get; set; }

    [HideInInspector] public Vector3 _BlockedEnemyPosition;

    private float _searchingTime;
    private float _seeAngleHalf;

    private float _checkDistance;
    private float _checkDistanceWhileInSmoke;

    private float _pushCounter;

    private Coroutine _blockCoroutine;

    private Coroutine _enableHeadAimCoroutine;
    private Coroutine _disableHeadAimCoroutine;
    private Coroutine _pushCoroutine;

    private Coroutine _dissolveCoroutine;

    private List<GameObject> SmokeTriggers;

    public List<GameObject> TouchingGrounds { get; private set; }
    public List<GameObject> TouchingRoofs { get; private set; }
    public List<GameObject> TouchingWalls { get; private set; }
    public List<GameObject> TouchingProps { get; private set; }

    public GameObject EyeObject;

    private char[] _arrayForChangeAnim;
    private char[] _newArrayForChangeAnim;

    private float _lastTimeWolfSpawned;

    private Material[] _materialsForDissolve;

    private void Awake()
    {
        TouchingGrounds = new List<GameObject>();
        TouchingRoofs = new List<GameObject>();
        TouchingWalls = new List<GameObject>();
        TouchingProps = new List<GameObject>();
        _rb = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();
        _bossMovement = GetComponent<BossMovement>();
        _bossAI = GetComponent<BossAI>();
        _bossCombat = GetComponent<BossCombat>();
        _animator = GetComponentInChildren<Animator>();
        SmokeTriggers = new List<GameObject>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _rig = GetComponentInChildren<Rig>();
        _mesh = GetComponentInChildren<SkinnedMeshRenderer>();
        _searchingTime = 5f;
        _seeAngleHalf = 80f;
        _checkDistance = 20f;
        _checkDistanceWhileInSmoke = 3.5f;
        _meshes = GetComponentsInChildren<SkinnedMeshRenderer>();

        var data = headAimConstraint.data.sourceObjects;
        data.SetTransform(0, GameManager._instance.PlayerHands.transform);
        headAimConstraint.data.sourceObjects = data;
        headAimConstraint.transform.parent.parent.GetComponent<RigBuilder>().Build();
    }
    private void Start()
    {
        EnterState(new BossStates.IdleMove());
        EnterAnimState(new BossAnimations.Idle());
    }
    public void TeleportAndDissolve(bool hasMovedAlready = false)
    {
        if (_bossAI._BossNumber != 3) return;
        GameManager._instance.CoroutineCall(ref _dissolveCoroutine, DissolveCoroutine(hasMovedAlready), this);
    }
    private IEnumerator DissolveCoroutine(bool hasMovedAlready)
    {

        List<GameObject> weapons = GetComponentInChildren<RagdollForWeapon>()._Weapons;
        List<MeshRenderer> renderers = new List<MeshRenderer>();
        string name = "";
        foreach (var weapon in weapons)
        {
            if (weapon.GetComponent<MeshRenderer>() == null) continue;
            renderers.Add(weapon.GetComponent<MeshRenderer>());
            renderers[renderers.Count - 1].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderers[renderers.Count - 1].material = weapon.GetComponent<PlaySoundOnCollision>()._dissolveMaterial;
        }
        foreach (var mesh in _meshes)
        {
            if (mesh.GetComponent<PlaySoundOnCollision>() != null)
            {
                mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mesh.material = _dissolveMaterial;
            }
            else if (_materialsForDissolve == null) 
            {
                _materialsForDissolve = mesh.materials;
                var yourMaterials = new Material[] { _dissolveMaterial, _dissolveMaterial, _dissolveMaterial };
                mesh.materials = yourMaterials;
                name = mesh.name;
            }
            else
            {
                var yourMaterials = new Material[] { _dissolveMaterial, _dissolveMaterial, _dissolveMaterial };
                mesh.materials = yourMaterials;
                name = mesh.name;
            }
        }

        while (_mesh.material.GetFloat("_CutoffHeight") > 0f)
        {
            foreach (var renderer in renderers)
            {
                renderer.material.SetFloat("_CutoffHeight", _mesh.material.GetFloat("_CutoffHeight") - 24f * Time.deltaTime);
            }
            foreach (var mesh in _meshes)
            {
                mesh.material.SetFloat("_CutoffHeight", _mesh.material.GetFloat("_CutoffHeight") - 18f * Time.deltaTime);
            }
            yield return null;
        }

        if (!hasMovedAlready && !(_bossState is BossStates.RetreatBoss1) && !(_bossState is BossStates.SpecialActionBoss1))
        {
            Vector3 direction = (GameManager._instance.PlayerRb.transform.position - _rb.transform.position).normalized;
            direction.y = 0f;
            _rb.transform.position = _rb.transform.position - (direction * Random.Range(6f, 12f));
        }

        _mesh.material.SetFloat("_CutoffHeight", 0f);
        foreach (var renderer in renderers)
        {
            renderer.material.SetFloat("_CutoffHeight", 0f);
        }
        foreach (var mesh in _meshes)
        {
            mesh.material.SetFloat("_CutoffHeight", 0f);
        }

        while (_mesh.material.GetFloat("_CutoffHeight") < 3f)
        {
            foreach (var renderer in renderers)
            {
                renderer.material.SetFloat("_CutoffHeight", _mesh.material.GetFloat("_CutoffHeight") + 12f * Time.deltaTime);
            }
            foreach (var mesh in _meshes)
            {
                mesh.material.SetFloat("_CutoffHeight", _mesh.material.GetFloat("_CutoffHeight") + 12f * Time.deltaTime);
            }
            yield return null;
        }

        foreach (var weapon in weapons)
        {
            if (weapon.GetComponent<PlaySoundOnCollision>()._normalMaterial != null)
            {
                weapon.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                weapon.GetComponent<MeshRenderer>().material = weapon.GetComponent<PlaySoundOnCollision>()._normalMaterial;
            }
        }
        foreach (var mesh in _meshes)
        {
            if (mesh.GetComponent<PlaySoundOnCollision>() != null)
            {
                mesh.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                mesh.GetComponent<SkinnedMeshRenderer>().material = mesh.GetComponent<PlaySoundOnCollision>()._normalMaterial;
            }
            else if (mesh.name.Equals(name) && _materialsForDissolve != null)
            {
                mesh.materials = _materialsForDissolve;
            }
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
        _bossState.DoState(_rb);
        _bossAnimState.DoState(_rb);
        ArrangeAnimStateParameter();
        ArrangeCombatStamina();
        CheckForPush();
        CheckForSpawnWolfs();
        _bossCombat.ArrangeBlockCounterTimer();
        GameManager._instance.ArrangeBossUI(_bossCombat._CombatStamina, _bossCombat._CombatStaminaLimit);
        ArrangeHeadAimWeight();
        //Debug.Log(_bossAnimState);
    }
    void LateUpdate()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isOnCutscene || GameManager._instance.isPlayerDead || _isDead) return;

        _bossState.DoStateLateUpdate(_rb);
        _bossAnimState.DoStateLateUpdate(_rb);
    }
    void FixedUpdate()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isOnCutscene || GameManager._instance.isPlayerDead || _isDead) return;

        _bossState.DoStateFixedUpdate(_rb);
        _bossAnimState.DoStateFixedUpdate(_rb);
    }
    public void EnterState(BossStates.IBossState newState)
    {
        if (_bossState != null)
            _bossState.Exit(_rb, newState);
        BossStates.IBossState oldState = _bossState;
        _bossState = newState;
        _bossState.Enter(_rb, oldState);
    }
    public void EnterAnimState(IBossAnimState newState)
    {
        if (_bossAnimState != null)
            _bossAnimState.Exit(_rb, newState);
        IBossAnimState oldState = _bossAnimState;
        _bossAnimState = newState;
        _bossAnimState.Enter(_rb, oldState);
    }

    public void BlendAnimationLocalPositions(float localX, float localZ)
    {
        _animator.SetFloat("LocalX", Mathf.Lerp(_animator.GetFloat("LocalX"), localX, Time.deltaTime * 3f));
        _animator.SetFloat("LocalZ", Mathf.Lerp(_animator.GetFloat("LocalZ"), localZ, Time.deltaTime * 3f));
    }
    private void ArrangeHeadAimWeight()
    {
        if ((GameManager._instance.PlayerRb.transform.position - transform.position).magnitude < 2.75f && _isHeadAimEnabled)
            DisableHeadAim();
    }
    private void CheckForPush()
    {
        if (_pushCounter > 0.75f && !_bossCombat._IsAttacking && !_bossCombat._IsDodging && !_bossCombat._IsBlocking)
        {
            _pushCounter = 0f;
            _bossCombat.Push();
        }
        else if ((GameManager._instance.PlayerRb.transform.position - transform.position).magnitude < 1.35f)
        {
            _pushCounter += Time.deltaTime;
        }
        else
        {
            _pushCounter = 0f;
        }
    }
    private void CheckForSpawnWolfs()
    {
        if (_bossAI._BossNumber == 3 && GameManager._instance.BossPhaseCounterBetweenScenes.transform.position.x == 2)
        {
            if (Time.time - _lastTimeWolfSpawned > 20f)
            {
                SpawnWolfs();
                _lastTimeWolfSpawned = Time.time;
            }
        }
    }
    private void SpawnWolfs()
    {
        GameObject wolf1 = Instantiate(PrefabHolder._instance.Wolf, new Vector3(5.06f, 2.25f, 47f), Quaternion.identity);
        GameObject wolf2 = Instantiate(PrefabHolder._instance.Wolf, new Vector3(9.66f, 2.25f, 47f), Quaternion.identity);

        wolf1.transform.LookAt(GameManager._instance.PlayerRb.transform);
        wolf2.transform.LookAt(GameManager._instance.PlayerRb.transform);

        wolf1.transform.localEulerAngles = new Vector3(0f, wolf1.transform.localEulerAngles.y, 0f);
        wolf2.transform.localEulerAngles = new Vector3(0f, wolf2.transform.localEulerAngles.y, 0f);
    }
    /// <returns>true if plays hand idle anim</returns>
    public bool ChangeAnimation(string name, float fadeTime = 0.2f, bool isInstantPlay = false)
    {
        if (name.Equals("Jump"))
        {
            EnterAnimState(new BossAnimations.Jump());
            _animator.CrossFadeInFixedTime(name, fadeTime);
            return false;
        }
        else if (name.Equals("Retreat") || name.Equals("GroundRetreat"))
        {
            EnterAnimState(new BossAnimations.Retreat());
            _animator.CrossFadeInFixedTime(name, fadeTime);
            return false;
        }
        else if (AnimNameWithoutNumber(name).Equals("Dodge"))
        {
            EnterAnimState(new BossAnimations.Dodge());
            _animator.CrossFadeInFixedTime("Block1", fadeTime);
            return false;
        }
        else if (name.Equals("Die"))
        {
            EnterAnimState(new BossAnimations.Die());
            _animator.CrossFadeInFixedTime(name, fadeTime);
            return false;
        }

        if (isInstantPlay)
            _animator.Play(name);
        else
            _animator.CrossFadeInFixedTime(name, fadeTime);

        if (name.Equals("Empty"))
            return true;
        return false;
    }
    private string AnimNameWithoutNumber(string name)
    {
        _arrayForChangeAnim = name.ToCharArray();
        _newArrayForChangeAnim = new char[_arrayForChangeAnim.Length - 1];
        for (int i = 0; i < _arrayForChangeAnim.Length - 1; i++)
        {
            _newArrayForChangeAnim[i] = _arrayForChangeAnim[i];
        }
        return new string(_newArrayForChangeAnim);
    }
    private void ArrangeAnimStateParameter()
    {
        bool isStanding = _agent.enabled ? _agent.velocity.magnitude < 0.1f : _rb.velocity.magnitude < 0.1f;
        _animator.SetBool("IsStanding", isStanding);
        if (_bossAnimState is BossAnimations.InAir && !_animator.GetCurrentAnimatorStateInfo(6).IsName("InAir") && !_animator.IsInTransition(6))
        {
            ChangeAnimation("InAir", 0.5f);
        }
        else if (_bossAnimState is BossAnimations.Jump && !_animator.GetCurrentAnimatorStateInfo(6).IsName("Jump") && !_animator.IsInTransition(6))
        {
            ChangeAnimation("Jump");
        }
        else if (_bossAnimState is BossAnimations.Idle || _bossAnimState is BossAnimations.Walk || _bossAnimState is BossAnimations.Run)
        {
            if (!_animator.GetCurrentAnimatorStateInfo(6).IsName("EmptyBody") && !_animator.IsInTransition(6))
            {
                ChangeAnimation("EmptyBody");
            }
        }

        if (_animator.GetCurrentAnimatorStateInfo(1).IsName("Empty"))
        {
            if (_agent.velocity.magnitude < 0.1f)
            {
                _animator.SetLayerWeight(4, Mathf.Lerp(_animator.GetLayerWeight(4), 0f, Time.deltaTime * 7f));
                _animator.SetLayerWeight(5, Mathf.Lerp(_animator.GetLayerWeight(5), 0f, Time.deltaTime * 7f));
            }
            else
            {
                _animator.SetLayerWeight(4, Mathf.Lerp(_animator.GetLayerWeight(4), 0f, Time.deltaTime * 7f));
                _animator.SetLayerWeight(5, Mathf.Lerp(_animator.GetLayerWeight(5), 1f, Time.deltaTime * 7f));
            }
        }
        else
        {
            _animator.SetLayerWeight(4, Mathf.Lerp(_animator.GetLayerWeight(4), 0.75f, Time.deltaTime * 7f));
            _animator.SetLayerWeight(5, Mathf.Lerp(_animator.GetLayerWeight(5), 0f, Time.deltaTime * 7f));
        }
    }
    private void ArrangeCombatStamina()
    {
        if (_bossState is BossStates.RetreatBoss1 || _bossState is BossStates.SpecialActionBoss1 || _bossState is BossStates.FastAttackBoss2 || _bossState is BossStates.TeleportBoss3)
        {
            _bossCombat._CombatStamina += Time.deltaTime * 0.75f * _bossCombat._CombatStaminaLimit / 100f;
        }
        else if (_bossCombat._IsInAttackPattern || _bossCombat._IsDodging || _bossCombat._IsBlocking)
        {
            _bossCombat._CombatStamina += Time.deltaTime * 1.2f * _bossCombat._CombatStaminaLimit / 100f;
        }
        else
        {
            _bossCombat._CombatStamina += Time.deltaTime * 2.85f * _bossCombat._CombatStaminaLimit / 100f;
        }
    }
    public void EnableHeadAim()
    {
        if (_enableHeadAimCoroutine != null)
            StopCoroutine(_enableHeadAimCoroutine);
        if (_disableHeadAimCoroutine != null)
            StopCoroutine(_disableHeadAimCoroutine);

        _isHeadAimEnabled = true;
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

        _isHeadAimEnabled = false;
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
    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null && collision.collider != null)
        {
            if (!TouchingGrounds.Contains(collision.collider.gameObject) && collision.collider.gameObject.layer == LayerMask.NameToLayer("Grounds"))
            {
                TouchingGrounds.Add(collision.collider.gameObject);
            }
            if (!TouchingRoofs.Contains(collision.collider.gameObject) && collision.collider.CompareTag("Roof"))
            {
                TouchingRoofs.Add(collision.collider.gameObject);
            }
            if (!TouchingWalls.Contains(collision.collider.gameObject) && collision.collider.CompareTag("WallTrigger"))
            {
                TouchingWalls.Add(collision.collider.gameObject);
            }
            if (!TouchingProps.Contains(collision.collider.gameObject) && GameManager._instance.IsProp(collision.collider))
            {
                TouchingProps.Add(collision.collider.gameObject);
            }
            if (collision.collider.CompareTag("Player") && GameManager._instance.PlayerLastSpeed > 13.5f && !GameManager._instance.isPlayerAttacking)
            {
                ChangeAnimation("Stun");
                SoundManager._instance.PlaySound(SoundManager._instance.SmallCrash, transform.position, 0.1f, false, UnityEngine.Random.Range(0.93f, 1.07f));
                //GameManager._instance.CoroutineCall(ref _pushCoroutine, PushCoroutine(), this);
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
            _rb.velocity = Vector3.Lerp(_rb.velocity, (direction * 1f * GameManager._instance.PlayerLastSpeed), Time.deltaTime * 15f);
            yield return null;
        }
        _rb.velocity = (direction * 1f * GameManager._instance.PlayerLastSpeed);
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
            if (TouchingRoofs.Contains(collision.collider.gameObject) && collision.collider.CompareTag("Roof"))
            {
                TouchingRoofs.Remove(collision.collider.gameObject);
            }
            if (TouchingWalls.Contains(collision.collider.gameObject) && collision.collider.CompareTag("WallTrigger"))
            {
                TouchingWalls.Remove(collision.collider.gameObject);
            }
            if (TouchingProps.Contains(collision.collider.gameObject) && GameManager._instance.IsProp(collision.collider))
            {
                TouchingProps.Remove(collision.collider.gameObject);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.gameObject != null)
        {
            if (other.CompareTag("Smoke") && !SmokeTriggers.Contains(other.gameObject))
            {
                SmokeTriggers.Add(other.gameObject);
            }
            if (!TouchingWalls.Contains(other.gameObject) && other.CompareTag("WallTrigger"))
            {
                TouchingWalls.Add(other.gameObject);
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
            if (TouchingWalls.Contains(other.gameObject) && other.CompareTag("WallTrigger"))
            {
                TouchingWalls.Remove(other.gameObject);
            }
        }
    }

    public void BlockOpen(Vector3 blockedEnemyPosition)
    {
        if (_bossState is BossStates.RetreatBoss1 || _bossState is BossStates.SpecialActionBoss1)
        {
            Debug.LogError("Special but block opened..");
            return;
        }

        GameManager._instance.CoroutineCall(ref _blockCoroutine, BlockCoroutine(), this);

        ChangeAnimation("Blocking");

        if (_bossCombat._IsInAttackPattern)
        {
            _bossCombat.StopAttackInstantly();
        }

        _bossCombat._IsBlocking = true;

        _BlockedEnemyPosition = blockedEnemyPosition;
        _bossMovement.AttackOrBlockRotation(false);
    }
    private IEnumerator BlockCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        _bossCombat._IsBlocking = false;
    }
    public void StopBlocking()
    {
        if (_blockCoroutine != null)
            StopCoroutine(_blockCoroutine);
        _bossCombat._IsBlocking = false;
    }
}
