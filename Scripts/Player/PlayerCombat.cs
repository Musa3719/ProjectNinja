using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour, IKillable
{
    private enum AttackType
    {
        Both,
        Left,
        Right
    }

    public static PlayerCombat _instance;
    private AttackType _attackType;

    public GameObject Object => gameObject;
    public bool IsDead => GameManager._instance.isPlayerDead;
    public bool IsBlockingGetter => _IsBlocking;
    public bool IsDodgingGetter => _IsDodging;
    public GameObject AttackCollider => _attackCollider.gameObject;
    public int InterruptAttackCounterGetter => _interruptAttackCounter;
    public float _CollisionVelocity => CollisionVelocity;
    public bool _DeflectedBuffer { get => false; set {  } }

    public CapsuleCollider _collider { get; private set; }

    private AttackType _lastAttackType;

    [SerializeField]
    private Transform _sparkPosition;
    [SerializeField]
    private CapsuleCollider _attackColliderWarning;
    [SerializeField]
    private BoxCollider _rightAttackColliderWarningForLeap;
    [SerializeField]
    private BoxCollider _leftAttackColliderWarningForLeap;
    [SerializeField]
    private CapsuleCollider _attackCollider;
    [SerializeField]
    private BoxCollider _rightAttackColliderForLeap;
    [SerializeField]
    private BoxCollider _leftAttackColliderForLeap;
    private int _interruptAttackCounter;

    private MeleeWeapon _blades;

    public GameObject MeleeWeapon;

    public List<IThrowableItem> _ThrowableInventory { get; private set; }
    public IThrowableItem _CurrentThrowableItem { get; set; }

    private Coroutine _isBlockedOrDeflectedCoroutine;
    private Coroutine _customPassOpenCoroutine;
    private Coroutine _attackMoveCoroutine;
    private Coroutine _dropWeaponStartCoroutine;

    public bool _IsStunned { get; private set; }

    private bool _isBlocking;
    public bool _IsDodging { get; set; }

    public bool _IsBlocking { get { return _isBlocking; } set { if (_isBlocking == false && value == true) { _blockStartTime = Time.time; } _isBlocking = value; } }
    public bool _IsBlockedOrDeflected { get; set; }
    public bool _IsAttacking { get; set; }
    public bool _IsAttackInterrupted { get; set; }
    public bool _isAllowedToBlock { get; set; }
    public bool _isAllowedToDodge { get; set; }
    public bool _isAllowedToAttack { get; set; }
    public bool _isAllowedToForwardLeap { get; set; }
    public bool _isAllowedToThrow { get; set; }

    public float _AttackBlockedCounter;

    private float _blockStartTime;
    private float _blockTimingValue;
    private float _blockWaitTime;
    private float _attackWaitTime;
    private float _attackTime;
    private float _forwardLeapWaitTime;
    private float _throwWaitTime;
    private float _dodgeWaitTime;
    private float _dodgeTime;

    public bool _isImmune { get; set; }

    private int _lastAttackDeflectedCounter;
    private float _lastAttackDeflectedTime;
    private float _lastDeflectedTime;

    public float _AttackTime => _attackTime;
    public float _forwardLeapTime { get; private set; }

    private float _crashStunCheckValue;
    public float CollisionVelocity;

    private GameObject _illusion;
    private Vector3 _weaponScale;
    private void Awake()
    {
        _instance = this;
        _collider = GetComponent<CapsuleCollider>();
        _ThrowableInventory = new List<IThrowableItem>();
        _blades = _attackCollider.GetComponent<MeleeWeapon>();
        _isAllowedToBlock = true;
        _isAllowedToDodge = true;
        _isAllowedToAttack = true;
        _isAllowedToForwardLeap = true;
        _isAllowedToThrow = true;
        _blockWaitTime = 0.1f;
        _attackWaitTime = 0.6f;
        _attackTime = 0.6f;
        _forwardLeapWaitTime = 3f;
        _throwWaitTime = 0.8f;
        _dodgeWaitTime = 0.75f;
        _dodgeTime = 0.05f;
        _forwardLeapTime = 0.2f;
        _blockTimingValue = 0.225f;
        _crashStunCheckValue = 11.25f;

        AttackCollider.transform.localPosition = new Vector3(0f, 0.2f, 0.65f);
        AttackCollider.GetComponent<CapsuleCollider>().radius = 0.65f;
    }
    
    private void FixedUpdate()
    {
        GameManager._instance.PlayerLastSpeed = CollisionVelocity;
        CollisionVelocity = PlayerStateController._instance._rb.velocity.magnitude;
    }
    
    public void StopBlockingAndDodge()
    {
        //
    }
    public void AttackDeflected(IKillable deflectedKillable)
    {
        CameraController.ShakeCamera(4f, 1.8f, 0.1f, 0.3f);
        PlayerMovement._instance._Stamina -= PlayerMovement._instance._attackDeflectedStaminaUse;
        PlayerStateController._instance.ChangeAnimation(GetAttackDeflectedName());
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.AttackDeflecteds), transform.position, 0.5f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        _IsAttacking = false;
        _IsAttackInterrupted = true;
        PlayerMovement._instance.AttackDeflectedMove(PlayerStateController._instance._rb);
        GameManager._instance.CallForAction(() => _IsAttackInterrupted = false, _attackWaitTime * 1.25f);
        _attackColliderWarning.gameObject.SetActive(false);
        _attackCollider.gameObject.SetActive(false);

        _isAllowedToAttack = false;
        GameManager._instance.CallForAction(() => _isAllowedToAttack = true, _attackWaitTime * 1.25f);
    }

    public void AddToThrowableInventory(IThrowableItem item)
    {
        if (_ThrowableInventory.Contains(item))
        {
            item.CountInterface++;
        }
        else
        {
            _ThrowableInventory.Add(item);
            item.CountInterface = 1;

            if (_ThrowableInventory.Count == 1)
            {
                _CurrentThrowableItem = item;
            }

        }
    }
    public void RemoveFromThrowableInventory(IThrowableItem item)
    {
        _ThrowableInventory.Remove(item);
        item.CountInterface = 0;

        if (_ThrowableInventory.Count > 0)
        {
            _CurrentThrowableItem = _ThrowableInventory[0];
        }
        else
        {
            _CurrentThrowableItem = null;
        }
    }
    public void UseThrowableItem(IThrowableItem item)
    {
        CameraController.ShakeCamera(1f, 1f, 0.1f, 0.3f);

        bool isCountZero = item.Use(PlayerStateController._instance._rb, _collider);
        if (isCountZero)
        {
            RemoveFromThrowableInventory(item);
        }
    }
    public void Dodge()
    {
        if (!_isAllowedToDodge) return;

        _isAllowedToDodge = false;
        _IsDodging = true;

        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.ArmorWalkSounds), transform.position, 0.14f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        PlayerStateController._instance.EnterAnimState(new PlayerAnimations.WaitForOneAnim(0.25f));
        CameraController.ShakeCamera(3f, 1.6f, 0.1f, 0.7f);

        Action OpenIsAllowedToDodge = () => {
            _isAllowedToDodge = true;
        };
        GameManager._instance.CallForAction(OpenIsAllowedToDodge, _dodgeWaitTime);

        Action CloseIsDodging = () => {
            _IsDodging = false;
        };
        GameManager._instance.CallForAction(CloseIsDodging, _dodgeTime);
    }
    public void DeflectWithBlock(Vector3 dir, IKillable attacker, bool isRangedAttack)
    {
        if (Time.time - _blockStartTime > _blockTimingValue)
        {
            if (PlayerMovement._instance._Stamina < PlayerMovement._instance._blockedStaminaUse)
            {
                Die(dir, attacker.Object.GetComponent<Rigidbody>().velocity.magnitude, null);
                return;
            }

            PlayerStateController._instance.ChangeAnimation(GetBlockedName());
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Blocks), transform.position, 0.175f, false, UnityEngine.Random.Range(0.93f, 1.07f));
            CameraController.ShakeCamera(4f, 1.15f, 0.15f, 0.65f);
            PlayerMovement._instance.BlockedMove(PlayerStateController._instance._rb, -dir);
            _AttackBlockedCounter = 0.4f;
            PlayerMovement._instance._Stamina -= PlayerMovement._instance._blockedStaminaUse;

            if (_isBlockedOrDeflectedCoroutine != null)
                StopCoroutine(_isBlockedOrDeflectedCoroutine);
            _isBlockedOrDeflectedCoroutine = StartCoroutine(IsBlockedOrDeflectedCoroutine());

            GameObject sparksVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.SparksVFX), _sparkPosition.position, Quaternion.identity);
            Destroy(sparksVFX, 4f);
            GameObject combatSmokeVFX = Instantiate(GameManager._instance.CombatSmokeVFX, transform.position + transform.forward * 0.1f, Quaternion.LookRotation(attacker.Object.transform.position - transform.position));
            Destroy(combatSmokeVFX, 4f);

            //_isAllowedToBlock = false;
            Action OpenIsAllowedToBlock = () => {
                _isAllowedToBlock = true;
            };
            GameManager._instance.CallForAction(OpenIsAllowedToBlock, _blockWaitTime * 2f);

            if (_isAllowedToAttack)
            {
                _isAllowedToAttack = false;
                Action OpenIsAllowedToAttack = () => {
                    _isAllowedToAttack = true;
                };
                GameManager._instance.CallForAction(OpenIsAllowedToAttack, _attackWaitTime / 1.15f);
            }

            _lastAttackDeflectedCounter = 0;
        }
        else
        {
            CameraController.ShakeCamera(3f, 1.1f, 0.2f, 0.5f);
            PlayerStateController._instance.ChangeAnimation(GetDeflectedName());
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Deflects), transform.position, 0.175f, false, UnityEngine.Random.Range(0.93f, 1.07f));

            if (_isBlockedOrDeflectedCoroutine != null)
                StopCoroutine(_isBlockedOrDeflectedCoroutine);
            _isBlockedOrDeflectedCoroutine = StartCoroutine(IsBlockedOrDeflectedCoroutine());

            _lastDeflectedTime = Time.time;

            GameObject sparksVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.ShiningSparksVFX), _sparkPosition.position, Quaternion.identity);
            Destroy(sparksVFX, 4f);
            GameObject combatSmokeVFX = Instantiate(GameManager._instance.CombatSmokeVFX, transform.position + transform.forward * 0.1f, Quaternion.LookRotation(attacker.Object.transform.position - transform.position));
            Destroy(combatSmokeVFX, 4f);

            if (attacker != null && attacker.Object.CompareTag("Boss"))
            {
                attacker.ChangeStamina(-1f);
                if (!isRangedAttack && _lastAttackDeflectedCounter >= attacker.InterruptAttackCounterGetter && _lastAttackDeflectedTime + 5f > Time.time)
                {
                    _lastAttackDeflectedCounter = -1;
                    attacker.AttackDeflected(this as IKillable);
                }
            }
            else
            {
                if (attacker != null && !isRangedAttack)
                    attacker.AttackDeflected(this as IKillable);
            }

            //_isAllowedToBlock = false;
            Action OpenIsAllowedToBlock = () => {
                _isAllowedToBlock = true;
            };
            GameManager._instance.CallForAction(OpenIsAllowedToBlock, _blockWaitTime);

            /*if (_isAllowedToAttack)
            {
                _isAllowedToAttack = false;
                Action OpenIsAllowedToAttack = () => {
                    _isAllowedToAttack = true;
                };
                GameManager._instance.CallForAction(OpenIsAllowedToAttack, _attackWaitTime / 4f);
            }*/

            _lastAttackDeflectedCounter++;
            _lastAttackDeflectedTime = Time.time;
        }
    }
    private IEnumerator IsBlockedOrDeflectedCoroutine()
    {
        _IsBlockedOrDeflected = true;
        yield return new WaitForSeconds(0.35f);
        _IsBlockedOrDeflected = false;
    }

    public void TakeWeapon()
    {
        if (MeleeWeapon != null) return;

        MeleeWeaponForPlayer meleeWeaponForPlayer = null;
        RaycastHit[] hits = new RaycastHit[9];
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f, Camera.main.transform.forward, out hits[0], 4f);
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f + transform.right / 5f, Camera.main.transform.forward, out hits[1], 4f);
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f - transform.right / 5f, Camera.main.transform.forward, out hits[2], 4f);
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f + transform.up / 5f, Camera.main.transform.forward, out hits[3], 4f);
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f - transform.up / 5f, Camera.main.transform.forward, out hits[4], 4f);
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f + transform.right / 5f + transform.up / 5f, Camera.main.transform.forward, out hits[5], 4f);
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f - transform.right / 5f + transform.up / 5f, Camera.main.transform.forward, out hits[6], 4f);
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f + transform.right / 5f - transform.up / 5f, Camera.main.transform.forward, out hits[7], 4f);
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f - transform.right / 5f - transform.up / 5f, Camera.main.transform.forward, out hits[8], 4f);
        foreach (var hit in hits)
        {
            if (hit.transform != null && hit.transform.GetComponent<MeleeWeaponForPlayer>() != null)
            {
                meleeWeaponForPlayer = hit.transform.GetComponent<MeleeWeaponForPlayer>();
                break;
            }
            else if (hit.transform != null && hit.transform.parent != null && hit.transform.parent.GetComponent<MeleeWeaponForPlayer>() != null)
            {
                meleeWeaponForPlayer = hit.transform.parent.GetComponent<MeleeWeaponForPlayer>();
                break;
            }
        }

        if (meleeWeaponForPlayer == null) return;

        MeleeWeapon = meleeWeaponForPlayer.gameObject;
        _weaponScale = MeleeWeapon.transform.localScale;
        MeleeWeapon.GetComponentInChildren<MeshRenderer>().renderingLayerMask = 1;
        MeleeWeapon.transform.parent = GameManager._instance.PlayerRightHandHolder;

        MeleeWeapon.transform.Find("AttackCollider").GetComponent<MeleeWeapon>().SetIgnoreCollisionColliderForTakeWeapon(_collider);

        if (MeleeWeapon.GetComponentInChildren<Collider>() != null)
            Destroy(MeleeWeapon.GetComponentInChildren<Collider>());
        if (MeleeWeapon.GetComponentInChildren<Rigidbody>() != null)
            Destroy(MeleeWeapon.GetComponentInChildren<Rigidbody>());
        if (MeleeWeapon.GetComponentInChildren<MeleeWeaponThrowed>() != null)
            Destroy(MeleeWeapon.GetComponentInChildren<MeleeWeaponThrowed>());

        MeleeWeapon.transform.Find("AttackCollider").gameObject.SetActive(false);
        MeleeWeapon.transform.Find("AttackColliderWarning").gameObject.SetActive(false);

        switch (meleeWeaponForPlayer.WeaponType)
        {
            case MeleeWeaponType.Sword:
                MeleeWeapon.transform.localPosition = new Vector3(0.00136f, 0.00403f, 0.00045f);
                MeleeWeapon.transform.localEulerAngles = new Vector3(7.847f, 12.163f, 95.998f);
                break;
            case MeleeWeaponType.Katana:
                MeleeWeapon.transform.localPosition = new Vector3(-0.00019f, 0.00416f, 0.00119f);
                MeleeWeapon.transform.localEulerAngles = new Vector3(89.158f, 111.111f, 192.244f);
                MeleeWeapon.transform.localScale = new Vector3(4f, 4f, 4f);
                break;
            case MeleeWeaponType.Mace:
                MeleeWeapon.transform.localPosition = new Vector3(0f, 0f, 0f);
                MeleeWeapon.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                break;
            case MeleeWeaponType.Hammer:
                MeleeWeapon.transform.localPosition = new Vector3(0f, 0f, 0f);
                MeleeWeapon.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                break;
            case MeleeWeaponType.Axe:
                MeleeWeapon.transform.localPosition = new Vector3(0f, 0f, 0f);
                MeleeWeapon.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                break;
            case MeleeWeaponType.Zweihander:
                MeleeWeapon.transform.localPosition = new Vector3(0f, 0f, 0f);
                MeleeWeapon.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                break;
            default:
                break;
        }
        MeleeWeapon.GetComponentInChildren<MeshRenderer>().gameObject.layer = LayerMask.NameToLayer("HandMesh");
    }
    public void DropWeaponStart()
    {
        if (_dropWeaponStartCoroutine != null)
            StopCoroutine(_dropWeaponStartCoroutine);
        _dropWeaponStartCoroutine = StartCoroutine(DropWeaponStartCoroutine());
    }
    private IEnumerator DropWeaponStartCoroutine()
    {
        PlayerStateController._instance.ChangeAnimation("ThrowWeaponStart");
        PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Wait());

        float startTime = Time.time;
        while (InputHandler.GetButton("WeaponChange"))
        {
            yield return null;
        }
        DropWeapon(Time.time - startTime + 1f);
    }
    private void DropWeapon(float power)
    {
        power = Mathf.Clamp(power, 1f, 3f);

        PlayerStateController._instance.ChangeAnimation("ThrowWeapon");
        PlayerStateController._instance.EnterAnimState(new PlayerAnimations.WaitForOneAnim(0.8f));

        MeleeWeapon.transform.parent = null;
        MeleeWeapon.GetComponentInChildren<MeshRenderer>().renderingLayerMask = 257;

        if (MeleeWeapon.transform.Find("AttackCollider") != null)
            if (power > 2f)
                MeleeWeapon.transform.Find("AttackCollider").gameObject.SetActive(true);
            else
                MeleeWeapon.transform.Find("AttackCollider").gameObject.SetActive(false);

        if (MeleeWeapon.GetComponentInChildren<MeshRenderer>() != null)
            MeleeWeapon.GetComponentInChildren<MeshRenderer>().gameObject.AddComponent(typeof(MeshCollider)).GetComponent<MeshCollider>().convex = true;
        else
            MeleeWeapon.AddComponent(typeof(BoxCollider));

        Rigidbody rb = (Rigidbody)MeleeWeapon.AddComponent(typeof(Rigidbody));
        MeleeWeaponThrowed weaponInAir = (MeleeWeaponThrowed)MeleeWeapon.AddComponent(typeof(MeleeWeaponThrowed));
        weaponInAir.IgnoreCollisionCollider = _collider;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        switch (MeleeWeapon.GetComponent<MeleeWeaponForPlayer>().WeaponType)
        {
            case MeleeWeaponType.Sword:
                power *= 0.8f;
                break;
            case MeleeWeaponType.Katana:
                power *= 0.9f;
                break;
            case MeleeWeaponType.Mace:
                power *= 0.8f;
                break;
            case MeleeWeaponType.Hammer:
                power *= 0.7f;
                break;
            case MeleeWeaponType.Axe:
                power *= 0.6f;
                break;
            case MeleeWeaponType.Zweihander:
                power *= 0.6f;
                break;
            default:
                break;
        }

        rb.velocity = Camera.main.transform.forward * 15f * power + Vector3.up * 1f * power;
        rb.angularVelocity = power * MeleeWeapon.transform.forward * -7f;

        MeleeWeapon.GetComponentInChildren<MeshRenderer>().gameObject.layer = LayerMask.NameToLayer("Default");
        MeleeWeapon.transform.localScale = _weaponScale;

        MeleeWeapon = null;
    }
    #region Attacks
    public void DashAttack()
    {
        if (!_isAllowedToAttack) return;

        if (MeleeWeapon != null && MeleeWeapon.GetComponent<MeleeWeaponForPlayer>().IsTeleportWeapon())
        {
            if (_attackMoveCoroutine != null)
                StopCoroutine(_attackMoveCoroutine);
            _attackMoveCoroutine = StartCoroutine(AttackTeleportCoroutine(true));
        }
        else
        {
            if (_attackMoveCoroutine != null)
                StopCoroutine(_attackMoveCoroutine);
            _attackMoveCoroutine = StartCoroutine(AttackTeleportCoroutine(false));
        }
    }
    public void Attack()
    {
        if (!_isAllowedToAttack) return;

        if (MeleeWeapon != null)
        {
            if (_attackMoveCoroutine != null)
                StopCoroutine(_attackMoveCoroutine);
            _attackMoveCoroutine = StartCoroutine(AttackWithWeaponNormal());
        }
        else
        {
            if (_attackMoveCoroutine != null)
                StopCoroutine(_attackMoveCoroutine);
            _attackMoveCoroutine = StartCoroutine(AttackWithBladesNormal());
        }
    }
    private IEnumerator AttackWithBladesNormal()
    {
        PlayerMovement._instance.AttackMoveNormal(PlayerStateController._instance._rb);

        yield return new WaitWhile(() => PlayerMovement._instance._isOnAttackOrAttackDeflectedMove);

        string attackName = GetAttackNameBladeNormal();

        PlayerMovement._instance._Stamina -= PlayerMovement._instance._attackStaminaUse;
        _isAllowedToAttack = false;
        PlayerCombat._instance._IsAttacking = true;

        //CameraController._instance.GetComponentInChildren<Animator>().CrossFade("Camera" + attackName, 0.15f);

        GameManager._instance.PlayerAttackHandle();
        GameObject aimAssistedEnemy = GameManager._instance.CheckForAimAssist();
        if (aimAssistedEnemy != null)
        {
            PlayerMovement._instance.AimAssistToEnemy(aimAssistedEnemy);
        }

        float localAttackWaitTime = _attackWaitTime;
        if (_lastDeflectedTime + 1.75f > Time.time || _isImmune)
            localAttackWaitTime /= 2f;
        GameManager._instance.CallForAction(() => { if (_IsAttackInterrupted) return; _isAllowedToAttack = true; }, localAttackWaitTime);

        PlayerStateController._instance.ChangeAnimation(attackName);
        GameManager._instance.CallForAction(() => SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Attacks), transform.position, 0.075f, false, UnityEngine.Random.Range(1f, 1.1f)), 0.2f);
        SoundManager._instance.PlaySound(SoundManager._instance.BladeSlide, transform.position, 0.12f, false, UnityEngine.Random.Range(0.65f, 0.75f));


        if (_attackColliderWarning.gameObject.activeSelf)
            _attackColliderWarning.gameObject.SetActive(false);
        _attackColliderWarning.gameObject.SetActive(true);

        GameManager._instance.CallForAction(() => { if (IsDead) return; _attackColliderWarning.gameObject.SetActive(false); }, _attackTime * 0.37f);

        GameManager._instance.CallForAction(() => { if (IsDead) return; _attackCollider.gameObject.SetActive(true); CameraController.ShakeCamera(3f, 2.5f, 0.2f, 0.4f); }, _attackTime * 0.32f);

        GameManager._instance.CallForAction(() => { if (IsDead) return; _attackCollider.gameObject.SetActive(false); }, _attackTime * 0.37f);

        GameManager._instance.CallForAction(() => { if (IsDead) return; PlayerCombat._instance._IsAttacking = false; }, _attackTime * 0.9f);
    }
    private IEnumerator AttackWithWeaponNormal()
    {
        PlayerMovement._instance.AttackMoveNormal(PlayerStateController._instance._rb);

        yield return new WaitWhile(() => PlayerMovement._instance._isOnAttackOrAttackDeflectedMove);

        string attackName = GetAttackNameWeapon(false);

        PlayerMovement._instance._Stamina -= PlayerMovement._instance._attackStaminaUse;
        _isAllowedToAttack = false;
        PlayerCombat._instance._IsAttacking = true;

        //CameraController._instance.GetComponentInChildren<Animator>().CrossFade("Camera" + attackName, 0.15f);

        GameManager._instance.PlayerAttackHandle();
        GameObject aimAssistedEnemy = GameManager._instance.CheckForAimAssist();
        if (aimAssistedEnemy != null)
        {
            PlayerMovement._instance.AimAssistToEnemy(aimAssistedEnemy);
        }

        float localAttackWaitTime = _attackWaitTime;
        if (_lastDeflectedTime + 1.75f > Time.time || _isImmune)
            localAttackWaitTime /= 2f;
        GameManager._instance.CallForAction(() => { if (_IsAttackInterrupted) return; _isAllowedToAttack = true; }, localAttackWaitTime);

        PlayerStateController._instance.ChangeAnimation(attackName);
        GameManager._instance.CallForAction(() => SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Attacks), transform.position, 0.075f, false, UnityEngine.Random.Range(1f, 1.1f)), 0.2f);

        if (_attackColliderWarning.gameObject.activeSelf)
            _attackColliderWarning.gameObject.SetActive(false);
        _attackColliderWarning.gameObject.SetActive(true);

        CameraController.ShakeCamera(4f, 3f, 0.2f, 0.5f); 

        GameManager._instance.CallForAction(() => { if (IsDead) return; PlayerCombat._instance._IsAttacking = false; _attackColliderWarning.gameObject.SetActive(false); }, _attackTime * 0.9f);
    }
    private IEnumerator AttackTeleportCoroutine(bool isWeapon)
    {
        if (_illusion != null) Destroy(_illusion);

        GameManager._instance.SlowTime();
        PlayerMovement._instance._Stamina -= PlayerMovement._instance._attackStaminaUse * 2.5f;
        float teleportDistance = 0f;
        _illusion = GameObject.Instantiate(GameManager._instance.TeleportIllusion, transform.position, Quaternion.identity);
        float rayDistance = 4f;
        while (InputHandler.GetButton("MiddleMouse"))
        {
            rayDistance += Time.unscaledDeltaTime * 4f;
            rayDistance = Mathf.Clamp(rayDistance, 4f, 14f);
            Physics.Raycast(transform.position, Camera.main.transform.forward, out RaycastHit hit, rayDistance);
            if (hit.collider != null)
                teleportDistance = (hit.point - transform.position).magnitude;
            else
                teleportDistance = rayDistance;
            _illusion.transform.position = transform.position + Camera.main.transform.forward * teleportDistance;
            _illusion.transform.LookAt(Camera.main.transform);
            yield return null;
        }
        Destroy(_illusion);
        GameManager._instance.TimeStopEndSignal = true;
        if (isWeapon)
            AttackWithWeaponTeleport(teleportDistance);
        else
            AttackWithBladesTeleport(teleportDistance);
    }
    private void AttackWithBladesTeleport(float teleportDistance)
    {
        PlayerMovement._instance.AttackMoveTeleport(PlayerStateController._instance._rb, teleportDistance);

        string attackName = GetAttackNameBladeTeleport();

        _isAllowedToAttack = false;
        PlayerCombat._instance._IsAttacking = true;

        GameManager._instance.PlayerAttackHandle();

        float localAttackWaitTime = _attackWaitTime;
        if (_lastDeflectedTime + 1.75f > Time.time || _isImmune)
            localAttackWaitTime /= 2f;
        GameManager._instance.CallForAction(() => { if (_IsAttackInterrupted) return; _isAllowedToAttack = true; }, localAttackWaitTime);

        PlayerStateController._instance.ChangeAnimation(attackName);
        GameManager._instance.CallForAction(() => SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Attacks), transform.position, 0.075f, false, UnityEngine.Random.Range(1f, 1.1f)), 0.2f);
        SoundManager._instance.PlaySound(SoundManager._instance.BladeSlide, transform.position, 0.12f, false, UnityEngine.Random.Range(0.65f, 0.75f));


        if (_attackColliderWarning.gameObject.activeSelf)
            _attackColliderWarning.gameObject.SetActive(false);
        _attackColliderWarning.gameObject.SetActive(true);

        GameManager._instance.CallForAction(() => { if (IsDead) return; _attackColliderWarning.gameObject.SetActive(false); }, _attackTime * 0.37f);

        GameManager._instance.CallForAction(() => { if (IsDead) return; _attackCollider.gameObject.SetActive(true); CameraController.ShakeCamera(2.5f, 2f, 0.2f, 0.4f);  }, _attackTime * 0.32f);

        GameManager._instance.CallForAction(() => { if (IsDead) return; _attackCollider.gameObject.SetActive(false);  }, _attackTime * 0.37f);

        GameManager._instance.CallForAction(() => { if (IsDead) return; PlayerCombat._instance._IsAttacking = false; }, _attackTime * 0.9f);
    }
    private void AttackWithWeaponTeleport(float teleportDistance)
    {
        PlayerMovement._instance.AttackMoveTeleport(PlayerStateController._instance._rb, teleportDistance);

        string attackName = GetAttackNameWeapon(true);

        _isAllowedToAttack = false;
        PlayerCombat._instance._IsAttacking = true;

        GameManager._instance.PlayerAttackHandle();

        float localAttackWaitTime = _attackWaitTime;
        if (_lastDeflectedTime + 1.75f > Time.time || _isImmune)
            localAttackWaitTime /= 2f;
        GameManager._instance.CallForAction(() => { if (_IsAttackInterrupted) return; _isAllowedToAttack = true; }, localAttackWaitTime);

        PlayerStateController._instance.ChangeAnimation(attackName);
        GameManager._instance.CallForAction(() => SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Attacks), transform.position, 0.075f, false, UnityEngine.Random.Range(1f, 1.1f)), 0.2f);

        if (_attackColliderWarning.gameObject.activeSelf)
            _attackColliderWarning.gameObject.SetActive(false);
        _attackColliderWarning.gameObject.SetActive(true);

        CameraController.ShakeCamera(4f, 3f, 0.2f, 0.5f);

        GameManager._instance.CallForAction(() => { if (IsDead) return; PlayerCombat._instance._IsAttacking = false; _attackColliderWarning.gameObject.SetActive(false); }, _attackTime * 0.9f);
    }


    #endregion
    private IEnumerator CustomPassOpenCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        GameManager._instance.CustomPassForHands.enabled = true;
    }
    private void SelectAttackType()
    {
        if (PlayerStateController._instance._playerState is PlayerStates.OnWall)
        {
            bool isLeft = !(PlayerStateController._instance._playerState as PlayerStates.OnWall)._isWallOnLeftSide;
            if(isLeft)
                _attackType = AttackType.Left;
            else
                _attackType = AttackType.Right;
        }
        else if (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(4).IsName("DeflectedToRight"))
        {
            _attackType = AttackType.Right;
        }
        else if (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(4).IsName("DeflectedToLeft"))
        {
            _attackType = AttackType.Left;
        }
        else
        {
            do
            {
                int random = UnityEngine.Random.Range(0, 3);
                if (random == 0)
                    _attackType = AttackType.Both;
                else if (random == 1)
                    _attackType = AttackType.Left;
                else if (random == 2)
                    _attackType = AttackType.Right;
            } while (_attackType == _lastAttackType);
        }

        if (_attackType != AttackType.Both)
        {
            if(!PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(4).IsName("EmptyCombat"))
                PlayerStateController._instance.ChangeAnimation("EmptyCombat", 0.25f);

            if (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(1).IsName("Idle") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(1).IsName("Walk") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(1).IsName("Run"))
            {
                if (_attackType == AttackType.Left)
                {
                    PlayerStateController._instance.ChangeAnimation("RightEmptyForAttack");
                    PlayerStateController._instance.EnterAnimState(new PlayerAnimations.WaitForOneAnim(0.25f));
                }
                else
                {
                    PlayerStateController._instance.ChangeAnimation("LeftEmptyForAttack");
                    PlayerStateController._instance.EnterAnimState(new PlayerAnimations.WaitForOneAnim(0.25f));
                }
                
            }

        }

        _lastAttackType = _attackType;
        
    }
    public void ChangeStamina(float amount)
    {
        //
    }
    public void ForwardLeap()
    {
        if (!_isAllowedToForwardLeap) return;

        PlayerMovement._instance._Stamina -= PlayerMovement._instance._forwardLeapStaminaUse;
        _isAllowedToForwardLeap = false;
        _IsDodging = true;

        CameraController.ShakeCamera(1.5f, 1.25f, 0.1f, 0.3f);

        GameManager._instance.CallForAction(() => { _isAllowedToForwardLeap = true; }, _forwardLeapWaitTime);

        GameManager._instance.CallForAction(() => { _IsDodging = false; }, _forwardLeapTime);

        PlayerStateController._instance.ChangeAnimation(GetForwardLeapAnimation());

        if (_rightAttackColliderWarningForLeap.gameObject.activeSelf)
            _rightAttackColliderWarningForLeap.gameObject.SetActive(false);
        if (_leftAttackColliderWarningForLeap.gameObject.activeSelf)
            _leftAttackColliderWarningForLeap.gameObject.SetActive(false);

        _rightAttackColliderWarningForLeap.gameObject.SetActive(true);
        GameManager._instance.CallForAction(() => { _rightAttackColliderWarningForLeap.gameObject.SetActive(false); }, _forwardLeapTime * 1f);
        _leftAttackColliderWarningForLeap.gameObject.SetActive(true);
        GameManager._instance.CallForAction(() => { _leftAttackColliderWarningForLeap.gameObject.SetActive(false); }, _forwardLeapTime * 1f);

        GameManager._instance.CallForAction(() => { _rightAttackColliderForLeap.gameObject.SetActive(true); }, _forwardLeapTime * 0.5f);
        GameManager._instance.CallForAction(() => { _rightAttackColliderForLeap.gameObject.SetActive(false); }, _forwardLeapTime * 1f);
        GameManager._instance.CallForAction(() => { _leftAttackColliderForLeap.gameObject.SetActive(true); }, _forwardLeapTime * 0.5f);
        GameManager._instance.CallForAction(() => { _leftAttackColliderForLeap.gameObject.SetActive(false); }, _forwardLeapTime * 1f);
    }
    public void ThrowKillObject(IThrowableItem item)
    {
        _isAllowedToThrow = false;
        Action OpenIsAllowedToThrow = () => {
            _isAllowedToThrow = true;
        };
        GameManager._instance.CallForAction(OpenIsAllowedToThrow, _throwWaitTime);

        PlayerStateController._instance.ChangeAnimation(GetThrowName());

        GameManager._instance.CallForAction(() => {SoundManager._instance.PlaySound(SoundManager._instance.Throw, transform.position, 0.25f, false, UnityEngine.Random.Range(0.93f, 1.07f)); UseThrowableItem(item); }, 0.15f);
    }
    public void BombDeflected()
    {
        Debug.Log("Deflected");
        //sound etc
    }
    public void Stun(float time, bool isSpeedChanges, Transform otherTransform)
    {
        if (_IsBlocking) return;

        _IsStunned = true;

        if (isSpeedChanges)
        {
            Vector3 tempSpeed = PlayerStateController._instance._rb.velocity;
            tempSpeed.y = 0f;
            Vector3 newSpeed = (transform.position - otherTransform.position).normalized * tempSpeed.magnitude;
            PlayerStateController._instance._rb.velocity = new Vector3(newSpeed.x, PlayerStateController._instance._rb.velocity.y, newSpeed.z);
        }

        GameManager._instance.CallForAction(() => { _IsStunned = false; }, time);
        PlayerStateController._instance.ChangeAnimation("Stun");
        SoundManager._instance.PlaySound(SoundManager._instance.SmallCrash, transform.position, 0.1f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        PlayerStateController._instance.EnterAnimState(new PlayerAnimations.WaitForOneAnim(0.2f));
    }
    public void HitBreakable(GameObject breakable)
    {
        PlayerStateController._instance.ChangeAnimation("HitBreakable");
        PlayerStateController._instance.EnterAnimState(new PlayerAnimations.WaitForOneAnim(0.4f));
    }

    private string GetForwardLeapAnimation()
    {
        return "ForwardLeap";
    } 
    public string GetBlockedName()
    {
        return "Blocked" + UnityEngine.Random.Range(1, 5).ToString();
    }
    public string GetDeflectedName()
    {
        if (UnityEngine.Random.Range(0, 2) == 0)
            return "DeflectedToRight";
        else
            return "DeflectedToLeft";
    }
    public string GetAttackDeflectedName()
    {
        if (_attackType != AttackType.Both)
        {
            if (_attackType == AttackType.Left)
                return "LeftAttackDeflected";
            return "RightAttackDeflected";
        }

        return "AttackDeflected" + UnityEngine.Random.Range(1, 3).ToString();
    }
    public string GetAttackNameBladeNormal()
    {
        SelectAttackType();
        if (_attackType == AttackType.Both)
        {
            int random = UnityEngine.Random.Range(1, 3);
            return "Attack" + random.ToString();
        }
        else
        {
            int random = UnityEngine.Random.Range(1, 3);

            if(_attackType == AttackType.Left)
                return "LeftAttack" + random.ToString();
            else
                return "RightAttack" + random.ToString();
        }
    }
    public string GetAttackNameBladeTeleport()
    {
        SelectAttackType();
        if (_attackType == AttackType.Both)
        {
            return "Attack3";
        }
        else
        {
            if (_attackType == AttackType.Left)
                return "LeftAttack3";
            else
                return "RightAttack3";
        }
    }
    public string GetAttackNameWeapon(bool isTeleport)
    {
        return "";
    }
    
    public string GetThrowName()
    {
        bool isLeft = false;

        if (PlayerStateController._instance._playerState is PlayerStates.OnWall)
        {
            isLeft = !(PlayerStateController._instance._playerState as PlayerStates.OnWall)._isWallOnLeftSide;
        }
        else
        {
            isLeft = UnityEngine.Random.Range(0, 2) == 0 ? true : false;
        }


        if (isLeft)
        {
            GameManager._instance.IsLeftThrowing = true;
            return "LeftThrow";
        }
        else
        {
            GameManager._instance.IsLeftThrowing = false;
            return "RightThrow";
        }
    }
    
    public void Die(Vector3 dir, float killersVelocityMagnitude, IKillObject killer)
    {
        if (GameManager._instance.isPlayerDead) return;

        if (_isImmune)
        {
            PlayerStateController._instance.ChangeAnimation("Born");
            PlayerStateController._instance.EnterAnimState(new PlayerAnimations.WaitForOneAnim(0.3f));
            SoundManager._instance.PlaySound(SoundManager._instance.Die, transform.position, 0.1f, false, UnityEngine.Random.Range(0.5f, 0.6f));
            GameObject deathVfx = Instantiate(GameManager._instance.DeathVFX, GameManager._instance.MainCamera.transform);
            deathVfx.transform.localPosition = new Vector3(0f, 0f, 0.1f);
            deathVfx.GetComponentInChildren<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.35f);
            Destroy(deathVfx, 1f);
            return;
        }

        CameraController.ShakeCamera(3.25f, 1.4f, 0.15f, 2.25f);
        GameManager._instance.BlurVolume.enabled = true;

        PlayerStateController._instance._Animator.SetTrigger("Death");
        var vfx = Instantiate(GameManager._instance.DeathVFX, GameManager._instance.MainCamera.transform);
        vfx.transform.localPosition = new Vector3(0f, 0f, 0.1f);
        SoundManager._instance.PlaySound(SoundManager._instance.Die, transform.position, 0.2f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        CameraController._instance.DeathMove();
        PlayerMovement._instance.DeathMove(dir, killersVelocityMagnitude);
        
        GameManager._instance.Die();
    }
    public void AttackWarning(Collider collider, bool isFast, Vector3 attackPosition)
    {
        //maybe some ui stuff later
    }
    public void OpenAttackCollider()
    {
        if (MeleeWeapon == null) return;

        GameObject attackCollider = MeleeWeapon.transform.Find("AttackCollider").gameObject;
        attackCollider.SetActive(true); CameraController.ShakeCamera(2.5f, 2f, 0.2f, 0.4f); 
    }
    public void CloseAttackCollider()
    {
        if (MeleeWeapon == null) return;

        GameObject attackCollider = MeleeWeapon.transform.Find("AttackCollider").gameObject;
        attackCollider.gameObject.SetActive(false); 
    }
}
