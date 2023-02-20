using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXUpdater : MonoBehaviour
{
    void Update()
    {
        transform.forward = -(GameManager._instance.MainCamera.transform.position - transform.position).normalized;
    }
}
