using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKillable
{
    public GameObject Object { get; }
    public bool IsDead { get; }
    public bool IsBlockingGetter { get; }
    public bool IsDodgingGetter { get; }
    public GameObject AttackCollider { get; }
    public int InterruptAttackCounterGetter { get; }

    public float _CollisionVelocity { get; }
    public void AttackDeflected(IKillable deflectedKillable);
    public void ChangeStamina(float amount);

    public void AttackWarning(Collider collider, bool isFastAttack, Vector3 attackPosition);
    public void DeflectWithBlock(Vector3 dir, IKillable attacker, bool isRangedAttack);
    public void BombDeflected();
    public void Stun(float time, bool isSpeedChanges, Transform otherTransform);
    public void HitBreakable(GameObject breakable);
    public void StopBlockingAndDodge();
    public void Die(Vector3 dir, float killersVelocityMagnitude);
}
