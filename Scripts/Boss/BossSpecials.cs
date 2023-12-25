using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class BossSpecial
{
    public int _phase { get; set; }
    public int _phaseCombatStaminaLimit { get; set; }
    public int _phaseCount { get; set; }
    public string _name { get; set; }
    public BossStateController _controller { get; set; }

    /// <returns>Action Time</returns>
    public abstract float DoRandomWallAction();
    public abstract float DoRandomGroundAction();
    public abstract float JumpToPlayer();

    public Coroutine _singleAttackCoroutine;
    public Coroutine _jumpCoroutine;
}
public class Boss1Special : BossSpecial
{
    public Boss1Special(BossStateController controller)
    {
        _phaseCount = 1;
        _phase = (int)GameManager._instance.BossPhaseCounterBetweenScenes.transform.position.x;
        _controller = controller;
        _name = "Ginhaeyr The Yielder";
        GameManager._instance.BossUI.GetComponentInChildren<TextMeshProUGUI>().text = _name;
    }
    public override float DoRandomWallAction()
    {
        return DoWallActionPhase2();
        /*
        switch (_phase)
        {
            case 1:
                return DoWallActionPhase1();
            case 2:
                return DoWallActionPhase2();
            default:
                return 0f;
        }*/
    }
    public float DoWallActionPhase1()
    {
        switch (UnityEngine.Random.Range(0, 3))
        {
            case 0:
                return ThrowKnife();
            case 1:
                return ThrowSmoke();
            case 2:
                return JumpToPlayer();
            default:
                return 0f;
        }
    }
    
    public float DoWallActionPhase2()
    {
        switch (UnityEngine.Random.Range(0, 5))
        {
            case 0:
                return ThrowKnife();
            case 1:
                return ThrowSmoke();
            case 2:
                return JumpToPlayer();
            case 3:
                return ThrowWeapon();
            case 4:
                return ThrowExplosive();
            default:
                return 0f;
        }
    }
    public override float DoRandomGroundAction()
    {
        return DoGroundActionPhase2();
        /*switch (_phase)
        {
            case 1:
                return DoGroundActionPhase1();
            case 2:
                return DoGroundActionPhase2();
            default:
                return 0f;
        }*/
    }
    public float DoGroundActionPhase1()
    {
        switch (UnityEngine.Random.Range(0, 3))
        {
            case 0:
                return Spell1();
            case 1:
                return Spell2();
            case 2:
                return JumpToPlayer();
            default:
                return 0f;
        }
    }
    public float DoGroundActionPhase2()
    {
        switch (UnityEngine.Random.Range(0, 3))
        {
            case 0:
                return Spell1();
            case 1:
                return Spell2();
            case 2:
                return JumpToPlayer();
            default:
                return 0f;
        }
    }
    private float GetAnimLenght(string name)
    {
        //no need for now
        //gamemanager.getanimtime()
        return 0f;
    }
    public float Spell1()
    {
        _controller.ChangeAnimation("Spell");
        GameObject obj = GameObject.Instantiate(PrefabHolder._instance.Spell1, _controller.transform.position + Vector3.up * 0.8f, Quaternion.identity);
        obj.GetComponentInChildren<Projectile>().IgnoreCollisionCollider = _controller._bossCombat.Collider;
        obj.GetComponentInChildren<Projectile>().WhenTriggered = obj.GetComponentInChildren<Projectile>().WhenTriggeredForKnife;
        obj.GetComponentInChildren<Projectile>().ChangeSoundObj(SoundManager._instance.PlaySound(SoundManager._instance.Spell1, obj.transform.position, 0.15f, true, 1f));
        obj.GetComponentInChildren<Rigidbody>().velocity = (-_controller.transform.forward + Vector3.up * 1f).normalized * 3f;
        float time = Random.Range(0.75f, 1.2f);
        GameManager._instance.CallForAction(() => { if (obj == null || _controller._isDead) return; obj.transform.Find("SpellProjectile").GetComponent<Collider>().enabled = false; obj.GetComponentInChildren<Rigidbody>().velocity = (GameManager._instance.PlayerRb.transform.position - obj.transform.position).normalized * 80f; }, time);
        GameManager._instance.CallForAction(() => { if (obj == null || _controller._isDead) return; obj.transform.Find("SpellProjectile").GetComponent<Collider>().enabled = true; obj.transform.Find("SpellProjectile").GetComponent<Collider>().isTrigger = true; obj.transform.Find("SpellWarning").GetComponent<Collider>().enabled = true; }, time + 0.15f);

        return GetAnimLenght("Spell");
    }
    public float Spell2()
    {
        _controller.ChangeAnimation("Spell");
        GameObject obj = GameObject.Instantiate(PrefabHolder._instance.Spell2, _controller.transform.position + Vector3.up * 0.8f, Quaternion.identity);
        obj.GetComponentInChildren<Projectile>().IgnoreCollisionCollider = _controller._bossCombat.Collider;
        obj.GetComponentInChildren<Projectile>().WhenTriggered = obj.GetComponentInChildren<Projectile>().WhenTriggeredForKnife;
        obj.GetComponentInChildren<Projectile>().ChangeSoundObj(SoundManager._instance.PlaySound(SoundManager._instance.Spell2, obj.transform.position, 0.15f, true, 1f));
        obj.GetComponentInChildren<Rigidbody>().velocity = (-_controller.transform.forward + Vector3.up * 1f).normalized * 3f;
        float time = Random.Range(0.5f, 1f);
        GameManager._instance.CallForAction(() => { if (obj == null || _controller._isDead) return; obj.transform.Find("SpellProjectile").GetComponent<Collider>().enabled = false;  obj.GetComponentInChildren<Rigidbody>().velocity = (GameManager._instance.PlayerRb.transform.position - obj.transform.position).normalized * 60f; }, time);
        GameManager._instance.CallForAction(() => { if (obj == null || _controller._isDead) return; obj.transform.Find("SpellProjectile").GetComponent<Collider>().enabled = true; obj.transform.Find("SpellProjectile").GetComponent<Collider>().isTrigger = true; obj.transform.Find("SpellWarning").GetComponent<Collider>().enabled = true; }, time + 0.15f);

        return GetAnimLenght("Spell");
    }

    /// <returns>-1 for identity</returns>
    public override float JumpToPlayer()
    {
        _controller.ChangeAnimation("JumpAttackFlying");
        GameManager._instance.StartCoroutine(JumpToPlayerCoroutine());
        return -1f;
    }
    private IEnumerator JumpToPlayerCoroutine()
    {
        _controller._rb.useGravity = true;
        Vector3 dir = (GameManager._instance.PlayerRb.transform.position - _controller.transform.position).normalized;
        float distance = (GameManager._instance.PlayerRb.transform.position - _controller.transform.position).magnitude;
        float speedMul = distance < 9f ? 0.3f : (distance < 18f ? 0.65f : 1f);
        _controller._rb.velocity = Vector3.up * speedMul * 12f + 50f * speedMul * dir;
        Vector3 velWithoutY;

        float time = 0f;
        float isGroundedCounter = 0f;
        while ((GameManager._instance.PlayerRb.transform.position - _controller.transform.position).magnitude > (_controller._rb.velocity.magnitude > 32f ? 6.5f : 4f))
        {
            time += Time.deltaTime;

            if (_controller._bossMovement.IsGrounded()) isGroundedCounter += Time.deltaTime;
            else isGroundedCounter = 0f;

            if (time > 3f || _controller._bossCombat.IsDead || (time > 0.75f && _controller._rb.velocity.magnitude < 0.5f) || isGroundedCounter > 0.8f)
                break;

            velWithoutY = _controller._rb.velocity;
            velWithoutY.y = 0f;
            dir = (GameManager._instance.PlayerRb.transform.position - _controller.transform.position - Vector3.up * 6f + _controller.transform.forward * 1.5f).normalized;
            Vector3 forwardToPlayerDirection = new Vector3(dir.x, 0f, dir.z);
            _controller.transform.forward = Vector3.Lerp(_controller.transform.forward, forwardToPlayerDirection, Time.deltaTime * 4f);
            velWithoutY = Vector3.Lerp(velWithoutY, velWithoutY.magnitude * dir, Time.deltaTime * 1.5f);
            velWithoutY *= (1f - Time.deltaTime / 2.5f);
            _controller._rb.velocity = new Vector3(velWithoutY.x, _controller._rb.velocity.y, velWithoutY.z);
            yield return null;
        }
        JumpToPlayerAttack();
    }
    private void JumpToPlayerAttack()
    {
        float animTime = GameManager._instance.GetAnimationTime("JumpAttack", _controller._animatorController);
        _controller._bossCombat.SingleAttack("JumpAttack");
        GameManager._instance.CoroutineCall(ref _singleAttackCoroutine, _controller._bossCombat.SingleAttack("JumpAttack"), _controller._bossCombat);
        GameManager._instance.CoroutineCall(ref _jumpCoroutine, JumpToPlayerAttackCoroutine(), _controller._bossCombat);
    }
    private IEnumerator JumpToPlayerAttackCoroutine()
    {
        float time = 0f;
        while (time < 0.7f)
        {
            time += Time.deltaTime;
            if(_controller._bossCombat.IsDead)
            {
                _controller.EnterState(new BossStates.Chase());
                yield break;
            }
            Vector3 dir = (GameManager._instance.PlayerRb.transform.position - _controller.transform.position).normalized;
            Vector3 forwardToPlayerDirection = new Vector3(dir.x, 0f, dir.z);
            _controller.transform.forward = Vector3.Lerp(_controller.transform.forward, forwardToPlayerDirection, Time.deltaTime * 15f);
            _controller._rb.velocity = Vector3.Lerp(_controller._rb.velocity, 6f * dir, Time.deltaTime * 10f);
            yield return null;
        }
        _controller._rb.velocity = Vector3.zero;

        _controller._rb.useGravity = true;
        _controller._rb.isKinematic = true;
        _controller._agent.enabled = true;

        _controller.EnterState(new BossStates.Chase());
    }

    public float ThrowKnife()
    {
        int random = Random.Range(0, 3);
        if (random == 0)
        {
            _controller.ChangeAnimation("Throw1");
            GameManager._instance.CallForAction(ThrowKnifeInstantiate, 0.2f);
            return GetAnimLenght("Throw1");
        }
        else if(random == 1)
        {
            _controller.ChangeAnimation("Throw2");
            GameManager._instance.CallForAction(ThrowKnifeInstantiate, 0.2f);
            GameManager._instance.CallForAction(ThrowKnifeInstantiate, 0.8f);
            return GetAnimLenght("Throw2");
        }
        else
        {
            _controller.ChangeAnimation("Throw3");
            GameManager._instance.CallForAction(ThrowKnifeInstantiate, 0.2f);
            GameManager._instance.CallForAction(ThrowKnifeInstantiate, 0.7f);
            GameManager._instance.CallForAction(ThrowKnifeInstantiate, 1.5f);
            return GetAnimLenght("Throw3");
        }
    }
    private void ThrowKnifeInstantiate()
    {
        if (_controller._isDead) return;

        GameObject obj = GameObject.Instantiate(PrefabHolder._instance.KnifePrefab, _controller.transform.position, Quaternion.identity);
        obj.GetComponentInChildren<Projectile>().IgnoreCollisionCollider = _controller._bossCombat.Collider;
        obj.GetComponentInChildren<Projectile>().WhenTriggered = obj.GetComponentInChildren<Projectile>().WhenTriggeredForKnife;
        obj.GetComponentInChildren<Rigidbody>().velocity = (GameManager._instance.PlayerRb.transform.position + GameManager._instance.PlayerRb.velocity * 0.05f - _controller.transform.position).normalized * 40f;
        obj.transform.forward = (GameManager._instance.PlayerRb.transform.position + GameManager._instance.PlayerRb.velocity * 0.05f - _controller.transform.position).normalized;
        obj.transform.localScale *= 2f;
    }

    public float ThrowSmoke()
    {
        _controller.ChangeAnimation("Throw1");
        GameObject obj = GameObject.Instantiate(PrefabHolder._instance.SmokeProjectilePrefab, _controller.transform.position, Quaternion.identity);
        obj.GetComponentInChildren<Projectile>().IgnoreCollisionCollider = _controller._bossCombat.Collider;
        obj.GetComponentInChildren<Projectile>().WhenTriggered = obj.GetComponentInChildren<Projectile>().WhenTriggeredForSmoke;
        Vector3 vel = (GameManager._instance.PlayerRb.transform.position - _controller.transform.position).normalized * 16f;
        vel.y = 2f;
        obj.GetComponentInChildren<Rigidbody>().velocity = vel;
        return GetAnimLenght("Throw1");
    }
    public float ThrowWeapon()
    {
        if (_controller._bossCombat.WeaponObject == null) return 0f;

        _controller.ChangeAnimation("ThrowWeapon");
        GameManager._instance.CallForAction(() => _controller._bossCombat.ThrowWeapon(), 0.5f);
        GameManager._instance.CallForAction(() => _controller._bossCombat.GetWeaponBack(), 2.5f);
        return GetAnimLenght("ThrowWeapon");
    }
    public float ThrowExplosive()
    {
        int random = Random.Range(0, 3);
        if (random == 0)
        {
            _controller.ChangeAnimation("Throw1");
            GameManager._instance.CallForAction(ThrowExplosiveInstantiate, 0.3f);
            return GetAnimLenght("Throw1");
        }
        else if (random == 1)
        {
            _controller.ChangeAnimation("Throw2");
            GameManager._instance.CallForAction(ThrowExplosiveInstantiate, 0.3f);
            GameManager._instance.CallForAction(ThrowExplosiveInstantiate, 0.5f);
            return GetAnimLenght("Throw2");
        }
        else
        {
            _controller.ChangeAnimation("Throw3");
            GameManager._instance.CallForAction(ThrowExplosiveInstantiate, 0.3f);
            GameManager._instance.CallForAction(ThrowExplosiveInstantiate, 0.5f);
            GameManager._instance.CallForAction(ThrowExplosiveInstantiate, 0.7f);
            return GetAnimLenght("Throw3");
        }
    }
    private void ThrowExplosiveInstantiate()
    {
        if (_controller._isDead) return;

        GameObject obj = GameObject.Instantiate(PrefabHolder._instance.L1Explosive, _controller.transform.position, Quaternion.identity);
        obj.GetComponentInChildren<Rigidbody>().velocity = (GameManager._instance.PlayerRb.transform.position - _controller.transform.position).normalized * 24f;
        obj.GetComponentInChildren<Rigidbody>().angularVelocity = new Vector3(10f, 10f, 10f);
        obj.GetComponentInChildren<ExplosiveL1>()._isTriggered = true;
    }
}

public class Boss2Special : BossSpecial
{
    public Boss2Special(BossStateController controller)
    {
        _phaseCount = 1;
        _phase = (int)GameManager._instance.BossPhaseCounterBetweenScenes.transform.position.x;
        _controller = controller;
        _name = "Pheardosa The Traitor";
        GameManager._instance.BossUI.GetComponentInChildren<TextMeshProUGUI>().text = _name;
    }

    public override float DoRandomWallAction()
    {
        switch (_phase)
        {
            case 1:
                return DoRandomWallActionPhase1();
            case 2:
                return DoRandomWallActionPhase2();
            default:
                return 0f;
        }
    }
    public float DoRandomWallActionPhase1()
    {
        switch (UnityEngine.Random.Range(0, 2))
        {
            case 0:
                return Action1();
            case 1:
                return Action2();
            default:
                return 0f;
        }
    }
    public float DoRandomWallActionPhase2()
    {
        switch (UnityEngine.Random.Range(0, 2))
        {
            case 0:
                return Action1();
            case 1:
                return Action2();
            default:
                return 0f;
        }
    }
    public override float DoRandomGroundAction()
    {
        switch (_phase)
        {
            case 1:
                return DoGroundActionPhase1();
            case 2:
                return DoGroundActionPhase2();
            default:
                return 0f;
        }
    }
    public float DoGroundActionPhase1()
    {
        switch (UnityEngine.Random.Range(0, 3))
        {
            case 0:
                return Action1();
            case 1:
                return Action2();
            case 2:
                return Action3();
            default:
                return 0f;
        }
    }
    public float DoGroundActionPhase2()
    {
        switch (UnityEngine.Random.Range(0, 5))
        {
            case 0:
                return Action1();
            case 1:
                return Action2();
            case 2:
                return Action3();
            case 3:
                return Action4();
            case 4:
                return Action5();
            default:
                return 0f;
        }
    }

    public override float JumpToPlayer()
    {
        _controller.ChangeAnimation("JumpAttackFlying");
        GameManager._instance.StartCoroutine(JumpToPlayerCoroutine());
        return -1f;
    }
    private IEnumerator JumpToPlayerCoroutine()
    {
        _controller._rb.useGravity = true;
        Vector3 dir = (GameManager._instance.PlayerRb.transform.position - _controller.transform.position).normalized;
        float distance = (GameManager._instance.PlayerRb.transform.position - _controller.transform.position).magnitude;
        float speedMul = distance < 9f ? 0.3f : (distance < 18f ? 0.65f : 1f);
        _controller._rb.velocity = Vector3.up * speedMul * 1.5f + 60f * speedMul * dir;
        Vector3 velWithoutY;

        float time = 0f;
        float isGroundedCounter = 0f;
        while ((GameManager._instance.PlayerRb.transform.position - _controller.transform.position).magnitude > (_controller._rb.velocity.magnitude > 32f ? 5.4f : 3.5f))
        {
            time += Time.deltaTime;

            if (_controller._bossMovement.IsGrounded()) isGroundedCounter += Time.deltaTime;
            else isGroundedCounter = 0f;

            if (time > 0.5f || _controller._bossCombat.IsDead || (time > 0.75f && _controller._rb.velocity.magnitude < 0.5f) || isGroundedCounter > 0.8f)
                break;

            velWithoutY = _controller._rb.velocity;
            velWithoutY.y = 0f;
            dir = (GameManager._instance.PlayerRb.transform.position - _controller.transform.position - Vector3.up * 6f + _controller.transform.forward * 1.5f).normalized;
            Vector3 forwardToPlayerDirection = new Vector3(dir.x, 0f, dir.z);
            _controller.transform.forward = Vector3.Lerp(_controller.transform.forward, forwardToPlayerDirection, Time.deltaTime * 4f);
            velWithoutY = Vector3.Lerp(velWithoutY, velWithoutY.magnitude * dir, Time.deltaTime * 1.5f);
            velWithoutY *= (1f - Time.deltaTime / 2.5f);
            _controller._rb.velocity = new Vector3(velWithoutY.x, _controller._rb.velocity.y, velWithoutY.z);
            yield return null;
        }
        JumpToPlayerAttack();
    }
    private void JumpToPlayerAttack()
    {
        float animTime = GameManager._instance.GetAnimationTime("JumpAttack", _controller._animatorController);
        _controller._bossCombat.SingleAttack("JumpAttack");
        GameManager._instance.CoroutineCall(ref _singleAttackCoroutine, _controller._bossCombat.SingleAttack("JumpAttack"), _controller._bossCombat);
        GameManager._instance.CoroutineCall(ref _jumpCoroutine, JumpToPlayerAttackCoroutine(), _controller._bossCombat);
    }
    private IEnumerator JumpToPlayerAttackCoroutine()
    {
        float time = 0f;
        while (time < 0.7f)
        {
            time += Time.deltaTime;
            if (_controller._bossCombat.IsDead)
            {
                _controller.EnterState(new BossStates.Chase());
                yield break;
            }
            Vector3 dir = (GameManager._instance.PlayerRb.transform.position - _controller.transform.position).normalized;
            Vector3 forwardToPlayerDirection = new Vector3(dir.x, 0f, dir.z);
            _controller.transform.forward = Vector3.Lerp(_controller.transform.forward, forwardToPlayerDirection, Time.deltaTime * 15f);
            _controller._rb.velocity = Vector3.Lerp(_controller._rb.velocity, 6f * dir, Time.deltaTime * 10f);
            yield return null;
        }
        _controller._rb.velocity = Vector3.zero;

        _controller._rb.useGravity = true;
        _controller._rb.isKinematic = true;
        _controller._agent.enabled = true;

        _controller.EnterState(new BossStates.Chase());
    }
    public float Action1()
    {
        return 0f;
    }
    public float Action2()
    {
        return 0f;
    }
    public float Action3()
    {
        return 0f;
    }
    //Phase 2//
    public float Action4()
    {
        return 0f;
    }
    public float Action5()
    {
        return 0f;
    }
}

public class Boss3Special : BossSpecial
{
    public Boss3Special(BossStateController controller)
    {
        _phaseCount = 1;
        _phase = 1;
        _controller = controller;
        _name = "Zec The Leader";
        GameManager._instance.BossUI.GetComponentInChildren<TextMeshProUGUI>().text = _name;
    }
    public override float DoRandomWallAction()
    {
        switch (_phase)
        {
            case 1:
                return DoWallActionPhase1();
            case 2:
                return DoWallActionPhase2();
            default:
                return 0f;
        }
    }
    public float DoWallActionPhase1()
    {
        switch (UnityEngine.Random.Range(0, 2))
        {
            case 0:
                return Action1();
            case 1:
                return Action2();
            default:
                return 0f;
        }
    }
    public float DoWallActionPhase2()
    {
        switch (UnityEngine.Random.Range(0, 2))
        {
            case 0:
                return Action1();
            case 1:
                return Action2();
            default:
                return 0f;
        }
    }
    public override float DoRandomGroundAction()
    {
        switch (_phase)
        {
            case 1:
                return DoGroundActionPhase1();
            case 2:
                return DoGroundActionPhase2();
            default:
                return 0f;
        }
    }
    public float DoGroundActionPhase1()
    {
        switch (UnityEngine.Random.Range(0, 3))
        {
            case 0:
                return Action1();
            case 1:
                return Action2();
            case 2:
                return Action3();
            default:
                return 0f;
        }
    }
    public float DoGroundActionPhase2()
    {
        switch (UnityEngine.Random.Range(0, 5))
        {
            case 0:
                return Action1();
            case 1:
                return Action2();
            case 2:
                return Action3();
            case 3:
                return Action4();
            case 4:
                return Action5();
            default:
                return 0f;
        }
    }

    public override float JumpToPlayer()
    {
        return -1f;
    }
    public float Action1()
    {
        return 0f;
    }
    public float Action2()
    {
        return 0f;
    }
    public float Action3()
    {
        return 0f;
    }
    //Phase 2//
    public float Action4()
    {
        return 0f;
    }
    public float Action5()
    {
        return 0f;
    }
}