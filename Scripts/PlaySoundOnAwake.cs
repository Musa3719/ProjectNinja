using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnAwake : MonoBehaviour
{
    [SerializeField] private bool _isMachine;
    [SerializeField] private AudioClip _clip;
    private void Start()
    {
        float volume = 1f;
        if (_clip.name.StartsWith("Burning")) volume = 0.05f;
        SoundManager._instance.PlaySound(_clip, transform.position, volume, true, Random.Range(0.9f, 1.1f), isMachine: _isMachine);
    }
}
