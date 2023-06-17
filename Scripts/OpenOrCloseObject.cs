using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenOrCloseObject : MonoBehaviour
{
    [SerializeField]
    private GameObject _object;
    [SerializeField]
    private bool _isOpen;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject!=null && other.CompareTag("Player"))
        {
            _object.SetActive(_isOpen);
        }
    }
}
