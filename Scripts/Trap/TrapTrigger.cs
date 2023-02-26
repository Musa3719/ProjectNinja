using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.gameObject.layer == LayerMask.NameToLayer("Killable"))
        {
            if(transform.parent.GetComponentInChildren<ITrap>()!=null)
                transform.parent.GetComponentInChildren<ITrap>()._CheckForActivate = true;
            else
                transform.parent.GetComponent<ITrap>()._CheckForActivate = true;
        }
    }
}
