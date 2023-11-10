using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopBleeding : MonoBehaviour
{
    private ParticleSystem[] _particles;
    private void Awake()
    {
        _particles = GetComponentsInChildren<ParticleSystem>();
        GameManager._instance.CallForAction(() =>
        {
            foreach (var particle in _particles)
            {
                particle.Stop();
            }
        }, 2f);
        Destroy(gameObject, 4f);
    }
    private void Update()
    {
        foreach (var particle in _particles)
        {
            var x = particle.main;
            x.startLifetimeMultiplier = Mathf.Lerp(x.startLifetimeMultiplier, 0.15f, Time.deltaTime);
            x.startSpeedMultiplier = Mathf.Lerp(x.startSpeedMultiplier, 4.2f, Time.deltaTime);
        }
    }
}
