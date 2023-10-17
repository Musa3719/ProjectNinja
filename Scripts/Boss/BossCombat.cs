using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.HighDefinition;

public class BossCombat : MonoBehaviour, IKillable
{
    private BossStateController _bossStateController;

    public GameObject Object => gameObject;
    public bool IsDead => _bossStateController._isDead;
    public bool IsBlockingGetter => _IsBlocking;
    public bool IsDodgingGetter => _IsDodging;
    public GameObject AttackCollider => _attackCollider.gameObject;
    public int InterruptAttackCounterGetter => _interruptAttackCounter;
    public Collider Collider => _collider;
    public float _CollisionVelocity => _collisionVelocity;

    private CapsuleCollider _collider;
    private Collider _attackCollider;
    private Collider _attackColliderWarning;
    private MeleeWeapon _meleeWeapon;

    public BoxCollider _rightBladeAttackCollider { get; private set; }
    public BoxCollider _rightBladeAttackWarning { get; private set; }
    public BoxCollider _leftBladeAttackCollider { get; private set; }
    public BoxCollider _leftBladeAttackWarning { get; private set; }


    [SerializeField]
    private GameObject _weaponObject;
    public GameObject WeaponObject => _weaponObject;

    private Collider[] _ragdollColliders;

    private Coroutine _attackCoroutine;
    private Coroutine _getWeaponBackCoroutine;
    private Coroutine _closeIsDeflectedLatelyCoroutine;
    private Coroutine _closeIsDodgedLatelyCoroutine;
    private Coroutine _openIsAllowedToAttackCoroutine;
    private Coroutine _closeIsAttackInterruptedCoroutine;
    private Coroutine _closeEyesCoroutine;

    public Dictionary<string, string> _attackNameToPrepareName;
    public Dictionary<string, float> _attackNameToHitOpenTime;

    public bool _IsStunned { get; private set; }

    [HideInInspector] public bool _IsInAttackPattern;
    [HideInInspector] public bool _IsAttacking;
    [HideInInspector] public bool _IsPreparingAttack;
    [HideInInspector] public bool _IsAttackInterrupted;
    public bool _IsDodging { get; set; }

    public bool _IsBlocking { get; set; }
    public bool _IsAllowedToAttack { get; set; }
    public bool _IsDeflectedLately { get; private set; }
    public bool _IsDodgedLately { get; private set; }
    public IThrowableItem _ThrowableItem { get; set; }

    [SerializeField]
    private List<float> _combatStaminaLimit;
    public float _CombatStaminaLimit => _combatStaminaLimit[(int)GameManager._instance.BossPhaseCounterBetweenScenes.transform.position.x - 1];
    private float _combatStamina;
    public float _CombatStamina { get => _combatStamina; set { 
            if (value > _CombatStaminaLimit) _combatStamina = _CombatStaminaLimit;
            else if (value < 0f) _combatStamina = 0f; else _combatStamina = value; }
            }
    public float _DodgeOrBlockStaminaUse { get; private set; }


    [SerializeField]
    private Transform _weaponHolderTransform;
    [SerializeField]
    private Transform _decalFollowTransform;

    [SerializeField]
    private int _attackAnimCount;
    public int _AttackAnimCount => _attackAnimCount;
    [SerializeField]
    private int _attackDeflectedAnimCount;
    public int _AttackDeflectedAnimCount => _attackDeflectedAnimCount;
    [SerializeField]
    private int _deflectAnimCount;
    public int _DeflectAnimCount => _deflectAnimCount;
    [SerializeField]
    private int _blockAnimCount;
    public int _BlockAnimCount => _blockAnimCount;

    [SerializeField]
    private Transform _sparkPosition;
    [SerializeField]
    private float _bossTypeSpeedMultiplier;
    [SerializeField]
    private float _attackRange;
    [SerializeField]
    private int _interruptAttackCounter;

    public float AttackRange => _attackRange;

    private int _lastAttackNumberForPattern;

    private float _attackWaitTime;
    private float _dodgeTime;
    public float _DodgeTime => _dodgeTime;
    private float _blockMoveTime;
    public float _BlockMoveTime => _blockMoveTime;

    private int _blockCounter;
    private float _blockCounterTimer;

    private float _crashStunCheckValue;
    private float _collisionVelocity;

    private Vector3 _weaponLocalPosition;
    private Vector3 _weaponLocalEulerAngles;

    [HideInInspector]
    public float _LastBlockOrDodgeTime;

    private void Awake()
    {
        _bossStateController = GetComponent<BossStateController>();
        _collider = GetComponent<CapsuleCollider>();
        if (_weaponObject != null)
        {
            _attackCollider = _weaponObject.transform.Find("AttackCollider").GetComponent<CapsuleCollider>();
            _attackColliderWarning = _weaponObject.transform.Find("AttackColliderWarning").GetComponent<CapsuleCollider>();
        }
        else
        {
            var components = GetComponentsInChildren<RotateBladeHumanoid>();
            foreach (var item in components)
            {
                if (item.name == "R_Blade")
                {
                    _rightBladeAttackCollider = item.transform.Find("AttackCollider").GetComponent<BoxCollider>();
                    _rightBladeAttackWarning = item.transform.Find("AttackColliderWarning").GetComponent<BoxCollider>();
                }
                else if (item.name == "L_Blade")
                {
                    _leftBladeAttackCollider = item.transform.Find("AttackCollider").GetComponent<BoxCollider>();
                    _leftBladeAttackWarning = item.transform.Find("AttackColliderWarning").GetComponent<BoxCollider>();
                }
            }
            _attackCollider = _rightBladeAttackCollider;
            _attackColliderWarning = _rightBladeAttackWarning;
        }
        _meleeWeapon = _attackCollider.GetComponent<MeleeWeapon>();

        _attackNameToPrepareName = new Dictionary<string, string>();
        _attackNameToHitOpenTime = new Dictionary<string, float>();

        _IsAllowedToAttack = true;
        _attackWaitTime = 1.25f;
        _dodgeTime = 0.8f;
        _blockMoveTime = 0.8f;
        _crashStunCheckValue = 13.5f;
        _CombatStamina = 100f;
        _DodgeOrBlockStaminaUse = 5f;

        if (_weaponObject != null)
        {
            _weaponLocalPosition = _weaponObject.transform.localPosition;
            _weaponLocalEulerAngles = _weaponObject.transform.localEulerAngles;
        }
       
    }

    private void Start()
    {
        if (_bossStateController._bossAI._BossNumber == 3)
        {
            ArrangeBoss3BladesToHands();
        }
        if (_bossStateController._bossAI._BossNumber == 2) _attackWaitTime = 0.25f;
        _bossStateController._bossMovement._moveSpeed = 4f * _bossTypeSpeedMultiplier;
        _bossStateController._bossMovement._runSpeed = 12f * _bossTypeSpeedMultiplier;
        ArrangeRagdoll();
    }
    private void FixedUpdate()
    {
        if (IsDead) return;

        if (_bossStateController._agent.enabled)
            _collisionVelocity = _bossStateController._agent.velocity.magnitude;
        else
            _collisionVelocity = 0f;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider != null && collision.collider.GetComponent<IKillable>() != null)
        {
            if (_collisionVelocity > _crashStunCheckValue || collision.collider.GetComponent<IKillable>()._CollisionVelocity > _crashStunCheckValue)
            {
                Stun(0.35f, false, collision.collider.transform);
            }
        }
    }
    public void ArrangeBlockCounterTimer()
    {
        if (_blockCounterTimer > 0)
            _blockCounterTimer -= Time.deltaTime;
        else if(_blockCounter > 0)
        {
            _blockCounter--;
            _blockCounterTimer = 1f;
        }
    }
    public void ThrowWeapon()
    {
        if (_bossStateController._isDead) return;

        //wait animTime * 0.83f
        _weaponObject.transform.parent = null;
        Rigidbody rb = (Rigidbody)_weaponObject.AddComponent(typeof(Rigidbody));
        WeaponInAir weaponInAir = (WeaponInAir)_weaponObject.AddComponent(typeof(WeaponInAir));
        weaponInAir.IgnoreCollisionCollider = Collider;
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.velocity = (GameManager._instance.PlayerRb.transform.position - transform.position).normalized * 37f;
        rb.angularVelocity = new Vector3(0f, 5f, 0f);
    }
    
    public void GetWeaponBack()
    {
        if (_bossStateController._isDead) return;

        if (_weaponObject.transform.parent == null && _getWeaponBackCoroutine == null)
        {
            _getWeaponBackCoroutine = StartCoroutine(GetWeaponBackCoroutine());
        }
    }
    private IEnumerator GetWeaponBackCoroutine()
    {
        Destroy(_weaponObject.GetComponent<WeaponInAir>());
        Destroy(_weaponObject.GetComponent<Rigidbody>());
        float startTime = Time.time;
        while ((_weaponObject.transform.position - _weaponHolderTransform.position).magnitude > 2f)
        {
            if (_bossStateController._isDead) yield break;
            float lerpSpeed = startTime + 0.4f > Time.time ? 2f : 4.5f;
            _weaponObject.transform.position = Vector3.Lerp(_weaponObject.transform.position, _weaponHolderTransform.position, Time.deltaTime * lerpSpeed);
            _weaponObject.transform.eulerAngles += Vector3.one * Time.deltaTime * 250f;
            yield return null;
        }

        _weaponObject.transform.position = _weaponHolderTransform.position;
        _weaponObject.transform.SetParent(_weaponHolderTransform);

        startTime = Time.time;
        while (startTime + 0.2f > Time.time)
        {
            if (_bossStateController._isDead) yield break;

            _weaponObject.transform.localPosition = Vector3.Lerp(_weaponObject.transform.localPosition, _weaponLocalPosition, Time.deltaTime * 15f);
            _weaponObject.transform.localEulerAngles = Vector3.Lerp(_weaponObject.transform.localEulerAngles, _weaponLocalEulerAngles, Time.deltaTime * 15f);
            yield return null;
        }

        _weaponObject.transform.localPosition = _weaponLocalPosition;
        _weaponObject.transform.localEulerAngles = _weaponLocalEulerAngles;
        _getWeaponBackCoroutine = null;
    }
    public void AttackDeflected(IKillable deflectedKillable)
    {
        StopAttackInstantly();

        if (!(_bossStateController._bossState is BossStates.RetreatBoss1) && !(_bossStateController._bossState is BossStates.SpecialAction))
        {
            _bossStateController._bossMovement.BlockMovement(deflectedKillable.Object.transform.position);
        }

        _CombatStamina -= _DodgeOrBlockStaminaUse / 2f;
        _bossStateController.ChangeAnimation(GetAttackDeflectedAnimName(), 0.2f, true);
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.AttackDeflecteds), transform.position, 0.5f, false, UnityEngine.Random.Range(0.93f, 1.07f));
    }
    public void ChangeStamina(float amount)
    {
        _CombatStamina += amount * _DodgeOrBlockStaminaUse;
    }
    public void StopAttackInstantly()
    {
        _LastBlockOrDodgeTime = Time.time;
        _IsInAttackPattern = false;
        _IsAttacking = false;
        _IsPreparingAttack = false;
        _bossStateController.EnableHeadAim();

        _IsAttackInterrupted = true;
        if (_closeIsAttackInterruptedCoroutine != null)
            StopCoroutine(_closeIsAttackInterruptedCoroutine);
        _closeIsAttackInterruptedCoroutine = StartCoroutine(CloseIsAttackInterruptedCoroutine(0.75f));

        _bossStateController.ChangeAnimation("Empty", 0.5f);
        _bossStateController.ChangeAnimation("EmptyBody", 0.5f);
        _attackColliderWarning.gameObject.SetActive(false);
        _attackCollider.gameObject.SetActive(false);

        _IsAllowedToAttack = false;
        if (_openIsAllowedToAttackCoroutine != null)
            StopCoroutine(_openIsAllowedToAttackCoroutine);
        _openIsAllowedToAttackCoroutine = StartCoroutine(OpenIsAllowedToAttackCoroutine(_attackWaitTime));
    }
    public IEnumerator CloseIsAttackInterruptedCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        _IsAttackInterrupted = false;
    }
    public IEnumerator OpenIsAllowedToAttackCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        _IsAllowedToAttack = true;
    }
    public void StopBlockingAndDodge()
    {
        _CombatStamina -= _DodgeOrBlockStaminaUse;//additional stamina use in the dodge
        _bossStateController.StopBlocking();
        if (_CombatStamina != 0)
        {
            bool isDodgingToRight = _bossStateController._bossMovement.Dodge();
            Dodge(isDodgingToRight);
        }
        
    }
    public void Dodge(bool isDodgingToRight)
    {
        if (_IsInAttackPattern)
        {
            StopAttackInstantly();

            /*if (UnityEngine.Random.Range(0, 101) >= 30)
            {
                _bossStateController._bossMovement.Teleport();
                return;
            }
            else
                StopAttackInstantly();*/
        }

        _LastBlockOrDodgeTime = Time.time;

        _IsDodgedLately = true;
        if (_closeIsDodgedLatelyCoroutine != null)
            StopCoroutine(_closeIsDodgedLatelyCoroutine);
        _closeIsDodgedLatelyCoroutine = StartCoroutine(CloseIsDodgedLatelyCoroutine());

        if (!_IsAllowedToAttack)
        {
            _IsAllowedToAttack = true;
            if (_openIsAllowedToAttackCoroutine != null)
                StopCoroutine(_openIsAllowedToAttackCoroutine);
        }


        _CombatStamina -= _DodgeOrBlockStaminaUse;
        _IsDodging = true;
        _bossStateController.ChangeAnimation(GetDodgeAnimName(isDodgingToRight));
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.ArmorWalkSounds), transform.position, 0.18f, false, UnityEngine.Random.Range(0.93f, 1.07f));

        Action CloseIsDodging = () => {
            _IsDodging = false;
        };
        GameManager._instance.CallForAction(CloseIsDodging, _dodgeTime);
    }
    private IEnumerator CloseIsDodgedLatelyCoroutine()
    {
        yield return new WaitForSeconds(1.5f);
        _IsDodgedLately = false;
    }
    public void DeflectWithBlock(Vector3 dir, IKillable attacker, bool isRangedAttack)
    {
        _LastBlockOrDodgeTime = Time.time;
        _IsBlocking = false;

        int chanceChange = 0;
        if (_blockCounter == 0) { chanceChange = 25; _blockCounter++; }
        else if (_blockCounter == 1) { chanceChange = 15; _blockCounter++; }
        _blockCounterTimer = 1f;


        if (UnityEngine.Random.Range(0, 100) < 60 + chanceChange && !_IsInAttackPattern)
        {
            GameObject sparksVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.SparksVFX), _sparkPosition.position, Quaternion.identity);
            Destroy(sparksVFX, 4f);
            GameObject combatSmokeVFX = Instantiate(GameManager._instance.CombatSmokeVFX, transform.position, Quaternion.LookRotation(attacker.Object.transform.position - transform.position));
            combatSmokeVFX.GetComponent<Rigidbody>().velocity = -transform.forward * 2f;
            Destroy(combatSmokeVFX, 4f);

            _bossStateController.ChangeAnimation(GetBlockAnimName(), 0.2f, true);
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Blocks), transform.position, 0.175f, false, UnityEngine.Random.Range(0.93f, 1.07f));

            _CombatStamina -= _DodgeOrBlockStaminaUse;
            if (!(_bossStateController._bossState is BossStates.RetreatBoss1) && !(_bossStateController._bossState is BossStates.SpecialAction))
            {
                _bossStateController._bossMovement.BlockMovement(_bossStateController._BlockedEnemyPosition);
            }

            _IsAllowedToAttack = false;
            if (_openIsAllowedToAttackCoroutine != null)
                StopCoroutine(_openIsAllowedToAttackCoroutine);
            _openIsAllowedToAttackCoroutine = StartCoroutine(OpenIsAllowedToAttackCoroutine(_attackWaitTime * 0.8f));
        }
        else
        {
            _IsDeflectedLately = true;
            if (_closeIsDeflectedLatelyCoroutine != null)
                StopCoroutine(_closeIsDeflectedLatelyCoroutine);
            _closeIsDeflectedLatelyCoroutine = StartCoroutine(CloseIsDeflectedLatelyCoroutine());

            _IsAllowedToAttack = false;
            if (_openIsAllowedToAttackCoroutine != null)
                StopCoroutine(_openIsAllowedToAttackCoroutine);
            _openIsAllowedToAttackCoroutine = StartCoroutine(OpenIsAllowedToAttackCoroutine(_attackWaitTime * 0.35f));

            GameObject sparksVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.ShiningSparksVFX), _sparkPosition.position, Quaternion.identity);
            Destroy(sparksVFX, 4f);
            GameObject combatSmokeVFX = Instantiate(GameManager._instance.CombatSmokeVFX, transform.position, Quaternion.LookRotation(attacker.Object.transform.position - transform.position));
            combatSmokeVFX.GetComponent<Rigidbody>().velocity = -transform.forward * 2f;
            Destroy(combatSmokeVFX, 4f);

            if (attacker != null && !isRangedAttack)
                attacker.AttackDeflected(this as IKillable);
            _bossStateController.ChangeAnimation(GetDeflectAnimName(), 0.2f, true);
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Deflects), transform.position, 0.175f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        }
    }
    private IEnumerator CloseIsDeflectedLatelyCoroutine()
    {
        yield return new WaitForSeconds(1.5f);
        _IsDeflectedLately = false;
    }
    private void PrepareAttack(string attackName)
    {
        return;
        if (!_attackNameToPrepareName.TryGetValue(attackName, out string x)) return;

        _IsPreparingAttack = true;

        StartCoroutine(PrepareAttackContinueOneFrameLater(attackName));
    }
    private IEnumerator PrepareAttackContinueOneFrameLater(string attackName)
    {
        _bossStateController.ChangeAnimation("Empty", 0.2f);

        if (_attackNameToPrepareName[attackName] != "Empty")
            yield return new WaitForSeconds(0.2f);

        if (!_IsAttackInterrupted)
        {
            bool isIdle = _bossStateController.ChangeAnimation(_attackNameToPrepareName[attackName], 0.2f);

            int animLayer = 1;
            if (isIdle)
            {
                GameManager._instance.CallForAction(() =>
                {
                    if (_IsAttackInterrupted) return;
                    _IsPreparingAttack = false;
                }, 0.05f);
                yield break;
            }


            yield return null;
            float time = 0f;
            bool isUsingCurrent = false;
            while (_bossStateController._animator.GetNextAnimatorClipInfo(animLayer).Length == 0)
            {
                time += Time.deltaTime;
                if (time > 0.05f && _bossStateController._animator.GetCurrentAnimatorClipInfo(animLayer).Length != 0)
                {
                    isUsingCurrent = true;
                    break;
                }
                yield return null;
            }
            float animTime = 0f;
            if (isUsingCurrent)
            {
                animTime = _bossStateController._animator.GetCurrentAnimatorClipInfo(animLayer)[0].clip.length / _bossStateController._animator.GetCurrentAnimatorStateInfo(animLayer).speed;
                animTime = animTime * 0.6f;
            }
            else
            {
                animTime = _bossStateController._animator.GetNextAnimatorClipInfo(animLayer)[0].clip.length / _bossStateController._animator.GetNextAnimatorStateInfo(animLayer).speed;
                animTime = animTime * 0.6f;
            }

            Action CloseIsPreparingToAttack = () => {
                if (_IsAttackInterrupted) return;
                _IsPreparingAttack = false;
            };
            GameManager._instance.CallForAction(CloseIsPreparingToAttack, animTime / 2f);
        }
    }
    public void AttackWithPattern()
    {
        if (_IsInAttackPattern) return;

        _IsAllowedToAttack = false;
        if (_openIsAllowedToAttackCoroutine != null)
            StopCoroutine(_openIsAllowedToAttackCoroutine);

        _IsInAttackPattern = true;
        _IsAttackInterrupted = false;
        if (_closeIsAttackInterruptedCoroutine != null)
            StopCoroutine(_closeIsAttackInterruptedCoroutine);

        List<string> patternNumbers = _bossStateController._bossAI.ChooseAttackPattern();
        if (_attackCoroutine != null)
            StopCoroutine(_attackCoroutine);
        _attackCoroutine = StartCoroutine(AttackPatternCoroutine(patternNumbers));
    }
    public IEnumerator AttackPatternCoroutine(List<string> patternNumbers)
    {
        _lastAttackNumberForPattern = -1;
        int c = 0;
        float lastTimeAttacked = Time.time;
        foreach (var attackName in patternNumbers)
        {
            if (lastTimeAttacked + 0.7f > Time.time)
                yield return new WaitForSeconds(0.7f - Mathf.Abs(Time.time - lastTimeAttacked));
            lastTimeAttacked = Time.time;

            if ((GameManager._instance.PlayerRb.transform.position - transform.position).magnitude > 12f)
            {
                break;
            }
            if (c == 0)
            {
                GameManager._instance.CallForAction(() => _bossStateController._bossMovement.MoveAfterAttack(true), 0.225f);
            }
            else
            {
                GameManager._instance.CallForAction(() => _bossStateController._bossMovement.MoveAfterAttack(false), 0.225f);
            }

            if (_lastAttackNumberForPattern == -1)
            {
                if (int.Parse(attackName.ToCharArray()[attackName.Length - 1].ToString()) != 1)
                {
                    PrepareAttack(attackName);
                    yield return new WaitWhile(() => _IsPreparingAttack);
                }
            }
            else if (_lastAttackNumberForPattern + 1 != int.Parse(attackName.ToCharArray()[attackName.Length - 1].ToString()))
            {
                PrepareAttack(attackName);
                yield return new WaitWhile(() => _IsPreparingAttack);
            }

            //check after prepare
            if (_IsAttackInterrupted)
            {
                yield break;
            }


            Attack(attackName);

            _lastAttackNumberForPattern = int.Parse(attackName.ToCharArray()[attackName.Length - 1].ToString());

            yield return new WaitWhile(() => _IsAttacking);

            //check after attack
            if (_IsAttackInterrupted)
            {
                yield break;
            }
            c++;
        }

        _bossStateController.ChangeAnimation("Empty", 0.35f);

        _IsAllowedToAttack = false;
        if (_openIsAllowedToAttackCoroutine != null)
            StopCoroutine(_openIsAllowedToAttackCoroutine);
        _openIsAllowedToAttackCoroutine = StartCoroutine(OpenIsAllowedToAttackCoroutine(_attackWaitTime));

        _IsInAttackPattern = false;

    }
    
    public void Attack(string attackName, int animLayer = 1, float animTime = 0f)
    {
        if (_IsAttacking || _IsDodging) return;
        _bossStateController.ChangeAnimation(attackName);

        int random = UnityEngine.Random.Range(0, 100);
        if (random > 80)
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.BossGrunts), transform.position, UnityEngine.Random.Range(0.2f, 0.4f), false, UnityEngine.Random.Range(0.93f, 1.07f));

        _IsAttacking = true;
        _bossStateController._bossMovement.AttackOrBlockRotation(true);
        _bossStateController.DisableHeadAim();

        if (!_attackNameToHitOpenTime.TryGetValue(attackName, out float hitOpenTime)) return;
        StartCoroutine(AttackContinueOneFrameLater(hitOpenTime, animLayer, animTime));
    }
    private IEnumerator AttackContinueOneFrameLater(float hitOpenTime, int animLayer, float animTime)
    {
        if (animTime == 0f)
        {
            yield return null;
            float time = 0f;
            bool isUsingCurrent = false;
            while (_bossStateController._animator.GetNextAnimatorClipInfo(animLayer).Length == 0)
            {
                time += Time.deltaTime;
                if (time > 0.25f && _bossStateController._animator.GetCurrentAnimatorClipInfo(animLayer).Length != 0)
                {
                    isUsingCurrent = true;
                    break;
                }
                yield return null;
            }

            if (isUsingCurrent)
            {
                animTime = _bossStateController._animator.GetCurrentAnimatorClipInfo(animLayer)[0].clip.length / _bossStateController._animator.GetCurrentAnimatorStateInfo(animLayer).speed;
                animTime = animTime * 0.85f;
                animTime -= time;
            }
            else
            {
                animTime = _bossStateController._animator.GetNextAnimatorClipInfo(animLayer)[0].clip.length / _bossStateController._animator.GetNextAnimatorStateInfo(animLayer).speed;
                animTime = animTime * 0.85f;
                animTime -= time;
            }
        }
        Action CloseIsAttacking = () => {
            if (_IsAttackInterrupted) return;
            _IsAttacking = false;
            _bossStateController.EnableHeadAim();
        };
        GameManager._instance.CallForAction(CloseIsAttacking, animTime);

        _attackColliderWarning.gameObject.SetActive(true);

        GameManager._instance.CallForAction(() => { if (_IsAttackInterrupted) return; _attackColliderWarning.gameObject.SetActive(false); }, animTime);

        /*Action OpenAttackCollider = () => {
            if (_bossStateController._isDead || _IsAttackInterrupted) return;
            _attackCollider.gameObject.SetActive(true);
        };
        GameManager._instance.CallForAction(OpenAttackCollider, hitOpenTime);

        GameManager._instance.CallForAction(() => { SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Attacks), transform.position, 0.3f, false, UnityEngine.Random.Range(0.93f, 1.07f)); }, hitOpenTime * 1.75f);

        Action CloseAttackCollider = () => {
            if (_IsAttackInterrupted) return;
            _attackCollider.gameObject.SetActive(false);
        };
        GameManager._instance.CallForAction(CloseAttackCollider, animTime);*/
    }
    public void OpenAttackCollider()
    {
        if (_bossStateController._isDead || _IsAttackInterrupted) return;

        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Attacks), transform.position, 0.3f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        _attackCollider.gameObject.SetActive(true);
    }
    public void CloseAttackCollider()
    {
        if (_IsAttackInterrupted) return;
        _attackCollider.gameObject.SetActive(false);
    }
    public void SingleAttack(string attackName, float animTime, int animLayer)
    {
        _IsInAttackPattern = true;
        _IsAttackInterrupted = false;
        if (_closeIsAttackInterruptedCoroutine != null)
            StopCoroutine(_closeIsAttackInterruptedCoroutine);

        Attack(attackName, animLayer, animTime);
        GameManager._instance.CallForAction(() => _IsInAttackPattern = false, animTime);

        _IsAllowedToAttack = false; 
        if (_openIsAllowedToAttackCoroutine != null)
            StopCoroutine(_openIsAllowedToAttackCoroutine);
        _openIsAllowedToAttackCoroutine = StartCoroutine(OpenIsAllowedToAttackCoroutine(_attackWaitTime));
    }
    public void AttackWarning(Collider collider, bool isFast, Vector3 attackPosition)
    {
        _bossStateController._bossAI._isAttackWarned = true;
        _bossStateController._bossAI._attackPosition = attackPosition;
    }
    public void BombDeflected()
    {
        _CombatStamina -= 1f;
    }
    public void Stun(float time, bool isSpeedChanges, Transform otherTransform)
    {
        /*_IsStunned = true;
          
        if(isSpeedChanges)
        {
            Vector3 tempSpeed = _bossStateController._rb.velocity;
            tempSpeed.y = 0f;
            Vector3 newSpeed = (transform.position - otherTransform.position).normalized * tempSpeed.magnitude;
            _bossStateController._rb.velocity = new Vector3(newSpeed.x, _bossStateController._rb.velocity.y, newSpeed.z); 
        }
         
        GameManager._instance.CallForAction(() => { _IsStunned = false; }, time);
        _bossStateController.ChangeAnimation("Stun");*/
    }
    public void Push()
    {
        _bossStateController.ChangeAnimation("Push", 0.05f);

        if (_bossStateController._bossAI._BossNumber == 1)
        {
            GameManager._instance._pushEvent?.Invoke((GameManager._instance.PlayerRb.transform.position - transform.position).normalized * 2f);
            GameManager._instance.EffectPlayerByDark();
        }
        else
            GameManager._instance._pushEvent?.Invoke((GameManager._instance.PlayerRb.transform.position - transform.position).normalized * 1f);

    }
    public void HitBreakable(GameObject breakable)
    {

    }
    private string GetDeflectAnimName()
    {
        string name = "Deflect" + UnityEngine.Random.Range(1, _DeflectAnimCount + 1);
        return name;
    }
    private string GetAttackDeflectedAnimName()
    {
        string name = "AttackDeflected" + UnityEngine.Random.Range(1, _AttackDeflectedAnimCount + 1);
        return name;
    }
    public string GetBlockAnimName()
    {
        string name = "Block" + UnityEngine.Random.Range(1, _BlockAnimCount + 1);
        return name;
    }
    private string GetDodgeAnimName(bool isDodgingToRight)
    {
        if (isDodgingToRight)
        {
            return "Dodge1";
        }
        else
        {
            return "Dodge2";
        }
    }


    public void ThrowKillObject(IThrowableItem item)
    {
        item.Use(_bossStateController._rb, _collider, false);
    }
    private void CloseEyes()
    {
        if (_closeEyesCoroutine != null)
            StopCoroutine(_closeEyesCoroutine);
        _closeEyesCoroutine = StartCoroutine(CloseEyesCoroutine());
    }
    private IEnumerator CloseEyesCoroutine()
    {
        var meshes = _bossStateController.EyeObject.GetComponentsInChildren<MeshRenderer>();
        Color emissiveColor = new Color(255f / 255f, 0f, 51f / 255f, 1f);
        float newIntensity = 11f;
        float startTime = Time.time;
        while(startTime + 3.5f > Time.time)
        {
            newIntensity = Mathf.Clamp(newIntensity - Time.deltaTime * 20f, 0f, 15f);
            foreach (var mesh in meshes)
            {
                mesh.material.SetFloat("_EmissiveIntensity", newIntensity);
                mesh.material.SetColor("_EmissiveColor", emissiveColor * newIntensity);
            }
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
        }
        _bossStateController.EyeObject.SetActive(false);
    }
    private void PhaseChange()
    {
        if (_IsInAttackPattern)
            StopAttackInstantly();

        if (_bossStateController._bossAI._BossNumber == 3)
        {
            ArrangeBoss3BladesToHands();
        }

        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Cuts), transform.position, 0.3f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.PlayerAttacks), transform.position, 0.45f, false, UnityEngine.Random.Range(1.1f, 1.25f));

        Vector3 VFXposition = transform.position + transform.forward * 0.5f;
        GameObject bloodVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.BloodVFX), VFXposition, Quaternion.identity);
        bloodVFX.GetComponentInChildren<Rigidbody>().velocity = Vector3.up * 2f + Vector3.right * UnityEngine.Random.Range(-0.1f, 0.1f) + Vector3.forward * UnityEngine.Random.Range(-0.1f, 0.1f);
        Destroy(bloodVFX, 5f);

        _IsAttackInterrupted = false;
        _IsDodging = false;
        _IsBlocking = false;
        _IsAllowedToAttack = true;
        _IsDeflectedLately = false;
        _IsDodgedLately = false;
        _bossStateController._bossMovement._isRetreatToSpecialAction = false;
        _bossStateController._isOnOffMeshLinkPath = false;
        _bossStateController._isInSmoke = false;
        _bossStateController._bossAI._isAttackWarned = false;
        _bossStateController._rb.isKinematic = true;
        _bossStateController._rb.useGravity = true;
        _bossStateController._agent.enabled = true;

        _bossStateController.EnterState(new BossStates.Chase());

        GameManager._instance.StopAllHumanoids();

        GameManager._instance.CloseBossUI();
        GameManager._instance.CallForAction(GameManager._instance.OpenBossUI, 2f);
        _bossStateController._bossAI._bossSpecial._phase++;

        string cutsceneName = "BossPhase" + _bossStateController._bossAI._bossSpecial._phase.ToString() + "Cutscene";
        GameManager._instance.EnterCutscene(cutsceneName);
        GameManager._instance.BossPhaseCounterBetweenScenes.transform.position = new Vector3(_bossStateController._bossAI._bossSpecial._phase, 0f, 0f);
        _CombatStamina = _CombatStaminaLimit;
        _bossStateController._bossMovement.PhaseChange();

        _bossStateController.StopAllCoroutines();
        _bossStateController._bossMovement.StopAllCoroutines();
        _bossStateController._bossCombat.StopAllCoroutines();
        _bossStateController._bossAI.StopAllCoroutines();
    }
    public void Die(Vector3 dir, float killersVelocityMagnitude, IKillObject killer)
    {
        if ((_bossStateController._bossState is BossStates.RetreatBoss1) || (_bossStateController._bossState is BossStates.SpecialAction))
        {
            _bossStateController.ChangeAnimation("BlockWhile");
            GameObject sparks = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.SparksVFX), _sparkPosition.position, Quaternion.identity);
            Destroy(sparks, 4f);
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Blocks), transform.position, 0.175f, false, UnityEngine.Random.Range(0.93f, 1.07f));
            return;
        }
        if (_CombatStamina > _DodgeOrBlockStaminaUse)
        {
            Debug.LogError("Die Error While Combat Stamina Is Enough");
            return;
        }

        if (_bossStateController._bossAI._bossSpecial._phase < _bossStateController._bossAI._bossSpecial._phaseCount)
        {
            PhaseChange();
            return;
        }

        if (IsDead) return;

        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Cuts), transform.position, 0.4f, false, UnityEngine.Random.Range(1.05f, 1.15f));
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.WeaponHitSounds), transform.position, 0.4f, false, UnityEngine.Random.Range(0.95f, 1.1f));
        //GameManager._instance.CallForAction(() => SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.EnemyDeathSounds), transform.position, 0.15f, false, UnityEngine.Random.Range(0.95f, 1.05f)), 1f);

        GameManager._instance.BossPhaseCounterBetweenScenes.transform.position = new Vector3(1f, 0f, 0f);
        if (SoundManager._instance.CurrentMusicObject != null)
        {
            Destroy(SoundManager._instance.CurrentMusicObject);
        }

        GameManager._instance.CloseBossUI();

        if (_bossStateController._bossAI._BossNumber == 1)
        {
            CloseEyes();
        }

        _bossStateController.StopAllCoroutines();
        _bossStateController._bossMovement.StopAllCoroutines();
        _bossStateController._bossCombat.StopAllCoroutines();
        _bossStateController._bossAI.StopAllCoroutines();

        //_enemyStateController._animator.SetTrigger("Death");
        Vector3 bloodDir = (GameManager._instance.MainCamera.transform.position - transform.position).normalized;
        bloodDir.y = 0f;
        Vector3 VFXposition = transform.position + bloodDir * 0.33f + Vector3.up * 0.07f;
        GameObject bloodVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.BloodVFX), VFXposition, Quaternion.identity);
        bloodVFX.GetComponentInChildren<Rigidbody>().velocity = Vector3.up * 1.25f + killersVelocityMagnitude * dir * 0.75f;
        Destroy(bloodVFX, 5f);

        GameObject bloodPrefab = GameManager._instance.BloodDecalPrefabs[UnityEngine.Random.Range(0, GameManager._instance.BloodDecalPrefabs.Count)];
        GameObject decal = Instantiate(bloodPrefab, transform);
        float size = UnityEngine.Random.Range(1.75f, 3f);
        decal.GetComponent<DecalProjector>().size = new Vector3(size, size, decal.GetComponent<DecalProjector>().size.z);
        decal.GetComponent<DecalFollow>().FollowingTransform = _decalFollowTransform;
        decal.GetComponent<DecalFollow>().LocalPosition = new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(0.2f, 0.7f), 0f);

        GameObject sparksVFX = Instantiate(GameManager._instance.ShiningSparksVFX[0], transform.position - transform.forward * 0.8f, Quaternion.identity);
        Destroy(sparksVFX, 4f);

        _bossStateController._isDead = true;
        _attackCollider.gameObject.SetActive(false);
        Vector3 currentVelocity = GetComponent<NavMeshAgent>().velocity;
        GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        ActivateRagdoll(dir, killersVelocityMagnitude, currentVelocity);

        if (!GameManager._instance.isPlayerDead)
        {
            GameManager._instance.SlowTime(1f);
            GameManager._instance.ActivatePassageToNextSceneFromBoss();
        }
    }
    private void ArrangeRagdoll()
    {
        _ragdollColliders = transform.Find("Model").GetComponentsInChildren<Collider>();

        foreach (var collider in _ragdollColliders)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast")) continue;

            collider.gameObject.AddComponent(typeof(PlaySoundOnCollision));
            collider.GetComponent<PlaySoundOnCollision>()._soundClip = SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.ArmorHitSounds);
            collider.isTrigger = true;
            collider.GetComponent<Rigidbody>().isKinematic = true;
            collider.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
            collider.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
            collider.gameObject.tag = "HitBox";
            collider.gameObject.layer = LayerMask.NameToLayer("TriggerHitBox");
        }
    }
    private void ActivateRagdoll(Vector3 dir, float killersVelocityMagnitude, Vector3 currentVelocity)
    {
        if (!IsDead) return;

        float forceMultiplier = 1000f;
        float forceUpMultiplier = -20f;

        _bossStateController._animator.enabled = false;
        //_enemyStateController._animator.avatar = null;
        _bossStateController._bossCombat._collider.enabled = false;
        _bossStateController._rig.weight = 0f;

        _bossStateController._rb.useGravity = false;
        _bossStateController._rb.velocity = Vector3.zero;
        _bossStateController._rb.isKinematic = true;

        foreach (SkinnedMeshRenderer mesh in _bossStateController._meshes)
        {
            mesh.updateWhenOffscreen = true;
        }

        if (_ragdollColliders == null)
            _ragdollColliders = transform.Find("Model").GetComponentsInChildren<Collider>();

        GetComponentInChildren<TransformBonePosition>().TransformPosition();
        Vector3 tempDirForWeapon = dir + new Vector3(UnityEngine.Random.Range(-0.25f, 0.25f), UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.25f, 0.25f));
        GetComponentInChildren<RagdollForWeapon>().SeperateWeaponsFromRagdoll(tempDirForWeapon, forceMultiplier, forceUpMultiplier, killersVelocityMagnitude);

        foreach (var collider in _ragdollColliders)
        {
            if (collider.GetComponent<PlaySoundOnCollision>() == null)
            {
                Debug.LogWarning(collider.name + " object does not have a playsoundoncollision component");
                continue;
            }
            collider.GetComponent<PlaySoundOnCollision>()._isEnabled = true;
            collider.isTrigger = false;
            collider.GetComponent<Rigidbody>().velocity = currentVelocity;
            collider.GetComponent<Rigidbody>().isKinematic = false;

            Vector3 tempDir = dir;
            tempDir += new Vector3(UnityEngine.Random.Range(-0.6f, 0.6f), UnityEngine.Random.Range(-0.25f, 0.25f), UnityEngine.Random.Range(-0.6f, 0.6f));
            //tempDir = tempDir.normalized; commented for power variety
            if (collider.gameObject.name == "CC_Base_Hip")
                collider.GetComponent<Rigidbody>().AddForce((tempDir * killersVelocityMagnitude * forceMultiplier / 4.5f + tempDir * forceMultiplier + Vector3.up * forceUpMultiplier) * 7.5f);

            collider.gameObject.layer = LayerMask.NameToLayer("Default");
        }

        //Destroy(_bossStateController._rb);
        //Destroy(_bossStateController._agent);
    }
    private void ArrangeBoss3BladesToHands()
    {
        GameObject rBlade = GameObject.Find("R_Blade");
        GameObject lBlade = GameObject.Find("L_Blade");

        rBlade.transform.parent = GameObject.Find("CC_Base_R_Hand").transform;
        lBlade.transform.parent = GameObject.Find("CC_Base_L_Hand").transform;

        Vector3 rBladeNewPos = new Vector3(0.001355575f, 0.1047062f, -0.004732913f);
        Vector3 rBladeNewRot = new Vector3(-108.37f, -21.97302f, 113.927f);
        Vector3 lBladeNewPos = new Vector3(-0.005183703f, 0.09486907f, -0.01518293f);
        Vector3 lBladeNewRot = new Vector3(-47.185f, 180.631f, -290.029f);

        rBlade.transform.localPosition = rBladeNewPos;
        rBlade.transform.localEulerAngles = rBladeNewRot;
        lBlade.transform.localPosition = lBladeNewPos;
        lBlade.transform.localEulerAngles = lBladeNewRot;

        lBlade.GetComponent<RotateBladeHumanoid>().DisableHaveWeapon();
        rBlade.GetComponent<RotateBladeHumanoid>().DisableHaveWeapon();
    }
}
