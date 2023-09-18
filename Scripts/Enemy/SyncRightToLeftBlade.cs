using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncRightToLeftBlade : MonoBehaviour
{
    [SerializeField] private bool isWarning;
    private EnemyCombat _enemyCombat;
    private void Awake()
    {
        _enemyCombat = GetParent(transform).GetComponent<EnemyCombat>();
    }
    private void OnEnable()
    {
        if (isWarning)
            _enemyCombat._leftBladeAttackWarning.gameObject.SetActive(true);
        else
            _enemyCombat._leftBladeAttackCollider.gameObject.SetActive(true);
    }
    private void OnDisable()
    {
        if (isWarning)
            _enemyCombat._leftBladeAttackWarning.gameObject.SetActive(false);
        else
            _enemyCombat._leftBladeAttackCollider.gameObject.SetActive(false);
    }
    private Transform GetParent(Transform getParent)
    {
        while (getParent.parent != null)
            getParent = getParent.parent;
        return getParent;
    }
}
