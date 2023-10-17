using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponThrowed : MonoBehaviour
{
    private Collider _ignoreCollisionCollider;
    public Collider IgnoreCollisionCollider { set { transform.Find("AttackCollider").GetComponent<MeleeWeapon>().SetIgnoreCollisionColliderForThrow(value); _ignoreCollisionCollider = value; } get => _ignoreCollisionCollider; }

    private Rigidbody _rb;
    private bool _isHitBefore;

    public List<IKillable> killables;
    public List<IKillable> Killables
    {
        get
        {
            if (killables == null)
            {
                killables = new List<IKillable>();
            }
            return killables;
        }
    }
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        transform.Find("AttackColliderWarning").gameObject.SetActive(true);
        transform.Find("AttackColliderWarning").transform.localScale *= 2f;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (GetParentCollider(collision.collider) != null && GetParentCollider(collision.collider) == IgnoreCollisionCollider) return;

        if ((collision.collider.isTrigger && collision.collider.CompareTag("HitBox")) || !collision.collider.isTrigger)
        {
            if (!_isHitBefore)
                StartCoroutine(WeaponHitCoroutine());
            _isHitBefore = true;

            if (transform.Find("AttackCollider").gameObject.activeInHierarchy && collision.collider.isTrigger && collision.collider.CompareTag("HitBox") && GetParentCollider(collision.collider).GetComponent<IKillable>() != null)
            {
                TryToKill(collision.collider);
            }
        }
    }
    private IEnumerator WeaponHitCoroutine()
    {
        SoundManager._instance.PlaySound(SoundManager._instance.Blocks[0], transform.position, 0.14f, false, UnityEngine.Random.Range(0.5f, 0.6f));
        transform.Find("AttackCollider").gameObject.SetActive(false);
        transform.Find("AttackColliderWarning").transform.localScale /= 2f;
        transform.Find("AttackColliderWarning").gameObject.SetActive(false);
        yield return new WaitForSeconds(4f);
        _rb.isKinematic = true;
        _rb.useGravity = false;
    }
    private void TryToKill(Collider other)
    {
        IKillable otherKillable = GameManager._instance.GetHitBoxIKillable(other);
        if (other != null && otherKillable != null && !otherKillable.IsDead && !Killables.Contains(otherKillable))
        {
            Killables.Add(otherKillable);

            bool isTargetDodging = otherKillable.IsDodgingGetter;
            bool isTargetBlocking = otherKillable.IsBlockingGetter;
            if (!isTargetBlocking && !isTargetDodging)
            {
                Kill(otherKillable, (otherKillable.Object.transform.position - GetParent(transform).position).normalized, GetParent(transform).GetComponent<Rigidbody>().velocity.magnitude, transform.Find("AttackCollider").GetComponent<MeleeWeapon>());
            }
        }
    }
    private void Kill(IKillable killable, Vector3 dir, float killersVelocityMagnitude, IKillObject killer)
    {
        if (transform.Find("AttackCollider").GetComponent<MeleeWeapon>().IsHardHitWeapon) SoundManager._instance.PlaySound(SoundManager._instance.HardHit, transform.position, 0.25f, false, Random.Range(0.9f, 1f));
        killable.Die(dir, killersVelocityMagnitude, killer);
    }
    private Collider GetParentCollider(Collider collider)
    {
        Transform parent = collider.transform;
        while (parent.parent != null)
            parent = parent.parent;
        return parent.GetComponent<Collider>();
    }
    private Transform GetParent(Transform tra)
    {
        Transform parent = tra;
        while (parent.parent != null)
            parent = parent.parent;
        return parent;
    }
}
