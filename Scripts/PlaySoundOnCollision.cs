using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnCollision : MonoBehaviour
{
    public Material _normalMaterial;
    public Material _dissolveMaterial;
    public AudioClip _soundClip;
    public bool _isEnabled;
    public bool _isDoor;
    private GameObject _doorSoundObj;
    private float _doorSoundCounter;
    private bool _isInDoorCoroutine;

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
        if (_doorSoundObj != null && _rb != null)
        {
            _doorSoundObj.transform.position = transform.position;

            _doorSoundObj.GetComponent<AudioSource>().pitch = Mathf.Lerp(_doorSoundObj.GetComponent<AudioSource>().pitch, Mathf.Clamp(_rb.velocity.magnitude * 2f, 0.7f, 1.5f), Time.deltaTime);

            if (_rb.velocity.magnitude < 0.1f && !_isInDoorCoroutine) _doorSoundCounter += Time.deltaTime;
            if (_doorSoundCounter > 0.2f && !_isInDoorCoroutine)
            {
                StartCoroutine(DoorSoundDestroyCoroutine());
            }
        }
    }
    private IEnumerator DoorSoundDestroyCoroutine()
    {
        _isInDoorCoroutine = true;
        _doorSoundCounter = 0f;
        float startTime = Time.time;
        AudioSource source = _doorSoundObj.GetComponent<AudioSource>();
        while (startTime + 2f > Time.time)
        {
            source.volume = Mathf.Lerp(source.volume, 0f, Time.deltaTime * 4f);
            yield return null;
        }
        Destroy(_doorSoundObj);
        _isInDoorCoroutine = false;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == null) return;

        float volume = 0.1f;
        float pitch = Random.Range(0.85f, 1.15f);

        if (_isDoor)
        {
            if (_doorSoundObj == null)
                _doorSoundObj = SoundManager._instance.PlaySound(SoundManager._instance.DoorSound, transform.position, 0.1f, true, 1f);

            pitch = Random.Range(0.8f, 1f);
            volume = 0.18f;
            _soundClip = SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.DoorHitSounds);
        }

        if (_isEnabled && _soundClip != null && (_collisionSpeed > 2f || _isDoor || collision.collider.CompareTag("Player") || collision.collider.CompareTag("Enemy") || collision.collider.CompareTag("Boss")))
            SoundManager._instance.PlaySound(_soundClip, transform.position, volume, false, pitch);
    }
}
