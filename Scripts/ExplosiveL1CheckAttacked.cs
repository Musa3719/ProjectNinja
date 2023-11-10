using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveL1CheckAttacked : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "AttackCollider" || other.GetComponentInChildren<Projectile>() != null)
        {
            DestroyWithoutExploding(other.transform);
        }
    }
    public void DestroyWithoutExploding(Transform other)
    {
        GetComponentInChildren<ExplosiveL1>().DestroyWithoutExploding(other);
    }
}
