using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.gameObject.layer == LayerMask.NameToLayer("Killable") && transform.parent != null && transform.parent.GetComponentInChildren<ITrap>() != null)
        {
            transform.parent.GetComponentInChildren<ITrap>()._CheckForActivate = true;
        }

        if (other != null && GameManager._instance.GetParent(other.transform).CompareTag("Player"))
        {
            SoundManager._instance.PlaySound(SoundManager._instance.HookReady, GameManager._instance.GetParent(other.transform).position, Mathf.Clamp(0.8f - (GameManager._instance.GetParent(other.transform).position - transform.position).magnitude / 30f, 0.2f, 0.8f), false, UnityEngine.Random.Range(0.55f, 0.65f));
        }
    }
    
}
