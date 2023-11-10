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
        transform.Find("AttackColliderWarning").transform.localScale *= 5f;
    }
    private void Update()
    {
        if (_rb.velocity.magnitude < 1f && !_isHitBefore)
        {
            StartCoroutine(WeaponHitCoroutine());
            _isHitBefore = true;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == null || GetParentCollider(collision.collider) == null || GetParentCollider(collision.collider) == IgnoreCollisionCollider) return;
        if ((collision.collider.isTrigger && collision.collider.CompareTag("HitBox")) || !collision.collider.isTrigger)
        {
            if (transform.Find("AttackCollider").gameObject.activeInHierarchy && (collision.collider.CompareTag("HitBox") || collision.collider.CompareTag("Enemy")) && GetParentCollider(collision.collider).GetComponent<IKillable>() != null)
                TryToKill(collision.collider);
            if (transform.Find("AttackCollider").gameObject.activeInHierarchy && collision.collider.CompareTag("ExplosiveL1"))
                collision.collider.GetComponent<ExplosiveL1CheckAttacked>().DestroyWithoutExploding(transform);
            if (!_isHitBefore)
                StartCoroutine(WeaponHitCoroutine());
            _isHitBefore = true;
        }
    }
    private IEnumerator WeaponHitCoroutine()
    {
        transform.Find("AttackCollider").gameObject.SetActive(false);
        transform.Find("AttackColliderWarning").transform.localScale /= 5f;
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
                Kill(otherKillable, _rb.velocity.normalized, GetParent(transform).GetComponent<Rigidbody>().velocity.magnitude, transform.Find("AttackCollider").GetComponent<MeleeWeapon>());
            }
        }
    }
    private void Kill(IKillable killable, Vector3 dir, float killersVelocityMagnitude, IKillObject killer)
    {
        killable.Die(dir, killersVelocityMagnitude, killer, GetComponent<MeleeWeaponForPlayer>().IsHardHit());
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
