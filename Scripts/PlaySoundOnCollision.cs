using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnCollision : MonoBehaviour
{
    public Material _normalMaterial;
    public Material _dissolveMaterial;
    public AudioClip _soundClip;
    public bool _isEnabled;

    private Rigidbody _rb;
    private float _collisionSpeed;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        if (_rb != null)
            _collisionSpeed = _rb.velocity.magnitude;
    }
    private void Update()
    {
        if (_isEnabled && _rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == null) return;

        if (_isEnabled && _soundClip != null && (_collisionSpeed > 2f || collision.collider.CompareTag("Player") || collision.collider.CompareTag("Enemy") || collision.collider.CompareTag("Boss")))
            SoundManager._instance.PlaySound(_soundClip, transform.position, 0.1f, false, Random.Range(0.85f, 1.15f));
    }
}
