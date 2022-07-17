using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private GameObject AudioSourcePrefab;

    private void Awake()
    {
        int sceneNumber = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        switch (sceneNumber)
        {
            case 0:
                PlayMenuMusic();
                break;
            case 1:
                PlayLevelMusic_1();
                break;
            default:
                break;
        }
    }
    public void PlayMenuMusic()
    {

    }
    public void PlayLevelMusic_1()
    {

    }


    public void PlaySoundAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        AudioSource.PlayClipAtPoint(clip, position, Options._instance.SoundVolume * volume);
    }
    public void PlaySound(AudioClip clip, Vector3 position, float volume = 1f, bool isLooping = false, float pitch = 1f)
    {
        AudioSource audioSource = Instantiate(AudioSourcePrefab, position, Quaternion.identity).GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = Options._instance.SoundVolume * volume;
        audioSource.loop = isLooping;
        audioSource.pitch = pitch;
        audioSource.Play();
    }
}
