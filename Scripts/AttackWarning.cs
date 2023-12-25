using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackWarning : MonoBehaviour
{
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
    [SerializeField]
    private bool isMeleeWeapon;

    [HideInInspector] public bool _isRunningForPlayer;

    private Collider IgnoreCollisionCollider;

    private void Awake()
    {
        if (transform.parent != null && transform.parent.gameObject.CompareTag("Player"))
            IgnoreCollisionCollider = transform.parent.GetComponent<Collider>();
        else if (isMeleeWeapon)
            IgnoreCollisionCollider = null;
        else if (transform.parent.GetComponentInChildren<Projectile>() != null)
            StartCoroutine(AssingIgnoreCollider());
        else
            IgnoreCollisionCollider = transform.parent.GetComponent<Collider>();
    }
    private IEnumerator AssingIgnoreCollider()
    {
        float time = 0f;
        while (transform.parent.GetComponentInChildren<Projectile>().IgnoreCollisionCollider == null)
        {
            time += Time.deltaTime;
            if (time > 2f) yield break;
            yield return null;
        }
        IgnoreCollisionCollider = transform.parent.GetComponentInChildren<Projectile>().IgnoreCollisionCollider;
    }
    private void OnEnable()
    {
        Killables.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null || !other.CompareTag("HitBox")) return;
        if (IgnoreCollisionCheck(IgnoreCollisionCollider, other)) return;
        IKillable otherKillable = GameManager._instance.GetHitBoxIKillable(other);
        if (other != null && otherKillable != null && !otherKillable.IsDead && !Killables.Contains(otherKillable))
        {
            Killables.Add(otherKillable);
            if (IgnoreCollisionCollider == null)
            {
                otherKillable.AttackWarning(GetComponent<Collider>(), false, transform.position);
            }
            else if (transform.parent.GetComponent<MeleeWeaponThrowed>() != null)
            {
                otherKillable.AttackWarning(GetComponent<Collider>(), Random.Range(0, 4) == 0, transform.position - transform.parent.gameObject.GetComponent<Rigidbody>().velocity / 3f);
            }
            else if (IgnoreCollisionCollider.gameObject.CompareTag("Player") && isMeleeWeapon)
            {
                Rigidbody rb = transform.parent.gameObject.GetComponent<Rigidbody>();
                if (rb == null) return;
                bool isFast = _isRunningForPlayer && rb.velocity.magnitude > 10.5f && !GameManager._instance.IsPlayerHasMeleeWeapon;
                otherKillable.AttackWarning(GetComponent<Collider>(), isFast, rb.transform.position);
            }
            else
                otherKillable.AttackWarning(GetComponent<Collider>(), false, IgnoreCollisionCollider.transform.position);
                
        }
    }
    private bool IgnoreCollisionCheck(Collider Ignored, Collider collisionCollider)
    {
        if (IgnoreCollisionCollider == null) return false;

        Transform collisionTransform = collisionCollider.transform;
        while (collisionTransform.parent != null)
        {
            collisionTransform = collisionTransform.parent;
        }
        if (collisionTransform.GetComponent<Collider>() == Ignored)
        {
            return true;
        }
        return false;
    }
    
}
