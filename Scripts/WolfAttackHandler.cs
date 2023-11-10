using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfAttackHandler : MonoBehaviour, IKillObject
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
    private Collider IgnoreCollisionCollider;
    public GameObject Owner => IgnoreCollisionCollider == null ? null : IgnoreCollisionCollider.gameObject;

    private void Awake()
    {
        if (IgnoreCollisionCollider == null)
            IgnoreCollisionCollider = GetParent(transform).GetComponent<Collider>();
    }

    private void OnEnable()
    {
        Killables.Clear();
    }
    public void Kill(IKillable killable, Vector3 dir, float killersVelocityMagnitude, IKillObject killer)
    {
        if (!killable.Object.name.StartsWith("Enemy_2"))
            killable.Die(dir, killersVelocityMagnitude, killer, true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (other.CompareTag("BreakableObject"))
        {
            other.GetComponent<BreakableObject>().BrakeObject((other.transform.position - GetParent(transform).position).normalized);
            SoundManager._instance.PlaySound(SoundManager._instance.HitWallWithWeapon, transform.position, 0.2f, false, UnityEngine.Random.Range(0.6f, 0.7f));
            return;
        }
        else if (other.CompareTag("Wall") || IsProp(other))
        {
            SoundManager._instance.PlaySound(SoundManager._instance.HitWallWithWeapon, IgnoreCollisionCollider.transform.position + IgnoreCollisionCollider.transform.forward * 0.5f, 0.1f, false, UnityEngine.Random.Range(0.6f, 0.7f));
            GameObject hitSmoke = Instantiate(GameManager._instance.HitSmokeVFX, IgnoreCollisionCollider.transform.position + IgnoreCollisionCollider.transform.forward * 0.7f + Vector3.up * 0.35f, Quaternion.identity);
            hitSmoke.transform.localScale *= 3f;
            Color temp = hitSmoke.GetComponentInChildren<SpriteRenderer>().color;
            hitSmoke.GetComponentInChildren<SpriteRenderer>().color = new Color(temp.r, temp.g, temp.b, 9f / 255f);
            Destroy(hitSmoke, 5f);
        }

        if (!other.CompareTag("HitBox")) return;
        if (IgnoreCollisionCheck(IgnoreCollisionCollider, other)) return;

        IKillable otherKillable = GameManager._instance.GetHitBoxIKillable(other);
        if (other != null && otherKillable != null && !otherKillable.IsDead && !Killables.Contains(otherKillable))
        {
            Killables.Add(otherKillable);

            bool isTargetDodging = otherKillable.IsDodgingGetter;
            bool isTargetBlocking = otherKillable.IsBlockingGetter;
            if (!isTargetBlocking && !isTargetDodging)
            {
                Kill(otherKillable, (otherKillable.Object.transform.position - GetParent(transform).position).normalized, GetParent(transform).GetComponent<Rigidbody>().velocity.magnitude, this);
            }
            else if (isTargetDodging)
            {
                if (otherKillable.Object.CompareTag("Enemy"))
                {
                    Kill(otherKillable, (otherKillable.Object.transform.position - GetParent(transform).position).normalized, GetParent(transform).GetComponent<Rigidbody>().velocity.magnitude, this);
                }
            }
            else if (isTargetBlocking)
            {
                if (IsInAngle(other))
                {
                    otherKillable.DeflectWithBlock((GetParent(transform).position - otherKillable.Object.transform.position).normalized, null, false);
                }
                else
                {
                    if (otherKillable.Object.CompareTag("Boss"))
                    {
                        otherKillable.StopBlockingAndDodge();
                    }
                    else
                    {
                        Kill(otherKillable, (other.transform.position - GetParent(transform).position).normalized, GetParent(transform).GetComponent<Rigidbody>().velocity.magnitude, this);
                    }
                }
            }
        }
    }
    private bool IsProp(Collider other)
    {
        if (other == null || other.CompareTag("Door")) return false;
        Transform temp = other.transform;
        while (temp.parent != null)
        {
            if (temp.CompareTag("Prop")) return true;
            temp = temp.parent;
        }
        return false;
    }
    private bool IsInAngle(Collider other)
    {
        Collider targetCollider = GetParentCollider(other);

        Vector3 otherForward = targetCollider.transform.forward;
        otherForward.y = 0f;

        Vector3 selfForward = IgnoreCollisionCollider.transform.forward;
        selfForward.y = 0f;

        float angle = Vector3.Angle(otherForward, selfForward);

        if (angle < 120f)
            return false;
        else
            return true;
    }
    private bool IgnoreCollisionCheck(Collider Ignored, Collider collisionCollider)
    {
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
    private Collider GetParentCollider(Collider col)
    {
        Transform parentTransform = col.transform;
        while (parentTransform.parent != null)
        {
            parentTransform = parentTransform.parent;
        }
        return parentTransform.GetComponent<Collider>();
    }
    private Transform GetParent(Transform tr)
    {
        Transform parentTransform = tr.transform;
        while (parentTransform.parent != null)
        {
            parentTransform = parentTransform.parent;
        }
        return parentTransform;
    }
}
