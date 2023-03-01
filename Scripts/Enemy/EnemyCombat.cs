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
    Katana
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
    public GameObject AttackCollider => _attackCollider.gameObject;
    public int InterruptAttackCounterGetter => _interruptAttackCounter;
    public float _CollisionVelocity => _collisionVelocity;

    private CapsuleCollider _collider;
    private CapsuleCollider _attackCollider;
    private CapsuleCollider _attackColliderWarning;
    private MeleeWeapon _meleeWeapon;
    public GameObject _weaponObject { get; private set; }
    private RangedWeapon _rangedWeapon;

    private int _interruptAttackCounter;

    private Collider[] _ragdollColliders;

    private Coroutine _attackCoroutine;
    private Coroutine _openIsAllowedToAttackCoroutine;
    private Coroutine _closeIsAttackInterruptedCoroutine;

    private Dictionary<string, string> _attackNameToPrepareName;
    private Dictionary<string, float> _attackNameToHitOpenTime;

    public bool _isStunned { get; private set; }

    public bool _isInAttackPattern { get; private set; }
    public bool _isAttacking { get; private set; }
    public bool _isPreparingAttack { get; private set; }
    public bool _isAttackInterrupted { get; private set; }
    public bool _IsDodging { get; set; }

    public bool _IsBlocking { get; set; }
    public bool _IsAllowedToAttack { get; set; }
    public bool _IsAllowedToThrow { get; set; }
    public IThrowableItem _ThrowableItem { get; set; }

    private float _attackRange;
    public float AttackRange => _attackRange;

    public bool _IsRanged { get; private set; }

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

    private void Awake()
    {
        _enemyStateController = GetComponent<EnemyStateController>();
        _collider = GetComponent<CapsuleCollider>();

        _IsAllowedToAttack = true;
        _IsAllowedToThrow = true;
        _attackWaitTime = 1f;
        _throwWaitTime = 1f;
        _dodgeTime = 0.8f;
        _blockMoveTime = 0.8f;
        _crashStunCheckValue = 10f;
    }
    
    private void Start()
    {
        ArrangeRagdoll();
        InitThrowableFromEnum();
        ArrangeThrowableCount();
        ArrangeMainWeapon();

        if (!_IsRanged)
        {
            _attackCollider = _weaponObject.transform.Find("AttackCollider").GetComponent<CapsuleCollider>();
            _attackColliderWarning = _weaponObject.transform.Find("AttackColliderWarning").GetComponent<CapsuleCollider>();
            _meleeWeapon = _attackCollider.GetComponent<MeleeWeapon>();
        }
    }
    private void ArrangeMainWeapon()
    {
        switch (_weaponType)
        {
            case WeaponTypeEnum.Sword:
                _attackRange = 8f;
                _weaponObject = Instantiate(PrefabHolder._instance.SwordPrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                _weaponObject.transform.localPosition = new Vector3(0.01773758f, 0.06871963f, -0.00327652f);
                _weaponObject.transform.localEulerAngles = new Vector3(173.077f, 30.243f, 1.591003f);
                _enemyStateController._enemyAI.SetValuesForAI(0.7f, 0.65f, 0.25f, 0.5f, 0.5f);
                //_enemyStateController._enemyAI.SetValuesForAI(0.7f, 1f, 0f, 0.5f, 0.5f);
                _attackNameToPrepareName = GameManager._instance.AttackNameToPrepareName;
                _attackNameToHitOpenTime = GameManager._instance.SwordAttackNameToHitOpenTime;
                _IsRanged = false;
                break;
            case WeaponTypeEnum.Axe:
                _attackRange = 9f;
                _weaponObject = Instantiate(PrefabHolder._instance.AxePrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                _weaponObject.transform.localPosition = new Vector3(0f, 0f, 0f);
                _weaponObject.transform.localEulerAngles = new Vector3(180f, 0f, 0f);
                _enemyStateController._enemyAI.SetValuesForAI(0.85f, 0.55f, 0.1f, 0.65f, 0.3f);
                _attackNameToPrepareName = GameManager._instance.AttackNameToPrepareName;
                _attackNameToHitOpenTime = GameManager._instance.AxeAttackNameToHitOpenTime;
                _IsRanged = false;
                break;
            case WeaponTypeEnum.Halberd:
                _attackRange = 11f;
                _weaponObject = Instantiate(PrefabHolder._instance.HalberdPrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                _weaponObject.transform.localPosition = new Vector3(0.013f, -0.002f, 0.005f);
                _weaponObject.transform.localEulerAngles = new Vector3(-188.181f, -182.17f, 11.242f);
                _enemyStateController._enemyAI.SetValuesForAI(0.9f, 0.65f, 0.05f, 0.7f, 0.2f);
                _attackNameToPrepareName = GameManager._instance.AttackNameToPrepareName;
                _attackNameToHitOpenTime = GameManager._instance.HalberdAttackNameToHitOpenTime;
                _IsRanged = false;
                break;
            case WeaponTypeEnum.Mace:
                _attackRange = 6f;
                _weaponObject = Instantiate(PrefabHolder._instance.MacePrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                _weaponObject.transform.localPosition = new Vector3(0f, 0f, 0f);
                _weaponObject.transform.localEulerAngles = new Vector3(180f, 0f, 0f);
                _enemyStateController._enemyAI.SetValuesForAI(0.35f, 0.7f, 0.1f, 0.75f, 0.3f);
                _attackNameToPrepareName = GameManager._instance.AttackNameToPrepareName;
                _attackNameToHitOpenTime = GameManager._instance.MaceAttackNameToHitOpenTime;
                _IsRanged = false;
                break;
            case WeaponTypeEnum.Hammer:
                _attackRange = 6f;
                _weaponObject = Instantiate(PrefabHolder._instance.HammerPrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                _weaponObject.transform.localPosition = new Vector3(0f, 0f, 0f);
                _weaponObject.transform.localEulerAngles = new Vector3(180f, 0f, 0f);
                _enemyStateController._enemyAI.SetValuesForAI(0.9f, 0.5f, 0.05f, 0.75f, 0.15f);
                _attackNameToPrepareName = GameManager._instance.AttackNameToPrepareName;
                _attackNameToHitOpenTime = GameManager._instance.HammerAttackNameToHitOpenTime;
                _IsRanged = false;
                break;
            case WeaponTypeEnum.Bow:
                _attackRange = 25f;
                _weaponObject = Instantiate(PrefabHolder._instance.BowPrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                _weaponObject.transform.localPosition = new Vector3(-0.007174938f, -0.01599412f, -0.02786018f);
                _weaponObject.transform.localEulerAngles = new Vector3(77.399f, -213.311f, -14.257f);
                _enemyStateController._enemyAI.SetValuesForAI(50f, 0.4f, 1f, 0.2f, 0f);
                _IsRanged = true;
                transform.Find("RangedWeapon").gameObject.SetActive(true);
                _rangedWeapon = GetComponentInChildren<RangedWeapon>();
                break;
            case WeaponTypeEnum.Crossbow:
                _attackRange = 35f;
                _weaponObject = Instantiate(PrefabHolder._instance.CrossbowPrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                _weaponObject.transform.localPosition = new Vector3(0.155f, 0.075f, -0.093f);
                _weaponObject.transform.localEulerAngles = new Vector3(2.278f, -60.697f, -34.288f);
                _enemyStateController._enemyAI.SetValuesForAI(60f, 0.2f, 1f, 0.2f, 0f);
                _IsRanged = true;
                transform.Find("RangedWeapon").gameObject.SetActive(true);
                _rangedWeapon = GetComponentInChildren<RangedWeapon>();
                break;
            case WeaponTypeEnum.Gun:
                _attackRange = 13f;
                _weaponObject = Instantiate(PrefabHolder._instance.GunPrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                _weaponObject.transform.localPosition = new Vector3(0.085f, 0.02f, -0.022f);
                _weaponObject.transform.localEulerAngles = new Vector3(82.834f, -74.831f, 86.947f);
                _enemyStateController._enemyAI.SetValuesForAI(70f, 0.4f, 1f, 0.2f, 0f);
                _IsRanged = true;
                transform.Find("RangedWeapon").gameObject.SetActive(true);
                _rangedWeapon = GetComponentInChildren<RangedWeapon>();
                break;
            case WeaponTypeEnum.Katana:
                _attackRange = 9f;
                _weaponObject = Instantiate(PrefabHolder._instance.KatanaPrefab, _weaponHolderTransform);
                GetComponentInChildren<RagdollForWeapon>()._Weapons.Add(_weaponObject);
                _weaponObject.transform.localPosition = new Vector3(0.01773758f, 0.06871963f, -0.00327652f);
                _weaponObject.transform.localEulerAngles = new Vector3(173.077f, 30.243f, 1.591003f);
                _enemyStateController._enemyAI.SetValuesForAI(0.85f, 0.8f, 0.1f, 0.8f, 0.75f);
                _attackNameToPrepareName = GameManager._instance.AttackNameToPrepareName;
                _attackNameToHitOpenTime = GameManager._instance.KatanaAttackNameToHitOpenTime;
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
        StopAttackInstantly();
        Stun(0.7f, false, deflectedKillable.Object.transform);
        _enemyStateController._enemyMovement.BlockMovement(deflectedKillable.Object.transform.position);
        _enemyStateController.ChangeAnimation(GetAttackDeflectedAnimName(), 0.2f, true);
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.AttackDeflecteds), transform.position, 0.5f, false, UnityEngine.Random.Range(0.93f, 1.07f));
    }
    public void StopAttackInstantly()
    {
        if (_IsRanged) return;

        _isInAttackPattern = false;
        _isAttacking = false;
        _isPreparingAttack = false;
        _enemyStateController.EnableHeadAim();

        _isAttackInterrupted = true;
        if (_closeIsAttackInterruptedCoroutine != null)
            StopCoroutine(_closeIsAttackInterruptedCoroutine);
        _closeIsAttackInterruptedCoroutine = StartCoroutine(CloseIsAttackInterruptedCoroutine(0.5f));

        _enemyStateController.ChangeAnimation("Empty");
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
        GameManager._instance.CallForAction(CloseIsDodging, _dodgeTime);
    }
    public void DeflectWithBlock(Vector3 dir, IKillable attacker, bool isRangedAttack)
    {
        _IsBlocking = false;

        Vector3 VFXposition = _meleeWeapon.transform.position - transform.forward * 1.25f;

        if (UnityEngine.Random.Range(0, 100) < 75)
        {
            GameObject sparksVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.SparksVFX), VFXposition, Quaternion.identity);
            Destroy(sparksVFX, 4f);

            _enemyStateController.ChangeAnimation(GetBlockAnimName(), 0.2f, true);
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Blocks), transform.position, 0.4f, false, UnityEngine.Random.Range(0.93f, 1.07f));

            _enemyStateController._enemyMovement.BlockMovement(_enemyStateController._BlockedEnemyPosition);

            _IsAllowedToAttack = false;
            if (_openIsAllowedToAttackCoroutine != null)
                StopCoroutine(_openIsAllowedToAttackCoroutine);
            _openIsAllowedToAttackCoroutine = StartCoroutine(OpenIsAllowedToAttackCoroutine(_attackWaitTime * 0.6f));
        }
        else
        {
            GameObject sparksVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.ShiningSparksVFX), VFXposition, Quaternion.identity);
            if (attacker != null && !isRangedAttack)
                attacker.AttackDeflected(this as IKillable);
            _enemyStateController.ChangeAnimation(GetDeflectAnimName(), 0.2f, true);
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Deflects), transform.position, 0.35f, false, UnityEngine.Random.Range(0.93f, 1.07f));
            Destroy(sparksVFX, 4f);

            _IsAllowedToAttack = false;
            if (_openIsAllowedToAttackCoroutine != null)
                StopCoroutine(_openIsAllowedToAttackCoroutine);
            _openIsAllowedToAttackCoroutine = StartCoroutine(OpenIsAllowedToAttackCoroutine(_attackWaitTime * 0.25f));
        }
    }
    private void PrepareAttack(string attackName)
    {
        if (WeaponType == WeaponTypeEnum.Katana) return;
        if (!_attackNameToPrepareName.TryGetValue(attackName, out string x)) return;

        _isPreparingAttack = true;

        StartCoroutine(PrepareAttackContinueOneFrameLater(attackName));
    }
    private IEnumerator PrepareAttackContinueOneFrameLater(string attackName)
    {
        _enemyStateController.ChangeAnimation("Empty", 0.2f);

        if (_attackNameToPrepareName[attackName] != "Empty")
            yield return new WaitForSeconds(0.2f);

        if (!_isAttackInterrupted)
        {
            bool isIdle = _enemyStateController.ChangeAnimation(_attackNameToPrepareName[attackName], 0.2f);

            int animLayer = 1;
            if (isIdle)
            {
                GameManager._instance.CallForAction(() =>
                {
                    if (_isAttackInterrupted) return;
                    _isPreparingAttack = false;
                }, 0.05f);
                yield break;
            }

            yield return null;
            float time = 0f;
            bool isUsingCurrent = false;
            while (_enemyStateController._animator.GetNextAnimatorClipInfo(animLayer).Length == 0)
            {
                time += Time.deltaTime;
                if (time > 0.05f && _enemyStateController._animator.GetCurrentAnimatorClipInfo(animLayer).Length != 0)
                {
                    isUsingCurrent = true;
                    break;
                }
                yield return null;
            }
            float animTime = 0f;
            if (isUsingCurrent)
            {
                animTime = _enemyStateController._animator.GetCurrentAnimatorClipInfo(animLayer)[0].clip.length / _enemyStateController._animator.GetCurrentAnimatorStateInfo(animLayer).speed;
                animTime = animTime * 0.6f;
            }
            else
            {
                animTime = _enemyStateController._animator.GetNextAnimatorClipInfo(animLayer)[0].clip.length / _enemyStateController._animator.GetNextAnimatorStateInfo(animLayer).speed;
                animTime = animTime * 0.6f;
            }

            Action CloseIsPreparingToAttack = () => {_attackCollider.gameObject.SetActive(false);
                if (_isAttackInterrupted) return;
                _isPreparingAttack = false;
            };
            GameManager._instance.CallForAction(CloseIsPreparingToAttack, animTime / 2f);
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
            List<string> patternNumbers = _enemyStateController._enemyAI.ChooseAttackPattern();
            if (_attackCoroutine != null)
                StopCoroutine(_attackCoroutine);
            _attackCoroutine = StartCoroutine(AttackPatternCoroutine(patternNumbers));
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
        _lastAttackNumberForPattern = -1;
        int c = 0;
        float lastTimeAttacked = Time.time;

        foreach (var attackName in patternNumbers)
        {
            if (lastTimeAttacked + 0.7f > Time.time)
                yield return new WaitForSeconds(0.7f - Mathf.Abs(Time.time - lastTimeAttacked));
            lastTimeAttacked = Time.time;
            if ((GameManager._instance.PlayerRb.transform.position-transform.position).magnitude > 8.5f)
            {
                break;
            }
            if (c == 0)
            {
                GameManager._instance.CallForAction(() => _enemyStateController._enemyMovement.MoveAfterAttack(true), 0.05f);
            }
            else
            {
                GameManager._instance.CallForAction(() => _enemyStateController._enemyMovement.MoveAfterAttack(false), 0.05f);
            }

            if (_lastAttackNumberForPattern == -1)
            {
                if (int.Parse(attackName.ToCharArray()[attackName.Length - 1].ToString()) != 1)
                {
                    PrepareAttack(attackName);
                    yield return new WaitWhile(() => _isPreparingAttack);
                }
            }
            else if (_lastAttackNumberForPattern + 1 != int.Parse(attackName.ToCharArray()[attackName.Length - 1].ToString()))
            {
                PrepareAttack(attackName);
                yield return new WaitWhile(() => _isPreparingAttack);
            }

            //check after prepare
            if (_isAttackInterrupted)
            {
                yield break;
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
        if (_openIsAllowedToAttackCoroutine != null)
            StopCoroutine(_openIsAllowedToAttackCoroutine);
        _openIsAllowedToAttackCoroutine = StartCoroutine(OpenIsAllowedToAttackCoroutine(_attackWaitTime));

        _isInAttackPattern = false;

    }
    private void Attack(string attackName, int animLayer = 1)
    {
        if (_isAttacking || _IsDodging) return;

        _enemyStateController.ChangeAnimation(attackName);

        _isAttacking = true;
        _enemyStateController._enemyMovement.AttackOrBlockRotation(true);
        _enemyStateController.DisableHeadAim();

        if (!_attackNameToHitOpenTime.TryGetValue(attackName, out float hitOpenTime)) return;

        StartCoroutine(AttackContinueOneFrameLater(hitOpenTime, animLayer));
        
    }
    private IEnumerator AttackContinueOneFrameLater(float hitOpenTime, int animLayer)
    {
        yield return null;
        float time = 0f;
        bool isUsingCurrent = false;
        while (_enemyStateController._animator.GetNextAnimatorClipInfo(animLayer).Length == 0)
        {
            time += Time.deltaTime;
            if (time > 0.25f && _enemyStateController._animator.GetCurrentAnimatorClipInfo(animLayer).Length != 0)
            {
                isUsingCurrent = true;
                break;
            }
            yield return null;
        }
        float animTime = 0f;
        if (isUsingCurrent)
        {
            animTime = _enemyStateController._animator.GetCurrentAnimatorClipInfo(animLayer)[0].clip.length / _enemyStateController._animator.GetCurrentAnimatorStateInfo(animLayer).speed;
            animTime = animTime * 0.95f;
            animTime -= time;
        }
        else
        {
            animTime = _enemyStateController._animator.GetNextAnimatorClipInfo(animLayer)[0].clip.length / _enemyStateController._animator.GetNextAnimatorStateInfo(animLayer).speed;
            animTime = animTime * 0.95f;
            animTime -= time;
        }
        

        Action CloseIsAttacking = () => {
            if (_isAttackInterrupted) return;
            _isAttacking = false;
            _enemyStateController.EnableHeadAim();
        };
        GameManager._instance.CallForAction(CloseIsAttacking, animTime);

        _attackColliderWarning.gameObject.SetActive(true);

        GameManager._instance.CallForAction(() => { if (_isAttackInterrupted) return; _attackColliderWarning.gameObject.SetActive(false); }, animTime);

        Action OpenAttackCollider = () => {
            if (_enemyStateController._isDead || _isAttackInterrupted) return;
            _attackCollider.gameObject.SetActive(true);
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Attacks), transform.position, 0.6f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        };
        GameManager._instance.CallForAction(OpenAttackCollider, hitOpenTime);

        Action CloseAttackCollider = () => {
            if (_isAttackInterrupted) return;
            _attackCollider.gameObject.SetActive(false);
        };
        GameManager._instance.CallForAction(CloseAttackCollider, animTime);
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
    private void RangedAttack()
    {
        if (_isAttacking || _IsDodging) return;

        _isAttacking = true;
        _enemyStateController.ChangeAnimation(GetRangedAttackName());

        StartCoroutine(RangedAttackContinueOneFrameLater());
    }
    private IEnumerator RangedAttackContinueOneFrameLater()
    {
        float startTime = Time.time;
        _rangedWarning.SetActive(true);

        while (_enemyStateController._animator.GetNextAnimatorClipInfo(1).Length == 0)
            yield return null;
        while(startTime + 0.15f > Time.time)
        {
            RangedAttackLookToPlayer();
            yield return null;
        }

        float animTime = _enemyStateController._animator.GetNextAnimatorClipInfo(1)[0].clip.length;

        _IsAllowedToAttack = false;
        if (_openIsAllowedToAttackCoroutine != null)
            StopCoroutine(_openIsAllowedToAttackCoroutine);
        _openIsAllowedToAttackCoroutine = StartCoroutine(OpenIsAllowedToAttackCoroutine(_attackWaitTime));

        SoundManager._instance.PlaySound(SoundManager._instance.BowFired, transform.position, 0.2f, false, UnityEngine.Random.Range(1.1f, 1.3f));

        float waitTime;
        if (WeaponType == WeaponTypeEnum.Bow)
            waitTime = 0.3f;
        else
            waitTime = 0.15f;
        startTime = Time.time;
        while (startTime + waitTime > Time.time)
        {
            RangedAttackLookToPlayer();
            yield return null;
        }
        _rangedWeapon.Fire(GameManager._instance.ArrowPrefab, _weaponObject.transform.position, _collider, new Vector3(transform.forward.x, (GameManager._instance.PlayerRb.transform.position - _weaponObject.transform.position).normalized.y, transform.forward.z), 36f);
        _rangedWarning.SetActive(false);
        _isAttacking = false;
        _isInAttackPattern = false;
    }
    public void RangedAttackLookToPlayer()
    {
        Vector3 lookAtPos = GameManager._instance.PlayerRb.transform.position + GameManager._instance.PlayerRb.velocity * 0.6f * 0.065f * (GameManager._instance.PlayerRb.transform.position - _enemyStateController.transform.position).magnitude;
        Vector3 normalDir = (GameManager._instance.PlayerRb.transform.position - _enemyStateController.transform.position).normalized;
        Vector3 futureDir = (GameManager._instance.PlayerRb.transform.position + GameManager._instance.PlayerRb.velocity * 0.6f * 0.065f * (GameManager._instance.PlayerRb.transform.position - _enemyStateController.transform.position).magnitude - _enemyStateController.transform.position).normalized;
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
        StartCoroutine(GunCoroutine());
    }
    private IEnumerator GunCoroutine()
    {
        _rangedWarning.SetActive(true);

        float aimTime = UnityEngine.Random.Range(0.7f, 2.2f);
        float startTime = Time.time;
        while (startTime + aimTime > Time.time)
        {
            if (_isAttackInterrupted)
                yield break;

            RangedAttackLookToPlayer();
            yield return null;
        }
        _enemyStateController.ChangeAnimation("GunFire");

        _IsAllowedToAttack = false;
        if (_openIsAllowedToAttackCoroutine != null)
            StopCoroutine(_openIsAllowedToAttackCoroutine);
        _openIsAllowedToAttackCoroutine = StartCoroutine(OpenIsAllowedToAttackCoroutine(_attackWaitTime));

        SoundManager._instance.PlaySound(SoundManager._instance.GunFired, transform.position, 0.25f, false, UnityEngine.Random.Range(1f, 1.1f));
        Instantiate(GameManager._instance.GunFireVFX, _rangedWeapon.transform.Find("BulletSpawnPos").position, Quaternion.identity);
        _rangedWeapon.Fire(GameManager._instance.BulletPrefab, _rangedWeapon.transform.Find("BulletSpawnPos").position, _collider, transform.forward, 60f);

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
        GameManager._instance._pushEvent?.Invoke((GameManager._instance.PlayerRb.transform.position - transform.position).normalized * 1f);
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
        string name = "Deflect" + UnityEngine.Random.Range(1, 3);
        return name;
    }
    private string GetAttackDeflectedAnimName()
    {
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
        _rangedWarning.SetActive(false);

        if (_ThrowableItem.CountInterface == 0) return;
        if (_IsRanged || _IsDodging || _IsBlocking || _isInAttackPattern || !_IsAllowedToThrow) return;
        
        _IsAllowedToThrow = false;
        Action OpenIsAllowedToThrow = () => {
            _IsAllowedToThrow = true;
        };
        GameManager._instance.CallForAction(OpenIsAllowedToThrow, _throwWaitTime);

        _ThrowableItem.Use(_enemyStateController._rb, _collider, false);
        _enemyStateController.ChangeAnimation(GetThrowName());
        SoundManager._instance.PlaySound(SoundManager._instance.Throw, transform.position, 0.25f, false, UnityEngine.Random.Range(0.93f, 1.07f));
    }

    public void Die(Vector3 dir, float killersVelocityMagnitude)
    {
        if (IsDead) return;

        _enemyStateController.StopAllCoroutines();
        _enemyStateController._enemyMovement.StopAllCoroutines();
        _enemyStateController._enemyCombat.StopAllCoroutines();
        _enemyStateController._enemyAI.StopAllCoroutines();

        _rangedWarning.SetActive(false);

        if (!GameManager._instance._isInBossLevel)
        {
            GameManager._instance.EnemyDied();
        }
        GameManager._instance.allEnemies.Remove(gameObject);

        //_enemyStateController._animator.SetTrigger("Death");
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Cuts), transform.position, 0.5f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        //die vfx, blood and sound etc
        Vector3 VFXposition = transform.position + transform.forward * 0.5f;
        GameObject bloodVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.BloodVFX), VFXposition, Quaternion.identity);
        bloodVFX.GetComponentInChildren<Rigidbody>().velocity = Vector3.up * 2f + transform.right * UnityEngine.Random.Range(-1f, 1f) + dir * UnityEngine.Random.Range(4f, 6f);
        Destroy(bloodVFX, 5f);

        GameObject bloodPrefab = GameManager._instance.BloodDecalPrefabs[UnityEngine.Random.Range(0, GameManager._instance.BloodDecalPrefabs.Count)];
        GameObject decal = Instantiate(bloodPrefab, transform);
        float size = UnityEngine.Random.Range(0.75f, 1.5f);
        decal.GetComponent<DecalProjector>().size = new Vector3(size, size, decal.GetComponent<DecalProjector>().size.z);
        decal.GetComponent<DecalFollow>().FollowingTransform = _decalFollowTransform;
        decal.GetComponent<DecalFollow>().LocalPosition = new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(0.2f, 0.7f), 0f);

        _enemyStateController._isDead = true;
        if (_attackCollider != null)
            _attackCollider.gameObject.SetActive(false);
        Vector3 currentVelocity = GetComponent<NavMeshAgent>().velocity;
        GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        ActivateRagdoll(dir, killersVelocityMagnitude, currentVelocity);

        if (!GameManager._instance.isPlayerDead)
        {
            GameManager._instance.SlowTime();
            GameManager._instance.ActivateWarningUI();
        }
    }
    private void ArrangeRagdoll()
    {
        _ragdollColliders = transform.Find("Model").GetComponentsInChildren<Collider>();

        foreach (var collider in _ragdollColliders)
        {
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

        float forceMultiplier = 700f;
        float forceUpMultiplier = 20f;

        _enemyStateController._animator.enabled = false;
        //_enemyStateController._animator.avatar = null;
        _enemyStateController._enemyCombat._collider.enabled = false;
        _enemyStateController._rig.weight = 0f;

        _enemyStateController._rb.useGravity = false;
        _enemyStateController._rb.velocity = Vector3.zero;
        _enemyStateController._rb.isKinematic = true;

        _enemyStateController._mesh.updateWhenOffscreen = true;

        if (_ragdollColliders == null)
            _ragdollColliders = transform.Find("Model").GetComponentsInChildren<Collider>();

        GetComponentInChildren<TransformBonePosition>().TransformPosition();
        Vector3 tempDirForWeapon = dir + new Vector3(UnityEngine.Random.Range(-0.6f, 0.6f), UnityEngine.Random.Range(-0.25f, 0.25f), UnityEngine.Random.Range(-0.6f, 0.6f));
        GetComponentInChildren<RagdollForWeapon>().SeperateWeaponsFromRagdoll(tempDirForWeapon, forceMultiplier, forceUpMultiplier, killersVelocityMagnitude);

        foreach (var collider in _ragdollColliders)
        {
            collider.GetComponent<PlaySoundOnCollision>()._isEnabled = true;
            collider.isTrigger = false;
            collider.GetComponent<Rigidbody>().velocity = currentVelocity;
            collider.GetComponent<Rigidbody>().isKinematic = false;

            Vector3 tempDir = dir;
            tempDir += new Vector3(UnityEngine.Random.Range(-0.6f, 0.6f), UnityEngine.Random.Range(-0.25f, 0.25f), UnityEngine.Random.Range(-0.6f, 0.6f));
            //tempDir = tempDir.normalized; commented for power variety
            if (collider.gameObject.name == "CC_Base_Hip" || collider.gameObject.name == "CC_Base_Head")
                collider.GetComponent<Rigidbody>().AddForce((tempDir * killersVelocityMagnitude * forceMultiplier / 50f + tempDir * forceMultiplier + Vector3.up * forceUpMultiplier) * 6f);
            else
                collider.GetComponent<Rigidbody>().AddForce((tempDir * killersVelocityMagnitude * forceMultiplier / 50f + tempDir * forceMultiplier + Vector3.up * forceUpMultiplier) * 2.5f);
        }

        //Destroy(_enemyStateController._rb);
        //Destroy(_enemyStateController._agent);
    }
}
