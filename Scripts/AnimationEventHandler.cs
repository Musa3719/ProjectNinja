using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    [SerializeField]
    private bool _isPlayer;
    public void OpenAttackCollider()
    {
        if(_isPlayer)
            GameManager._instance.PlayerRb.GetComponent<IKillable>().OpenAttackCollider();
        else
            GetParent().GetComponent<IKillable>().OpenAttackCollider();
    }
    public void CloseAttackCollider()
    {
        if (_isPlayer)
            GameManager._instance.PlayerRb.GetComponent<IKillable>().CloseAttackCollider();
        else
            GetParent().GetComponent<IKillable>().CloseAttackCollider();
    }

    private Transform GetParent()
    {
        Transform parent = transform;
        while (parent.parent != null)
            parent = parent.parent;
        return parent;
    }
    public void MeleeAttackFinished()
    {
        GetParent().GetComponent<IKillable>().MeleeAttackFinished();
    }
}
