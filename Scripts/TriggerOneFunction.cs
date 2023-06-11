using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerOneFunction : MonoBehaviour
{
    [SerializeField]
    private UnityEvent _event;
    private void OnTriggerEnter(Collider other)
    {
        if(other!=null && other.CompareTag("Player"))
        {
            _event?.Invoke();
        }
    }
}
