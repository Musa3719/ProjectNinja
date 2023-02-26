using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileStopped : MonoBehaviour
{
    private Rigidbody _rb;
    private float _stopCounter;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
            _rb = transform.parent.GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if (_rb.velocity.magnitude < 0.25f && !_rb.isKinematic)
        {
            _stopCounter += Time.deltaTime;
            if (_stopCounter >= 1f)
            {
                _rb.isKinematic = true;
                GetComponent<Collider>().enabled = false;
            }
        }
        else
        {
            _stopCounter = 0f;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (_rb.velocity.magnitude > 2f || (collision.collider.GetComponentInChildren<Rigidbody>() != null && collision.collider.GetComponentInChildren<Rigidbody>().velocity.magnitude > 2f))
            SoundManager._instance.PlaySound(SoundManager._instance.StoneHit, transform.position, 0.05f, false, UnityEngine.Random.Range(0.93f, 1.07f));
    }
}
