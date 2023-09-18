using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != null && other.CompareTag("Player"))
        GameManager._instance.IsFollowPlayerTriggered = true;
    }
}
