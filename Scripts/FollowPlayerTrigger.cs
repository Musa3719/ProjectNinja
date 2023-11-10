using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerTrigger : MonoBehaviour
{
    [SerializeField] private bool _isChasingFromStart;
    private void Awake()
    {
        if (_isChasingFromStart) GameManager._instance.IsFollowPlayerTriggered = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != null && other.CompareTag("Player"))
            GameManager._instance.IsFollowPlayerTriggered = true;
    }
}
