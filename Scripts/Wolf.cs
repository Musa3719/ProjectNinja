using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering.HighDefinition;

public class Wolf : MonoBehaviour
{
    public bool IsDead { get; private set; }
    public bool IsAttacking { get; private set; }

    [SerializeField] private MultiAimConstraint headAimConstraint;
    [SerializeField] private GameObject _rangedWarning;
    [SerializeField] private Material _dissolveMaterial;

    private Collider[] _ragdollColliders;
    private GameObject _attackCollider;
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private Rigidbody _rb;

    private bool _isInIdle;
    private bool _isRunBackDirChangingToRight;
    private bool _isRunBackStopped;
    private bool _isOnRunBack;
    private float _viewRange;
    private float _attackRange;
    private float _lastTimeAttacked;
    private float _attackWaitTime;
    private float _runBackHorizontal;
    private float _chanceRunBackDirMultiplier;
    private float _runBackCounterForStopped;
    private float _backToIdleCounter;

    private Coroutine _attackCoroutine;
    private Coroutine _headAimCoroutine;
    private Coroutine _rangedWarningCoroutine;
    private Coroutine _dissolveCoroutine;

    private void Awake()
    {
        Spawn();
        _attackCollider = transform.Find("AttackCollider").gameObject;
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _isInIdle = true;
        _viewRange = 27f;
        _attackRange = 6.5f;
        _attackWaitTime = 1f;
        _chanceRunBackDirMultiplier = 1f;

        var data = headAimConstraint.data.sourceObjects;
        data.SetTransform(0, Camera.main.transform);
        headAimConstraint.data.sourceObjects = data;
        headAimConstraint.transform.parent.GetComponent<RigBuilder>().Build();
        GameManager._instance.CoroutineCall(ref _headAimCoroutine, DisableHeadAnim(), this);

    }

    private void Update()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isOnCutscene || GameManager._instance.isPlayerDead || IsDead || IsAttacking) return;

        _animator.SetFloat("VelocityMagnitude", _navMeshAgent.velocity.magnitude);

        if (_isInIdle)
        {
            Physics.Raycast(transform.position, (GameManager._instance.PlayerRb.transform.position - transform.position).normalized, out RaycastHit hit, _viewRange, GameManager._instance.LayerMaskForVisibleWithSolidTransparent);
            if (hit.collider != null && GetParent(hit.collider.transform).CompareTag("Player"))
            {
                float angle = Vector3.Angle((GameManager._instance.PlayerRb.transform.position - transform.position).normalized, transform.forward);
                bool isInAngle = angle < 75f;
                if (isInAngle || (GameManager._instance.PlayerRb.transform.position - transform.position).magnitude < 15f)
                {
                    _isInIdle = false;
                    _animator.CrossFade("WolfMove", 0.3f);
                    SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.WolfBarks), transform.position, Random.Range(0.3f, 0.5f), false, UnityEngine.Random.Range(0.9f, 1f));
                    if (Random.Range(1, 11) > 5)
                        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.WolfBarks), transform.position, Random.Range(0.3f, 0.5f), false, UnityEngine.Random.Range(0.8f, 1f));
                    if (Random.Range(1, 11) > 8)
                        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.WolfBarks), transform.position, Random.Range(0.3f, 0.5f), false, UnityEngine.Random.Range(0.8f, 0.9f));
                }
            }
        }
        else
        {
            Physics.Raycast(transform.position, (GameManager._instance.PlayerRb.transform.position - transform.position).normalized, out RaycastHit hit, _viewRange, GameManager._instance.LayerMaskForVisibleWithSolidTransparent);
            if (!(hit.collider != null && GetParent(hit.collider.transform).CompareTag("Player")))
            {
                _backToIdleCounter += Time.deltaTime;
                if (_backToIdleCounter > 7f)
                {
                    _isOnRunBack = false;
                    _backToIdleCounter = 0f;
                    _isInIdle = true;
                    _isRunBackStopped = false;
                    _chanceRunBackDirMultiplier = 1f;
                    DisableHeadAnim();
                    _animator.CrossFade("WolfIdle", 0.3f);
                    MoveToPosition(transform.position, transform.forward);
                }
            }
            else
                _backToIdleCounter = 0f;

            if (Random.Range(0, 1000) < Time.deltaTime * 60f)
                SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.WolfBarks), transform.position, Random.Range(0.35f, 0.45f), false, 0.7f);

            float distanceToPlayer = (GameManager._instance.PlayerRb.transform.position - transform.position).magnitude;
            
            if (!_rangedWarning.activeInHierarchy && (_lastTimeAttacked + _attackWaitTime < Time.time && distanceToPlayer < _attackRange * 1.25f) || (_lastTimeAttacked + _attackWaitTime - 0.25f < Time.time && distanceToPlayer < _attackRange))
            {
                _runBackHorizontal = 10f;
                _rangedWarning.SetActive(true);
                GameManager._instance.CoroutineCall(ref _rangedWarningCoroutine, CloseRangedWarningCoroutine(), this);
            }

            if (_lastTimeAttacked + _attackWaitTime < Time.time && distanceToPlayer < _attackRange)
            {
                _runBackHorizontal = 10f;
                Attack();
            }
            else if(distanceToPlayer >= _attackRange && _lastTimeAttacked + _attackWaitTime < Time.time)
            {
                _isOnRunBack = false;
                if (_isRunBackStopped)
                {
                    _isRunBackStopped = false;
                    GameManager._instance.CoroutineCall(ref _headAimCoroutine, DisableHeadAnim(), this);
                }
                _runBackHorizontal = 10f;
                Vector3 targetPos = GameManager._instance.PlayerRb.transform.position;
                MoveToPosition(targetPos, targetPos);
                _animator.SetFloat("RunSpeedMul", _navMeshAgent.velocity.magnitude / 5f);
            }
            else
            {
                _isOnRunBack = true;

                if (_runBackHorizontal == 10f)
                    _runBackHorizontal = Random.Range(-0.3f, 0.3f);
                else
                {
                    if (_runBackHorizontal > 0.5f && _isRunBackDirChangingToRight)
                    {
                        _chanceRunBackDirMultiplier = 2.5f;
                        _isRunBackDirChangingToRight = false;
                    }
                    else if (_runBackHorizontal < -0.5f && !_isRunBackDirChangingToRight)
                    {
                        _chanceRunBackDirMultiplier = 2.5f;
                        _isRunBackDirChangingToRight = true;
                    }
                    else if (_chanceRunBackDirMultiplier != 1f)
                    {
                        if (_runBackHorizontal > 0f && _isRunBackDirChangingToRight)
                        {
                            _chanceRunBackDirMultiplier = 1f;
                        }
                        else if (_runBackHorizontal < 0f && !_isRunBackDirChangingToRight)
                        {
                            _chanceRunBackDirMultiplier = 1f;
                        }
                    }
                    else if (Random.Range(0, 1000) < Time.deltaTime * 60f * 10f)
                    {
                        _isRunBackDirChangingToRight = !_isRunBackDirChangingToRight;
                    }

                    _runBackHorizontal += Time.deltaTime * Random.Range(0f, 0.5f) * _chanceRunBackDirMultiplier * (_isRunBackDirChangingToRight ? 1f : -1f);
                }

                if (_isRunBackStopped)
                {
                    Vector3 targetPos = transform.position + (GameManager._instance.PlayerRb.transform.position - transform.position).normalized * 0.4f + _runBackHorizontal * transform.right * 0.6f;
                    if ((GameManager._instance.PlayerRb.transform.position - transform.position).magnitude < 10f)
                        MoveToPosition(targetPos, targetPos, speedMul: 0.1f);
                    else
                        MoveToPosition(targetPos, targetPos, speedMul: 0.3f);
                    _animator.SetFloat("RunSpeedMul", _navMeshAgent.velocity.magnitude / 5f);
                }
                else
                {
                    Vector3 targetPos = transform.position - (GameManager._instance.PlayerRb.transform.position - transform.position).normalized + _runBackHorizontal * transform.right * 1f;
                    MoveToPosition(targetPos, targetPos, speedMul: 0.8f);
                    _animator.SetFloat("RunSpeedMul", _navMeshAgent.velocity.magnitude / 5f);
                }
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider != null && _isOnRunBack && !_isRunBackStopped && (collision.collider.CompareTag("Wall") || IsProp(collision.collider)))
        {
            _isRunBackStopped = true;
            GameManager._instance.CoroutineCall(ref _headAimCoroutine, EnableHeadAnim(), this);
        }
    }
    private bool IsProp(Collider other)
    {
        if (other == null) return false;
        Transform temp = other.transform;
        while (temp.parent != null)
        {
            if (temp.CompareTag("Prop") || other.CompareTag("Door")) return true;
            temp = temp.parent;
        }
        return false;
    }
    public void MoveToPosition(Vector3 position, Vector3 lookAtPos, float rotationLerpSpeed = 7.5f, float speedMul = 1f)
    {
        if (!_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh) return;

        _navMeshAgent.acceleration = 7.5f;
        _navMeshAgent.speed = 14f * speedMul;

        Vector3 direction = (lookAtPos - transform.position).normalized;
        transform.forward = Vector3.Lerp(transform.forward, new Vector3(direction.x, 0f, direction.z), Time.deltaTime * rotationLerpSpeed);

        if (_navMeshAgent.destination != position)
        {
            _navMeshAgent.SetDestination(position);
        }
    }
    
    private void Attack()
    {
        GameManager._instance.CoroutineCall(ref _headAimCoroutine, DisableHeadAnim(), this);
        _runBackCounterForStopped = 0f;
        _isRunBackStopped = false;
        _isOnRunBack = false;

        _lastTimeAttacked = Time.time;
        _attackWaitTime = Random.Range(2.25f, 4.75f);

        if (_attackCoroutine != null)
            StopCoroutine(_attackCoroutine);
        _attackCoroutine = StartCoroutine(AttackCoroutine());
    }
    private IEnumerator AttackCoroutine()
    {
        _rangedWarning.SetActive(false);

        IsAttacking = true;
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.WolfAttacks), transform.position, 0.65f, false, UnityEngine.Random.Range(0.85f, 1f));
        _animator.CrossFade("WolfAttack", 0.15f);

        _navMeshAgent.enabled = false;
        //_rb.isKinematic = false;
        _rb.velocity = transform.forward * 17f + Vector3.up * 3.5f;

        float startTime = Time.time;
        while (startTime + 0.55f > Time.time)
        {
            float tempY = _rb.velocity.y;
            _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z).magnitude * transform.forward;
            _rb.velocity = new Vector3(_rb.velocity.x, tempY, _rb.velocity.z);
            yield return null;
        }
        while (!IsGrounded())
        {
            float newX = _rb.velocity.x - 3f * Time.deltaTime * _rb.velocity.x > 0f ? 1f : -1f;
            float newZ = _rb.velocity.z - 3f * Time.deltaTime * _rb.velocity.z > 0f ? 1f : -1f;
            if (Mathf.Abs(_rb.velocity.x) < 1.5f)
                newX = _rb.velocity.x;
            if (Mathf.Abs(_rb.velocity.z) < 1.5f)
                newX = _rb.velocity.z;
            _rb.velocity = new Vector3(newX, _rb.velocity.y, newZ);
            yield return null;
        }
        //_animator.CrossFade("WolfIdle", 0.1f);
        //yield return new WaitForSeconds(0.1f);
        _animator.CrossFade("WolfMove", 0.2f);

        _navMeshAgent.enabled = true;
        //_rb.isKinematic = true;
        IsAttacking = false;
    }
    private IEnumerator CloseRangedWarningCoroutine()
    {
        yield return new WaitForSeconds(0.25f);
        _rangedWarning.SetActive(false);
    }
    private bool IsGrounded()
    {
        Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, 0.65f, GameManager._instance.LayerMaskForVisible); 
        if (hit.collider != null)
            return true;
        return false;
    }
    public void OpenAttackCollider()
    {
        _attackCollider.SetActive(true);
    }

    public void CloseAttackCollider()
    {
        _attackCollider.SetActive(false);
    }
    private IEnumerator EnableHeadAnim()
    {
        float startTime = Time.time;
        while (startTime + 1f > Time.time)
        {
            headAimConstraint.weight = Mathf.Lerp(headAimConstraint.weight, 1f, Time.deltaTime * 4f);
            yield return null;
        }
        headAimConstraint.weight = 1f;
    }
    private IEnumerator DisableHeadAnim()
    {
        float startTime = Time.time;
        while (startTime + 1f > Time.time)
        {
            headAimConstraint.weight = Mathf.Lerp(headAimConstraint.weight, 0f, Time.deltaTime * 4f);
            yield return null;
        }
        headAimConstraint.weight = 0f;
    }
    public void Die(Vector3 dir, float killersVelocityMagnitude, IKillObject killer)
    {
        if (IsDead) return;

        StopAllCoroutines();

        GetComponent<CapsuleCollider>().enabled = false;
        IsDead = true;
        GameManager._instance.CoroutineCall(ref _headAimCoroutine, DisableHeadAnim(), this);
        _rangedWarning.SetActive(false);

        DeathVFX(dir, killersVelocityMagnitude, killer); 

        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.WolfAttacks), transform.position, 0.4f, false, UnityEngine.Random.Range(0.85f, 1f));
        //_animator.CrossFade("WolfDeath", 0.15f);
        _animator.enabled = false;
        _navMeshAgent.enabled = false;
        _rb.velocity = Vector3.zero;
        _rb.useGravity = false;
        _rb.isKinematic = true;

        if (_attackCollider != null)
            _attackCollider.gameObject.SetActive(false);

        DissolveAndDestroy();
    }
    private void DissolveAndDestroy()
    {
        GameManager._instance.CallForAction(() => { GameManager._instance.CoroutineCall(ref _dissolveCoroutine, DissolveCoroutine(), this); }, 0.1f);
    }
    private IEnumerator DissolveCoroutine()
    {
        var meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var mesh in meshes)
        {
            mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mesh.material = _dissolveMaterial;
        }

        while (meshes[0].material.GetFloat("_CutoffHeight") > 0f)
        {
            foreach (var mesh in meshes)
            {
                mesh.material.SetFloat("_CutoffHeight", mesh.material.GetFloat("_CutoffHeight") - 1.5f * Time.deltaTime);
            }
            yield return null;
        }

        gameObject.SetActive(false);
    }
    private void Spawn()
    {
        GameManager._instance.CoroutineCall(ref _dissolveCoroutine, SpawnCoroutine(), this);
    }
    private IEnumerator SpawnCoroutine()
    {
        Dictionary<SkinnedMeshRenderer, Material> materials = new Dictionary<SkinnedMeshRenderer, Material>();
        var meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var mesh in meshes)
        {
            materials.Add(mesh, mesh.material);
            mesh.material = _dissolveMaterial;
        }

        foreach (var mesh in meshes)
        {
            mesh.material.SetFloat("_CutoffHeight", 0f);
        }
        while (meshes[0].material.GetFloat("_CutoffHeight") < 3f)
        {
            foreach (var mesh in meshes)
            {
                mesh.material.SetFloat("_CutoffHeight", mesh.material.GetFloat("_CutoffHeight") + 1.5f * Time.deltaTime);
            }
            yield return null;
        }
        foreach (var mesh in meshes)
        {
            mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            mesh.material = materials[mesh];
        }

    }
    private void DeathVFX(Vector3 dir, float killersVelocityMagnitude, IKillObject killer)
    {
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Crushs), transform.position, 0.2f, false, UnityEngine.Random.Range(1.15f, 1.25f));
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.WeaponHitSounds), transform.position, 0.3f, false, UnityEngine.Random.Range(0.95f, 1.1f));
        Vector3 bloodDir = (GameManager._instance.MainCamera.transform.position - transform.position).normalized;
        bloodDir.y = 0f;

        GameObject bloodVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.BloodVFX), transform.position, Quaternion.identity);
        bloodVFX.GetComponentInChildren<Rigidbody>().velocity = Vector3.up * 1.25f + killersVelocityMagnitude * dir * 0.75f;
        Destroy(bloodVFX, 5f);

        GameObject bleedingVFX = Instantiate(GameManager._instance.BleedingVFX, Vector3.zero, Quaternion.identity);
        bleedingVFX.transform.parent = transform;
        bleedingVFX.transform.localPosition = Vector3.up * UnityEngine.Random.Range(-0.1f, 0.25f);
        bleedingVFX.transform.forward = -dir;

        GameObject bloodPrefab = GameManager._instance.BloodDecalPrefabs[UnityEngine.Random.Range(0, GameManager._instance.BloodDecalPrefabs.Count)];
        GameObject decal = Instantiate(bloodPrefab, transform);
        float sizeMul = UnityEngine.Random.Range(0.75f, 1.25f);
        decal.GetComponent<DecalProjector>().size = new Vector3(decal.GetComponent<DecalProjector>().size.x * sizeMul, decal.GetComponent<DecalProjector>().size.y * sizeMul, decal.GetComponent<DecalProjector>().size.z);
        decal.GetComponent<DecalFollow>().FollowingTransform = transform;
        decal.GetComponent<DecalFollow>().LocalPosition = new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(0.2f, 0.7f), 0f);

        Vector3 pos = transform.position + dir * UnityEngine.Random.Range(0.25f, 1.25f);
        Physics.Raycast(pos, -Vector3.up, out RaycastHit hit, 50f, GameManager._instance.LayerMaskForVisible);
        pos = hit.collider == null ? transform.position - Vector3.up * 0.7f : hit.point;
        bloodPrefab = GameManager._instance.BloodDecalPrefabs[UnityEngine.Random.Range(0, GameManager._instance.BloodDecalPrefabs.Count)];
        GameObject groundDecal = Instantiate(bloodPrefab, pos, Quaternion.identity);
        groundDecal.transform.forward = hit.collider == null ? Vector3.up : -hit.normal;
        groundDecal.GetComponent<DecalProjector>().size = new Vector3(groundDecal.GetComponent<DecalProjector>().size.x * sizeMul, groundDecal.GetComponent<DecalProjector>().size.y * sizeMul, groundDecal.GetComponent<DecalProjector>().size.z);
        groundDecal.GetComponent<DecalProjector>().decalLayerMask = DecalLayerEnum.DecalLayerDefault;

        GameObject sparksVFX = Instantiate(GameManager._instance.ShiningSparksVFX[0], transform.position - transform.forward * 0.8f, Quaternion.identity);
        Destroy(sparksVFX, 4f);
    }
    
    private Transform GetParent(Transform trs)
    {
        Transform parentTransform = trs.transform;
        while (parentTransform.parent != null)
        {
            parentTransform = parentTransform.parent;
        }
        return parentTransform;
    }
}

   
