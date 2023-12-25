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
    private enum WeaponAttackType
    {
        Right,
        Left,
        Up
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
    public bool _DeflectedBuffer { get => false; set { } }

    public CapsuleCollider _collider { get; private set; }

    private AttackType _lastAttackType;
    private WeaponAttackType _lastWeaponAttackType;

    [SerializeField]
    private Transform _sparkPosition;
    [SerializeField]
    private CapsuleCollider _attackColliderWarning;
    [SerializeField]
    private CapsuleCollider _attackCollider;
    private int _interruptAttackCounter;

    private TrailRenderer[] _trails;

    private MeleeWeapon _blades;

    public GameObject MeleeWeapon;

    public List<IThrowableItem> _ThrowableInventory { get; private set; }
    public IThrowableItem _CurrentThrowableItem { get; set; }

    private Coroutine _isBlockedOrDeflectedCoroutine;
    private Coroutine _attackMoveCoroutine;
    private Coroutine _dropWeaponStartCoroutine;
    private Coroutine _takeWeaponLerpCoroutine;
    private Coroutine _resetWeaponAttackNumberCoroutine;

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
    public bool _isAllowedToThrow { get; set; }

    public float _AttackBlockedCounter;

    private float _blockStartTime;
    private float _blockTimingValue;
    private float _blockWaitTime;

    private float _AttackWaitTime { get { if (MeleeWeapon == null) return 0.5f; else return MeleeWeapon.GetComponent<MeleeWeaponForPlayer>().GetAttackTime() / 3.25f; } }
    private float _AttackTime { get { if (MeleeWeapon == null) return 0.5f; else return MeleeWeapon.GetComponent<MeleeWeaponForPlayer>().GetAttackTime(); } }

    private float _throwWaitTime;
    private float _dodgeWaitTime;
    private float _dodgeTime;

    public bool _isImmune { get; set; }

    private int _lastAttackDeflectedCounter;
    private float _lastAttackDeflectedTime;
    private float _lastDeflectedTime;

    private float _crashStunCheckValue;
    public float CollisionVelocity;

    private GameObject _illusion;
    private Vector3 _weaponScale;
    public bool _isTakeOrDropWeapon;
    private int _weaponAttackNumber;

    private RaycastHit[] _hitsForTakeWeapon;
    private void Awake()
    {
        _hitsForTakeWeapon = new RaycastHit[9];
        _instance = this;
        _collider = GetComponent<CapsuleCollider>();
        _ThrowableInventory = new List<IThrowableItem>();
        _blades = _attackCollider.GetComponent<MeleeWeapon>();
        _isAllowedToBlock = true;
        _isAllowedToDodge = true;
        _isAllowedToAttack = true;
        _isAllowedToThrow = true;
        _blockWaitTime = 0.1f;
        _throwWaitTime = 0.75f;
        _dodgeWaitTime = 0.75f;
        _dodgeTime = 0.05f;
        _blockTimingValue = 0.19f;
        _crashStunCheckValue = 11.25f;

        _trails = GameManager._instance.PlayerHands.GetComponentsInChildren<TrailRenderer>();
        CloseTrails();
    }

    private void FixedUpdate()
    {
        GameManager._instance.PlayerLastSpeed = CollisionVelocity;
        CollisionVelocity = PlayerStateController._instance._rb.velocity.magnitude;
    }
    public void CloseTrails()
    {
        foreach (var trail in _trails)
        {
            trail.enabled = false;
        }
    }
    public void OpenTrails()
    {
        foreach (var trail in _trails)
        {
            trail.enabled = true;
        }
    }
    public void StopBlockingAndDodge()
    {
        //
    }
    public void CheckMeleeAttackAgainstWall(Collider other)
    {
        if (MeleeWeapon == null) return;
        
        transform.Find("WallCheckForMeleeWeapon").gameObject.SetActive(false);
        CameraController.ShakeCamera(4f, 1.8f, 0.1f, 0.3f);
        PlayerMovement._instance._Stamina -= PlayerMovement._instance._attackDeflectedStaminaUse / 2.5f;
        PlayerStateController._instance.ChangeAnimation(GetAttackDeflectedName(), checkForNameChange: false);
        SoundManager._instance.PlaySound(SoundManager._instance.HitWallWithWeapon, transform.position + transform.forward / 4f, 0.1f, false, UnityEngine.Random.Range(0.5f, 0.55f));
        _IsAttacking = false;
        _IsAttackInterrupted = true;
        GameManager._instance.CallForAction(() => _IsAttackInterrupted = false, _AttackWaitTime * 1.25f);
        _attackColliderWarning.gameObject.SetActive(false);
        _attackCollider.gameObject.SetActive(false);

        if (MeleeWeapon.transform.Find("Trail") != null)
            MeleeWeapon.transform.Find("Trail").gameObject.SetActive(false);

        _isAllowedToAttack = false;
        GameManager._instance.CallForAction(() => _isAllowedToAttack = true, _AttackWaitTime * 1.25f);
    }
    public void AttackDeflected(IKillable deflectedKillable)
    {
        transform.Find("WallCheckForMeleeWeapon").gameObject.SetActive(false);
        CameraController.ShakeCamera(4f, 1.8f, 0.1f, 0.3f);
        PlayerMovement._instance._Stamina -= PlayerMovement._instance._attackDeflectedStaminaUse;
        PlayerStateController._instance.ChangeAnimation(GetAttackDeflectedName(), checkForNameChange: false);
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.AttackDeflecteds), transform.position, 0.25f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        _IsAttacking = false;
        _IsAttackInterrupted = true;
        PlayerMovement._instance.AttackDeflectedMove(PlayerStateController._instance._rb);
        GameManager._instance.CallForAction(() => _IsAttackInterrupted = false, _AttackWaitTime * 1.25f);
        _attackColliderWarning.gameObject.SetActive(false);
        _attackCollider.gameObject.SetActive(false);

        if (MeleeWeapon != null && MeleeWeapon.transform.Find("Trail") != null)
            MeleeWeapon.transform.Find("Trail").gameObject.SetActive(false);

        _isAllowedToAttack = false;
        GameManager._instance.CallForAction(() => _isAllowedToAttack = true, _AttackWaitTime * 1.25f);
    }

    public void AddToThrowableInventory(IThrowableItem item)
    {
        SoundManager._instance.PlaySound(SoundManager._instance.HookNotReady, transform.position, 0.1f, pitch: UnityEngine.Random.Range(0.475f, 0.525f));
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

        Action OpenIsAllowedToDodge = () =>
        {
            _isAllowedToDodge = true;
        };
        GameManager._instance.CallForAction(OpenIsAllowedToDodge, _dodgeWaitTime);

        Action CloseIsDodging = () =>
        {
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
                if (attacker == null)
                    Die(dir, 0f, null, false);
                else
                    Die(dir, attacker.Object.GetComponent<Rigidbody>().velocity.magnitude, null, false);
                return;
            }

            PlayerStateController._instance.ChangeAnimation(GetBlockedName(), checkForNameChange: false);
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Blocks), transform.position, 0.175f, false, UnityEngine.Random.Range(0.93f, 1.07f));
            CameraController.ShakeCamera(4f, 1.15f, 0.15f, 0.65f);
            PlayerMovement._instance.BlockedMove(PlayerStateController._instance._rb, -dir);
            _AttackBlockedCounter = 0.4f;
            PlayerMovement._instance._Stamina -= PlayerMovement._instance._blockedStaminaUse;

            GameManager._instance.CoroutineCall(ref _isBlockedOrDeflectedCoroutine, IsBlockedOrDeflectedCoroutine(), this);

            GameObject sparksVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.SparksVFX), _sparkPosition.position, Quaternion.identity);
            Destroy(sparksVFX, 4f);
            GameObject combatSmokeVFX = Instantiate(GameManager._instance.CombatSmokeVFX, transform.position + transform.forward * 0.1f, Quaternion.LookRotation(attacker == null ? (transform.forward) : (attacker.Object.transform.position - transform.position)));
            Destroy(combatSmokeVFX, 4f);

            //_isAllowedToBlock = false;
            Action OpenIsAllowedToBlock = () =>
            {
                _isAllowedToBlock = true;
            };
            GameManager._instance.CallForAction(OpenIsAllowedToBlock, _blockWaitTime * 2f);

            if (_isAllowedToAttack)
            {
                _isAllowedToAttack = false;
                Action OpenIsAllowedToAttack = () =>
                {
                    _isAllowedToAttack = true;
                };
                GameManager._instance.CallForAction(OpenIsAllowedToAttack, _AttackWaitTime / 1.15f);
            }

            _lastAttackDeflectedCounter = 0;
        }
        else
        {
            CameraController.ShakeCamera(3f, 1.1f, 0.2f, 0.5f);
            PlayerStateController._instance.ChangeAnimation(GetDeflectedName(), checkForNameChange: false);
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Deflects), transform.position, 0.175f, false, UnityEngine.Random.Range(0.93f, 1.07f));

            GameManager._instance.CoroutineCall(ref _isBlockedOrDeflectedCoroutine, IsBlockedOrDeflectedCoroutine(), this);

            _lastDeflectedTime = Time.time;

            GameObject sparksVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.ShiningSparksVFX), _sparkPosition.position, Quaternion.identity);
            Destroy(sparksVFX, 4f);
            GameObject combatSmokeVFX = Instantiate(GameManager._instance.CombatSmokeVFX, transform.position + transform.forward * 0.1f, Quaternion.LookRotation(attacker == null ? (transform.forward) : (attacker.Object.transform.position - transform.position)));
            Destroy(combatSmokeVFX, 4f);

            if (attacker != null && attacker.Object.CompareTag("Boss"))
            {
                if (!isRangedAttack && _lastAttackDeflectedCounter >= attacker.InterruptAttackCounterGetter && _lastAttackDeflectedTime + 5f > Time.time)
                {
                    _lastAttackDeflectedCounter = -1;
                    attacker.AttackDeflected(this as IKillable);
                }
                else
                {
                    attacker.ChangeStamina(-0.65f);
                }
            }
            else
            {
                if (attacker != null && !isRangedAttack)
                    attacker.AttackDeflected(this as IKillable);
            }

            //_isAllowedToBlock = false;
            Action OpenIsAllowedToBlock = () =>
            {
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
        if (MeleeWeapon != null || _isTakeOrDropWeapon) return;

        MeleeWeaponForPlayer meleeWeaponForPlayer = null;
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f, Camera.main.transform.forward, out _hitsForTakeWeapon[0], 4f);
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f + transform.right / 5f, Camera.main.transform.forward, out _hitsForTakeWeapon[1], 4f);
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f - transform.right / 5f, Camera.main.transform.forward, out _hitsForTakeWeapon[2], 4f);
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f + transform.up / 5f, Camera.main.transform.forward, out _hitsForTakeWeapon[3], 4f);
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f - transform.up / 5f, Camera.main.transform.forward, out _hitsForTakeWeapon[4], 4f);
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f + transform.right / 5f + transform.up / 5f, Camera.main.transform.forward, out _hitsForTakeWeapon[5], 4f);
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f - transform.right / 5f + transform.up / 5f, Camera.main.transform.forward, out _hitsForTakeWeapon[6], 4f);
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f + transform.right / 5f - transform.up / 5f, Camera.main.transform.forward, out _hitsForTakeWeapon[7], 4f);
        Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward / 4f - transform.right / 5f - transform.up / 5f, Camera.main.transform.forward, out _hitsForTakeWeapon[8], 4f);
        foreach (var hit in _hitsForTakeWeapon)
        {
            if (hit.transform != null && hit.transform.GetComponent<CrossBowFire>() != null)
            {
                hit.transform.GetComponent<CrossBowFire>().FireArrow();
            }
            else if (hit.transform != null && hit.transform.GetComponent<MeleeWeaponForPlayer>() != null)
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
        _weaponAttackNumber = 1;
        GameManager._instance.CoroutineCall(ref _resetWeaponAttackNumberCoroutine, ResetWeaponAttackNumberCoroutine(), this);

        PlayerStateController._instance.ChangeAnimation("EmptyLeft");
        PlayerStateController._instance.ChangeAnimation("EmptyRight");

        GameManager._instance.IsPlayerHasMeleeWeapon = true;
        _weaponScale = MeleeWeapon.transform.localScale;
        MeleeWeapon.GetComponentInChildren<MeshRenderer>().renderingLayerMask = 1;
        MeleeWeapon.GetComponentInChildren<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        MeleeWeapon.transform.parent = GameManager._instance.PlayerRightHandHolder;

        MeleeWeapon.transform.Find("AttackCollider").GetComponent<MeleeWeapon>().SetIgnoreCollisionColliderForTakeWeapon(_collider);

        Rigidbody childRb;

        if (MeleeWeapon.GetComponentInChildren<Collider>() != null)
            Destroy(MeleeWeapon.GetComponentInChildren<Collider>());
        if (MeleeWeapon.GetComponentInChildren<Rigidbody>() != null)
            Destroy(MeleeWeapon.GetComponentInChildren<Rigidbody>());
        if (MeleeWeapon.GetComponentInChildren<MeleeWeaponThrowed>() != null)
            Destroy(MeleeWeapon.GetComponentInChildren<MeleeWeaponThrowed>());

        if (MeleeWeapon.transform.Find("AttackCollider").GetComponent<Rigidbody>() == null)
            childRb = MeleeWeapon.transform.Find("AttackCollider").gameObject.AddComponent<Rigidbody>();
        else
            childRb = MeleeWeapon.transform.Find("AttackCollider").GetComponent<Rigidbody>();

        childRb.mass = 1f;
        childRb.freezeRotation = true;
        childRb.isKinematic = true;
        childRb.useGravity = false;

        MeleeWeapon.transform.Find("AttackCollider").gameObject.SetActive(false);
        MeleeWeapon.transform.Find("AttackColliderWarning").gameObject.SetActive(false);

        Vector3 targetPos = Vector3.zero;
        Vector3 targetAngles = Vector3.zero;
        MeleeWeapon.transform.Find("AttackCollider").GetComponent<CapsuleCollider>().radius *= 1.5f;
        MeleeWeapon.transform.Find("AttackCollider").GetComponent<CapsuleCollider>().height *= 1.5f;
        switch (meleeWeaponForPlayer.WeaponType)
        {
            case MeleeWeaponType.Sword:
                if (meleeWeaponForPlayer.name.StartsWith("Weapon"))
                {
                    MeleeWeapon.transform.localScale *= 1.25f;
                    targetPos = new Vector3(0.01005f, 0.00269f, -0.00187f);
                    targetAngles = new Vector3(274.173f, 124.703f, -22.346f);
                }
                else if (meleeWeaponForPlayer.name.StartsWith("One"))
                {
                    MeleeWeapon.transform.localScale *= 1.5f;
                    targetPos = new Vector3(0.0072f, 0.0031f, -0.00015f);
                    targetAngles = new Vector3(8.772f, 98.66f, 91.531f);
                }
                else if (meleeWeaponForPlayer.name.StartsWith("Two"))
                {
                    MeleeWeapon.transform.localScale *= 1.5f;
                    targetPos = new Vector3(0.00992f, 0.00405f, -0.00091f);
                    targetAngles = new Vector3(-0.669f, 100.972f, 100.084f);
                }
                else
                {
                    MeleeWeapon.transform.localScale *= 1.1f;
                    targetPos = new Vector3(0.00136f, 0.00403f, 0.00045f);
                    targetAngles = new Vector3(7.847f, 12.163f, 95.998f);
                }
                break;
            case MeleeWeaponType.Katana:
                MeleeWeapon.transform.localScale = new Vector3(4f, 4f, 4f);
                targetPos = new Vector3(-0.00019f, 0.00416f, 0.00119f);
                targetAngles = new Vector3(89.158f, 111.111f, 192.244f);
                break;
            case MeleeWeaponType.Mace:
                targetPos = new Vector3(0.00253f, 0.0041f, 0f);
                targetAngles = new Vector3(7.628f, 14.218f, 94.901f);
                break;
            case MeleeWeaponType.Hammer:
                MeleeWeapon.transform.localScale = new Vector3(0.03792851f, 0.02679463f, 0.0283048f);
                targetPos = new Vector3(-0.016f, 0.00325f, 0.00696f);
                targetAngles = new Vector3(72.36f, -85.358f, -108.179f);
                break;
            case MeleeWeaponType.Axe:
                MeleeWeapon.transform.localScale = new Vector3(0.05615159f, 0.04364127f, 0.05714701f);
                targetPos = new Vector3(0.00136f, 0.00403f, 0.00045f);
                targetAngles = new Vector3(31.121f, 190.416f, 264.401f);
                break;
            case MeleeWeaponType.Zweihander:
                MeleeWeapon.transform.localScale = new Vector3(0.05793774f, 0.03435662f, 0.05504524f);
                targetPos = new Vector3(-0.00059f, 0.00378f, 0.00081f);
                targetAngles = new Vector3(10.677f, 10.387f, 88.938f);
                break;
            case MeleeWeaponType.Spear:
                targetPos = new Vector3(-0.02574f, 0.00394f, 0.0109f);
                targetAngles = new Vector3(7.852f, 21.055f, -88.966f);
                break;
            default:
                MeleeWeapon.transform.localScale *= 1.1f;
                targetPos = new Vector3(0.00136f, 0.00403f, 0.00045f);
                targetAngles = new Vector3(7.847f, 12.163f, 95.998f);
                break;
        }

        GameManager._instance.CoroutineCall(ref _takeWeaponLerpCoroutine, TakeWeaponLerpCoroutine(targetPos, targetAngles), this);
        MeleeWeapon.GetComponentInChildren<MeshRenderer>().gameObject.layer = LayerMask.NameToLayer("HandMesh");
    }
    private IEnumerator TakeWeaponLerpCoroutine(Vector3 targetPos, Vector3 targetAngles)
    {
        _isTakeOrDropWeapon = true;
        PlayerStateController._instance.ChangeAnimation("TakeWeaponStart", 0.05f, checkForNameChange: false);
        PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Wait());
        SoundManager._instance.PlaySound(SoundManager._instance.TimeSlowEnter, transform.position, 0.125f, false, UnityEngine.Random.Range(1.8f, 2.2f));
        float lerpSpeed = 1f;
        Quaternion targetRot = Quaternion.Euler(targetAngles);
        float startTime = Time.time;
        while (startTime + 0.5f > Time.time)
        {
            MeleeWeapon.transform.localPosition = Vector3.Lerp(MeleeWeapon.transform.localPosition, targetPos, Time.deltaTime * lerpSpeed);
            MeleeWeapon.transform.localRotation = Quaternion.Lerp(MeleeWeapon.transform.localRotation, targetRot, Time.deltaTime * 5f);
            lerpSpeed += Time.deltaTime * 20f;
            yield return null;
        }
        MeleeWeapon.transform.localPosition = targetPos;
        _isTakeOrDropWeapon = false;

        if (MeleeWeapon.GetComponent<MeleeWeaponForPlayer>().WeaponType != MeleeWeaponType.Spear)
            PlayerStateController._instance.ChangeAnimation("TakeWeapon", checkForNameChange: false);
        else
            PlayerStateController._instance.ChangeAnimation("Idle");

        PlayerStateController._instance.EnterAnimState(new PlayerAnimations.WaitForOneAnim(0.4f));
        //SoundManager._instance.PlaySound(SoundManager._instance.TimeSlowExit, transform.position, 0.125f, false, UnityEngine.Random.Range(1.8f, 2.2f));
        lerpSpeed = 20f;
        startTime = Time.time;
        while (startTime + 0.5f > Time.time)
        {
            MeleeWeapon.transform.localRotation = Quaternion.Lerp(MeleeWeapon.transform.localRotation, targetRot, Time.deltaTime * lerpSpeed);
            yield return null;
        }
        MeleeWeapon.transform.localEulerAngles = targetAngles;
    }
    public void DropWeaponStart(bool isSpear = false)
    {
        if (_isTakeOrDropWeapon) return;

        if (_takeWeaponLerpCoroutine != null)
            StopCoroutine(_takeWeaponLerpCoroutine);
        if (_dropWeaponStartCoroutine != null)
            StopCoroutine(_dropWeaponStartCoroutine);
        _dropWeaponStartCoroutine = StartCoroutine(DropWeaponStartCoroutine(isSpear));
    }
    private IEnumerator DropWeaponStartCoroutine(bool isSpear)
    {
        _isTakeOrDropWeapon = true;

        if(!isSpear)
            PlayerStateController._instance.ChangeAnimation("ThrowWeaponStart", checkForNameChange: false);
        else
            PlayerStateController._instance.ChangeAnimation("ThrowSpearStart", checkForNameChange: false);

        PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Wait());
        //SoundManager._instance.PlaySound(SoundManager._instance.TimeSlowEnter, transform.position, 0.125f, false, UnityEngine.Random.Range(1.8f, 2.2f));

        float startTime = Time.time;
        while (InputHandler.GetButton("WeaponChange"))
        {
            PlayerMovement._instance._Stamina -= Time.deltaTime * 28f;
            yield return null;
        }
        SoundManager._instance.PlaySound(SoundManager._instance.TimeSlowExit, transform.position, 0.125f, false, UnityEngine.Random.Range(1.8f, 2.2f));
        StartCoroutine(DropWeapon(Time.time - startTime + 1f, isSpear));
    }
    private IEnumerator DropWeapon(float power, bool isSpear)
    {
        if (!IsDead)
        {
            power = Mathf.Clamp(power, 1f, 3f);
            if (!isSpear)
                PlayerStateController._instance.ChangeAnimation("ThrowWeapon", checkForNameChange: false);
            else
                PlayerStateController._instance.ChangeAnimation("ThrowSpear", checkForNameChange: false);
            PlayerStateController._instance.EnterAnimState(new PlayerAnimations.WaitForOneAnim(0.25f));
        }
        yield return new WaitForSeconds(0.08f);

        MeleeWeapon.transform.parent = null;
        //MeleeWeapon.GetComponentInChildren<MeshRenderer>().renderingLayerMask = 257;
        MeleeWeapon.GetComponentInChildren<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        if (MeleeWeapon.transform.Find("AttackCollider") != null)
            if (power > 2f)
                MeleeWeapon.transform.Find("AttackCollider").gameObject.SetActive(true);
            else
                MeleeWeapon.transform.Find("AttackCollider").gameObject.SetActive(false);


        if (MeleeWeapon.GetComponent<PlaySoundOnCollision>() == null)
        {
            MeleeWeapon.AddComponent<PlaySoundOnCollision>();
            MeleeWeapon.GetComponent<PlaySoundOnCollision>()._isEnabled = true;
            MeleeWeapon.GetComponent<PlaySoundOnCollision>().pitch = 0.6f;
            MeleeWeapon.GetComponent<PlaySoundOnCollision>()._soundClip = SoundManager._instance.Blocks[0];
        }

        if (MeleeWeapon.GetComponentInChildren<MeshRenderer>() != null)
            MeleeWeapon.GetComponentInChildren<MeshRenderer>().gameObject.AddComponent(typeof(MeshCollider)).GetComponent<MeshCollider>().convex = true;
        else
            MeleeWeapon.AddComponent(typeof(BoxCollider));

        if (MeleeWeapon.transform.Find("AttackCollider").GetComponent<Rigidbody>() != null)
            Destroy(MeleeWeapon.transform.Find("AttackCollider").GetComponent<Rigidbody>());

        Rigidbody rb = (Rigidbody)MeleeWeapon.AddComponent(typeof(Rigidbody));
        MeleeWeaponThrowed weaponInAir = (MeleeWeaponThrowed)MeleeWeapon.AddComponent(typeof(MeleeWeaponThrowed));
        weaponInAir.IgnoreCollisionCollider = _collider;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        MeleeWeapon.transform.Find("AttackCollider").GetComponent<CapsuleCollider>().radius /= 1.5f;
        MeleeWeapon.transform.Find("AttackCollider").GetComponent<CapsuleCollider>().height /= 1.5f;
        switch (MeleeWeapon.GetComponent<MeleeWeaponForPlayer>().WeaponType)
        {
            case MeleeWeaponType.Sword:
                power *= 0.8f;
                rb.mass = 2f;
                break;
            case MeleeWeaponType.Katana:
                power *= 0.9f;
                rb.mass = 2f;
                break;
            case MeleeWeaponType.Mace:
                power *= 0.8f;
                rb.mass = 2f;
                break;
            case MeleeWeaponType.Hammer:
                power *= 0.7f;
                rb.mass = 3.5f;
                break;
            case MeleeWeaponType.Axe:
                power *= 0.6f;
                rb.mass = 3f;
                break;
            case MeleeWeaponType.Zweihander:
                power *= 0.6f;
                rb.mass = 3.5f;
                break;
            case MeleeWeaponType.Spear:
                power *= 0.85f;
                rb.mass = 1.5f;
                break;
            default:
                break;
        }

        if (isSpear)
            rb.velocity = rb.transform.up * 15f * power;
        else
            rb.velocity = Camera.main.transform.forward * 15f * power + Vector3.up * 1f * power;
        
        if (MeleeWeapon.GetComponent<MeleeWeaponForPlayer>().WeaponType == MeleeWeaponType.Katana)
            rb.angularVelocity = power * MeleeWeapon.transform.right * -7f;
        else if (MeleeWeapon.name.StartsWith("One") || MeleeWeapon.name.StartsWith("Two"))
            rb.angularVelocity = power * MeleeWeapon.transform.up * -7f;
        else if (!isSpear)
            rb.angularVelocity = power * MeleeWeapon.transform.forward * -7f;

        if(isSpear)
            if (Steamworks.SteamClient.IsValid && !new Steamworks.Data.Achievement("SpearThrowAchievement").State)
                new Steamworks.Data.Achievement("SpearThrowAchievement").Trigger();

        MeleeWeapon.GetComponentInChildren<MeshRenderer>().gameObject.layer = LayerMask.NameToLayer("Default");
        MeleeWeapon.transform.localScale = _weaponScale;

        GameManager._instance.IsPlayerHasMeleeWeapon = false;
        MeleeWeapon = null;
        GameManager._instance.CallForAction(() => _isTakeOrDropWeapon = false, 0.75f);

    }
    #region Attacks
    public void DashAttack()
    {
        if (!_isAllowedToAttack) return;

        if (MeleeWeapon != null)
        {
            GameManager._instance.CoroutineCall(ref _attackMoveCoroutine, AttackTeleportCoroutine(true), this);
        }
        else
        {
            GameManager._instance.CoroutineCall(ref _attackMoveCoroutine, AttackTeleportCoroutine(false), this);
        }
    }
    public void Attack()
    {
        if (!_isAllowedToAttack) return;

        if (MeleeWeapon != null)
        {
            GameManager._instance.CoroutineCall(ref _attackMoveCoroutine, AttackWithWeaponNormal(), this);
        }
        else
        {
            GameManager._instance.CoroutineCall(ref _attackMoveCoroutine, AttackWithBladesNormal(), this);
        }
    }
    private IEnumerator AttackWithBladesNormal()
    {
        PlayerMovement._instance._Stamina -= PlayerMovement._instance._attackStaminaUse;
        _isAllowedToAttack = false;
        _attackColliderWarning.GetComponent<AttackWarning>()._isRunningForPlayer = PlayerStateController.IsRunning();
        OpenTrails();
        PlayerCombat._instance._IsAttacking = true;
        GameManager._instance.PlayerAttackHandle();

        PlayerMovement._instance.AttackMoveNormal(PlayerStateController._instance._rb);

        GameObject aimAssistedEnemy = GameManager._instance.CheckForAimAssist();
        if (aimAssistedEnemy != null && PlayerMovement._instance.IsGrounded() && PlayerStateController._instance._rb.velocity.magnitude > PlayerMovement._instance._MoveSpeed)
        {
            PlayerMovement._instance.AimAssistToEnemy(aimAssistedEnemy);
        }

        yield return new WaitWhile(() => PlayerMovement._instance._isOnAttackOrAttackDeflectedMove);

        string attackName = GetAttackNameBlade();

        //CameraController._instance.GetComponentInChildren<Animator>().CrossFade("Camera" + attackName, 0.15f);



        float localAttackWaitTime = _AttackWaitTime;
        if (_lastDeflectedTime + 1.75f > Time.time || _isImmune)
            localAttackWaitTime /= 2f;
        GameManager._instance.CallForAction(() => { if (_IsAttackInterrupted) return; _isAllowedToAttack = true; }, localAttackWaitTime);

        PlayerStateController._instance.ChangeAnimation(attackName, checkForNameChange: false);
        GameManager._instance.CallForAction(() => SoundManager._instance.PlaySound(SoundManager._instance.GetAttackSoundForPlayer(PlayerCombat._instance.MeleeWeapon), transform.position, 0.22f, false, UnityEngine.Random.Range(0.8f, 0.85f)), 0.125f);


        if (_attackColliderWarning.gameObject.activeSelf)
            _attackColliderWarning.gameObject.SetActive(false);
        _attackColliderWarning.gameObject.SetActive(true);

        GameManager._instance.CallForAction(() => { if (IsDead) return; PlayerCombat._instance._IsAttacking = false; _attackColliderWarning.gameObject.SetActive(false); CloseTrails(); }, _AttackTime * 0.7f);
    }
    private IEnumerator AttackWithWeaponNormal()
    {
        PlayerMovement._instance._Stamina -= PlayerMovement._instance._attackStaminaUse;
        _isAllowedToAttack = false;
        PlayerCombat._instance._IsAttacking = true;
        GameManager._instance.PlayerAttackHandle();

        PlayerMovement._instance.AttackMoveNormal(PlayerStateController._instance._rb);

        yield return new WaitWhile(() => PlayerMovement._instance._isOnAttackOrAttackDeflectedMove);

        string attackName = GetAttackNameWeapon(false);
        _weaponAttackNumber += 1;
        GameManager._instance.CoroutineCall(ref _resetWeaponAttackNumberCoroutine, ResetWeaponAttackNumberCoroutine(), this);
        //CameraController._instance.GetComponentInChildren<Animator>().CrossFade("Camera" + attackName, 0.15f);

        if (MeleeWeapon != null && MeleeWeapon.transform.Find("Trail") != null)
            MeleeWeapon.transform.Find("Trail").gameObject.SetActive(true);

        float localAttackWaitTime = _AttackWaitTime;
        if (_lastDeflectedTime + 1.75f > Time.time || _isImmune)
            localAttackWaitTime /= 2f;
        GameManager._instance.CallForAction(() => { if (_IsAttackInterrupted) return; _isAllowedToAttack = true; }, localAttackWaitTime);

        PlayerStateController._instance.ChangeAnimation(attackName, checkForNameChange: false);

        float attackSoundWaitTime = attackName.StartsWith("ZweiAxe") ? 0.55f : 0.225f;
        GameManager._instance.CallForAction(() => SoundManager._instance.PlaySound(SoundManager._instance.GetAttackSoundForPlayer(PlayerCombat._instance.MeleeWeapon), transform.position, 0.22f, false, UnityEngine.Random.Range(0.8f, 0.85f)), attackSoundWaitTime);

        if (_attackColliderWarning.gameObject.activeSelf)
            _attackColliderWarning.gameObject.SetActive(false);
        _attackColliderWarning.gameObject.SetActive(true);

        GameManager._instance.CallForAction(() =>
        {
            if (IsDead) return; PlayerCombat._instance._IsAttacking = false; _attackColliderWarning.gameObject.SetActive(false);
            transform.Find("WallCheckForMeleeWeapon").gameObject.SetActive(false);
            if (MeleeWeapon != null && MeleeWeapon.transform.Find("Trail") != null)
                MeleeWeapon.transform.Find("Trail").gameObject.SetActive(false);
        }, _AttackTime * 0.9f);
    }
    private IEnumerator AttackTeleportCoroutine(bool isWeapon)
    {
        if (_illusion != null) Destroy(_illusion);

        _isAllowedToAttack = false;
        PlayerCombat._instance._IsAttacking = true;

        if (isWeapon)
        {
            string name = MeleeWeapon.GetComponent<MeleeWeaponForPlayer>().WeaponType == MeleeWeaponType.Katana ? "KatanaTeleport" : "SwordTeleport";
            PlayerStateController._instance.ChangeAnimation(name, checkForNameChange: false);
            PlayerStateController._instance.EnterAnimState(new PlayerAnimations.Wait());
        }

        GameManager._instance.MidScreenDot.gameObject.SetActive(false);
        GameManager._instance.SlowTime();
        PlayerMovement._instance._Stamina -= PlayerMovement._instance._dashAttackStaminaUse;
        float teleportDistance = 0f;
        _illusion = GameObject.Instantiate(GameManager._instance.TeleportIllusion, transform.position, Quaternion.identity);
        float rayDistance = 4f;
        while (InputHandler.GetButton("MiddleMouse"))
        {
            rayDistance += Time.unscaledDeltaTime * 4f;
            rayDistance = Mathf.Clamp(rayDistance, 2.5f, 16f);
            Physics.Raycast(transform.position, Camera.main.transform.forward, out RaycastHit hit, rayDistance);
            if (hit.collider != null)
                teleportDistance = (hit.point - transform.position).magnitude;
            else
                teleportDistance = rayDistance;
            _illusion.transform.position = transform.position + Camera.main.transform.forward * teleportDistance;
            _illusion.transform.LookAt(Camera.main.transform);
            yield return null;
        }
        GameManager._instance.MidScreenDot.gameObject.SetActive(true);
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
        
        string attackName = GetAttackNameBlade();

        GameManager._instance.PlayerAttackHandle();

        float localAttackWaitTime = _AttackWaitTime;
        if (_lastDeflectedTime + 1.75f > Time.time || _isImmune)
            localAttackWaitTime /= 2f;
        GameManager._instance.CallForAction(() => { if (_IsAttackInterrupted) return; _isAllowedToAttack = true; }, localAttackWaitTime);

        PlayerStateController._instance.ChangeAnimation(attackName, checkForNameChange: false);
        GameManager._instance.CallForAction(() => SoundManager._instance.PlaySound(SoundManager._instance.GetAttackSoundForPlayer(PlayerCombat._instance.MeleeWeapon), transform.position, 0.22f, false, UnityEngine.Random.Range(0.8f, 0.85f)), 0.125f);
        SoundManager._instance.PlaySound(SoundManager._instance.BladeSlide, transform.position, 0.12f, false, UnityEngine.Random.Range(0.65f, 0.75f));


        if (_attackColliderWarning.gameObject.activeSelf)
            _attackColliderWarning.gameObject.SetActive(false);
        _attackColliderWarning.gameObject.SetActive(true);
        
        GameManager._instance.CallForAction(() => { if (IsDead) return; PlayerCombat._instance._IsAttacking = false; _attackColliderWarning.gameObject.SetActive(false); }, _AttackTime * 0.9f);

        GameManager._instance.CallForAction(() => { if (_IsAttackInterrupted) return; PlayerCombat._instance._IsAttacking = false; _isAllowedToAttack = true; }, _AttackTime * 0.7f);
    }
    private void AttackWithWeaponTeleport(float teleportDistance)
    {
        PlayerMovement._instance.AttackMoveTeleport(PlayerStateController._instance._rb, teleportDistance);

        string attackName = GetAttackNameWeapon(true);
        _weaponAttackNumber += 1;
        GameManager._instance.PlayerAttackHandle();

        if (MeleeWeapon != null && MeleeWeapon.transform.Find("Trail") != null)
            MeleeWeapon.transform.Find("Trail").gameObject.SetActive(true);

        float localAttackWaitTime = _AttackWaitTime;
        if (_lastDeflectedTime + 1.75f > Time.time || _isImmune)
            localAttackWaitTime /= 2f;
        GameManager._instance.CallForAction(() => { if (_IsAttackInterrupted) return; _isAllowedToAttack = true; }, localAttackWaitTime);

        PlayerStateController._instance.ChangeAnimation(attackName, checkForNameChange: false);
        PlayerStateController._instance.EnterAnimState(new PlayerAnimations.WaitForOneAnim(0.75f));

        float attackSoundWaitTime = attackName.StartsWith("ZweiAxe") ? 0.55f : 0.2f;
        GameManager._instance.CallForAction(() => SoundManager._instance.PlaySound(SoundManager._instance.GetAttackSoundForPlayer(PlayerCombat._instance.MeleeWeapon), transform.position, 0.22f, false, UnityEngine.Random.Range(0.8f, 0.85f)), attackSoundWaitTime);

        if (_attackColliderWarning.gameObject.activeSelf)
            _attackColliderWarning.gameObject.SetActive(false);
        _attackColliderWarning.gameObject.SetActive(true);

        GameManager._instance.CallForAction(() => { if (IsDead) return; PlayerCombat._instance._IsAttacking = false; _attackColliderWarning.gameObject.SetActive(false);
            transform.Find("WallCheckForMeleeWeapon").gameObject.SetActive(false);
            if (MeleeWeapon != null && MeleeWeapon.transform.Find("Trail") != null)
                MeleeWeapon.transform.Find("Trail").gameObject.SetActive(false);
        }, _AttackTime * 0.9f);
    }


    #endregion
    private void SelectAttackType()
    {
        if (PlayerStateController._instance._playerState is PlayerStates.OnWall)
        {
            bool isLeft = !(PlayerStateController._instance._playerState as PlayerStates.OnWall)._isWallOnLeftSide;
            if (isLeft)
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
        else if (PlayerStateController._instance._rb.velocity.magnitude < 12f)
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
        else
        {
            do
            {
                int random = UnityEngine.Random.Range(0, 6);
                if (random == 0 || random == 1 || random == 2 || random == 3)
                    _attackType = AttackType.Both;
                else if (random == 4)
                    _attackType = AttackType.Left;
                else if (random == 5)
                    _attackType = AttackType.Right;
            } while (_attackType == _lastAttackType);
        }

        if (_attackType != AttackType.Both)
        {
            if (!PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(4).IsName("EmptyCombat"))
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

    public void ThrowKillObject(IThrowableItem item)
    {
        _isAllowedToThrow = false;
        Action OpenIsAllowedToThrow = () =>
        {
            _isAllowedToThrow = true;
        };
        GameManager._instance.CallForAction(OpenIsAllowedToThrow, _throwWaitTime);

        PlayerStateController._instance.ChangeAnimation(GetThrowName());

        GameManager._instance.CallForAction(() => { SoundManager._instance.PlaySound(SoundManager._instance.Throw, transform.position, 0.25f, false, UnityEngine.Random.Range(0.93f, 1.07f)); UseThrowableItem(item); }, 0.15f);
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
    public string GetBlockedName()
    {
        if (MeleeWeapon == null)
            return "Blocked" + UnityEngine.Random.Range(1, 5).ToString();
        return "WeaponBlocked" + UnityEngine.Random.Range(1, 3).ToString();
    }
    public string GetDeflectedName()
    {
        if (MeleeWeapon == null)
        {
            if (UnityEngine.Random.Range(0, 2) == 0)
                return "DeflectedToRight";
            else
                return "DeflectedToLeft";
        }
        else
        {
            if (UnityEngine.Random.Range(0, 2) == 0)
                return "DeflectedToRightWeapon";
            else
                return "DeflectedToLeftWeapon";
        }

    }
    public string GetAttackDeflectedName()
    {
        if (MeleeWeapon == null)
        {
            if (_attackType != AttackType.Both)
            {
                if (_attackType == AttackType.Left)
                    return "LeftAttackDeflected";
                return "RightAttackDeflected";
            }
            return "AttackDeflected" + UnityEngine.Random.Range(1, 3).ToString();
        }
        else
        {
            if (_lastWeaponAttackType == WeaponAttackType.Right || _lastWeaponAttackType == WeaponAttackType.Up)
                return "RightAttackDeflectedWeapon";
            else
                return "LeftAttackDeflectedWeapon";
        }
    }
    public string GetAttackNameBlade()
    {
        SelectAttackType();
        if (_attackType == AttackType.Both)
        {
            SetLastBladeAttackType(WeaponAttackType.Up, GetVectorFromWeaponType(WeaponAttackType.Up));

            int random = UnityEngine.Random.Range(1, 4);
            return "Attack" + random.ToString();
        }
        else
        {
            int random = UnityEngine.Random.Range(1, 3);

            if (_attackType == AttackType.Left)
            {
                SetLastBladeAttackType(WeaponAttackType.Right, GetVectorFromWeaponType(WeaponAttackType.Right));

                return "LeftAttack" + random.ToString();
            }
            else
            {
                SetLastBladeAttackType(WeaponAttackType.Left, GetVectorFromWeaponType(WeaponAttackType.Left));

                return "RightAttack" + random.ToString();
            }
        }
    }
    public string GetAttackNameWeapon(bool isTeleport)
    {
        if (MeleeWeapon == null) return "";

        if (isTeleport)
        {
            switch (MeleeWeapon.GetComponent<MeleeWeaponForPlayer>().WeaponType)
            {
                case MeleeWeaponType.Mace:
                case MeleeWeaponType.Hammer:
                case MeleeWeaponType.Sword:
                    SetLastWeaponAttackType(WeaponAttackType.Right, GetVectorFromWeaponType(WeaponAttackType.Right));
                    return "SwordTeleportAttack";
                case MeleeWeaponType.Katana:
                    SetLastWeaponAttackType(WeaponAttackType.Left, GetVectorFromWeaponType(WeaponAttackType.Left));
                    return "KatanaTeleportAttack";
                case MeleeWeaponType.Axe:
                case MeleeWeaponType.Zweihander:
                    SetLastWeaponAttackType(WeaponAttackType.Right, GetVectorFromWeaponType(WeaponAttackType.Right));
                    return "SwordTeleportAttack";
                case MeleeWeaponType.Spear:
                default:
                    return "";
            }
        }

        if (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(4).IsName("DeflectedToRightWeapon") || PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(4).IsName("DeflectedToLeftWeapon"))
        {
            string attackTypeToNumber;
            if (PlayerStateController._instance._Animator.GetCurrentAnimatorStateInfo(4).IsName("DeflectedToRightWeapon"))
            {
                SetLastWeaponAttackType(WeaponAttackType.Right, GetVectorFromWeaponType(WeaponAttackType.Right));
                attackTypeToNumber = "1";
            }
            else
            {
                SetLastWeaponAttackType(WeaponAttackType.Left, GetVectorFromWeaponType(WeaponAttackType.Left));
                attackTypeToNumber = "2";
            }

            switch (MeleeWeapon.GetComponent<MeleeWeaponForPlayer>().WeaponType)
            {
                case MeleeWeaponType.Mace:
                case MeleeWeaponType.Hammer:
                case MeleeWeaponType.Sword:
                    return "SwordMaceHammer" + attackTypeToNumber;
                    case MeleeWeaponType.Katana:
                    return "Katana" + attackTypeToNumber;
                case MeleeWeaponType.Axe:
                case MeleeWeaponType.Zweihander:
                    return "ZweiAxe" + attackTypeToNumber;
                case MeleeWeaponType.Spear:
                default:
                    Debug.LogError("Weapon Type Not Found..");
                    return "";
            }
        }

        _weaponAttackNumber = _weaponAttackNumber >= 4 ? 1 : _weaponAttackNumber;
        bool isSecondaryRightAttack = false;
        if (_weaponAttackNumber == 1)
        {
            if (UnityEngine.Random.Range(0, 100) < 25)
                _weaponAttackNumber = 2;
            else if (UnityEngine.Random.Range(0, 100) < 33)
                isSecondaryRightAttack = true;
        }

        SetLastWeaponAttackType((WeaponAttackType)_weaponAttackNumber, GetVectorFromWeaponType((WeaponAttackType)_weaponAttackNumber));

        switch (MeleeWeapon.GetComponent<MeleeWeaponForPlayer>().WeaponType)
        {
            case MeleeWeaponType.Mace:
            case MeleeWeaponType.Hammer:
            case MeleeWeaponType.Sword:
                if (isSecondaryRightAttack)
                    return "SwordMaceHammer4";
                return "SwordMaceHammer" + _weaponAttackNumber;
            case MeleeWeaponType.Katana:
                if (isSecondaryRightAttack)
                    return "Katana4";
                return "Katana" + _weaponAttackNumber;
            case MeleeWeaponType.Axe:
            case MeleeWeaponType.Zweihander:
                if (isSecondaryRightAttack)
                    return "ZweiAxe4";
                return "ZweiAxe" + _weaponAttackNumber;
            case MeleeWeaponType.Spear:
            default:
                Debug.LogError("Weapon Type Not Found..");
                return "";
        }
    }
    private Vector3 GetVectorFromWeaponType(WeaponAttackType type)
    {
        switch (type)
        {
            case WeaponAttackType.Right:
                return transform.right;
            case WeaponAttackType.Left:
                return -transform.right;
            case WeaponAttackType.Up:
                return transform.up;
        }
        return transform.right;
    }
    private void SetLastWeaponAttackType(WeaponAttackType type, Vector3 weaponVelocity)
    {
        _lastWeaponAttackType = type;
        MeleeWeapon.transform.Find("AttackCollider").GetComponent<MeleeWeapon>().WeaponVelocityForPlayer = weaponVelocity;
    }
    private void SetLastBladeAttackType(WeaponAttackType type, Vector3 weaponVelocity)
    {
        _lastWeaponAttackType = type;
        AttackCollider.GetComponent<MeleeWeapon>().WeaponVelocityForPlayer = weaponVelocity;
    }
    private IEnumerator ResetWeaponAttackNumberCoroutine()
    {
        yield return new WaitForSeconds(1.75f);
        _weaponAttackNumber = 1;
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

    public void Die(Vector3 dir, float killersVelocityMagnitude, IKillObject killer, bool isHardHit)
    {
        if (GameManager._instance.isPlayerDead || GameManager._instance.isOnCutscene || GameManager._instance.isGameStopped) return;

        if (killer != null && killer is Projectile)
            SoundManager._instance.PlaySound(SoundManager._instance.DeathByProjectile, transform.position, 0.9f, false, UnityEngine.Random.Range(0.65f, 0.75f));

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

        foreach (var script in GetComponents<MonoBehaviour>())
        {
            script.StopAllCoroutines();
        }

        GameManager._instance.Die();

        if (MeleeWeapon != null)
            StartCoroutine(DropWeapon(0.02f, false));
        CameraController.ShakeCamera(3.25f, 1.4f, 0.15f, 2.25f);
        GameManager._instance.BlurVolume.enabled = true;

        PlayerStateController._instance._Animator.SetTrigger("Death");
        var vfx = Instantiate(GameManager._instance.DeathVFX, GameManager._instance.MainCamera.transform);
        vfx.transform.localPosition = new Vector3(0f, 0f, 0.1f);
        SoundManager._instance.PlaySound(SoundManager._instance.Die, transform.position, 0.2f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        CameraController._instance.DeathMove();
        PlayerMovement._instance.DeathMove(dir, killersVelocityMagnitude);

    }
    public void AttackWarning(Collider collider, bool isFast, Vector3 attackPosition)
    {
        //maybe some ui stuff later
    }
    public void OpenAttackCollider()
    {
        if (IsDead || _IsAttackInterrupted) return;

        if (MeleeWeapon == null)
        {
            GameObject attackCollider = _attackCollider.gameObject;
            attackCollider.SetActive(true);
            CameraController.ShakeCamera(3.5f, 2.5f, 0.2f, 0.4f);
        }
        else
        {
            GameObject attackCollider = MeleeWeapon.transform.Find("AttackCollider").gameObject;
            attackCollider.SetActive(true); CameraController.ShakeCamera(4f, 3f, 0.2f, 0.5f);
            transform.Find("WallCheckForMeleeWeapon").gameObject.SetActive(true);
        }
    }
    public void CloseAttackCollider()
    {
        if (IsDead || _IsAttackInterrupted) return;

        if (PlayerStateController._instance._Animator.IsInTransition(4) && PlayerStateController._instance._Animator.GetNextAnimatorStateInfo(4).IsName("EmptyCombat")) return;

        if (MeleeWeapon == null)
        {
            GameObject attackCollider = _attackCollider.gameObject;
            attackCollider.gameObject.SetActive(false);
        }
        else
        {
            GameObject attackCollider = MeleeWeapon.transform.Find("AttackCollider").gameObject;
            attackCollider.gameObject.SetActive(false);
        }
    }
    public void MeleeAttackFinished()
    {

    }
}
