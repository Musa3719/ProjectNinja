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
    public void TransformPositionForCutscene()
    {
        Transform _targetTransform = transform.Find("Armature").Find("RL_BoneRoot").Find("CC_Base_Hip");
        SkinnedMeshRenderer[] skinnedMeshRenderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        Vector3 distance = _targetTransform.position - transform.position;
        distance.y = 0f;
        transform.parent.position += distance;
        _targetTransform.position -= distance;
    }
}
