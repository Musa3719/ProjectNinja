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
    public GameObject CurrentAtmosphereObject { get; private set; }
    private Coroutine StopCurrentMusicCoroutine;
    private Coroutine StopCurrentAtmosphereCoroutine;


    #region Sounds
    [SerializeField]
    public List<AudioClip> WolfBarks;
    [SerializeField]
    public List<AudioClip> WolfAttacks;
    [SerializeField]
    public AudioClip Laser;
    [SerializeField]
    public AudioClip DeathByProjectile;
    [SerializeField]
    public AudioClip TextSound;
    [SerializeField]
    public AudioClip TimeSlowEnter;
    [SerializeField]
    public AudioClip TimeSlowExit;
    [SerializeField]
    public AudioClip Button;
    [SerializeField]
    public AudioClip ButtonHover;
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
    public AudioClip LightFlicker;
    [SerializeField]
    public AudioClip TutorialText;
    [SerializeField]
    public AudioClip DoorKeyUsed;

    [SerializeField]
    public List<AudioClip> Cuts;
    [SerializeField]
    public List<AudioClip> Crushs;
    [SerializeField]
    public List<AudioClip> Blocks;
    [SerializeField]
    public List<AudioClip> Deflects;
    [SerializeField]
    public List<AudioClip> LightAttacks;
    [SerializeField]
    public List<AudioClip> HeavyAttacks;
    [SerializeField]
    public List<AudioClip> AttackDeflecteds;
    [SerializeField]
    public List<AudioClip> EnemyDeathSounds;

    [SerializeField]
    public List<AudioClip> ArmorHitSounds;
    [SerializeField]
    public List<AudioClip> WeaponHitSounds;

    [SerializeField]
    public List<AudioClip> DoorHitSounds;
    [SerializeField]
    public AudioClip DoorSound;

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

    [SerializeField]
    private List<AudioClip> AtmosphereSounds;

    public AudioClip Boss1Enter;
    public AudioClip Boss2Enter;
    public AudioClip Boss3Enter;

    #region Musics

    [SerializeField]
    private AudioClip MenuMusic;

    [SerializeField]
    private List<AudioClip> LevelMusics;

    [SerializeField]
    private List<AudioClip> BossMusics;

    [SerializeField]
    public List<AudioClip> Boss1Grunts;
    [SerializeField]
    public List<AudioClip> Boss2GruntsPhase1;
    [SerializeField]
    public List<AudioClip> Boss2GruntsPhase2;

    [SerializeField]
    public List<AudioClip> Boss3GunShots;
    [SerializeField]
    public List<AudioClip> Boss3GunPowerUpShots;
    [SerializeField]
    private AudioClip Boss3Reload;

    private AudioClip Boss2Phase2Music;
    private AudioClip Boss3Phase2Music;

    [SerializeField]
    private AudioClip EndingMusic;

    #endregion

    
    private void Awake()
    {
        _instance = this;
        int sceneNumber = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;


        CurrentMusicObject = GameObject.Find("MusicObject");
        CurrentAtmosphereObject = GameObject.Find("AtmosphereObject");

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "DemoEnded") return;

        if(sceneNumber == 0)
        {
            PlayMusic(MenuMusic);
            return;
        }

        if (IsBossLevel(sceneNumber))
        {
            StartCoroutine(PlayBossMusic(GetBossNumberFromLevel(sceneNumber)));
        }
        else if (sceneNumber < GameManager.Level2Index)
        {
            PlayMusic(LevelMusics[0], AtmosphereSounds[0]);
        }
        else if (sceneNumber < GameManager.Level3Index)
        {
            PlayMusic(LevelMusics[1], AtmosphereSounds[1]);
        }
        else if(sceneNumber == GameManager.LastLevelIndex)
        {
            PlayMusic(EndingMusic);
        }
        else
        {
            PlayMusic(LevelMusics[2], AtmosphereSounds[2]);
        }
    }
    private void LateUpdate()
    {
        if (SoundManager._instance.CurrentMusicObject != null && GameManager._instance != null)
        {
            CurrentMusicObject.transform.position = GameManager._instance.MainCamera.transform.position;
        }
        if (SoundManager._instance.CurrentAtmosphereObject != null && GameManager._instance != null)
        {
            CurrentAtmosphereObject.transform.position = GameManager._instance.MainCamera.transform.position;
        }
    }
    private bool IsBossLevel(int sceneNumber)
    {
        return sceneNumber == GameManager.Boss1LevelIndex || sceneNumber == GameManager.Boss2LevelIndex || sceneNumber == GameManager.Boss3LevelIndex;
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
            default:
                return GameManager.Boss1LevelIndex;
        }
    }
    public AudioClip GetAttackSound(Collider attackCollider)
    {
        if (attackCollider.transform.parent != null && attackCollider.transform.parent.GetComponent<MeleeWeaponForPlayer>() != null && attackCollider.transform.parent.GetComponent<MeleeWeaponForPlayer>().IsHardHit())
            return GetRandomSoundFromList(HeavyAttacks);
        else
            return GetRandomSoundFromList(LightAttacks);
    }
    public AudioClip GetAttackSoundForPlayer(GameObject meleeWeapon)
    {
        if (meleeWeapon==null || !meleeWeapon.GetComponent<MeleeWeaponForPlayer>().IsHardHit())
            return GetRandomSoundFromList(LightAttacks);
        else
            return GetRandomSoundFromList(HeavyAttacks);
    }
    /// <summary>
    /// Arranges Movement Sounds.
    /// </summary>
    public void PlayPlaneOrWallSound(PlaneSoundType type, float speed, float pitchMultiplier = 1f, float volumeMultiplier = 0.75f)
    {
        if (Camera.main == null) return;

        float pitchFromSpeed = speed / 45f + 0.8f;
        pitchFromSpeed *= pitchMultiplier;
        switch (type)
        {
            case PlaneSoundType.Metal:
                AudioClip clip = GetRandomSoundFromList(MetalPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.07f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.TechMetal:
                clip = GetRandomSoundFromList(TechMetalPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.07f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.ThinMetal:
                clip = GetRandomSoundFromList(ThinMetalPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.07f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Glass:
                clip = GetRandomSoundFromList(GlassPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.07f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Marble:
                clip = GetRandomSoundFromList(MarblePlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.07f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Brick:
                clip = GetRandomSoundFromList(BrickPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.07f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Stone:
                clip = GetRandomSoundFromList(StonePlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.07f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Dirt:
                clip = GetRandomSoundFromList(DirtPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.07f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Grass:
                clip = GetRandomSoundFromList(GrassPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.07f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Wood:
                clip = GetRandomSoundFromList(WoodPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.07f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Fabric:
                clip = GetRandomSoundFromList(FabricPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.07f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Water:
                clip = GetRandomSoundFromList(WaterPlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.07f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
                break;
            case PlaneSoundType.Ice:
                clip = GetRandomSoundFromList(IcePlaneSounds);
                PlaySound(clip, Camera.main.transform.position - Vector3.up * 2f, 0.07f * volumeMultiplier, false, pitchFromSpeed + UnityEngine.Random.Range(-0.1f, 0.1f));
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
    public IEnumerator PlayBossMusic(int bossNumber)
    {
        yield return new WaitForSeconds(0.25f);
        yield return new WaitWhile(() => GameManager._instance.isOnCutscene);
        PlayMusic(BossMusics[bossNumber - 1]);
    }
    public void PlayMusic(AudioClip clip, AudioClip atmosphere = null)
    {
        if (atmosphere != null)
        {
            if (CurrentAtmosphereObject == null)
            {
                PlayAtmosphereArrangement(atmosphere);
            }
            else
            {
                if (StopCurrentAtmosphereCoroutine != null)
                    StopCoroutine(StopCurrentAtmosphereCoroutine);
                StopCurrentAtmosphereCoroutine = StartCoroutine(StopCurrentAtmosphereAndPlay(atmosphere));
            }
        }

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
    private IEnumerator StopCurrentAtmosphereAndPlay(AudioClip clip)
    {
        AudioSource source = CurrentAtmosphereObject.GetComponent<AudioSource>();

        if (clip.name == source.clip.name) yield break;

        while (source.volume > 0.05f)
        {
            source.volume -= Time.deltaTime * 3f;
            yield return null;
        }
        Destroy(CurrentAtmosphereObject);

        PlayAtmosphereArrangement(clip);
    }
    /// <summary>
    /// Stops current music AND plays the next.
    /// </summary>
    private IEnumerator StopCurrentMusicAndPlay(AudioClip clip)
    {
        AudioSource source = CurrentMusicObject.GetComponent<AudioSource>();

        if (clip.name == source.clip.name) yield break;

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
        float volume = 0.25f;
        if (clip.name == "MenuForDemo") volume = 0.35f;
        if (clip.name == "SongForDemo") volume = 0.35f;
        CurrentMusicObject = PlaySound(clip, Vector3.zero, volume, true, 1f, true, false);
        if (CurrentMusicObject != null)
        {
            CurrentMusicObject.name = "MusicObject";
            DontDestroyOnLoad(CurrentMusicObject);
        }
    }

    private void PlayAtmosphereArrangement(AudioClip clip)
    {
        CurrentAtmosphereObject = PlaySound(clip, Vector3.zero, 0.045f, true, 1f, false, true);
        if (CurrentAtmosphereObject != null)
        {
            CurrentAtmosphereObject.name = "AtmosphereObject";
            DontDestroyOnLoad(CurrentAtmosphereObject);
        }
    }

    public GameObject PlayBossEnterSound(AudioClip clip)
    {
        return PlaySound(clip, GameManager._instance.MainCutsceneCamera.transform.position, 0.5f);
    }

    /// <summary>
    /// Play Sound by creating a AudioSourcePrefab Object.
    /// </summary>
    public GameObject PlaySound(AudioClip clip, Vector3 position, float volume = 1f, bool isLooping = false, float pitch = 1f, bool isMusic = false, bool isAtmosphere = false, bool isMachine = false)
    {
        if (clip == null) return null;

        AudioSource audioSource = Instantiate(AudioSourcePrefab, position, Quaternion.identity).GetComponent<AudioSource>();
        audioSource.clip = clip;
        if(isMusic)
            audioSource.volume = Options._instance.MusicVolume * volume;
        else
            audioSource.volume = Options._instance.SoundVolume * volume;
        audioSource.transform.localEulerAngles = new Vector3(volume, 0f, 0f);
        audioSource.loop = isLooping;
        audioSource.pitch = pitch;
        audioSource.Play();
        if (!isMusic && !isAtmosphere)
            audioSource.gameObject.transform.SetParent(SoundObjectsParent.transform);
        if (isMusic || isAtmosphere)
            audioSource.spatialBlend = 0f;
        if (isMachine)
            audioSource.gameObject.AddComponent<AudioCheckWalls>();

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
        if (CurrentAtmosphereObject != null)
            CurrentAtmosphereObject.GetComponent<AudioSource>().Pause();
    }
    public void ContinueMusic()
    {
        if (CurrentMusicObject != null)
            CurrentMusicObject.GetComponent<AudioSource>().UnPause();
        if (CurrentAtmosphereObject != null)
            CurrentAtmosphereObject.GetComponent<AudioSource>().UnPause();
    }

    public void SlowDownMusic()
    {
        if (CurrentMusicObject != null)
            CurrentMusicObject.GetComponent<AudioSource>().pitch = 0.5f;
        if (CurrentAtmosphereObject != null)
            CurrentAtmosphereObject.GetComponent<AudioSource>().pitch = 0.5f;
    }
    public void UnSlowDownMusic()
    {
        if (CurrentMusicObject != null)
            CurrentMusicObject.GetComponent<AudioSource>().pitch = 1f;
        if (CurrentAtmosphereObject != null)
            CurrentAtmosphereObject.GetComponent<AudioSource>().pitch = 1f;
    }

    public void SlowDownAllSound()
    {
        foreach (Transform sound in SoundObjectsParent.transform)
        {
            sound.GetComponent<AudioSource>().pitch = 0.85f;
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