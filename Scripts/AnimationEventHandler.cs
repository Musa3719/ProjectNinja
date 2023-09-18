using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public void OpenAttackCollider()
    {
        transform.parent.GetComponent<IKillable>().OpenAttackCollider();
    }
    public void CloseAttackCollider()
    {
        transform.parent.GetComponent<IKillable>().CloseAttackCollider();
    }
}
