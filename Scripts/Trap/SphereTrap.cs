using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereTrap : MonoBehaviour, IKillObject
{
    public GameObject Owner => gameObject;

    public void Kill(IKillable killable, Vector3 dir, float killersVelocityMagnitude, IKillObject killer)
    {
        killable.Die(dir, killersVelocityMagnitude, killer, true);
    }
   
    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.CompareTag("HitBox"))
        {
            Kill(GameManager._instance.GetHitBoxIKillable(other), Vector3.zero, 0f, this);
        }
    }
}
