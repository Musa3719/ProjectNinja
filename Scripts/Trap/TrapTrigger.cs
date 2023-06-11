using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.gameObject.layer == LayerMask.NameToLayer("Killable"))
        {
            transform.parent.GetComponentInChildren<ITrap>()._CheckForActivate = true;
        }
    }
}
