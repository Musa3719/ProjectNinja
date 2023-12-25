using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCheckForMeleeWeapon : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other!=null && other.CompareTag("Wall"))
        {
            transform.parent.GetComponent<PlayerCombat>().CheckMeleeAttackAgainstWall(other);
        }
    }
}
