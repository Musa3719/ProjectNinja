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
    private Coroutine _meleeAttackFinishedCoroutine;
    private Coroutine Boss3SpecialAttackCoroutine;

    public bool _IsStunned { get; private set; }

    [HideInInspector] public bool _IsInAttackPattern;
    [HideInInspector] public bool _IsAttacking;
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
    public float _CombatStamina
    {
        get => _combatStamina; set
        {
            if (value > _CombatStaminaLimit) _combatStamina = _CombatStaminaLimit;
            else if (value < 0f) _combatStamina = 0f; else _combatStamina = value;
        }
    }
    public float _DodgeOrBlockStaminaUse { get; private set; }


    [SerializeField]
    private Transform _weaponHolderTransform;
    [SerializeField]
    private Transform _decalFollowTransform;
    [SerializeField]
    public GameObject _rangedWarning;

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

    private Transform _followPlayerTransform;
    [HideInInspector]
    public GameObject Boss3ExtraWeapon;

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

        _IsAllowedToAttack = true;
        _attackWaitTime = 0.5f;
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
        if (_bossStateController._bossAI._BossNumber == 3) ArrangeBoss3BladesToHands();
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
        else if (_blockCounter > 0)
        {
            _blockCounter = 0;
        }
    }
    public void ThrowWeapon()
    {
        if (_bossStateController._isDead) return;

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

        if (_weaponObject.transform.parent == null)
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

        if (!(_bossStateController._bossState is BossStates.RetreatBoss1) && !(_bossStateController._bossState is BossStates.SpecialActionBoss1))
        {
            _bossStateController._bossMovement.BlockMovement(deflectedKillable.Object.transform.position);
        }

        _CombatStamina -= _DodgeOrBlockStaminaUse;
        _bossStateController.ChangeAnimation(GetAttackDeflectedAnimName(), 0.2f, true);
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.AttackDeflecteds), transform.position, 0.25f, false, UnityEngine.Random.Range(0.93f, 1.07f));
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
        _bossStateController.EnableHeadAim();


        if (_meleeWeapon != null && _meleeWeapon.transform.parent != null && _meleeWeapon.transform.parent.Find("Trail") != null)
            _meleeWeapon.transform.parent.Find("Trail").gameObject.SetActive(false);

        _IsAttackInterrupted = true;
        GameManager._instance.CoroutineCall(ref _closeIsAttackInterruptedCoroutine, CloseIsAttackInterruptedCoroutine(0.75f), this);

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
        _CombatStamina -= _DodgeOrBlockStaminaUse / 2f;//additional stamina use in the dodge
        _bossStateController.StopBlocking();
        if (_CombatStamina != 0)
        {
            bool isDodgingToRight = _bossStateController._bossMovement.Dodge();
            Dodge(isDodgingToRight);
        }

    }
    public void Dodge(bool isDodgingToRight)
    {
        if (_IsInAttackPattern || _bossStateController._bossState is BossStates.FastAttackBoss2 || _bossStateController._bossState is BossStates.TeleportBoss3)
        {
            StopAttackInstantly();
        }

        _LastBlockOrDodgeTime = Time.time;

        _IsDodgedLately = true;
        GameManager._instance.CoroutineCall(ref _closeIsDodgedLatelyCoroutine, CloseIsDodgedLatelyCoroutine(), this);

        if (!_IsAllowedToAttack)
        {
            _IsAllowedToAttack = true;
            if (_openIsAllowedToAttackCoroutine != null)
                StopCoroutine(_openIsAllowedToAttackCoroutine);
        }


        _IsDodging = true;
        _bossStateController.ChangeAnimation(GetDodgeAnimName(isDodgingToRight));
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.ArmorWalkSounds), transform.position, 0.18f, false, UnityEngine.Random.Range(0.93f, 1.07f));

        Action CloseIsDodging = () =>
        {
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
        if (_blockCounter == 0) { chanceChange = 30; _blockCounter++; }
        else if (_blockCounter == 1) { chanceChange = 15; _blockCounter++; }
        else if (_blockCounter > 1) { chanceChange = -20; _blockCounter++; }
        _blockCounterTimer = 1.25f;


        if (UnityEngine.Random.Range(0, 100) < 60 + chanceChange && !_IsAttacking)
        {
            GameObject sparksVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.SparksVFX), _sparkPosition.position, Quaternion.identity);
            Destroy(sparksVFX, 4f);
            GameObject combatSmokeVFX = Instantiate(GameManager._instance.CombatSmokeVFX, transform.position, Quaternion.LookRotation(transform.forward));
            combatSmokeVFX.GetComponent<Rigidbody>().velocity = -transform.forward * 2f;
            Destroy(combatSmokeVFX, 4f);

            _bossStateController.ChangeAnimation(GetBlockAnimName(), 0.2f, true);
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Blocks), transform.position, 0.135f, false, UnityEngine.Random.Range(0.93f, 1.07f));

            _CombatStamina -= _DodgeOrBlockStaminaUse;
            if (GameManager._instance.IsPlayerHasMeleeWeapon)
                _CombatStamina -= _DodgeOrBlockStaminaUse * 0.55f;
            if (!(_bossStateController._bossState is BossStates.RetreatBoss1) && !(_bossStateController._bossState is BossStates.SpecialActionBoss1))
            {
                _bossStateController._bossMovement.BlockMovement(_bossStateController._BlockedEnemyPosition);
            }

            _IsAllowedToAttack = false;
            GameManager._instance.CoroutineCall(ref _openIsAllowedToAttackCoroutine, OpenIsAllowedToAttackCoroutine(_attackWaitTime * 0.8f), this);
        }
        else
        {
            _IsDeflectedLately = true;
            GameManager._instance.CoroutineCall(ref _closeIsDeflectedLatelyCoroutine, CloseIsDeflectedLatelyCoroutine(), this);

            _IsAllowedToAttack = false;
            GameManager._instance.CoroutineCall(ref _openIsAllowedToAttackCoroutine, OpenIsAllowedToAttackCoroutine(_attackWaitTime * 0.35f), this);

            GameObject sparksVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.ShiningSparksVFX), _sparkPosition.position, Quaternion.identity);
            Destroy(sparksVFX, 4f);
            GameObject combatSmokeVFX = Instantiate(GameManager._instance.CombatSmokeVFX, transform.position, Quaternion.LookRotation(transform.forward));
            combatSmokeVFX.GetComponent<Rigidbody>().velocity = -transform.forward * 2f;
            Destroy(combatSmokeVFX, 4f);

            if (attacker != null && !isRangedAttack)
                attacker.AttackDeflected(this as IKillable);
            _bossStateController.ChangeAnimation(GetDeflectAnimName(), 0.2f, true);
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Deflects), transform.position, 0.11f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        }
    }
    private IEnumerator CloseIsDeflectedLatelyCoroutine()
    {
        yield return new WaitForSeconds(1.5f);
        _IsDeflectedLately = false;
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
        GameManager._instance.CoroutineCall(ref _attackCoroutine, AttackPatternCoroutine(patternNumbers), this);
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

            if (_IsAttackInterrupted)
            {
                yield break;
            }
            if ((GameManager._instance.PlayerRb.transform.position - transform.position).magnitude > 12f)
            {
                break;
            }

            if (_bossStateController._bossAI._BossNumber == 3)
            {
                GameManager._instance.CallForAction(() => _bossStateController._bossMovement.MoveAfterAttack(false), 0.19f);
            }
            else if (c == 0)
            {
                GameManager._instance.CallForAction(() => _bossStateController._bossMovement.MoveAfterAttack(true), 0.3f);
            }
            else
            {
                GameManager._instance.CallForAction(() => _bossStateController._bossMovement.MoveAfterAttack(false), 0.3f);
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
        GameManager._instance.CoroutineCall(ref _openIsAllowedToAttackCoroutine, OpenIsAllowedToAttackCoroutine(_attackWaitTime), this);
        _IsInAttackPattern = false;

    }

    public void Attack(string attackName)
    {
        if (_IsAttacking || _IsDodging) return;
        _bossStateController.ChangeAnimation(attackName);

        if (_meleeWeapon != null && _meleeWeapon.transform.parent != null && _meleeWeapon.transform.parent.Find("Trail") != null)
            _meleeWeapon.transform.parent.Find("Trail").gameObject.SetActive(true);

        _IsAttacking = true;
        _bossStateController._bossMovement.AttackOrBlockRotation(true);
        _bossStateController.DisableHeadAim();

        if (_bossStateController._bossAI._BossNumber == 2)
        {
            GameManager._instance.CoroutineCall(ref _meleeAttackFinishedCoroutine, MeleeAttackFinishedCoroutine(1f), this);
        }
    }
    private IEnumerator MeleeAttackFinishedCoroutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        MeleeAttackFinished();
    }
    public void MeleeAttackFinished()
    {
        if (_bossStateController._isDead || _IsAttackInterrupted || !_IsAttacking) return;
        _IsAttacking = false;
        _attackColliderWarning.gameObject.SetActive(false);
        _bossStateController.EnableHeadAim();

        if (_meleeWeapon != null && _meleeWeapon.transform.Find("Trail") != null)
            _meleeWeapon.transform.Find("Trail").gameObject.SetActive(false);
    }
    public void OpenAttackCollider()
    {
        if (_bossStateController._isDead || _IsAttackInterrupted) return;

        int random = UnityEngine.Random.Range(0, 100);
        if (_bossStateController._bossAI._BossNumber == 1 && random > 45)
        {
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Boss1Grunts), transform.position, UnityEngine.Random.Range(0.4f, 0.6f), false, UnityEngine.Random.Range(0.93f, 1.07f));
        }
        else if (_bossStateController._bossAI._BossNumber == 2 && random > 25)
        {
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Boss2Grunts), transform.position, UnityEngine.Random.Range(0.35f, 0.5f), false, UnityEngine.Random.Range(0.93f, 1.07f));
        }
        else if (_bossStateController._bossAI._BossNumber == 3 && random > 62)
        {
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Boss1Grunts), transform.position, UnityEngine.Random.Range(0.16f, 0.24f), false, UnityEngine.Random.Range(1.7f, 2.2f));
        }

        SoundManager._instance.PlaySound(SoundManager._instance.GetAttackSound(_attackCollider), transform.position, 0.275f, false, UnityEngine.Random.Range(1.02f, 1.12f));
        _attackCollider.gameObject.SetActive(true);
    }
    public void CloseAttackCollider()
    {
        if (_bossStateController._isDead || _IsAttackInterrupted) return;
        _attackCollider.gameObject.SetActive(false);
    }
    public IEnumerator SingleAttack(string attackName)
    {
        _IsAllowedToAttack = false;
        if (_openIsAllowedToAttackCoroutine != null)
            StopCoroutine(_openIsAllowedToAttackCoroutine);

        _IsAttackInterrupted = false;
        if (_closeIsAttackInterruptedCoroutine != null)
            StopCoroutine(_closeIsAttackInterruptedCoroutine);

        _IsInAttackPattern = true;

        Attack(attackName);
        yield return new WaitWhile(() => _IsAttacking);

        _IsInAttackPattern = false;
        _IsAllowedToAttack = false;
        GameManager._instance.CoroutineCall(ref _openIsAllowedToAttackCoroutine, OpenIsAllowedToAttackCoroutine(_attackWaitTime), this);
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
    public void GunAttack()
    {
        GameManager._instance.CoroutineCall(ref Boss3SpecialAttackCoroutine, GunAttackCoroutine(), this);
    }
    private IEnumerator GunAttackCoroutine()
    {
        GameObject Gun = Instantiate(PrefabHolder._instance.GunPrefab, _weaponHolderTransform);
        Boss3ExtraWeapon = Gun;
        Gun.transform.localPosition = new Vector3(-0.089f, 0.195f, 0.183f);
        Gun.transform.localEulerAngles = new Vector3(40.921f, -42.733f, -88.043f);
        _bossStateController.ChangeAnimation("GunAim");

        float aimTime = UnityEngine.Random.Range(1.4f, 2.4f);
        float startTime = Time.time;
        while (startTime + aimTime > Time.time)
        {
            if (_IsAttackInterrupted)
            {
                Destroy(Gun);
                _bossStateController.EnterState(new BossStates.Chase());
                yield break;
            }
            if(_bossStateController._agent.isOnNavMesh)
                _bossStateController._agent.SetDestination(transform.position);
            RangedAttackLookToPlayer();
            yield return null;
        }
        _bossStateController.ChangeAnimation("GunFire");
        SoundManager._instance.PlaySound(SoundManager._instance.GunFired, transform.position, 0.24f, false, UnityEngine.Random.Range(1.07f, 1.25f));
        Instantiate(GameManager._instance.GunFireVFX, Gun.transform.Find("GunMesh").Find("BulletSpawnPos").position, Quaternion.identity);
        Gun.GetComponentInChildren<RangedWeapon>().Fire(GameManager._instance.BulletPrefab, Gun.transform.Find("GunMesh").Find("BulletSpawnPos").position, _collider, transform.forward, 60f);
        Destroy(Gun);

        _bossStateController.EnterState(new BossStates.Chase());
    }
    public void LaserAttack()
    {
        GameManager._instance.CoroutineCall(ref Boss3SpecialAttackCoroutine, LaserAttackCoroutine(), this);
    }
    private IEnumerator LaserAttackCoroutine()
    {
        _followPlayerTransform = GameObject.Find("FollowPlayer").transform;
        GameObject Laser = Instantiate(PrefabHolder._instance.LaserPrefabBoss, _weaponHolderTransform);
        Boss3ExtraWeapon = Laser;
        Laser.transform.localPosition = new Vector3(0.0227931f, 0.05016499f, 0.02713142f);
        Laser.transform.localScale /= 2f;
        Laser.transform.localEulerAngles = new Vector3(3.4f, -275.86f, 55.65f);
        _bossStateController.ChangeAnimation("HoldLaser");

        float aimTime = UnityEngine.Random.Range(3f, 5f);
        float startTime = Time.time;
        while (startTime + aimTime > Time.time)
        {
            if (_IsAttackInterrupted)
            {
                _bossStateController.ChangeAnimation("EmptyBody");
                Destroy(Laser);
                _bossStateController.EnterState(new BossStates.Chase());
                yield break;
            }
            if (_bossStateController._agent.isOnNavMesh)
                _bossStateController._agent.SetDestination(transform.position);
            LaserAttackLookToPlayer();
            yield return null;
        }
        _bossStateController.ChangeAnimation("EmptyBody");
        Destroy(Laser);

        yield return new WaitForSeconds(0.5f);
        _bossStateController.EnterState(new BossStates.Chase());
    }
    public void Throw()
    {
        GameManager._instance.CoroutineCall(ref Boss3SpecialAttackCoroutine, ThrowCoroutine(), this);
    }
    private IEnumerator ThrowCoroutine()
    {
        int random = UnityEngine.Random.Range(0, 5);
        if (random == 0)
            _ThrowableItem = new Smoke();
        else if (random == 1 || random == 2)
            _ThrowableItem = new Shuriken();
        else
            _ThrowableItem = new Knife();
        _ThrowableItem.CountInterface = 1;
        _rangedWarning.SetActive(true);
        yield return new WaitForSeconds(0.35f);
        _rangedWarning.SetActive(false);
        _bossStateController.ChangeAnimation("Throw3");
        GameManager._instance.CallForAction(() => _ThrowableItem.Use(_bossStateController._rb, _collider, false), 0.2f);
        SoundManager._instance.PlaySound(SoundManager._instance.Throw, transform.position, 0.25f, false, UnityEngine.Random.Range(0.93f, 1.07f));

        float waitTime = UnityEngine.Random.Range(0.65f, 0.8f);
        float startTime = Time.time;
        while (startTime + waitTime > Time.time)
        {
            if (_bossStateController._agent.isOnNavMesh)
                _bossStateController._agent.SetDestination(transform.position);
            RangedAttackLookToPlayer();
            yield return null;
        }
        _bossStateController.EnterState(new BossStates.Chase());
    }
    private void RangedAttackLookToPlayer()
    {
        float random = UnityEngine.Random.Range(0.1f, 0.4f);
        Vector3 lookAtPos = GameManager._instance.PlayerRb.transform.position + GameManager._instance.PlayerRb.velocity * random * 0.07f * (GameManager._instance.PlayerRb.transform.position - _bossStateController.transform.position).magnitude;
        Vector3 normalDir = (GameManager._instance.PlayerRb.transform.position - _bossStateController.transform.position).normalized;
        Vector3 futureDir = (GameManager._instance.PlayerRb.transform.position + GameManager._instance.PlayerRb.velocity * random * 0.07f * (GameManager._instance.PlayerRb.transform.position - _bossStateController.transform.position).magnitude - _bossStateController.transform.position).normalized;
        float angle = Vector3.Angle(normalDir, futureDir);
        if (angle > 30f)
        {
            if (Vector3.Angle(Quaternion.AngleAxis(30f, Vector3.up) * normalDir, futureDir) < angle)
                lookAtPos = Quaternion.AngleAxis(30f, Vector3.up) * normalDir;
            else
                lookAtPos = Quaternion.AngleAxis(-30f, Vector3.up) * normalDir;
        }
        _bossStateController._bossMovement.MoveToPosition(_bossStateController.transform.position, lookAtPos);
    }
    private void LaserAttackLookToPlayer()
    {
        _bossStateController._bossMovement.MoveToPosition(_bossStateController.transform.position, _followPlayerTransform.position, rotationLerpSpeed: 60f);
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
        GameManager._instance.CoroutineCall(ref _closeEyesCoroutine, CloseEyesCoroutine(), this);
    }
    private IEnumerator CloseEyesCoroutine()
    {
        var meshes = _bossStateController.EyeObject.GetComponentsInChildren<MeshRenderer>();
        Color emissiveColor = new Color(255f / 255f, 0f, 51f / 255f, 1f);
        float newIntensity = 11f;
        float startTime = Time.time;
        while (startTime + 3.5f > Time.time)
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

        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Crushs), transform.position, 0.3f, false, UnityEngine.Random.Range(0.93f, 1.07f));

        GameObject bloodVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.BloodVFX), transform.position, Quaternion.identity);
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
        GameManager._instance.PlayerGainStamina(100f);

        GameManager._instance.CloseBossUI();
        GameManager._instance.CallForAction(GameManager._instance.OpenBossUI, 2f);
        _bossStateController._bossAI._bossSpecial._phase++;

        string cutsceneName = "BossPhase2Cutscene";
        GameManager._instance.EnterCutscene(cutsceneName);
        GameManager._instance.BossPhaseCounterBetweenScenes.transform.position = new Vector3(_bossStateController._bossAI._bossSpecial._phase, 0f, 0f);
        _CombatStamina = _CombatStaminaLimit;
        _bossStateController._bossMovement.PhaseChange();

        foreach (var script in GetComponents<MonoBehaviour>())
        {
            script.StopAllCoroutines();
        }
    }
    public void Die(Vector3 dir, float killersVelocityMagnitude, IKillObject killer, bool isHardHit)
    {
        if ((_bossStateController._bossState is BossStates.RetreatBoss1) || (_bossStateController._bossState is BossStates.SpecialActionBoss1))
        {
            _bossStateController.ChangeAnimation("BlockWhile");
            GameObject sparks = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.SparksVFX), _sparkPosition.position, Quaternion.identity);
            Destroy(sparks, 4f);
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Blocks), transform.position, 0.175f, false, UnityEngine.Random.Range(0.93f, 1.07f));
            return;
        }
        if (_CombatStamina > _DodgeOrBlockStaminaUse)
        {
            if (killer != null && killer.Owner.CompareTag("Player"))
                DeflectWithBlock(dir, GameManager._instance.PlayerRb.GetComponent<IKillable>(), false);
            else
                DeflectWithBlock(dir, null, false);

            Debug.LogError("Die Error While Combat Stamina Is Enough");
            return;
        }

        if (_bossStateController._bossAI._bossSpecial._phase < _bossStateController._bossAI._bossSpecial._phaseCount)
        {
            PhaseChange();
            return;
        }

        if (IsDead) return;

        if (isHardHit)
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Crushs), transform.position, 0.6f, false, UnityEngine.Random.Range(0.9f, 1f));
        else
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Cuts), transform.position, 0.7f, false, UnityEngine.Random.Range(0.9f, 1f));

        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.WeaponHitSounds), transform.position, 0.45f, false, UnityEngine.Random.Range(0.6f, 0.65f));
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
        else if (_bossStateController._bossAI._BossNumber == 3)
        {
            GetComponent<BossTextHandler>().StopTalking();
            GameManager._instance.CallForAction(() => SceneController._instance.NextScene(), 6f);
        }

        foreach (var script in GetComponents<MonoBehaviour>())
        {
            script.StopAllCoroutines();
        }

        GameObject bloodVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.BloodVFX), transform.position - Vector3.up * 0.25f, Quaternion.identity);
        bloodVFX.GetComponentInChildren<Rigidbody>().velocity = -Vector3.up * 1.25f + killersVelocityMagnitude * dir * 0.3f;
        Destroy(bloodVFX, 5f);

        GameObject bloodPrefab = GameManager._instance.BloodDecalPrefabs[UnityEngine.Random.Range(0, GameManager._instance.BloodDecalPrefabs.Count)];
        GameObject decal = Instantiate(bloodPrefab, transform);
        float sizeMul = UnityEngine.Random.Range(0.75f, 1.25f);
        decal.GetComponent<DecalProjector>().size = new Vector3(decal.GetComponent<DecalProjector>().size.x * sizeMul, decal.GetComponent<DecalProjector>().size.y * sizeMul, decal.GetComponent<DecalProjector>().size.z);
        decal.GetComponent<DecalFollow>().FollowingTransform = _decalFollowTransform;
        decal.GetComponent<DecalFollow>().LocalPosition = new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(0.2f, 0.7f), 0f);

        Vector3 pos = transform.position + dir * UnityEngine.Random.Range(0.25f, 1.7f);
        Physics.Raycast(pos, -Vector3.up, out RaycastHit hit, 50f, GameManager._instance.LayerMaskForVisible);
        pos = hit.collider == null ? transform.position - Vector3.up * 0.7f : hit.point;
        bloodPrefab = GameManager._instance.BloodDecalPrefabs[UnityEngine.Random.Range(0, GameManager._instance.BloodDecalPrefabs.Count)];
        GameObject groundDecal = Instantiate(bloodPrefab, pos, Quaternion.identity);
        groundDecal.transform.forward = hit.collider == null ? Vector3.up : -hit.normal;
        groundDecal.GetComponent<DecalProjector>().size = new Vector3(groundDecal.GetComponent<DecalProjector>().size.x * sizeMul, groundDecal.GetComponent<DecalProjector>().size.y * sizeMul, groundDecal.GetComponent<DecalProjector>().size.z);
        groundDecal.GetComponent<DecalProjector>().decalLayerMask = DecalLayerEnum.DecalLayerDefault;

        GameObject sparksVFX = Instantiate(GameManager._instance.ShiningSparksVFX[0], transform.position - transform.forward * 0.8f, Quaternion.identity);
        Destroy(sparksVFX, 4f);

        _bossStateController._isDead = true;
        _attackCollider.gameObject.SetActive(false);
        Vector3 currentVelocity = GetComponent<NavMeshAgent>().velocity;
        GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        ActivateRagdoll(dir, killersVelocityMagnitude, currentVelocity);

        if (_bossStateController._bossAI._BossNumber == 1)
        {
            if (Steamworks.SteamClient.IsValid && !new Steamworks.Data.Achievement("DefeatBoss1Achievement").State)
                new Steamworks.Data.Achievement("DefeatBoss1Achievement").Trigger();
        }
        else if (_bossStateController._bossAI._BossNumber == 2)
        {
            if (Steamworks.SteamClient.IsValid && !new Steamworks.Data.Achievement("DefeatBoss2Achievement").State)
                new Steamworks.Data.Achievement("DefeatBoss2Achievement").Trigger();
        }
        else if (_bossStateController._bossAI._BossNumber == 3)
        {
            if (Steamworks.SteamClient.IsValid && !new Steamworks.Data.Achievement("DefeatBoss3Achievement").State)
                new Steamworks.Data.Achievement("DefeatBoss3Achievement").Trigger();
        }


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
        float forceUpMultiplier = -40f;

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
            if (collider.gameObject.name == "CC_Base_Hip")
            {
                GameObject bleedingVFX = Instantiate(GameManager._instance.BleedingVFX, Vector3.zero, Quaternion.identity);
                bleedingVFX.transform.parent = collider.transform.Find("CC_Base_Waist").Find("CC_Base_Spine01").Find("CC_Base_Spine02");
                bleedingVFX.transform.localPosition = Vector3.up * UnityEngine.Random.Range(-0.1f, 0.25f);
                bleedingVFX.transform.forward = -dir;
            }

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
                collider.GetComponent<Rigidbody>().AddForce((tempDir * killersVelocityMagnitude * forceMultiplier / 3.5f + tempDir * forceMultiplier + Vector3.up * forceUpMultiplier) * 5f);

            collider.gameObject.layer = LayerMask.NameToLayer("Default");
        }

        //Destroy(_bossStateController._rb);
        //Destroy(_bossStateController._agent);
    }
    private void ArrangeBoss3BladesToHands()
    {
        GameObject rBlade = null, lBlade = null;

        var blades = GetComponentsInChildren<RotateBladeHumanoid>();
        foreach (var blade in blades)
        {
            if (blade.name.Equals("R_Blade"))
                rBlade = blade.gameObject;
            else
                lBlade = blade.gameObject;
        }

        rBlade.transform.parent = GameObject.Find("CC_Base_R_Hand").transform;
        lBlade.transform.parent = GameObject.Find("CC_Base_L_Hand").transform;

        Vector3 rBladeNewPos = new Vector3(-0.01950011f, 0.1074f, -0.004499607f);
        Vector3 rBladeNewRot = new Vector3(7.861f, -2.673f, 79.488f);
        Vector3 lBladeNewPos = new Vector3(0.02410386f, 0.1007111f, -0.008408492f);
        Vector3 lBladeNewRot = new Vector3(104.661f, -41.14398f, -127.66f);

        rBlade.transform.localPosition = rBladeNewPos;
        rBlade.transform.localEulerAngles = rBladeNewRot;
        lBlade.transform.localPosition = lBladeNewPos;
        lBlade.transform.localEulerAngles = lBladeNewRot;

        lBlade.GetComponent<RotateBladeHumanoid>().DisableHaveWeapon();
        rBlade.GetComponent<RotateBladeHumanoid>().DisableHaveWeapon();
    }
}
