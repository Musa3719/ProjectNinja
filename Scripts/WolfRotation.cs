using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WolfRotation : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    private Vector3 _oldAngles;
    private void Awake()
    {
        _navMeshAgent = transform.parent.GetComponent<NavMeshAgent>();
    }
    private void Update()
    {
        if (!_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh) return;

        Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, 0.5f, GameManager._instance.LayerMaskForVisible);
        if (hit.collider != null)
        {
            _oldAngles = transform.localEulerAngles;
            transform.forward = hit.normal;
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, _oldAngles.y, _oldAngles.z);
        }
    }
}
