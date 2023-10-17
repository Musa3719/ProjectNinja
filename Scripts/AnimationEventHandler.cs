using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public void OpenAttackCollider()
    {
        GetParent().GetComponent<IKillable>().OpenAttackCollider();
    }
    public void CloseAttackCollider()
    {
        GetParent().GetComponent<IKillable>().CloseAttackCollider();
    }

    private Transform GetParent()
    {
        Transform parent = transform;
        while (parent.parent != null)
            parent = parent.parent;
        return parent;
    }
}
