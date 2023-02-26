using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInAir : MonoBehaviour
{
    private Collider _ignoreCollisionCollider;
    public Collider IgnoreCollisionCollider { set { transform.Find("AttackCollider").GetComponent<MeleeWeapon>().SetIgnoreCollisionColliderForThrow(value); _ignoreCollisionCollider = value; } get => _ignoreCollisionCollider; }

    private Rigidbody _rb;
    private bool _isHitBefore;
    private void Awake()
    {
        transform.Find("AttackCollider").gameObject.SetActive(true);
        _rb = GetComponent<Rigidbody>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || other.name.Equals("Player") || other.name.Equals("Enemy")) return;

        if (GetParentCollider(other) != null && GetParentCollider(other) == IgnoreCollisionCollider) return;

        if (!_isHitBefore)
            StartCoroutine(WeaponHitCoroutine());
        _isHitBefore = true;
    }
    private IEnumerator WeaponHitCoroutine()
    {
        SoundManager._instance.PlaySound(SoundManager._instance.WeaponStickGround, transform.position, 0.6f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        GameObject hitSmoke = Instantiate(GameManager._instance.HitSmokeVFX, transform.position + Vector3.up * 0.75f, Quaternion.identity);
        hitSmoke.transform.localScale *= 10f;
        Destroy(hitSmoke, 5f);
        yield return new WaitForSeconds(0.02f);
        transform.Find("AttackCollider").gameObject.SetActive(false);
        _rb.isKinematic = true;
    }

    private void Update()
    {
        if (_isHitBefore) return;

        _rb.velocity = Vector3.Lerp(_rb.velocity, (GameManager._instance.PlayerRb.transform.position - transform.position).normalized * 37f, Time.deltaTime * 4f);
        float zAngleTemp = transform.localEulerAngles.z + Time.deltaTime * 640f;
        transform.forward = Vector3.Lerp(transform.forward, -(GameManager._instance.PlayerRb.transform.position - transform.position).normalized, Time.deltaTime * 5f);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, zAngleTemp);
    }
    private Collider GetParentCollider(Collider collider)
    {
        Transform parent = collider.transform;
        while (parent.parent != null)
            parent = parent.parent;
        return parent.GetComponent<Collider>();
    }
}