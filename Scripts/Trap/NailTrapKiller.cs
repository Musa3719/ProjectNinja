using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NailTrapKiller : MonoBehaviour, IKillObject
{
    public void Kill(IKillable killable, Vector3 dir, float killersVelocityMagnitude)
    {
        killable.Die(dir, killersVelocityMagnitude);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.CompareTag("HitBox"))
        {
            Kill(GameManager._instance.GetHitBoxIKillable(other), Vector3.zero, 0f);
        }
    }
}
