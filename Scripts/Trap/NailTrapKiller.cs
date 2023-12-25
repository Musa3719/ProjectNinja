using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NailTrapKiller : MonoBehaviour, IKillObject
{
    public GameObject Owner => gameObject;
    public void Kill(IKillable killable, Vector3 dir, float killersVelocityMagnitude, IKillObject killer)
    {
        killable.Die(dir, killersVelocityMagnitude, killer, true);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.transform.parent != null && other.transform.parent.CompareTag("Wolf"))
        {
            other.transform.parent.GetComponent<Wolf>().Die(Vector3.zero, 0f, this);
            return;
        }
        if (other != null && other.CompareTag("HitBox"))
        {
            if (GetParent(other.transform).CompareTag("Enemy"))
            {
                if (Random.Range(1, 101) < 5)
                    Kill(GameManager._instance.GetHitBoxIKillable(other), Vector3.zero, 0f, this);
            }
            else if (GetParent(other.transform).CompareTag("Boss"))
            {
                
            }
            else
            {
                Kill(GameManager._instance.GetHitBoxIKillable(other), Vector3.zero, 0f, this);
            }
        }
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
