using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotater : MonoBehaviour
{
    [SerializeField]
    private float RotateSpeed;

    [SerializeField]
    private bool XAxis;
    [SerializeField]
    private bool YAxis;
    [SerializeField]
    private bool ZAxis;

    private void Update()
    {
        transform.Rotate(new Vector3(XAxis ? 1f : 0f, YAxis ? 1f : 0f, ZAxis ? 1f : 0f) * RotateSpeed * Time.deltaTime);
    }
}
