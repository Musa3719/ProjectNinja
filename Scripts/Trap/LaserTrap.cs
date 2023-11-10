using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTrap : MonoBehaviour
{
    private GameObject _laser;
    private GameObject _laserWarning;

    private float _waitForReActivateTime;
    private float _lastTimeActivated;

    private GameObject _sound;

    private Coroutine _soundPitchCoroutine;
    private void Awake()
    {
        _waitForReActivateTime = Random.Range(3f, 11f);
        _laser = transform.Find("Laser").gameObject;
        _laserWarning = transform.Find("LaserWarning").gameObject;
    }
    void Update()
    {
        if (_lastTimeActivated + _waitForReActivateTime < Time.time)
        {
            StartWarning();
        }
    }
    public void StartWarning()
    {
        _lastTimeActivated = Time.time;
        _waitForReActivateTime = Random.Range(9f, 18f);

        if (_soundPitchCoroutine != null)
            StopCoroutine(_soundPitchCoroutine);
        _soundPitchCoroutine = StartCoroutine(SoundPitchCoroutine(true));

        _laserWarning.SetActive(true);
        GameManager._instance.CallForAction(() => ActivateLaser(), 1.25f);
    }
    private void ActivateLaser()
    {
        _laserWarning.GetComponent<TubeRenderer>().enabled = false;
        _laser.SetActive(true);

        GameManager._instance.CallForAction(() => {
            if (_soundPitchCoroutine != null)
                StopCoroutine(_soundPitchCoroutine);
            _soundPitchCoroutine = StartCoroutine(SoundPitchCoroutine(false));
            _laser.SetActive(false);
            _laserWarning.GetComponent<TubeRenderer>().enabled = true;
            _laserWarning.SetActive(false);
        }, 4f);
    }
    private IEnumerator SoundPitchCoroutine(bool isCreating)
    {
        if (isCreating)
            _sound = SoundManager._instance.PlaySound(SoundManager._instance.Laser, transform.position, 0.8f, true, 0f);

        float startTime = Time.time;
        while (startTime + 1.5f > Time.time)
        {
            if (isCreating)
                _sound.GetComponent<AudioSource>().pitch = Mathf.Lerp(_sound.GetComponent<AudioSource>().pitch, 0.9f, Time.deltaTime * 3f);
            else
                _sound.GetComponent<AudioSource>().pitch = Mathf.Lerp(_sound.GetComponent<AudioSource>().pitch, 0f, Time.deltaTime * 10f);

            yield return null;
        }

        if(!isCreating)
            Destroy(_sound);
    }
}