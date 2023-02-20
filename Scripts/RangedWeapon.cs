using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeapon : MonoBehaviour
{
    public void Fire(GameObject projectilePrefab, Vector3 projectilePosition, Collider creatorCollider, Vector3 target, float speed)
    {
        Vector3 dir = (target - projectilePosition).normalized;
        GameObject newProjectile = Instantiate(projectilePrefab, projectilePosition, Quaternion.identity);
        newProjectile.GetComponent<Projectile>().WhenTriggered = newProjectile.GetComponent<Projectile>().WhenTriggeredForKnife;
        newProjectile.GetComponent<Projectile>().IgnoreCollisionCollider = creatorCollider;
        newProjectile.GetComponent<Rigidbody>().velocity = dir * speed;
        newProjectile.transform.forward = dir;
    }
}
