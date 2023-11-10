using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour, IKillObject
{
    GameObject IKillObject.Owner => gameObject;

    public void Kill(IKillable killable, Vector3 dir, float killersVelocityMagnitude, IKillObject killer)
    {
        killable.Die(dir, killersVelocityMagnitude, killer, true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!GameManager._instance.isGameStopped && !GameManager._instance.isOnCutscene && !GameManager._instance.isPlayerDead)
        {
            if (other != null && other.CompareTag("HitBox"))
            {
                IKillable killable = GameManager._instance.GetHitBoxIKillable(other);
                if (killable != null && !killable.IsDead && !killable.IsDodgingGetter)
                {
                    Kill(killable, Vector3.up, 0f, this);
                }
            }
        }
    }
}
