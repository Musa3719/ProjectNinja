using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour, IKillObject
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
    private bool _isRanged;
    private bool _isDecalCreatedForThisAttack;

    private void Awake()
    {
        IgnoreCollisionCollider = GetParent(transform).GetComponent<Collider>();
    }

    private void OnEnable()
    {
        Killables.Clear();
        _isRanged = false;
        _isDecalCreatedForThisAttack = false;
    }
    public void SetIgnoreCollisionColliderForThrow(Collider col)
    {
        IgnoreCollisionCollider = col;
        _isRanged = true;
    }

    public void Kill(IKillable killable, Vector3 dir, float killersVelocityMagnitude)
    {
        killable.Die(dir, killersVelocityMagnitude);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (other.CompareTag("BreakableObject"))
        {
            other.GetComponent<BreakableObject>().BrakeObject((other.transform.position - GetParent(transform).position).normalized);
            SoundManager._instance.PlaySound(SoundManager._instance.HitWallWithWeapon, transform.position, 0.7f, false, UnityEngine.Random.Range(0.7f, 0.9f));
            return;
        }
        if ((other.CompareTag("Wall") || other.CompareTag("Prop")) && !_isDecalCreatedForThisAttack)
        {
            _isDecalCreatedForThisAttack = true;
            SoundManager._instance.PlaySound(SoundManager._instance.HitWallWithWeapon, IgnoreCollisionCollider.transform.position + IgnoreCollisionCollider.transform.forward * 0.5f, 0.4f, false, UnityEngine.Random.Range(0.93f, 1.07f));
            GameObject decal = Instantiate(GameManager._instance.HoleDecal, transform.position, Quaternion.identity);

            if (other.CompareTag("Wall"))
                decal.transform.forward = other.transform.right;
            else
                decal.transform.forward = -IgnoreCollisionCollider.transform.forward;
            decal.transform.localEulerAngles = new Vector3(decal.transform.localEulerAngles.x, decal.transform.localEulerAngles.y, UnityEngine.Random.Range(0f, 360f));
            
            if (IgnoreCollisionCollider.CompareTag("Player"))
            {
                GameObject hitSmoke = Instantiate(GameManager._instance.HitSmokeVFX, IgnoreCollisionCollider.transform.position + IgnoreCollisionCollider.transform.forward * 0.7f, Quaternion.identity);
                hitSmoke.transform.position = new Vector3(hitSmoke.transform.position.x, GameManager._instance.PlayerLeftHandTransform.position.y, hitSmoke.transform.position.z);
                Destroy(hitSmoke, 5f);

                decal.transform.position = new Vector3(decal.transform.position.x, GameManager._instance.PlayerLeftHandTransform.position.y, decal.transform.position.z);
            }
            else
            {
                Destroy(Instantiate(GameManager._instance.HitSmokeVFX, IgnoreCollisionCollider.transform.position + IgnoreCollisionCollider.transform.forward * 0.7f + Vector3.up * 0.35f, Quaternion.identity), 5f);
            }


        }

        if (!other.CompareTag("HitBox")) return;
        if (IgnoreCollisionCheck(IgnoreCollisionCollider, other)) return;

        IKillable otherKillable = GameManager._instance.GetHitBoxIKillable(other);
        if (other!=null && otherKillable != null && !otherKillable.IsDead && !Killables.Contains(otherKillable))
        {
            Killables.Add(otherKillable);

            bool isTargetDodging = otherKillable.IsDodgingGetter;
            bool isTargetBlocking = otherKillable.IsBlockingGetter;
            if (!isTargetBlocking && !isTargetDodging)
            {
                Kill(otherKillable, (otherKillable.Object.transform.position- GetParent(transform).position).normalized, GetParent(transform).GetComponent<Rigidbody>().velocity.magnitude);
            }
            else if (isTargetDodging)
            {
                if (otherKillable.Object.CompareTag("Enemy"))
                {
                    Kill(otherKillable, (otherKillable.Object.transform.position - GetParent(transform).position).normalized, GetParent(transform).GetComponent<Rigidbody>().velocity.magnitude);
                }
            }
            else if (isTargetBlocking)
            {
                if (_isRanged) return;

                if (IsInAngle(other))
                {
                    otherKillable.DeflectWithBlock((GetParent(transform).position - otherKillable.Object.transform.position).normalized, IgnoreCollisionCollider.GetComponent<IKillable>(), false);
                }
                else
                {
                    if (otherKillable.Object.CompareTag("Boss"))
                    {
                        otherKillable.StopBlockingAndDodge();
                    }
                    else
                    {
                        Kill(otherKillable, (other.transform.position - GetParent(transform).position).normalized, GetParent(transform).GetComponent<Rigidbody>().velocity.magnitude);
                    }
                }
            }
        }
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