using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeapon : MonoBehaviour
{
    public void Fire(GameObject projectilePrefab, Vector3 projectilePosition, Collider creatorCollider, Vector3 forward, float speed)
    {
        GameObject newProjectile = Instantiate(projectilePrefab, projectilePosition, Quaternion.identity);
        newProjectile.GetComponentInChildren<Projectile>().WhenTriggered = newProjectile.GetComponentInChildren<Projectile>().WhenTriggeredForKnife;
        newProjectile.GetComponentInChildren<Projectile>().IgnoreCollisionCollider = creatorCollider;
        newProjectile.GetComponentInChildren<Rigidbody>().velocity = forward * speed;
        newProjectile.transform.forward = forward;
    }
}
