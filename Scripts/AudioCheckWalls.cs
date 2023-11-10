using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCheckWalls : MonoBehaviour
{
    private AudioSource _source;
    private void Awake()
    {
        _source = GetComponent<AudioSource>();
    }
    void Update()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, (GameManager._instance.PlayerRb.transform.position - transform.position).normalized, (GameManager._instance.PlayerRb.transform.position - transform.position).magnitude, GameManager._instance.LayerMaskForVisible);
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Wall"))
            {
                _source.volume = Mathf.Lerp(_source.volume, transform.localEulerAngles.x * Options._instance.SoundVolume * 0.3f, Time.deltaTime * 4f);
                return;
            }
        }
        _source.volume = Mathf.Lerp(_source.volume, transform.localEulerAngles.x * Options._instance.SoundVolume, Time.deltaTime * 4f);
    }
}
