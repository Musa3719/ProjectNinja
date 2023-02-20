using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager _instance;

    [SerializeField]
    private GameObject AudioSourcePrefab;
    [SerializeField]
    public GameObject SoundObjectsParent;
    public static Action<Vector3, float> ProjectileTriggeredSoundArtificial;

    public GameObject CurrentMusicObject { get; private set; }
    private Coroutine StopCurrentMusicCoroutine;


    #region Sounds
    [SerializeField]
    public AudioClip Jump;
    [SerializeField]
    public AudioClip Sliding;
    [SerializeField]
    public AudioClip BladeSpin;
    [SerializeField]
    public AudioClip BladeSlide;
    [SerializeField]
    public AudioClip StaminaReload;
    [SerializeField]
    public AudioClip SmallCrash;
    [SerializeField]
    public AudioClip ThrowHook;
    [SerializeField]
    public AudioClip ProjectileMoving;
    [SerializeField]
    public AudioClip Spell1;
    [SerializeField]
    public AudioClip Spell2;
    [SerializeField]
    public AudioClip Darken;
    [SerializeField]
    public AudioClip WeaponStickGround;
    [SerializeField]
    public AudioClip HookNotReady;
    [SerializeField]
    public AudioClip HookReady;
    [SerializeField]
    public AudioClip MachineWorking1;
    [SerializeField]
    public AudioClip MachineWorking2;
    [SerializeField]
    public AudioClip MachineWorking3;
    [SerializeField]
    public AudioClip BowFired;
    [SerializeField]
    public AudioClip GunFired;
    [SerializeField]
    public AudioClip Stab;
    [SerializeField]
    public List<AudioClip> Cuts;//Sword etc
    [SerializeField]
    public AudioClip Throw;
    [SerializeField]
    public AudioClip StoneHit;
    [SerializeField]
    public AudioClip GlassBroken;
    [SerializeField]
    public AudioClip Die;
    [SerializeField]
    public AudioClip Born;
    [SerializeField]
    public AudioClip SmokeExplode;
    [SerializeField]
    public AudioClip BombExplode;
    [SerializeField]
    public AudioClip ReadyForExplosion;
    [SerializeField]
    public AudioClip HitWallWithWeapon;
    [SerializeField]
    public AudioClip RoomPassed;

    [SerializeField]
    public List<AudioClip> Blocks;
    [SerializeField]
    public List<AudioClip> Deflects;
    [SerializeField]
    public List<AudioClip> Attacks;
    [SerializeField]
    public List<AudioClip> AttackDeflecteds;
    [SerializeField]
    public List<AudioClip> PlayerAttacks;

    [SerializeField]
    public List<AudioClip> EnemyGrumbles;//different for each level
    [SerializeField]
    public List<AudioClip> BossGrumbles;//different for each level

    [SerializeField]
    public List<AudioClip> ArmorHitSounds;
    [SerializeField]
    public List<AudioClip> WeaponHitSounds;

    [SerializeField]
    public List<AudioClip> ArmorWalkSounds;

    [SerializeField]
    public List<AudioClip> MetalPlaneSounds;
    [SerializeField]
    public List<AudioClip> TechMetalPlaneSounds;
    [SerializeField]
    public List<AudioClip> ThinMetalPlaneSounds;
    [SerializeField]
    public List<AudioClip> GlassPlaneSounds;
    [SerializeField]
    public List<AudioClip> MarblePlaneSounds;
    [SerializeField]
    public List<AudioClip> BrickPlaneSounds;
    [SerializeField]
    public List<AudioClip> StonePlaneSounds;
    [SerializeField]
    public List<AudioClip> DirtPlaneSounds;
    [SerializeField]
    public List<AudioClip> GrassPlaneSounds;
    [SerializeField]
    public List<AudioClip> WoodPlaneSounds;
    [SerializeField]
    public List<AudioClip> FabricPlaneSounds;
    [SerializeField]
    public List<AudioClip> WaterPlaneSounds;
    [SerializeField]
    public List<AudioClip> IcePlaneSounds;

    #endregion

    #region Musics

    [SerializeField]
    private AudioClip MenuMusic;

    [SerializeField]
    private List<AudioClip> LevelMusics;

    [SerializeField]
    private List<AudioClip> BossMusics;

    [SerializeField]
    private AudioClip EndingMusic;

    #endregion

    private bool IsBossLevel(int sceneNumber)
    {
        return sceneNumber == GameManager.Boss1LevelIndex || sceneNumber == GameManager.Boss2LevelIndex || sceneNumber == GameManager.Boss3LevelIndex || sceneNumber == GameManager.Boss4LevelIndex || sceneNumber == GameManager.Boss5LevelIndex || sceneNumber == GameManager.Boss6LevelIndex;
    }
    private int GetBossNumberFromLevel(int sceneNumber)
    {
        switch (sceneNumber)
        {
            case GameManager.Boss1LevelIndex:
                return 1;
            case GameManager.Boss2LevelIndex:
                return 2;
            case GameManager.Boss3LevelIndex:
                return 3;
            case GameManager.Boss4LevelIndex:
                return 4;
            case GameManager.Boss5LevelIndex:
                return 5;
            case GameManager.Boss6LevelIndex:
                return 6;
            default:
                return GameManager.Boss1LevelIndex;
        }
    }
    private void Awake()
    {
        _instance = this;
        int sceneNumber = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

        if(sceneNumber == 0)
        {
            PlayMusic(MenuMusic);
            return;
        }

        if (IsBossLevel(sceneNumber) && CurrentMusicObject == null)
        {
            PlayBossMusic(GetBossNumberFromLevel(sceneNumber));
            return;
        }

        if (sceneNumber < GameManager.Level2Index)
        {
            PlayMusic(LevelMusics[0]);
        }
        else if (sceneNumber < GameManager.Level3Index)
        {
            PlayMusic(LevelMusics[1]);
        }
        else if (sceneNumber < GameManager.Level4Index)
        {
            PlayMusic(LevelMusics[2]);
        }
        else if (sceneNumber < GameManager.Level5Index)
        {
            PlayMusic(LevelMusics[3]);
        }
        else if(sceneNumber == GameManager.LastLevelIndex)
        {
            PlayMusic(EndingMusic);
        }
        else
        {
            PlayMusic(LevelMusics[4]);
        }
    }
    /// <summary>
    /// Arranges Movement Sounds.
    /// </summary>
    public void PlayPlaneOrWallSound(PlaneSoundType type, float speed, float pitchMultiplier = 1f, float volumeMultiplier = 1f)
    {
        float pitchFromSpeed = speed / 45f + 0.8f;
        pitchFromSpeed *= pitchMultiplier;

        switch (type)
        {
            case PlaneSoundType.Metal:
                AudioClip clip = GetRandomSoundFromList(MetalPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.1f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.TechMetal:
                clip = GetRandomSoundFromList(TechMetalPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.1f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.ThinMetal:
                clip = GetRandomSoundFromList(ThinMetalPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.1f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Glass:
                clip = GetRandomSoundFromList(GlassPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.1f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Marble:
                clip = GetRandomSoundFromList(MarblePlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.1f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Brick:
                clip = GetRandomSoundFromList(BrickPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.1f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Stone:
                clip = GetRandomSoundFromList(StonePlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.1f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Dirt:
                clip = GetRandomSoundFromList(DirtPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.1f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Grass:
                clip = GetRandomSoundFromList(GrassPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.1f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Wood:
                clip = GetRandomSoundFromList(WoodPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.1f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Fabric:
                clip = GetRandomSoundFromList(FabricPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.1f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Water:
                clip = GetRandomSoundFromList(WaterPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.1f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Ice:
                clip = GetRandomSoundFromList(IcePlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.1f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            default:
                break;
        }
    }

    public AudioClip GetRandomSoundFromList(List<AudioClip> list)
    {
        if (list == null || list.Count == 0) return null;

        return list[UnityEngine.Random.Range(0, list.Count)];
    }


    public void PlayBossMusicPhaseChanged(AudioClip clip)
    {
        PlayMusic(clip);
    }

    /// <summary>
    /// BossNumber Starts From 1
    /// </summary>
    /// <param name="bossNumber"></param>
    public void PlayBossMusic(int bossNumber)
    {
        PlayMusic(BossMusics[bossNumber - 1]);
    }
    public void PlayMusic(AudioClip clip)
    {
        if (CurrentMusicObject != null)
        {
            if (StopCurrentMusicCoroutine != null)
                StopCoroutine(StopCurrentMusicCoroutine);
            StopCurrentMusicCoroutine = StartCoroutine(StopCurrentMusicAndPlay(clip));
        }
        else
        {
            PlayMusicArrangement(clip);
        }
    }
    /// <summary>
    /// Stops current music AND plays the next.
    /// </summary>
    private IEnumerator StopCurrentMusicAndPlay(AudioClip clip)
    {
        AudioSource source = CurrentMusicObject.GetComponent<AudioSource>();
        while (source.volume > 0.05f)
        {
            source.volume -= Time.deltaTime * 3f;
            yield return null;
        }
        Destroy(CurrentMusicObject);

        PlayMusicArrangement(clip);
    }
    private void PlayMusicArrangement(AudioClip clip)
    {
        CurrentMusicObject = PlaySound(clip, Vector3.zero, 1f, true, 1f);
        if (CurrentMusicObject != null)
            DontDestroyOnLoad(CurrentMusicObject);
    }

    /// <summary>
    /// Play Sound by creating a AudioSourcePrefab Object.
    /// </summary>
    public GameObject PlaySound(AudioClip clip, Vector3 position, float volume = 1f, bool isLooping = false, float pitch = 1f)
    {
        if (clip == null) return null;

        AudioSource audioSource = Instantiate(AudioSourcePrefab, position, Quaternion.identity).GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = Options._instance.SoundVolume * volume;
        audioSource.transform.localEulerAngles = new Vector3(volume, 0f, 0f);
        audioSource.loop = isLooping;
        audioSource.pitch = pitch;
        audioSource.Play();
        audioSource.gameObject.transform.SetParent(SoundObjectsParent.transform);

        if (!isLooping)
            Destroy(audioSource.gameObject, audioSource.clip.length / Mathf.Clamp(pitch, 0.1f, 1f) + 1f);
        return audioSource.gameObject;
    }


    public void PauseAllSound()
    {
        foreach (Transform sound in SoundObjectsParent.transform)
        {
            sound.GetComponent<AudioSource>().Pause();
        }
    }
    public void ContinueAllSound()
    {
        foreach (Transform sound in SoundObjectsParent.transform)
        {
            sound.GetComponent<AudioSource>().UnPause();
        }
    }

    public void PauseMusic()
    {
        if (CurrentMusicObject != null)
            CurrentMusicObject.GetComponent<AudioSource>().Pause();
    }
    public void ContinueMusic()
    {
        if (CurrentMusicObject != null)
            CurrentMusicObject.GetComponent<AudioSource>().UnPause();
    }

    public void SlowDownMusic()
    {
        if (CurrentMusicObject != null)
            CurrentMusicObject.GetComponent<AudioSource>().pitch = 0.82f;
    }
    public void UnSlowDownMusic()
    {
        if (CurrentMusicObject != null)
            CurrentMusicObject.GetComponent<AudioSource>().pitch = 1f;
    }

    public void SlowDownAllSound()
    {
        foreach (Transform sound in SoundObjectsParent.transform)
        {
            sound.GetComponent<AudioSource>().pitch = 0.82f;
        }
    }
    public void UnSlowDownAllSound()
    {
        foreach (Transform sound in SoundObjectsParent.transform)
        {
            sound.GetComponent<AudioSource>().pitch = 1f;
        }
    }
}