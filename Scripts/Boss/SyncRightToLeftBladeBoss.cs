using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncRightToLeftBladeBoss : MonoBehaviour
{
    [SerializeField] private bool isWarning;
    private BossCombat _bossCombat;
    private void Awake()
    {
        _bossCombat = GetParent(transform).GetComponent<BossCombat>();
    }
    private void OnEnable()
    {
        if (isWarning)
            _bossCombat._leftBladeAttackWarning.gameObject.SetActive(true);
        else
            _bossCombat._leftBladeAttackCollider.gameObject.SetActive(true);
    }
    private void OnDisable()
    {
        if (isWarning)
            _bossCombat._leftBladeAttackWarning.gameObject.SetActive(false);
        else
            _bossCombat._leftBladeAttackCollider.gameObject.SetActive(false);
    }
    private Transform GetParent(Transform getParent)
    {
        while (getParent.parent != null)
            getParent = getParent.parent;
        return getParent;
    }
}
