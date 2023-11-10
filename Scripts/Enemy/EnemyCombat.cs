using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.HighDefinition;

public enum WeaponTypeEnum
{
    Sword,
    Axe,
    Halberd,
    Mace,
    Hammer,
    Bow,
    Crossbow,
    Gun,
    Katana,
    Exist,
    Zweihander
}
public enum ThrowableEnemyTypeEnum
{
    Shuriken,
    Knife,
    Bomb
}
public class EnemyCombat : MonoBehaviour, IKillable
{
    private EnemyStateController _enemyStateController;

    public GameObject Object => gameObject;
    public bool IsDead => _enemyStateController._isDead;
    public bool IsBlockingGetter => _IsBlocking;
    public bool IsDodgingGetter => _IsDodging;
    public GameObject AttackCollider { get { if (_attackCollider == null) return null; return _attackCollider.gameObject; } }
    public int InterruptAttackCounterGetter => _interruptAttackCounter;
    public float _CollisionVelocity => _collisionVelocity;

    private CapsuleCollider _collider;
    private Collider _attackCollider;
    private Collider _attackColliderWarning;
    private MeleeWeapon _meleeWeapon;

    public BoxCollider _rightBladeAttackCollider { get; private set; }
    public BoxCollider _rightBladeAttackWarning { get; private set; }
    public BoxCollider _leftBladeAttackCollider { get; private set; }
    public BoxCollider _leftBladeAttackWarning { get; private set; }

    public GameObject _weaponObject { get; private set; }
    private RangedWeapon _rangedWeapon;

    private int _interruptAttackCounter;

    private Collider[] _ragdollColliders;
    private List<string> _patternNumbers;

    private Coroutine _attackCoroutine;
    private Coroutine _openIsAllowedToAttackCoroutine;
    private Coroutine _closeIsAttackInterruptedCoroutine;
    private Coroutine _rangedAttackCoroutine;

    public bool _isStunned { get; private set; }

    public bool _isInAttackPattern { get; private set; }
    public bool _isAttacking { get; private set; }
    public bool _isAttackInterrupted { get; private set; }
    public bool _IsDodging { get; set; }

    public bool _IsBlocking { get; set; }
    public bool _IsAllowedToAttack { get; set; }
    public bool _IsAllowedToThrow { get; set; }
    public IThrowableItem _ThrowableItem { get; set; }
    public float _lastAttackDeflectedTime { get; private set; }

    private float _attackRange;
    public float AttackRange => _attackRange;

    public bool _IsRanged { get; private set; }

    [SerializeField]
    private Transform _sparkPosition;
    [SerializeField]
    private float _enemyTypeSpeedMultiplier;
    [SerializeField]
    private WeaponTypeEnum _weaponType;
    public WeaponTypeEnum WeaponType => _weaponType;

    [SerializeField]
    private ThrowableEnemyTypeEnum _throwableType;
    public ThrowableEnemyTypeEnum ThrowableType => _throwableType;

    [SerializeField]
    private Transform _weaponHolderTransform;


    [SerializeField]
    public GameObject _rangedWarning;
    [SerializeField]
    private Transform _decalFollowTransform;

    private int _lastAttackNumberForPattern;

    private float _attackWaitTime;
    private float _throwWaitTime;
    private float _dodgeTime;
    public float _DodgeTime => _dodgeTime;
    private float _blockMoveTime;
    public float _BlockMoveTime => _blockMoveTime;

    private float _crashStunCheckValue;
    private float _collisionVelocity;

    private Dictionary<string, string> _rbOfJoint;
    private Dictionary<string, float> _massOfRb;
    private void Awake()
    {
        _enemyStateController = GetComponent<EnemyStateController>();
        _collider = GetComponent<CapsuleCollider>();

        _rbOfJoint = new Dictionary<string, string>();
        _massOfRb = new Dictionary<string, float>();

        _IsAllowedToAttack = true;
        _IsAllowedToThrow = true;
        _attackWaitTime = 0.5f;
        _throwWaitTime = 0.4f;
        _dodgeTime = 0.8f;
        _blockMoveTime = 0.8f;
        _crashStunCheckValue = 13.5f;
    }
    
    private void Start()
    {
        ArrangeRagdoll();
        InitThrowableFromEnum();
        ArrangeThrowableCount();
        ArrangeMainWeapon();

        if (!_IsRanged && _weaponObject != null)
        {
            _attackCollider = _weaponObject.transform.Find("AttackCollider").GetComponent<CapsuleCollider>();
            _attackColliderWarning = _weaponObject.transform.Find("AttackColliderWarning").GetComponent<CapsuleCollider>();
            _meleeWeapon = _attackCollider.GetComponent<MeleeWeapon>();
        }
        else if (_weaponObject == null)
        {
            _attackCollider = _rightBladeAttackCollider;
            _attackColliderWarning = _rightBladeAttackWarning;
        }
    }
    private void ArrangeMainWeapon()
    {
        switch (_weaponType)
        {
            case WeaponTypeEnum.Sword:
                _attackRange = 8f;
                _enemyStateController._enemyMovement._moveSpeed = 4f * _enemyTypeSpeedMultiplier;
                _enemyStateController._enemyMovement._runSpeed = 11f * _enemyTypeSpeedMultiplier;
                _weaponObject = Instantiate(PrefabHolder._instance.SwordPrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                switch (_enemyStateController.EnemyNumber)
                {
                    case 1:
                        _weaponObject.transform.localPosition = new Vector3(0.01773758f, 0.06871963f, -0.00327652f);
                        _weaponObject.transform.localEulerAngles = new Vector3(173.077f, 30.243f, 1.591003f);
                        break;
                    case 2:
                        _weaponObject.transform.localPosition = new Vector3(-0.0062f, 0.0963f, -0.0112f);
                        _weaponObject.transform.localEulerAngles = new Vector3(-174.082f, -78.83499f, 6.264008f);
                        break;
                    case 3:
                        _weaponObject.transform.localPosition = new Vector3(-0.0007f, 0.0941f, 0.0158f);
                        _weaponObject.transform.localEulerAngles = new Vector3(185.918f, -78.83499f, 4.585007f);
                        break;
                    case 4:
                        _weaponObject.transform.localPosition = new Vector3(0.058f, 0.056f, -0.013f);
                        _weaponObject.transform.localEulerAngles = new Vector3(171.496f, 15.78f, 3.740997f);
                        break;
                    default:
                        Debug.LogError("Number Not Found...");
                        break;
                }
                _enemyStateController._enemyAI.SetValuesForAI(0.7f, 0.75f, 0.25f, 0.5f, 0.5f);
                _IsRanged = false;
                break;
            case WeaponTypeEnum.Axe:
                _attackRange = 9f;
                _enemyStateController._enemyMovement._moveSpeed = 3.5f * _enemyTypeSpeedMultiplier;
                _enemyStateController._enemyMovement._runSpeed = 10f * _enemyTypeSpeedMultiplier;
                _weaponObject = Instantiate(PrefabHolder._instance.AxePrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                _weaponObject.transform.localPosition = new Vector3(0.013f, -0.002f, 0.005f);
                _weaponObject.transform.localEulerAngles = new Vector3(-188.181f, -182.17f, 11.242f);
                _enemyStateController._enemyAI.SetValuesForAI(0.75f, 0.7f, 0.1f, 0.65f, 0.3f);
                _IsRanged = false;
                break;
            case WeaponTypeEnum.Halberd:
                _attackRange = 11f;
                _enemyStateController._enemyMovement._moveSpeed = 3f * _enemyTypeSpeedMultiplier;
                _enemyStateController._enemyMovement._runSpeed = 8.5f * _enemyTypeSpeedMultiplier;
                _weaponObject = Instantiate(PrefabHolder._instance.HalberdPrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                _weaponObject.transform.localPosition = new Vector3(0.006f, 0.035f, 0.01f);
                _weaponObject.transform.localEulerAngles = new Vector3(5.095f, -2.786f, -171.695f);
                _enemyStateController._enemyAI.SetValuesForAI(0.9f, 0.7f, 0.05f, 0.7f, 0.2f);
                _IsRanged = false;
                break;
            case WeaponTypeEnum.Mace:
                _attackRange = 6f;
                _enemyStateController._enemyMovement._moveSpeed = 3.5f * _enemyTypeSpeedMultiplier;
                _enemyStateController._enemyMovement._runSpeed = 10f * _enemyTypeSpeedMultiplier;
                _weaponObject = Instantiate(PrefabHolder._instance.MacePrefab, _weaponHolderTransform);
                _weaponObject.transform.Find("AttackCollider").GetComponent<MeleeWeapon>().IsHardHitWeapon = true;
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                _weaponObject.transform.localPosition = new Vector3(0.045f, 0.03f, -0.002f);
                _weaponObject.transform.localEulerAngles = new Vector3(-192.652f, -0.9320068f, -5.200989f);
                _enemyStateController._enemyAI.SetValuesForAI(0.35f, 0.65f, 0.1f, 0.75f, 0.3f);
                _IsRanged = false;
                break;
            case WeaponTypeEnum.Hammer:
                _attackRange = 6f;
                _enemyStateController._enemyMovement._moveSpeed = 3.5f * _enemyTypeSpeedMultiplier;
                _enemyStateController._enemyMovement._runSpeed = 10f * _enemyTypeSpeedMultiplier;
                _weaponObject = Instantiate(PrefabHolder._instance.HammerPrefab, _weaponHolderTransform);
                _weaponObject.transform.Find("AttackCollider").GetComponent<MeleeWeapon>().IsHardHitWeapon = true;
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                _weaponObject.transform.localPosition = new Vector3(0.177f, -0.422f, 0.052f);
                _weaponObject.transform.localEulerAngles = new Vector3(35.244f, -49.668f, -253.006f);
                _enemyStateController._enemyAI.SetValuesForAI(0.9f, 0.6f, 0.05f, 0.75f, 0.15f);
                _IsRanged = false;
                break;
            case WeaponTypeEnum.Bow:
                _attackRange = 15f;
                _enemyStateController._enemyMovement._moveSpeed = 3f * _enemyTypeSpeedMultiplier;
                _enemyStateController._enemyMovement._runSpeed = 8f * _enemyTypeSpeedMultiplier;
                _weaponObject = Instantiate(PrefabHolder._instance.BowPrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                _enemyStateController._enemyAI.SetValuesForAI(50f, 0.5f, 1f, 0.2f, 0f);
                _IsRanged = true;
                transform.Find("RangedWeapon").gameObject.SetActive(true);
                _rangedWeapon = GetComponentInChildren<RangedWeapon>();
                if (_enemyStateController.EnemyNumber == 3)
                {
                    _weaponObject.transform.localPosition = new Vector3(-0.001922454f, -0.03527873f, -0.03264877f);
                    _weaponObject.transform.localEulerAngles = new Vector3(81.258f, 190.303f, 84.48f);
                }
                else
                {
                    _weaponObject.transform.localPosition = new Vector3(-0.007174938f, -0.01599412f, -0.02786018f);
                    _weaponObject.transform.localEulerAngles = new Vector3(77.399f, -213.311f, -14.257f);
                }
                break;
            case WeaponTypeEnum.Crossbow:
                _attackRange = 12f;
                _enemyStateController._enemyMovement._moveSpeed = 2f * _enemyTypeSpeedMultiplier;
                _enemyStateController._enemyMovement._runSpeed = 7.5f * _enemyTypeSpeedMultiplier;
                _weaponObject = Instantiate(PrefabHolder._instance.CrossbowPrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                _enemyStateController._enemyAI.SetValuesForAI(60f, 0.4f, 1f, 0.2f, 0f);
                _IsRanged = true;
                transform.Find("RangedWeapon").gameObject.SetActive(true);
                _rangedWeapon = GetComponentInChildren<RangedWeapon>();
                if (_enemyStateController.EnemyNumber == 3)
                {
                    _weaponObject.transform.localPosition = new Vector3(0.034f, 0.009f, 0.23f);
                    _weaponObject.transform.localEulerAngles = new Vector3(-3.926f, -176.243f, 8.143f);
                }
                else
                {
                    _weaponObject.transform.localPosition = new Vector3(0.155f, 0.075f, -0.093f);
                    _weaponObject.transform.localEulerAngles = new Vector3(2.278f, -60.697f, -34.288f);
                }
                break;
            case WeaponTypeEnum.Gun:
                _attackRange = 10f;
                _enemyStateController._enemyMovement._moveSpeed = 6f * _enemyTypeSpeedMultiplier;
                _enemyStateController._enemyMovement._runSpeed = 14f * _enemyTypeSpeedMultiplier;
                _weaponObject = Instantiate(PrefabHolder._instance.GunPrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                _enemyStateController._enemyAI.SetValuesForAI(70f, 0.4f, 1f, 0.2f, 0f);
                _IsRanged = true;
                transform.Find("RangedWeapon").gameObject.SetActive(true);
                _rangedWeapon = GetComponentInChildren<RangedWeapon>();
                if (_enemyStateController.EnemyNumber == 3)
                {
                    _weaponObject.transform.localPosition = new Vector3(0.019f, -0.007f, 0.101f);
                    _weaponObject.transform.localEulerAngles = new Vector3(88.114f, -205.657f, 51.895f);
                }
                else
                {
                    _weaponObject.transform.localPosition = new Vector3(0.085f, 0.02f, -0.022f);
                    _weaponObject.transform.localEulerAngles = new Vector3(82.834f, -74.831f, 86.947f);
                }
                break;
            case WeaponTypeEnum.Katana:
                _attackRange = 9f;
                _enemyStateController._enemyMovement._moveSpeed = 6f * _enemyTypeSpeedMultiplier;
                _enemyStateController._enemyMovement._runSpeed = 14f * _enemyTypeSpeedMultiplier;
                _weaponObject = Instantiate(PrefabHolder._instance.KatanaPrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                switch (_enemyStateController.EnemyNumber)
                {
                    case 3:
                        _weaponObject.transform.localPosition = new Vector3(0.002f, 0.084f, 0.01f);
                        _weaponObject.transform.localEulerAngles = new Vector3(-177.844f, 11.32899f, 6.315994f);
                        break;
                    case 6:
                        _weaponObject.transform.localPosition = new Vector3(0.01773758f, 0.06871963f, -0.00327652f);
                        _weaponObject.transform.localEulerAngles = new Vector3(173.077f, 30.243f, 1.591003f);
                        break;
                    case 2:
                        _weaponObject.transform.localPosition = new Vector3(0.0007f, 0.0936f, -0.0003f);
                        _weaponObject.transform.localEulerAngles = new Vector3(-181.055f, 16.806f, -3.485992f);
                        break;
                    default:
                        Debug.LogError("Number Not Found...");
                        break;
                }
                _enemyStateController._enemyAI.SetValuesForAI(0.95f, 0.8f, 0.05f, 0.15f, 0.6f);
                _IsRanged = false;
                break;
            case WeaponTypeEnum.Exist:
                _attackRange = 9f;
                _enemyStateController._enemyMovement._moveSpeed = 6f * _enemyTypeSpeedMultiplier;
                _enemyStateController._enemyMovement._runSpeed = 14f * _enemyTypeSpeedMultiplier;
                _enemyStateController._enemyAI.SetValuesForAI(0.85f, 0.7f, 0.2f, 0.25f, 1f);
                _IsRanged = false;
                if (_weaponHolderTransform.Find("Weapon") == null)
                {
                    _weaponObject = null;
                    var components = GetComponentsInChildren<RotateBladeHumanoid>();
                    foreach (var item in components)
                    {
                        if (item.name == "R_Blade")
                        {
                            _rightBladeAttackCollider = item.transform.GetChild(0).GetComponent<BoxCollider>();
                            _rightBladeAttackWarning = item.transform.GetChild(1).GetComponent<BoxCollider>();
                        }
                        else if (item.name == "L_Blade")
                        {
                            _leftBladeAttackCollider = item.transform.GetChild(0).GetComponent<BoxCollider>();
                            _leftBladeAttackWarning = item.transform.GetChild(1).GetComponent<BoxCollider>();
                        }
                    }
                }
                else
                    _weaponObject = _weaponHolderTransform.Find("Weapon").gameObject;
                break;
            case WeaponTypeEnum.Zweihander:
                _attackRange = 12f;
                _enemyStateController._enemyMovement._moveSpeed = 3f * _enemyTypeSpeedMultiplier;
                _enemyStateController._enemyMovement._runSpeed = 9f * _enemyTypeSpeedMultiplier;
                _weaponObject = Instantiate(PrefabHolder._instance.ZweihanderPrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                switch (_enemyStateController.EnemyNumber)
                {
                    case 3:
                        _weaponObject.transform.localPosition = new Vector3(-0.0014f, 0.0324f, 0.0143f);
                        _weaponObject.transform.localEulerAngles = new Vector3(-183.127f, -80.23901f, 3.561005f);
                        break;
                    case 2:
                        _weaponObject.transform.localPosition = new Vector3(0.0102f, 0.0681f, 0.0072f);
                        _weaponObject.transform.localEulerAngles = new Vector3(183.871f, 91.047f, -6.632019f);
                        break;
                    case 4:
                        _weaponObject.transform.localPosition = new Vector3(0.0253f, 0.0165f, -0.0018f);
                        _weaponObject.transform.localEulerAngles = new Vector3(161.349f, 15.692f, 4.076996f);
                        break;
                    default:
                        Debug.LogError("Number Not Found...");
                        break;
                }
                _enemyStateController._enemyAI.SetValuesForAI(0.95f, 0.75f, 0f, 0.075f, 0.15f);
                _IsRanged = false;
                break;
            default:
                Debug.LogError("Wrong Weapon Type");
                break;
        }
    }
    private void InitThrowableFromEnum()
    {
        switch (_throwableType)
        {
            case ThrowableEnemyTypeEnum.Knife:
                _ThrowableItem = new Knife();
                break;
            case ThrowableEnemyTypeEnum.Shuriken:
                _ThrowableItem = new Shuriken();
                break;
            case ThrowableEnemyTypeEnum.Bomb:
                _ThrowableItem = new Bomb();
                break;
            default:
                Debug.LogError("Throwable Item Enum is not correct...");
                break;
        }
    }
    private void ArrangeThrowableCount()
    {
        int number = UnityEngine.Random.Range(0, 101);

        if (_throwableType == ThrowableEnemyTypeEnum.Bomb)
        {
            if (number < 33)
            {
                _ThrowableItem.CountInterface = 0;
            }

            else if (number < 90)
            {
                _ThrowableItem.CountInterface = 1;
            }
            else
            {
                _ThrowableItem.CountInterface = 2;
            }
        }
        else
        {
            if (number < 33)
            {
                _ThrowableItem.CountInterface = 0;
            }
            else if (number < 50)
            {
                _ThrowableItem.CountInterface = 1;
            }
            else if (number < 70)
            {
                _ThrowableItem.CountInterface = 2;
            }
            else if (number < 90)
            {
                _ThrowableItem.CountInterface = 3;
            }
            else if (number < 97)
            {
                _ThrowableItem.CountInterface = 4;
            }
            else
            {
                _ThrowableItem.CountInterface = 5;
            }
        }

    }

    private void FixedUpdate()
    {
        if (IsDead) return;

        if (_enemyStateController._agent.enabled)
            _collisionVelocity = _enemyStateController._agent.velocity.magnitude;
        else
            _collisionVelocity = 0f;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider != null && collision.collider.GetComponent<IKillable>() != null)
        {
            if ((_collisionVelocity > _crashStunCheckValue || collision.collider.GetComponent<IKillable>()._CollisionVelocity > _crashStunCheckValue))
            {
                Stun(0.35f, true, collision.collider.transform);
            }
        }
    }
    public void AttackDeflected(IKillable deflectedKillable)
    {
        _lastAttackDeflectedTime = Time.time;
        StopAttackInstantly();
        Stun(0.7f, false, deflectedKillable.Object.transform);
        _enemyStateController._enemyMovement.BlockMovement(deflectedKillable.Object.transform.position);
        _enemyStateController.ChangeAnimation(GetAttackDeflectedAnimName(), 0.2f, true);
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.AttackDeflecteds), transform.position, 0.25f, false, UnityEngine.Random.Range(0.93f, 1.07f));
    }
    public void StopAttackInstantly()
    {
        if (_IsRanged)
        {
            _IsAllowedToAttack = false;
            GameManager._instance.CoroutineCall(ref _openIsAllowedToAttackCoroutine, OpenIsAllowedToAttackCoroutine(_attackWaitTime), this);

            _enemyStateController.ChangeAnimation("Empty");

            _isAttackInterrupted = true;
            GameManager._instance.CoroutineCall(ref _closeIsAttackInterruptedCoroutine, CloseIsAttackInterruptedCoroutine(0.5f), this);
            return;
        }

        _isInAttackPattern = false;
        _isAttacking = false;
        _enemyStateController.EnableHeadAim();

        if (_meleeWeapon != null && _meleeWeapon.transform.parent != null && _meleeWeapon.transform.parent.Find("Trail") != null)
            _meleeWeapon.transform.parent.Find("Trail").gameObject.SetActive(false);

        _isAttackInterrupted = true;
        GameManager._instance.CoroutineCall(ref _closeIsAttackInterruptedCoroutine, CloseIsAttackInterruptedCoroutine(0.5f), this);

        _enemyStateController.ChangeAnimation("Empty");
        _attackColliderWarning.gameObject.SetActive(false);
        _attackCollider.gameObject.SetActive(false);

        _IsAllowedToAttack = false;
        GameManager._instance.CoroutineCall(ref _openIsAllowedToAttackCoroutine, OpenIsAllowedToAttackCoroutine(_attackWaitTime), this);
    }
    public IEnumerator CloseIsAttackInterruptedCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        _isAttackInterrupted = false;
    }
    public IEnumerator OpenIsAllowedToAttackCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        _IsAllowedToAttack = true;
    }
    public void StopBlockingAndDodge()
    {
        /*_enemyStateController.StopBlocking();
        bool isDodgingToRight = _enemyStateController._enemyMovement.Dodge();
        _enemyStateController._enemyCombat.Dodge(isDodgingToRight);*/
    }
    public void Dodge(bool isDodgingToRight)
    {
        if(_isInAttackPattern)
        {
            StopAttackInstantly();
        }

        _IsDodging = true;
        _enemyStateController.ChangeAnimation(GetDodgeAnimName(isDodgingToRight));
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.ArmorWalkSounds), transform.position, 0.18f, false, UnityEngine.Random.Range(0.93f, 1.07f));

        Action CloseIsDodging = () => {
            _IsDodging = false;
        };
        GameManager._instance.CallForAction(CloseIsDodging, _dodgeTime / 2);
    }
    public void DeflectWithBlock(Vector3 dir, IKillable attacker, bool isRangedAttack)
    {
        _IsBlocking = false;

        if (!(_enemyStateController._enemyState is EnemyStates.StepBack) && (!_enemyStateController._enemyAI.CanDeflectAttack || _isInAttackPattern || UnityEngine.Random.Range(0, 100) < 85))
        {
            GameObject sparksVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.SparksVFX), _sparkPosition.position, Quaternion.identity);
            Destroy(sparksVFX, 4f);
            GameObject combatSmokeVFX = Instantiate(GameManager._instance.CombatSmokeVFX, transform.position, Quaternion.LookRotation(transform.forward));
            combatSmokeVFX.GetComponent<Rigidbody>().velocity = -transform.forward * 2f;
            Destroy(combatSmokeVFX, 4f);

            _enemyStateController.ChangeAnimation(GetBlockAnimName(), 0.2f, true);
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Blocks), transform.position, 0.135f, false, UnityEngine.Random.Range(0.93f, 1.07f));

            _enemyStateController._enemyMovement.BlockMovement(_enemyStateController._BlockedEnemyPosition);

            _IsAllowedToAttack = false;
            GameManager._instance.CoroutineCall(ref _openIsAllowedToAttackCoroutine, OpenIsAllowedToAttackCoroutine(_attackWaitTime * 0.8f), this);
        }
        else
        {
            GameObject sparksVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.ShiningSparksVFX), _sparkPosition.position, Quaternion.identity);
            Destroy(sparksVFX, 4f);
            GameObject combatSmokeVFX = Instantiate(GameManager._instance.CombatSmokeVFX, transform.position, Quaternion.LookRotation(transform.forward));
            combatSmokeVFX.GetComponent<Rigidbody>().velocity = -transform.forward * 2f;
            Destroy(combatSmokeVFX, 4f);

            if (attacker != null && !isRangedAttack)
                attacker.AttackDeflected(this as IKillable);
            _enemyStateController.ChangeAnimation(GetDeflectAnimName(), 0.2f, true);
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Deflects), transform.position, 0.11f, false, UnityEngine.Random.Range(0.93f, 1.07f));

            _IsAllowedToAttack = false;
            GameManager._instance.CoroutineCall(ref _openIsAllowedToAttackCoroutine, OpenIsAllowedToAttackCoroutine(_attackWaitTime * 0.35f), this);
        }
    }
    public void AttackWithPattern()
    {
        if (_isInAttackPattern || !_IsAllowedToAttack) return;

        _IsAllowedToAttack = false;
        if (_openIsAllowedToAttackCoroutine != null)
            StopCoroutine(_openIsAllowedToAttackCoroutine);
        
        _isInAttackPattern = true;
        _isAttackInterrupted = false;
        if (_closeIsAttackInterruptedCoroutine != null)
            StopCoroutine(_closeIsAttackInterruptedCoroutine);

        if (!_IsRanged)
        {
            _patternNumbers = _enemyStateController._enemyAI.ChooseAttackPattern();
            GameManager._instance.CoroutineCall(ref _attackCoroutine, AttackPatternCoroutine(_patternNumbers), this);
        }
        else
        {
            if (WeaponType == WeaponTypeEnum.Gun)
                RangedGunAttack();
            else
                RangedAttack();
        }
        
    }
    public IEnumerator AttackPatternCoroutine(List<string> patternNumbers)
    {
        while (_enemyStateController._agent.velocity.magnitude > 7.5f) 
        {
            _enemyStateController._agent.speed = 0f;
            yield return null;
        }
        _lastAttackNumberForPattern = -1;
        int c = 0;
        float lastTimeAttacked = Time.time;

        foreach (var attackName in patternNumbers)
        {
            if (lastTimeAttacked + 0.7f > Time.time)
                yield return new WaitForSeconds(0.7f - Mathf.Abs(Time.time - lastTimeAttacked));
            lastTimeAttacked = Time.time;

            if (_isAttackInterrupted)
            {
                yield break;
            }
            if ((GameManager._instance.PlayerRb.transform.position - transform.position).magnitude > 8.5f || _enemyStateController._enemyAI.CheckForAttackFriendlyFire())
            {
                break;
            }

            if (c == 0)
            {
                GameManager._instance.CallForAction(() => _enemyStateController._enemyMovement.MoveAfterAttack(true), 0.225f);
            }
            else
            {
                GameManager._instance.CallForAction(() => _enemyStateController._enemyMovement.MoveAfterAttack(false), 0.225f);
            }

            Attack(attackName);

            _lastAttackNumberForPattern = int.Parse(attackName.ToCharArray()[attackName.Length - 1].ToString());

            yield return new WaitWhile(() => _isAttacking);

            //check after attack
            if (_isAttackInterrupted)
            {
                yield break;
            }
            c++;
        }

        _enemyStateController.ChangeAnimation("Empty", 0.75f);

        _IsAllowedToAttack = false;
        _isInAttackPattern = false;
        GameManager._instance.CoroutineCall(ref _openIsAllowedToAttackCoroutine, OpenIsAllowedToAttackCoroutine(_attackWaitTime), this);

    }
    private void Attack(string attackName)
    {
        if (_isAttacking || _IsDodging) return;

        _enemyStateController.ChangeAnimation(attackName);

        if (_meleeWeapon != null && _meleeWeapon.transform.parent != null && _meleeWeapon.transform.parent.Find("Trail") != null)
            _meleeWeapon.transform.parent.Find("Trail").gameObject.SetActive(true);

        _isAttacking = true;
        _enemyStateController._enemyMovement.AttackOrBlockRotation(true);
        _enemyStateController.DisableHeadAim();
        _attackColliderWarning.gameObject.SetActive(true);
    }
    public void MeleeAttackFinished()
    {
        if (_isAttackInterrupted) return;
        _isAttacking = false;
        _attackColliderWarning.gameObject.SetActive(false);
        _enemyStateController.EnableHeadAim();

        if (_meleeWeapon != null && _meleeWeapon.transform.parent != null && _meleeWeapon.transform.parent.Find("Trail") != null)
            _meleeWeapon.transform.parent.Find("Trail").gameObject.SetActive(false);
    }
    public void OpenAttackCollider()
    {
        if (_enemyStateController._isDead || _isAttackInterrupted) return;

        SoundManager._instance.PlaySound(SoundManager._instance.GetAttackSound(_attackCollider), transform.position, 0.225f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        _attackCollider.gameObject.SetActive(true);
    }
    public void CloseAttackCollider()
    {
        if (_isAttackInterrupted) return;
        _attackCollider.gameObject.SetActive(false);
    }
    public void AttackWarning(Collider collider, bool isFast, Vector3 attackPosition)
    {
        _enemyStateController._enemyAI._isAttackWarned = true;
        _enemyStateController._enemyAI._isAttackFast = isFast;
        _enemyStateController._enemyAI._attackPosition = attackPosition;
    }
    public void ChangeStamina(float amount)
    {
        //
    }
    public void TrapTriggered()
    {
        if (!_IsDodging && UnityEngine.Random.Range(1, 101) < 65)
        {
            if (UnityEngine.Random.Range(1, 101) < 60)
            {
                _IsDodging = true;
                GameManager._instance.CallForAction(() =>
                {
                    if (_isAttacking) { _IsDodging = false; return; }
                    bool isDodgingToRight = _enemyStateController._enemyMovement.DodgeForward();
                    _enemyStateController._enemyCombat.Dodge(isDodgingToRight);
                }, 0.3f);
            }
            else
            {
                _IsDodging = true;
                GameManager._instance.CallForAction(() =>
                {
                    if (_isAttacking) { _IsDodging = false; return; }
                    bool isDodgingToRight = _enemyStateController._enemyMovement.Dodge();
                    _enemyStateController._enemyCombat.Dodge(isDodgingToRight);
                }, 0.3f);
            }
        }
    }
    public void LaserTrapTriggered()
    {
        if (_IsDodging) return;

        bool isForward;
        if (_enemyStateController._enemyState is EnemyStates.StepBack)
            isForward = true;
        else
            isForward = false;

        if (UnityEngine.Random.Range(1, 101) < 90)
        {
            bool isDodgingToRight;
            if (isForward)
                isDodgingToRight = _enemyStateController._enemyMovement.DodgeForward();
            else
                isDodgingToRight = _enemyStateController._enemyMovement.DodgeToDirection(-transform.forward);

            _enemyStateController._enemyCombat.Dodge(isDodgingToRight);
        }
    }
    private void RangedAttack()
    {
        if (_isAttacking || _IsDodging) return;

        _isAttacking = true;
        _enemyStateController.ChangeAnimation(GetRangedAttackName());
        GameManager._instance.CoroutineCall(ref _rangedAttackCoroutine, RangedAttackCoroutine(), this);
    }
    private IEnumerator RangedAttackCoroutine()
    {
        _rangedWarning.SetActive(true);
        float startTime = Time.time;
        while(startTime + 0.4f > Time.time)
        {
            if (_isAttackInterrupted)
            {
                _isAttacking = false;
                _isInAttackPattern = false;
                _rangedWarning.SetActive(false);
                yield break;
            }

            RangedAttackLookToPlayer();
            yield return null;
        }

        float waitTime;
        if (WeaponType == WeaponTypeEnum.Bow)
            waitTime = 0.3f;
        else
            waitTime = 0.15f;
        startTime = Time.time;
        while (startTime + waitTime > Time.time)
        {
            if (_isAttackInterrupted)
            {
                _isAttacking = false;
                _isInAttackPattern = false;
                _rangedWarning.SetActive(false);
                yield break;
            }

            RangedAttackLookToPlayer();
            yield return null;
        }
        if (!_enemyStateController._enemyAI.CheckForAttackFriendlyFire())
        {
            _rangedWeapon.Fire(GameManager._instance.ArrowPrefab, _weaponObject.transform.position, _collider, new Vector3(transform.forward.x, (GameManager._instance.PlayerRb.transform.position - _weaponObject.transform.position).normalized.y, transform.forward.z), 36f);
            SoundManager._instance.PlaySound(SoundManager._instance.BowFired, transform.position, 0.2f, false, UnityEngine.Random.Range(1.1f, 1.3f));
        }
        _IsAllowedToAttack = false;
        GameManager._instance.CoroutineCall(ref _openIsAllowedToAttackCoroutine, OpenIsAllowedToAttackCoroutine(UnityEngine.Random.Range(3.75f, 5f)), this);

        _rangedWarning.SetActive(false);
        _isAttacking = false;
        _isInAttackPattern = false;
    }
    public void RangedAttackLookToPlayer()
    {
        float random = UnityEngine.Random.Range(0.5f, 0.825f);
        Vector3 lookAtPos = GameManager._instance.PlayerRb.transform.position + GameManager._instance.PlayerRb.velocity * random * 0.065f * (GameManager._instance.PlayerRb.transform.position - _enemyStateController.transform.position).magnitude;
        Vector3 normalDir = (GameManager._instance.PlayerRb.transform.position - _enemyStateController.transform.position).normalized;
        Vector3 futureDir = (GameManager._instance.PlayerRb.transform.position + GameManager._instance.PlayerRb.velocity * random * 0.065f * (GameManager._instance.PlayerRb.transform.position - _enemyStateController.transform.position).magnitude - _enemyStateController.transform.position).normalized;
        float angle = Vector3.Angle(normalDir, futureDir);
        if (angle > 30f)
        {
            if (Vector3.Angle(Quaternion.AngleAxis(30f, Vector3.up) * normalDir, futureDir) < angle)
                lookAtPos = Quaternion.AngleAxis(30f, Vector3.up) * normalDir;
            else
                lookAtPos = Quaternion.AngleAxis(-30f, Vector3.up) * normalDir;
        }
        _enemyStateController._enemyMovement.MoveToPosition(_enemyStateController.transform.position, lookAtPos);
    }

    private void RangedGunAttack()
    {
        if (_isAttacking || _IsDodging) return;

        _isAttacking = true;
        _enemyStateController.ChangeAnimation("GunAim");
        GameManager._instance.CoroutineCall(ref _rangedAttackCoroutine, GunCoroutine(), this);
    }
    private IEnumerator GunCoroutine()
    {
        _rangedWarning.SetActive(true);

        float aimTime = UnityEngine.Random.Range(0.45f, 1.55f);
        float startTime = Time.time;
        while (startTime + aimTime > Time.time)
        {
            if (_isAttackInterrupted)
            {
                _isAttacking = false;
                _isInAttackPattern = false;
                _rangedWarning.SetActive(false);
                yield break;
            }

            RangedAttackLookToPlayer();
            yield return null;
        }
        if (!_enemyStateController._enemyAI.CheckForAttackFriendlyFire())
        {
            _enemyStateController.ChangeAnimation("GunFire");
            SoundManager._instance.PlaySound(SoundManager._instance.GunFired, transform.position, 0.22f, false, UnityEngine.Random.Range(1f, 1.1f));
            Instantiate(GameManager._instance.GunFireVFX, _rangedWeapon.transform.Find("BulletSpawnPos").position, Quaternion.identity);
            _rangedWeapon.Fire(GameManager._instance.BulletPrefab, _rangedWeapon.transform.Find("BulletSpawnPos").position, _collider, transform.forward, 60f);
        }
        _IsAllowedToAttack = false;
        GameManager._instance.CoroutineCall(ref _openIsAllowedToAttackCoroutine, OpenIsAllowedToAttackCoroutine(6f), this);

        _isAttacking = false;
        _isInAttackPattern = false;
        _rangedWarning.SetActive(false);
    }
    public void BombDeflected()
    {

    }
    public void Stun(float time, bool isSpeedChanges, Transform otherTransform)
    {
        _isStunned = true;

        if (isSpeedChanges)
        {
            Vector3 tempSpeed = _enemyStateController._rb.velocity;
            tempSpeed.y = 0f;
            Vector3 newSpeed = (transform.position - otherTransform.position).normalized * tempSpeed.magnitude;
            _enemyStateController._rb.velocity = new Vector3(newSpeed.x, _enemyStateController._rb.velocity.y, newSpeed.z);
        }
        
        GameManager._instance.CallForAction(() => { _isStunned = false; }, time);
        _enemyStateController.ChangeAnimation("Stun");
    }
    public void Push()
    {
        _enemyStateController.ChangeAnimation("Push", 0.05f);
        GameManager._instance._pushEvent?.Invoke((GameManager._instance.PlayerRb.transform.position - transform.position).normalized * 0.75f);
    }
    public void HitBreakable(GameObject breakable)
    {

    }

    private string GetRangedAttackName()
    {
        switch (_weaponType)
        {
            case WeaponTypeEnum.Bow:
                return "BowAttack";
            case WeaponTypeEnum.Crossbow:
                return "CrossbowAttack";
            default:
                return "BowAttack";
        }
    }
    private string GetThrowName()
    {
        string numberOfAttack = UnityEngine.Random.Range(1, 2).ToString();

        return "Throw" + numberOfAttack;
    }
    private string GetDeflectAnimName()
    {
        if (_enemyStateController.EnemyNumber == 3 && _weaponType == WeaponTypeEnum.Exist) return "Deflect";
        string name = "Deflect" + UnityEngine.Random.Range(1, 3);
        return name;
    }
    private string GetAttackDeflectedAnimName()
    {
        if (_enemyStateController.EnemyNumber == 3 && _weaponType == WeaponTypeEnum.Exist) return "AttackDeflected";
        string name = "AttackDeflected" + UnityEngine.Random.Range(1, 3);
        return name;
    }
    public string GetBlockAnimName()
    {
        string name = "Block" + UnityEngine.Random.Range(1, 3);
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
    public void ThrowKillObject()
    {
        if (IsDead) return;

        _rangedWarning.SetActive(false);

        if (_ThrowableItem.CountInterface == 0) return;
        if (_enemyStateController._enemyAI.CheckForAttackFriendlyFire(15f)) return;
        if (_IsRanged || _IsDodging || _IsBlocking || _isInAttackPattern || !_IsAllowedToThrow) return;
        
        _IsAllowedToThrow = false;
        Action OpenIsAllowedToThrow = () => {
            _IsAllowedToThrow = true;
        };
        GameManager._instance.CallForAction(OpenIsAllowedToThrow, _throwWaitTime);

        GameManager._instance.CallForAction(() => _ThrowableItem.Use(_enemyStateController._rb, _collider, false), 0.2f);
        _enemyStateController.ChangeAnimation(GetThrowName());
        SoundManager._instance.PlaySound(SoundManager._instance.Throw, transform.position, 0.25f, false, UnityEngine.Random.Range(0.93f, 1.07f));
    }

    public void Die(Vector3 dir, float killersVelocityMagnitude, IKillObject killer, bool isHardHit)
    {
        if (IsDead) return;

        foreach (var script in GetComponents<MonoBehaviour>())
        {
            script.StopAllCoroutines();
        }

        _rangedWarning.SetActive(false);

        _enemyStateController._isDead = true;

        GameManager._instance.allEnemies.Remove(gameObject);

        //_enemyStateController._animator.SetTrigger("Death");
        if(isHardHit)
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Crushs), transform.position, 0.2f, false, UnityEngine.Random.Range(1.15f, 1.25f));
        else
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Cuts), transform.position, 0.2f, false, UnityEngine.Random.Range(0.95f, 1.05f));

        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.WeaponHitSounds), transform.position, 0.7f, false, UnityEngine.Random.Range(0.6f, 0.65f));
        GameManager._instance.CallForAction(() => SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.EnemyDeathSounds), transform.position, 0.06f, false, UnityEngine.Random.Range(0.95f, 1.05f)), 1f);
        Vector3 bloodDir = (GameManager._instance.MainCamera.transform.position - transform.position).normalized;
        bloodDir.y = 0f;

        GameObject bloodVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.BloodVFX), transform.position - Vector3.up * 0.25f, Quaternion.identity);
        bloodVFX.GetComponentInChildren<Rigidbody>().velocity = -Vector3.up * 1.25f + killersVelocityMagnitude * dir * 0.3f;
        Destroy(bloodVFX, 5f);

        GameObject bloodPrefab = GameManager._instance.BloodDecalPrefabs[UnityEngine.Random.Range(0, GameManager._instance.BloodDecalPrefabs.Count)];
        GameObject decal = Instantiate(bloodPrefab, transform);
        float sizeMul = UnityEngine.Random.Range(0.75f, 1.25f);
        decal.GetComponent<DecalProjector>().size = new Vector3(decal.GetComponent<DecalProjector>().size.x * sizeMul, decal.GetComponent<DecalProjector>().size.y * sizeMul, decal.GetComponent<DecalProjector>().size.z);
        decal.GetComponent<DecalFollow>().FollowingTransform = _decalFollowTransform;
        decal.GetComponent<DecalFollow>().LocalPosition = new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(0.2f, 0.7f), 0f);

        Vector3 pos = transform.position + dir * UnityEngine.Random.Range(0.25f, 1.25f);
        Physics.Raycast(pos, -Vector3.up, out RaycastHit hit, 50f, GameManager._instance.LayerMaskForVisible);
        pos = hit.collider == null ? transform.position - Vector3.up * 0.7f : hit.point;
        bloodPrefab = GameManager._instance.BloodDecalPrefabs[UnityEngine.Random.Range(0, GameManager._instance.BloodDecalPrefabs.Count)];
        GameObject groundDecal = Instantiate(bloodPrefab, pos, Quaternion.identity);
        groundDecal.transform.forward= hit.collider == null ? Vector3.up : -hit.normal;
        groundDecal.GetComponent<DecalProjector>().size = new Vector3(groundDecal.GetComponent<DecalProjector>().size.x * sizeMul, groundDecal.GetComponent<DecalProjector>().size.y * sizeMul, groundDecal.GetComponent<DecalProjector>().size.z);
        groundDecal.GetComponent<DecalProjector>().decalLayerMask = DecalLayerEnum.DecalLayerDefault;

        GameObject sparksVFX = Instantiate(GameManager._instance.ShiningSparksVFX[0], transform.position - transform.forward * 0.8f, Quaternion.identity);
        Destroy(sparksVFX, 4f);

        if (_attackCollider != null)
            _attackCollider.gameObject.SetActive(false);
        Vector3 currentVelocity = GetComponent<NavMeshAgent>().velocity;
        GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;

        bool isKilledByPlayer = false;
        if (!GameManager._instance._isInBossLevel && !GameManager._instance.isPlayerDead)
        {
            if (killer != null && killer.Owner != null && killer.Owner.CompareTag("Player"))
                isKilledByPlayer = true;
            MakeDeathSoundForEnemies();
            GameManager._instance.EnemyDied(isKilledByPlayer);
        }
        ActivateRagdoll(dir, killersVelocityMagnitude, currentVelocity, isKilledByPlayer);
    }
    private void MakeDeathSoundForEnemies()
    {
        foreach (var item in GameManager._instance.allEnemies)
        {
            if (item.GetComponent<EnemyAI>() != null && (item.transform.position - transform.position).magnitude < 22.5f && MathF.Abs(item.transform.position.y - transform.position.y) < 2.5f)
            {
                item.GetComponent<EnemyAI>().HearArtificialSound(transform.position);
            }
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

            if (collider.gameObject.name != "CC_Base_Pelvis")
                _rbOfJoint.Add(collider.name, collider.gameObject.GetComponent<CharacterJoint>().connectedBody.name);
            _massOfRb.Add(collider.name, collider.gameObject.GetComponent<Rigidbody>().mass);

            collider.gameObject.tag = "HitBox";
            collider.gameObject.layer = LayerMask.NameToLayer("TriggerHitBox");

            Destroy(collider.gameObject.GetComponent<Joint>());
            Destroy(collider.gameObject.GetComponent<Rigidbody>());
        }
    }
    private void ActivateRagdoll(Vector3 dir, float killersVelocityMagnitude, Vector3 currentVelocity, bool isKilledByPlayer)
    {
        if (!IsDead) return;

        float forceMultiplier = 600f;
        float forceUpMultiplier = -20f;

        if (isKilledByPlayer)
        {
            forceMultiplier *= 1.5f;
        }

        _enemyStateController._animator.enabled = false;
        //_enemyStateController._animator.avatar = null;
        _enemyStateController._enemyCombat._collider.enabled = false;
        _enemyStateController._rig.weight = 0f;

        _enemyStateController._rb.useGravity = false;
        _enemyStateController._rb.velocity = Vector3.zero;
        _enemyStateController._rb.isKinematic = true;

        foreach (SkinnedMeshRenderer mesh in _enemyStateController._meshes)
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
            if (collider.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast")) continue;

            collider.gameObject.AddComponent<Rigidbody>();
            collider.gameObject.GetComponent<Rigidbody>().mass = _massOfRb[collider.name];

            if (collider.GetComponent<PlaySoundOnCollision>() == null)
            {
                Debug.LogWarning(collider.name + " object does not have a playsoundoncollision component");
                continue;
            }
            collider.GetComponent<PlaySoundOnCollision>()._isEnabled = true;
            collider.isTrigger = false;
            collider.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
            collider.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
            collider.GetComponent<Rigidbody>().isKinematic = false;
            collider.GetComponent<Rigidbody>().useGravity = true;
            collider.GetComponent<Rigidbody>().velocity = currentVelocity;
        }
        foreach (var collider in _ragdollColliders)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast")) continue;

            if (_rbOfJoint.ContainsKey(collider.name) && collider.gameObject.name != "CC_Base_Pelvis")
            {
                if (collider.gameObject.name == "CC_Base_Hip")
                {
                    GameObject bleedingVFX = Instantiate(GameManager._instance.BleedingVFX, Vector3.zero, Quaternion.identity);
                    bleedingVFX.transform.parent = collider.transform.Find("CC_Base_Waist").Find("CC_Base_Spine01").Find("CC_Base_Spine02");
                    bleedingVFX.transform.localPosition = Vector3.up * UnityEngine.Random.Range(-0.1f, 0.25f);
                    bleedingVFX.transform.forward = -dir;
                }

                collider.gameObject.AddComponent<CharacterJoint>();
                string nameOfRb = _rbOfJoint[collider.name];
                foreach (var collider2 in _ragdollColliders)
                {
                    if (collider2.name == nameOfRb)
                        collider.GetComponent<CharacterJoint>().connectedBody = collider2.GetComponent<Rigidbody>();
                }
                if (collider.name == "CC_Base_Head")
                {
                    collider.GetComponent<CharacterJoint>().swingAxis = Vector3.zero;
                    collider.GetComponent<CharacterJoint>().axis = new Vector3(1f, 0f, 0f);
                    var s = new SoftJointLimit();
                    s.limit = -10f;
                    collider.GetComponent<CharacterJoint>().lowTwistLimit = s;
                    s.limit = 15f;
                    collider.GetComponent<CharacterJoint>().highTwistLimit = s;
                    s.limit = 10f;
                    collider.GetComponent<CharacterJoint>().swing1Limit = s;
                    s.limit = 0f;
                    collider.GetComponent<CharacterJoint>().swing2Limit = s;
                }
                else
                    collider.GetComponent<CharacterJoint>().swingAxis /= 2f;

            }

            Vector3 tempDir = dir;
            tempDir += new Vector3(UnityEngine.Random.Range(-0.6f, 0.6f), UnityEngine.Random.Range(-0.25f, 0.25f), UnityEngine.Random.Range(-0.6f, 0.6f));
            //tempDir = tempDir.normalized; commented for power variety
            if (collider.gameObject.name == "CC_Base_Hip" || collider.gameObject.name == "CC_Base_Head")
                collider.GetComponent<Rigidbody>().AddForce((tempDir * killersVelocityMagnitude * forceMultiplier / 3.5f + tempDir * forceMultiplier + Vector3.up * forceUpMultiplier / 2f) * 4f);
            else
                collider.GetComponent<Rigidbody>().AddForce((tempDir * killersVelocityMagnitude * forceMultiplier / 3.5f + tempDir * forceMultiplier + Vector3.up * forceUpMultiplier / 2f) * 1.5f);

            collider.gameObject.layer = LayerMask.NameToLayer("Default");

            GameManager._instance.CallForAction(() => Destroy(collider.GetComponent<Collider>()), 10f);
            GameManager._instance.CallForAction(() => Destroy(collider.GetComponent<Joint>()), 10f);
            GameManager._instance.CallForAction(() => Destroy(collider.GetComponent<Rigidbody>()), 10f);
        }

        //Destroy(_enemyStateController._rb);
        //Destroy(_enemyStateController._agent);
    }
}
