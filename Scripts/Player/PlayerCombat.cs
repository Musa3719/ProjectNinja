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
    public bool IsDodgingGetter => _IsDodgingOrForwardLeap;
    public GameObject AttackCollider => _attackCollider.gameObject;
    public int InterruptAttackCounterGetter => _interruptAttackCounter;
    public float _CollisionVelocity => _collisionVelocity;
    public bool _DeflectedBuffer { get => false; set {  } }

    public CapsuleCollider _collider { get; private set; }

    private float _lastAttackTime;
    private int _lastAttackNumber;
    
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

    private MeleeWeapon _meleeWeapon;

    public List<IThrowableItem> _ThrowableInventory { get; private set; }
    public IThrowableItem _CurrentThrowableItem { get; set; }

    private Coroutine _isBlockedOrDeflectedCoroutine;

    public bool _IsStunned { get; private set; }

    private bool _isBlocking;
    public bool _IsDodgingOrForwardLeap { get; set; }

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

    private int _lastAttackDeflectedCounter;
    private float _lastAttackDeflectedTime;

    public float _AttackTime => _attackTime;
    public float _forwardLeapTime { get; private set; }

    private float _crashStunCheckValue;
    private float _collisionVelocity;

    private void Awake()
    {
        _instance = this;
        _collider = GetComponent<CapsuleCollider>();
        _ThrowableInventory = new List<IThrowableItem>();
        _meleeWeapon = _attackCollider.GetComponent<MeleeWeapon>();
        _isAllowedToBlock = true;
        _isAllowedToDodge = true;
        _isAllowedToAttack = true;
        _isAllowedToForwardLeap = true;
        _isAllowedToThrow = true;
        _blockWaitTime = 0.1f;
        _attackWaitTime = 0.6f;
        _attackTime = 0.75f;
        _forwardLeapWaitTime = 3f;
        _throwWaitTime = 0.8f;
        _dodgeWaitTime = 0.75f;
        _dodgeTime = 0.1f;
        _forwardLeapTime = 0.2f;
        _blockTimingValue = 0.3f;
        _crashStunCheckValue = 10f;
    }
    
    private void FixedUpdate()
    {
        _collisionVelocity = PlayerStateController._instance._rb.velocity.magnitude;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider != null && collision.collider.GetComponent<IKillable>() != null)
        {
            if (_collisionVelocity > _crashStunCheckValue || collision.collider.GetComponent<IKillable>()._CollisionVelocity > _crashStunCheckValue)
            {
                Stun(0.35f, true, collision.collider.transform);
            }
        }

    }
    public void StopBlockingAndDodge()
    {
        //
    }
    public void AttackDeflected(IKillable deflectedKillable)
    {
        CameraController.ShakeCamera(2.2f, 1.4f, 0.1f, 0.3f);
        PlayerMovement._instance._Stamina -= PlayerMovement._instance._attackDeflectedStaminaUse;
        PlayerStateController._instance.ChangeAnimation(GetAttackDeflectedName());
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.AttackDeflecteds), transform.position, 0.5f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        _IsAttacking = false;
        _IsAttackInterrupted = true;
        PlayerMovement._instance.AttackDeflectedMove(PlayerStateController._instance._rb);
        GameManager._instance.CallForAction(() => _IsAttackInterrupted = false, _attackWaitTime * 2f);
        _attackColliderWarning.gameObject.SetActive(false);
        _attackCollider.gameObject.SetActive(false);

        _isAllowedToAttack = false;
        GameManager._instance.CallForAction(() => _isAllowedToAttack = true, _attackWaitTime * 2f);
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
        _IsDodgingOrForwardLeap = true;

        PlayerStateController._instance.ChangeAnimation("Dodge");
        SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.ArmorWalkSounds), transform.position, 0.18f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        PlayerStateController._instance.EnterAnimState(new PlayerAnimations.WaitForOneAnim(0.3f));
        CameraController.ShakeCamera(3f, 1.6f, 0.1f, 0.7f);

        Action OpenIsAllowedToDodge = () => {
            _isAllowedToDodge = true;
        };
        GameManager._instance.CallForAction(OpenIsAllowedToDodge, _dodgeWaitTime);

        Action CloseIsDodging = () => {
            _IsDodgingOrForwardLeap = false;
        };
        GameManager._instance.CallForAction(CloseIsDodging, _dodgeTime);
    }
    public void DeflectWithBlock(Vector3 dir, IKillable attacker, bool isRangedAttack)
    {
        _IsBlocking = false;


        if (Time.time - _blockStartTime > _blockTimingValue)
        {
            if (PlayerMovement._instance._Stamina < PlayerMovement._instance._blockedStaminaUse) return;

            Vector3 VFXposition = _meleeWeapon.transform.position - transform.forward * 1.5f;
            PlayerStateController._instance.ChangeAnimation(GetBlockedName());
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Blocks), transform.position, 0.4f, false, UnityEngine.Random.Range(0.93f, 1.07f));
            CameraController.ShakeCamera(4f, 1.15f, 0.15f, 0.65f);
            PlayerMovement._instance.BlockedMove(PlayerStateController._instance._rb, -dir);
            _AttackBlockedCounter = 0.4f;
            PlayerMovement._instance._Stamina -= PlayerMovement._instance._blockedStaminaUse;

            if (_isBlockedOrDeflectedCoroutine != null)
                StopCoroutine(_isBlockedOrDeflectedCoroutine);
            _isBlockedOrDeflectedCoroutine = StartCoroutine(IsBlockedOrDeflectedCoroutine());

            GameObject sparksVFX = Instantiate(GameManager._instance.GetRandomFromList(GameManager._instance.SparksVFX), VFXposition, Quaternion.identity);
            Destroy(sparksVFX, 4f);

            Debug.Log("Blocked with bad timing...");
            _isAllowedToBlock = false;
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
                GameManager._instance.CallForAction(OpenIsAllowedToAttack, _attackWaitTime * 1.5f);
            }

            _lastAttackDeflectedCounter = 0;
        }
        else
        {
            Vector3 VFXposition = _meleeWeapon.transform.position + transform.forward * 0.5f;
            CameraController.ShakeCamera(3f, 1.1f, 0.2f, 0.5f);
            PlayerStateController._instance.ChangeAnimation(GetDeflectedName());
            SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Deflects), transform.position, 0.3f, false, UnityEngine.Random.Range(0.93f, 1.07f));

            if (_isBlockedOrDeflectedCoroutine != null)
                StopCoroutine(_isBlockedOrDeflectedCoroutine);
            _isBlockedOrDeflectedCoroutine = StartCoroutine(IsBlockedOrDeflectedCoroutine());

            GameObject sparksVFX = Instantiate(GameManager._instance.ShiningSparksVFX[1], VFXposition, Quaternion.identity);
            Destroy(sparksVFX, 4f);

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

            _isAllowedToBlock = false;
            Action OpenIsAllowedToBlock = () => {
                _isAllowedToBlock = true;
            };
            GameManager._instance.CallForAction(OpenIsAllowedToBlock, _blockWaitTime);

            if (_isAllowedToAttack)
            {
                _isAllowedToAttack = false;
                Action OpenIsAllowedToAttack = () => {
                    _isAllowedToAttack = true;
                };
                GameManager._instance.CallForAction(OpenIsAllowedToAttack, _attackWaitTime / 2f);
            }

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

    public void Attack()
    {
        if (!_isAllowedToAttack) return;

        PlayerMovement._instance._Stamina -= PlayerMovement._instance._attackStaminaUse;
        _isAllowedToAttack = false;
        PlayerCombat._instance._IsAttacking = true;

        CameraController.ShakeCamera(2f, 1.75f, 0.15f, 0.35f);

        GameManager._instance.PlayerAttackHandle();
        GameManager._instance.CallForAction(() => { if (_IsAttackInterrupted) return; _isAllowedToAttack = true; }, _attackWaitTime);

        PlayerStateController._instance.ChangeAnimation(GetAttackName());
        GameManager._instance.CallForAction(() => SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Attacks), transform.position, 0.3f, false, UnityEngine.Random.Range(0.9f, 1f)), 0.3f);
        SoundManager._instance.PlaySound(SoundManager._instance.BladeSlide, transform.position, 0.4f, false, UnityEngine.Random.Range(0.8f, 0.95f));


        /*GameManager._instance.CallForAction(() =>
        {
            if (_attackColliderWarning.gameObject.activeSelf)
                _attackColliderWarning.gameObject.SetActive(false);
            _attackColliderWarning.gameObject.SetActive(true);
        }, _attackTime * 5f / 9f);*/

        if (_attackColliderWarning.gameObject.activeSelf)
            _attackColliderWarning.gameObject.SetActive(false);
        _attackColliderWarning.gameObject.SetActive(true);

        GameManager._instance.CallForAction(() => { if (IsDead) return; _attackColliderWarning.gameObject.SetActive(false); }, _attackTime);

        GameManager._instance.CallForAction(() => { if (IsDead) return; _attackCollider.gameObject.SetActive(true);}, _attackTime * 0.5f);

        GameManager._instance.CallForAction(() => { if (IsDead) return; _attackCollider.gameObject.SetActive(false); PlayerCombat._instance._IsAttacking = false; }, _attackTime);
    }
    private void SelectAttackType()
    {
        if (PlayerStateController._instance._playerState is PlayerStates.OnWall)
        {
            bool isLeft = !(PlayerStateController._instance._playerState as PlayerStates.OnWall).isWallOnLeftSide;
            if(isLeft)
                _attackType = AttackType.Left;
            else
                _attackType = AttackType.Right;
            return;
        }

        _attackType = AttackType.Both;

        /*int random = UnityEngine.Random.Range(0, 25);
        if (random != 23 && random != 24)
            _attackType = AttackType.Both;
        else if (random == 23)
            _attackType = AttackType.Left;
        else if (random == 24)
            _attackType = AttackType.Right;*/
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
        _IsDodgingOrForwardLeap = true;

        CameraController.ShakeCamera(1.5f, 1.25f, 0.1f, 0.3f);

        GameManager._instance.CallForAction(() => { _isAllowedToForwardLeap = true; }, _forwardLeapWaitTime);

        GameManager._instance.CallForAction(() => { _IsDodgingOrForwardLeap = false; }, _forwardLeapTime);

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
        SoundManager._instance.PlaySound(SoundManager._instance.Throw, transform.position, 0.25f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        GameManager._instance.CallForAction(() => UseThrowableItem(item), 0.2f);
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
        SoundManager._instance.PlaySound(SoundManager._instance.SmallCrash, transform.position, 0.5f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        PlayerStateController._instance.EnterAnimState(new PlayerAnimations.WaitForOneAnim(1f));
    }
    public void HitBreakable(GameObject breakable)
    {
        PlayerStateController._instance.ChangeAnimation("HitBreakable");
        PlayerStateController._instance.EnterAnimState(new PlayerAnimations.WaitForOneAnim(0.8f));
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
        return "Deflected" + UnityEngine.Random.Range(1, 4).ToString();
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
    public string GetAttackName()
    {
        SelectAttackType();

        if (_attackType == AttackType.Both)
        {
            if (_lastAttackTime + 1.5f > Time.time)
            {
                _lastAttackNumber++;
                if (_lastAttackNumber > 5) _lastAttackNumber = 1;
                _lastAttackTime = Time.time;
                return "Attack" + _lastAttackNumber;
            }
            _lastAttackNumber = 1;
            _lastAttackTime = Time.time;
            return "Attack" + _lastAttackNumber;
        }
        else
        {
            _lastAttackTime = Time.time;
            _lastAttackNumber = 0;
            if(_attackType == AttackType.Left)
                return "LeftAttack" + UnityEngine.Random.Range(1, 2).ToString();
            else
                return "RightAttack" + UnityEngine.Random.Range(1, 2).ToString();
        }
    }
    public string GetThrowName()
    {
        bool isLeft = false;

        if (PlayerStateController._instance._playerState is PlayerStates.OnWall)
        {
            isLeft = !(PlayerStateController._instance._playerState as PlayerStates.OnWall).isWallOnLeftSide;
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
    public void Die(Vector3 dir, float killersVelocityMagnitude)
    {
        CameraController.ShakeCamera(3.25f, 1.4f, 0.15f, 2.25f);

        PlayerStateController._instance._Animator.SetTrigger("Death");
        var vfx = Instantiate(GameManager._instance.DeathVFX, Camera.main.transform);
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
}
