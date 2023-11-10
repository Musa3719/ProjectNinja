using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System;
using UnityEngine.Animations.Rigging;
using UnityEngine.AI;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    public VolumeProfile NormalSettingsVolume;
    public VolumeProfile LowSettingsVolume;
    public Material CarpetNormalSetting;
    public Material CarpetLowSetting;
    public Animator _Animator;
    public static GameManager _instance;
    [HideInInspector]
    public GameObject MainCamera;
    private GameObject _defaultCamera;

    public Transform PlayerFootPos;
    public Transform PlayerRightHandHolder;

    public bool IsTimeSlowed;

    private Coroutine _slowTimeCoroutine;
    private Coroutine _effectPlayerByDarkCoroutine;
    private Coroutine _playerAttackHandleCoroutine;
    private Coroutine _tutorialTextCoroutine;
    private Coroutine _openInvincibleScreen;

    public bool isGameStopped { get; set; }
    public bool isOnCutscene { get; private set; }
    public bool isPlayerDead { get; private set; }
    public bool isPlayerAttacking { get; private set; }

    public bool IsPlayerOnWall;
    public bool IsPlayerHasMeleeWeapon;
    public bool _isInBossLevel { get; private set; }

    public const int Boss1LevelIndex = 8;
    public const int Boss2LevelIndex = 16;
    public const int Boss3LevelIndex = 24;

    public const int Level1Index = 1;
    public const int Level2Index = 9;
    public const int Level3Index = 17;
    public const int LastLevelIndex = 25;

    public bool isTeleportSkillOpen { get; private set; }
    public bool isIceSkillOpen { get; private set; }
    public bool isInvertedMirrorSkillOpen { get; private set; }
    public float InvertedMirrorFunctionalTime { get; private set; }
    public float TeleportAvailableTimeAfterHologram { get; private set; }
    public GameObject CutsceneCamerasGetter => CutsceneCameras;


    public LayerMask LayerMaskWithoutTriggerColliders;
    public LayerMask LayerMaskForVisible;
    public LayerMask LayerMaskForVisibleWithSolidTransparent;
    public LayerMask MirrorRayLayer;
    public LayerMask WallLayer;

    public Color MidScreenDotMovementColor;
    public Color MidScreenDotOnWallColor;
    public Color ThrowableColor;

    [SerializeField]
    public GameObject TeleportIllusion;
    [SerializeField]
    public GameObject TutorialTextUI;
    [SerializeField]
    public GameObject PlayerHands;
    [SerializeField]
    public GameObject WaitingScreenPrefab;
    [SerializeField]
    public GameObject GlassBrokenPrefab;
    [SerializeField]
    private GameObject WarningUIPrefab;
    [SerializeField]
    private GameObject WarningUIParent;
    public Image MidScreenDot;
    public GameObject BossTextUI;
    public Volume BlurVolume { get; private set; }
    private Volume ChromaticVolume;
    public GameObject NewspaperUI;
    public GameObject MaxSpeedCounterUI;
    private GameObject CutsceneCameras;
    public GameObject MainCutsceneCamera { get; private set; }
    [SerializeField]
    private GameObject DeathScreen;
    [SerializeField]
    private GameObject InvincibleScreen;
    [SerializeField]
    public GameObject StopScreen;
    [SerializeField]
    private GameObject PassCutsceneButton;
    public GameObject InGameScreen;
    [SerializeField]
    private SlicedFilledImage StaminaBar;
    [SerializeField]
    private SlicedFilledImage BossStaminaBar;
    [SerializeField]
    private GameObject BossUI;
    [SerializeField]
    private GameObject DarkenFromBossUI;
    [SerializeField]
    private TextMeshProUGUI SpeedText;
    [SerializeField]
    private TextMeshProUGUI FrameRate;
    [SerializeField]
    private Image CurrentThrowable;
    [SerializeField]
    private Image NextThrowable;
    [SerializeField]
    private Image BeforeThrowable;

    private Queue<int> _frameRates;
    private int _frameRateCountForAvarage;

    private float _clothTimer;
    private float _volumetricCheckTimer;

    private Color _originalStaminaColor;
    public List<GameObject> Projectiles { get; private set; }
    public List<Cloth> Cloths { get; private set; }
    private List<Cloth> _tempListForCloths;

    [HideInInspector]
    public Rigidbody PlayerRb;
    [HideInInspector]
    public float PlayerLastSpeed;
    public Transform PlayerRightHandTransform;
    public Transform PlayerLeftHandTransform;
    public bool IsLeftThrowing { get; set; }
    public float PlayerRunningSpeed { get; set; }

    public GameObject ArrowPrefab;
    public GameObject BulletPrefab;
    public GameObject SmokePrefab;

    private List<HDAdditionalLightData> MixedLights;

    [SerializeField]
    private List<VideoClip> VideosForTutorial;

    [SerializeField]
    private RuntimeAnimatorController Enemy_1_Animator;
    [SerializeField]
    private RuntimeAnimatorController Enemy_2_Animator;
    [SerializeField]
    private RuntimeAnimatorController Enemy_3_Animator;
    [SerializeField]
    private RuntimeAnimatorController Enemy_4_Animator;
    [SerializeField]
    private RuntimeAnimatorController Enemy_5_Animator;
    [SerializeField]
    private RuntimeAnimatorController KatanaAnimator;

    [SerializeField]
    private RuntimeAnimatorController Boss1Animator;
    public RuntimeAnimatorController Boss1AnimatorGetter => Boss1Animator;
    [SerializeField]
    private RuntimeAnimatorController Boss2Animator;
    public RuntimeAnimatorController Boss2AnimatorGetter => Boss2Animator;
    [SerializeField]
    private RuntimeAnimatorController Boss3Animator;
    public RuntimeAnimatorController Boss3AnimatorGetter => Boss3Animator;

    public List<GameObject> allEnemies { get; private set; }
    public List<GameObject> enemiesNearPlayer { get; private set; }
    public List<GameObject> tempListForNearPlayers { get; private set; }

    public Dictionary<string, float> Enemy1AnimNameToSpeed;
    public Dictionary<string, float> Enemy2AnimNameToSpeed;
    public Dictionary<string, float> Enemy3AnimNameToSpeed;
    public Dictionary<string, float> Enemy4AnimNameToSpeed;
    public Dictionary<string, float> Enemy5AnimNameToSpeed;
    public Dictionary<string, float> KatanaAnimNameToSpeed;

    public Dictionary<string, float> Boss1AnimNameToSpeed;
    public Dictionary<string, float> Boss2AnimNameToSpeed;
    public Dictionary<string, float> Boss3AnimNameToSpeed;

    public List<GameObject> BloodDecalPrefabs;
    public GameObject HoleDecal;
    public GameObject DeathVFX;
    public GameObject HitSmokeVFX;
    public GameObject CombatSmokeVFX;
    public GameObject GunFireVFX;
    public GameObject BleedingVFX;
    public List<GameObject> BloodVFX;
    public List<GameObject> ExplosionVFX;
    public List<GameObject> SparksVFX;
    public List<GameObject> ShiningSparksVFX;

    public int _enemy1AttackAnimCount;
    public int _enemy2AttackAnimCount;
    public int _enemy3AttackAnimCount;
    public int _enemy4AttackAnimCount;
    public int _enemy5AttackAnimCount;
    public int _katanaAttackAnimCount;


    private GameObject ToNextSceneObject;
    public float BossArenaGroundYPosition;

    [HideInInspector] public float BladeSpeed;

    [HideInInspector] public float RunSpeedAdditionActiveTime;

    [HideInInspector] public bool TimeStopEndSignal;

    public bool IsPlayerInSmoke { get; set; }
    public bool IsFollowPlayerTriggered { get; set; }

    public GameObject BossPhaseCounterBetweenScenes { get; private set; }
    public GameObject LevelNumberObject { get; private set; }

    private GameObject _bossEnterSound;

    private NextSceneHandler _nextSceneHandler;
    private int _lastTutorialTextNumber;

    public event Action _isGroundedWorkedThisFrameEvent;
    public event Action<float> _staminaGainEvent;
    public event Action<string, float> _playAnimEvent;
    public Action<Vector3> _pushEvent;
    public event Action _bornEvent;
    public event Action _enemyDiedEvent;

    [HideInInspector]public CustomPassVolume CustomPassForHands;

    private GameObject[] _enemies;
    public T GetRandomFromList<T>(List<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
    public static void Shuffle(List<string> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            string value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    /// <param name="speed">1/second</param>
    public float LimitLerpFloat(float startValue, float endValue, float speed)
    {
        return Mathf.Lerp(startValue, endValue, Time.deltaTime * speed * 7f * (endValue - startValue));
    }
    /// <param name="speed">1/second</param>
    public Vector2 LimitLerpVector2(Vector2 startValue, Vector2 endValue, float speed)
    {
        return Vector2.Lerp(startValue, endValue, Time.deltaTime * speed * 7f / (endValue - startValue).magnitude);
    }
    /// <param name="speed">1/second</param>
    public Vector3 LimitLerpVector3(Vector3 startValue, Vector3 endValue, float speed)
    {
        return Vector3.Lerp(startValue, endValue, Time.deltaTime * speed * 7f / (endValue - startValue).magnitude);
    }

    /// <param name="speed">1/second</param>
    public float LinearLerpFloat(float startValue, float endValue, float speed)
    {
        float startTime = Time.time;
        float endTime = startTime + 1 / speed;
        return Mathf.Lerp(startValue, endValue, (Time.time - startTime) / (endTime - startTime));
    }
    /// <param name="speed">1/second</param>
    public Vector2 LinearLerpVector2(Vector2 startValue, Vector2 endValue, float speed)
    {
        float startTime = Time.time;
        float endTime = startTime + 1 / speed;
        return Vector2.Lerp(startValue, endValue, (Time.time - startTime) / (endTime - startTime));
    }
    /// <param name="speed">1/second</param>
    public Vector3 LinearLerpVector3(Vector3 startValue, Vector3 endValue, float speed)
    {
        float startTime = Time.time;
        float endTime = startTime + 1 / speed;
        return Vector3.Lerp(startValue, endValue, (Time.time - startTime) / (endTime - startTime));
    }
    public bool RandomPercentageChance(float percentage)
    {
        return percentage >= UnityEngine.Random.Range(1f, 100f);
    }
    public void CoroutineCall(ref Coroutine coroutine, IEnumerator method, MonoBehaviour script)
    {
        if (coroutine != null)
            script.StopCoroutine(coroutine);
        coroutine = script.StartCoroutine(method);
    }
    public void CallForAction(Action action, float time)
    {
        StartCoroutine(CallForActionCoroutine(action, time));
    }
    private IEnumerator CallForActionCoroutine(Action action, float time)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }
    public Transform GetParent(Transform tr)
    {
        Transform parentTransform = tr.transform;
        while (parentTransform.parent != null)
        {
            parentTransform = parentTransform.parent;
        }
        return parentTransform;
    }
    public IKillable GetHitBoxIKillable(Collider other)
    {
        Transform parentSearch = other.transform;
        while (parentSearch.parent != null)
        {
            parentSearch = parentSearch.parent;
        }

        if (parentSearch.GetComponent<IKillable>() != null)
            return parentSearch.GetComponent<IKillable>();
        return null;
    }
    public GameObject GetHitBoxIKillableObject(Collider other)
    {
        Transform parentSearch = other.transform;
        while (parentSearch.parent != null)
        {
            parentSearch = parentSearch.parent;
        }

        return parentSearch.gameObject;
    }
    private void Awake()
    {
        //Application.targetFrameRate = 10;
        _instance = this;
        _tempListForCloths = new List<Cloth>();
        CutsceneCameras = GameObject.FindGameObjectWithTag("CutsceneCameras");
        MainCutsceneCamera = CutsceneCameras.transform.Find("CutsceneMainCam").gameObject;
        CutsceneCameras.SetActive(false);
        BlurVolume = GameObject.FindGameObjectWithTag("Volume").transform.Find("Blur").GetComponent<Volume>();
        ChromaticVolume = GameObject.FindGameObjectWithTag("Volume").transform.Find("Chromatic").GetComponent<Volume>();
        BlurVolume.enabled = false;
        GameObject[] carpets = GameObject.FindGameObjectsWithTag("Carpet");
        foreach (var item in carpets)
        {
            item.GetComponent<MeshRenderer>().material = CarpetLowSetting;
        }

        MidScreenDotMovementColor = new Color(195f / 255f, 195f / 255f, 195f / 255f, 0.8f);
        MidScreenDotOnWallColor = new Color(255f / 255f, 0f / 255f, 217f / 255f, 204f/255f);

        ThrowableColor = new Color(0.78f, 0.7f, 0.1f, 219f / 255f);

        Projectiles = new List<GameObject>();
        Cloths = new List<Cloth>();

        PlayerFootPos = GameObject.FindGameObjectWithTag("Player").transform.Find("FootPos");

        ArrangePhaseCounter();
        ArrangeLevelNumber();
        ArrangeDicts();
        ArrangeIsInBossRoom();

        _originalStaminaColor = StaminaBar.color;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Application.targetFrameRate = 60;
        _defaultCamera = Camera.main.gameObject;
        MainCamera = _defaultCamera;
        PlayerRb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        enemiesNearPlayer = new List<GameObject>();
        tempListForNearPlayers = new List<GameObject>();
        _frameRates = new Queue<int>();
        _frameRateCountForAvarage = 60;

        InvertedMirrorFunctionalTime = 5f;
        TeleportAvailableTimeAfterHologram = 2f;
        RunSpeedAdditionActiveTime = 2f;

        allEnemies = new List<GameObject>();

        ToNextSceneObject = GameObject.FindGameObjectWithTag("DoorHandler");
        _nextSceneHandler = ToNextSceneObject.GetComponent<NextSceneHandler>();

        SetMixedLights();
        StartCoroutine(RenderShadowsByTimeCoroutine());

        StartCoroutine(ArrangeEnemiesNearPlayerList());
    }
    private void Start()
    {
        CustomPassForHands = Camera.main.transform.parent.parent.GetComponent<CustomPassVolume>();

        if (LevelNumberObject.transform.position.x != SceneManager.GetActiveScene().buildIndex)
        {
            if (GameObject.FindGameObjectWithTag("PhaseCounter") == null || GameObject.FindGameObjectWithTag("PhaseCounter").transform.position.x == 1f)
                EnterCutscene("SceneOpeningCutscene");
            else if (GameObject.FindGameObjectWithTag("PhaseCounter") != null && GameObject.FindGameObjectWithTag("PhaseCounter").transform.position.x != 1f)
                EnterCutscene("BossPhase" + GameObject.FindGameObjectWithTag("PhaseCounter").transform.position.x.ToString() + "Cutscene");
            else
            {
                _bornEvent?.Invoke();
            }
        }
        LevelNumberObject.transform.position = new Vector3(SceneManager.GetActiveScene().buildIndex, 0f, 0f);
    }
    private void Update()
    {
        //if (InputHandler.GetButtonDown("PastLevelDebug")) SceneController._instance.LoadNextSceneAsync();
        _isGroundedWorkedThisFrameEvent?.Invoke();

        if (InputHandler.GetButtonDown("Esc") && !isPlayerDead)
        {
            if (isOnCutscene)
            {
                if (isGameStopped)
                {
                    CloseStopScreen();
                }
                else
                {
                    OpenStopScreen();
                    PassCutsceneButton.SetActive(true);
                }
            }
            else if (TutorialTextUI.activeInHierarchy)
            {
                CloseTutorialVideo();
            }
            else if (_nextSceneHandler._isInInteract)
            {
                //nothing
            }
            else
            {
                if (isGameStopped)
                {
                    CloseStopScreen();
                }
                else
                {
                    OpenStopScreen();
                }
            }
        }

        ClothRender();
        CheckForVolumetrics();
    }
    private void ClothRender()
    {
        _clothTimer += Time.deltaTime;
        if (_clothTimer < 1.25f) return;
        _clothTimer = 0f;

        SortCloths();
        int numberOfCloths = 0;
        switch (Options._instance.Quality)
        {
            case 0:
                numberOfCloths = 1;
                break;
            case 1:
                numberOfCloths = 1;
                break;
            case 2:
                numberOfCloths = 1;
                break;
            default:
                break;
        }
        int i = 0;
        foreach (Cloth cloth in Cloths)
        {
            if (i < numberOfCloths)
            {
                if (!cloth.enabled) cloth.enabled = true;
            }
            else
            {
                if (cloth.enabled) cloth.enabled = false;
            }
                
            i++;
        }
    }
    private void SortCloths()
    {
        foreach (Cloth cloth in Cloths)
            if (cloth == null) Cloths.Remove(cloth);

        _tempListForCloths.Clear();
        int count = Cloths.Count;
        for (int i = 0; i < count; i++)
        {
            float minLenght = 100f;
            Cloth tempCloth = null;
            foreach (Cloth cloth in Cloths)
            {
                float distance = (cloth.transform.position - GameManager._instance.PlayerRb.transform.position).magnitude;
                if (distance < minLenght)
                {
                    tempCloth = cloth;
                    minLenght = distance;
                }
            }
            if (tempCloth != null)
            {
                _tempListForCloths.Add(tempCloth);
                Cloths.Remove(tempCloth);
            }
        }
        Cloths = _tempListForCloths;
    }
    public bool IsProp(Collider other)
    {
        if (other == null) return false;
        Transform temp = other.transform;
        while (temp.parent != null)
        {
            if (temp.CompareTag("Prop") || other.CompareTag("Door")) return true;
            temp = temp.parent;
        }
        return false;
    }
    private IEnumerator RenderShadowsByTimeCoroutine()
    {
        while (true)
        {
            CheckForRenderShadows();
            //CheckForVolumetrics();
            yield return null;
        }
    }
   
    private void CheckForRenderShadows()
    {
        float distanceMultiplier = Options._instance.Quality == 2 ? 0.5f : 1f;
        foreach (HDAdditionalLightData light in MixedLights)
        {
            bool isGoingToRender = false;
            if ((light.transform.position - GameManager._instance.PlayerRb.transform.position).magnitude < 10f * distanceMultiplier && Time.realtimeSinceStartup - light.GetComponent<FloatHolder>().Value >= 0.03f)
                isGoingToRender = true;
            else if ((light.transform.position - GameManager._instance.PlayerRb.transform.position).magnitude < 15f * distanceMultiplier && Time.realtimeSinceStartup - light.GetComponent<FloatHolder>().Value >= 0.08f)
                isGoingToRender = true;
            else if ((light.transform.position - GameManager._instance.PlayerRb.transform.position).magnitude < 20f * distanceMultiplier && Time.realtimeSinceStartup - light.GetComponent<FloatHolder>().Value >= 0.15f)
                isGoingToRender = true;
            else if ((light.transform.position - GameManager._instance.PlayerRb.transform.position).magnitude < 30f * distanceMultiplier && Time.realtimeSinceStartup - light.GetComponent<FloatHolder>().Value >= 0.25f)
                isGoingToRender = true;
            else if (Time.realtimeSinceStartup - light.GetComponent<FloatHolder>().Value >= 2f)
                isGoingToRender = false;

            if (isGoingToRender)
            {
                light.GetComponent<FloatHolder>().Value = Time.realtimeSinceStartup;
                light.RequestShadowMapRendering();
            }
            
        }
    }
    private void CheckForVolumetrics()
    {
        if (Options._instance.Quality == 2) return;

        if (_volumetricCheckTimer < 0.25f)
        {
            _volumetricCheckTimer += Time.deltaTime;
            return;
        }
        _volumetricCheckTimer = 0f;

        foreach (HDAdditionalLightData light in MixedLights)
        {
            if ((light.transform.position - GameManager._instance.PlayerRb.transform.position).magnitude < 15f)
            {
                if (light.GetComponentInChildren<HDAdditionalLightData>().affectsVolumetric == false)
                    light.GetComponentInChildren<HDAdditionalLightData>().affectsVolumetric = true;
            }
            else
            {
                if (light.GetComponentInChildren<HDAdditionalLightData>().affectsVolumetric == true)
                    light.GetComponentInChildren<HDAdditionalLightData>().affectsVolumetric = false;
            }
            
        }
    }
    private void SetMixedLights()
    {
        if (MixedLights != null) return;

        MixedLights = new List<HDAdditionalLightData>();
        Light[] lights = GameObject.Find("Level").transform.Find("Lights").GetComponentsInChildren<Light>();
        foreach (Light light in lights)
        {
            if (light.GetComponent<HDAdditionalLightData>() != null && light.GetComponent<FloatHolder>() != null)
            {
                MixedLights.Add(light.GetComponent<HDAdditionalLightData>());
            }
        }
    }
    
    public void PlayerGainStamina(float additionStamina)
    {
        _staminaGainEvent?.Invoke(additionStamina);
    }
    /// <param name="time">time for anim state waiting</param>
    public void PlayerPlayAnim(string name, float time)
    {
        _playAnimEvent?.Invoke(name, time);
    }
    private void ArrangeDicts()
    {
        Enemy1AnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 0.6f, ["Attack2"] = 1.1f, ["Attack3"] = 1.2f, ["Attack4"] = 0.85f, ["Attack5"] = 1f, ["Attack6"] = 0.85f };
        Enemy2AnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 0.6f, ["Attack2"] = 1.1f, ["Attack3"] = 1.2f, ["Attack4"] = 0.85f, ["Attack5"] = 1f, ["Attack6"] = 0.85f };
        Enemy3AnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 0.6f, ["Attack2"] = 1.1f, ["Attack3"] = 1.2f, ["Attack4"] = 0.85f, ["Attack5"] = 1f, ["Attack6"] = 0.85f };
        Enemy4AnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 1.35f, ["Attack2"] = 1.15f, ["Attack3"] = 1f, ["Attack4"] = 0.75f, ["Attack5"] = 0.75f, ["Attack6"] = 0.75f };
        Enemy5AnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 1.35f, ["Attack2"] = 1.15f, ["Attack3"] = 1f, ["Attack4"] = 0.75f, ["Attack5"] = 0.75f, ["Attack6"] = 0.75f };
        KatanaAnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 0.65f * 4.5f, ["Attack2"] = 0.65f * 4.5f, ["Attack3"] = 0.65f * 4.5f, ["Attack4"] = 0.65f * 4.5f, ["Attack5"] = 0.65f * 4.5f, ["Attack6"] = 0.65f * 4.5f };

        Boss1AnimNameToSpeed = new Dictionary<string, float> { ["JumpAttack"] = 1f, ["Attack1"] = 1f, ["Attack2"] = 1f, ["Attack3"] = 1f, ["Attack4"] = 0.85f, ["Attack5"] = 0.8f, ["Attack6"] = 1.15f, ["Attack7"] = 1.2f, ["Attack8"] = 1f, ["Attack9"] = 1f, ["Attack10"] = 1.15f };
        Boss2AnimNameToSpeed = new Dictionary<string, float> { ["JumpAttack"] = 1f, ["Attack1"] = 1f, ["Attack2"] = 1f, ["Attack3"] = 1f, ["Attack4"] = 0.85f, ["Attack5"] = 0.8f, ["Attack6"] = 1.15f, ["Attack7"] = 1.2f, ["Attack8"] = 1f, ["Attack9"] = 1f, ["Attack10"] = 1.15f };
        Boss3AnimNameToSpeed = new Dictionary<string, float> { ["JumpAttack"] = 1f, ["Attack1"] = 1f, ["Attack2"] = 1f, ["Attack3"] = 1f, ["Attack4"] = 0.85f, ["Attack5"] = 0.8f, ["Attack6"] = 1.15f, ["Attack7"] = 1.2f, ["Attack8"] = 1f, ["Attack9"] = 1f, ["Attack10"] = 1.15f };

    }

    /// <returns>attack anim time multiplied by 0.5 by default</returns>
    private float GetAttackAnimationOpenTime(string name, RuntimeAnimatorController animator, float openTimeNormalized = 0.35f)
    {
        float lenght = GetAnimationTime(name, animator);
        return lenght * openTimeNormalized;
    }
    public float GetAnimationTime(string name, RuntimeAnimatorController animator)
    {
        AnimationClip clip = null;
        foreach (var item in animator.animationClips)
        {
            if (item.name.Equals(name))
            {
                clip = item;
                break;
            }
        }
        if (clip == null) return 0f;

        float clipTime = clip.length;
        float clipSpeed = GetAnimSpeedDictionary(animator)[name];
        
        if (clipSpeed == 0f) return 0f;

        float lenght = clipTime / clipSpeed;
        return lenght;
    }
    private Dictionary<string, float> GetAnimSpeedDictionary(RuntimeAnimatorController controller)
    {
        switch (controller.name)
        {
            case "Enemy1":
                return Enemy1AnimNameToSpeed;
            case "Enemy2":
                return Enemy2AnimNameToSpeed;
            case "Enemy3":
                return Enemy3AnimNameToSpeed;
            case "Enemy4":
                return Enemy4AnimNameToSpeed;
            case "Enemy5":
                return Enemy5AnimNameToSpeed;
            case "Katana":
                return KatanaAnimNameToSpeed;
            case "Boss1":
                return Boss1AnimNameToSpeed;
            case "Boss2":
                return Boss2AnimNameToSpeed;
            case "Boss3":
                return Boss3AnimNameToSpeed;
            default:
                return Enemy1AnimNameToSpeed;
        }
    }
    public void StopAllHumanoids()
    {
        _enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");

        PlayerRb.GetComponent<Rigidbody>().velocity = Vector3.zero;
        if (boss != null)
            boss.GetComponent<Rigidbody>().velocity = Vector3.zero;
        foreach (var enemy in _enemies)
        {
            enemy.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
    private void ArrangePhaseCounter()
    {
        if (GameObject.FindGameObjectWithTag("PhaseCounter") == null)
        {
            BossPhaseCounterBetweenScenes = new GameObject();
            BossPhaseCounterBetweenScenes.name = "BossPhaseCounterBetweenScenes";
            BossPhaseCounterBetweenScenes.tag = "PhaseCounter";
            BossPhaseCounterBetweenScenes.transform.position = new Vector3(1f, 0f, 0f);
            DontDestroyOnLoad(BossPhaseCounterBetweenScenes);
        }
        else
        {
            BossPhaseCounterBetweenScenes = GameObject.FindGameObjectWithTag("PhaseCounter");
        }
    }
    private void ArrangeLevelNumber()
    {
        if (GameObject.FindGameObjectWithTag("LevelNumber") == null)
        {
            LevelNumberObject = new GameObject();
            LevelNumberObject.name = "LevelNumber";
            LevelNumberObject.tag = "LevelNumber";
            DontDestroyOnLoad(LevelNumberObject);
        }
        else
        {
            LevelNumberObject = GameObject.FindGameObjectWithTag("LevelNumber");
        }
    }
    private void ArrangeIsInBossRoom()
    {
        int sceneNumber = SceneManager.GetActiveScene().buildIndex;
        if (sceneNumber == Boss1LevelIndex || sceneNumber == Boss2LevelIndex || sceneNumber == Boss3LevelIndex)
        {
            CallForAction(OpenBossUI, 1f);
            _isInBossLevel = true;
        }
        else
            _isInBossLevel = false;
    }
    public void TriggerTutorialVideo(string text, int number)
    {
        if (_lastTutorialTextNumber == number) return;

        _lastTutorialTextNumber = number;
        TutorialTextUI.SetActive(true);
        SoundManager._instance.PlaySound(SoundManager._instance.TutorialText, MainCamera.transform.position, 0.12f, false, 0.8f);
        TutorialTextUI.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = text;
        OpenTutorialUI();

        if (number == 2 || number == 5 || number == 7 || number == 14)
        {
            TutorialTextUI.transform.Find("VideoDual_Left").gameObject.SetActive(true);
            TutorialTextUI.transform.Find("VideoDual_Left").GetComponent<VideoPlayer>().clip = VideosForTutorial[number - 1];
            TutorialTextUI.transform.Find("VideoDual_Left").GetComponent<VideoPlayer>().SetDirectAudioVolume(0, Options._instance.SoundVolume);
            TutorialTextUI.transform.Find("VideoDual_Left").GetComponent<VideoPlayer>().Play();

            TutorialTextUI.transform.Find("VideoDual_Right").gameObject.SetActive(true);
            TutorialTextUI.transform.Find("VideoDual_Right").GetComponent<VideoPlayer>().clip = VideosForTutorial[number];
            TutorialTextUI.transform.Find("VideoDual_Right").GetComponent<VideoPlayer>().SetDirectAudioVolume(0, Options._instance.SoundVolume);
            TutorialTextUI.transform.Find("VideoDual_Right").GetComponent<VideoPlayer>().Play();
        }
        else
        {
            TutorialTextUI.transform.Find("VideoSingle").gameObject.SetActive(true);
            TutorialTextUI.transform.Find("VideoSingle").GetComponent<VideoPlayer>().clip = VideosForTutorial[number - 1];
            TutorialTextUI.transform.Find("VideoSingle").GetComponent<VideoPlayer>().SetDirectAudioVolume(0, Options._instance.SoundVolume);
            TutorialTextUI.transform.Find("VideoSingle").GetComponent<VideoPlayer>().Play();
        }
        
        Time.timeScale = 0f;
        SoundManager._instance.PauseMusic();
        SoundManager._instance.PauseAllSound();
        isGameStopped = true;
    }
    public void OpenTutorialUI()
    {
        CoroutineCall(ref _tutorialTextCoroutine, TutorialTextCoroutine(1f, TutorialTextUI.transform.Find("Text").GetComponent<TextMeshProUGUI>()), this);
    }

    private IEnumerator TutorialTextCoroutine(float targetTransparency, TextMeshProUGUI childText)
    {
        float startTime = Time.realtimeSinceStartup;
        while (startTime + 2f > Time.realtimeSinceStartup)
        {
            var color = childText.color;
            childText.color = Color.Lerp(color, new Color(color.r, color.g, color.b, targetTransparency), Time.unscaledDeltaTime * 5f);
            yield return null;
        }

        if (targetTransparency == 0f) TutorialTextUI.SetActive(false);
    }
    
    

    public void EnemyDied(bool isKilledByPlayer)
    {
        if (isPlayerDead) return;

        _enemies = GameObject.FindGameObjectsWithTag("Enemy");

        int numberOfEnemies = 0;
        foreach (var enemy in _enemies)
        {
            if (enemy.GetComponent<IKillable>()!=null && !enemy.GetComponent<IKillable>().IsDead)
                numberOfEnemies++;
        }

        if (numberOfEnemies <= 2)
        {
            ActivatePassageToNextScene();
        }

        if (isKilledByPlayer)
        {
            _enemyDiedEvent?.Invoke();
            CoroutineCall(ref _openInvincibleScreen, OpenInvincibleScreen(), this);
            GameManager._instance.SlowTime(0.6f);
        }
        
    }
    private IEnumerator OpenInvincibleScreen()
    {
        InvincibleScreen.SetActive(true);

        Color color = InvincibleScreen.transform.Find("Panel").GetComponent<Image>().color;
        InvincibleScreen.transform.Find("Panel").GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0f);

        float startTime = Time.time;
        while(startTime + 0.2f > Time.time)
        {
            color = InvincibleScreen.transform.Find("Panel").GetComponent<Image>().color;
            InvincibleScreen.transform.Find("Panel").GetComponent<Image>().color = new Color(color.r, color.g, color.b, Mathf.Lerp(color.a, 15f / 255f, Time.deltaTime * 7f));
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);

        startTime = Time.time;
        while (startTime + 0.2f > Time.time)
        {
            color = InvincibleScreen.transform.Find("Panel").GetComponent<Image>().color;
            InvincibleScreen.transform.Find("Panel").GetComponent<Image>().color = new Color(color.r, color.g, color.b, Mathf.Lerp(color.a, 0f, Time.deltaTime * 7f));
            yield return null;
        }
        InvincibleScreen.SetActive(false);

    }
    public void PlayerAttackHandle()
    {
        CoroutineCall(ref _playerAttackHandleCoroutine, PlayerAttackHandleCoroutine(), this);
    }
    private IEnumerator PlayerAttackHandleCoroutine()
    {
        isPlayerAttacking = true;
        yield return new WaitForSeconds(0.8f);
        isPlayerAttacking = false;
    }
    
    public void EffectPlayerByDark()
    {
        SoundManager._instance.PlaySound(SoundManager._instance.Darken, PlayerRb.transform.position, 0.5f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        CoroutineCall(ref _effectPlayerByDarkCoroutine, EffectPlayerByDarkCoroutine(), this);
    }
    private IEnumerator EffectPlayerByDarkCoroutine()
    {
        DarkenFromBossUI.SetActive(true);

        Image image = DarkenFromBossUI.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);

        float startTime = Time.time;
        while (startTime + 1f > Time.time)
        {
            image.color = new Color(0f, 0f, 0f, Mathf.Lerp(image.color.a, 247f / 255f, Time.deltaTime * 3.5f));
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        startTime = Time.time;
        while (startTime + 1f > Time.time)
        {
            image.color = new Color(0f, 0f, 0f, Mathf.Lerp(image.color.a, 0f, Time.deltaTime * 0.75f));
            yield return null;
        }

        DarkenFromBossUI.SetActive(false);
    }
    public void ArrangeSkillUI(GameObject UIImage, Color color)
    {
        UIImage.GetComponent<Image>().color = color;
    }
    private IEnumerator ArrangeEnemiesNearPlayerList()
    {
        while (true)
        {
            foreach (var enemy in allEnemies)
            {
                if (enemy.GetComponent<Collider>().enabled && (PlayerRb.position-enemy.transform.position).magnitude < 45f && !enemiesNearPlayer.Contains(enemy))
                {
                    enemiesNearPlayer.Add(enemy);
                }
            }

            tempListForNearPlayers.Clear();
            foreach (var item in enemiesNearPlayer)
            {
                tempListForNearPlayers.Add(item);
            }

            foreach (var enemyNear in tempListForNearPlayers)
            {
                if((!enemyNear.GetComponent<Collider>().enabled) || ((PlayerRb.position - enemyNear.transform.position).magnitude >= 45f && enemiesNearPlayer.Contains(enemyNear)))
                {
                    enemiesNearPlayer.Remove(enemyNear);
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
    public GameObject CheckForAimAssist()
    {
        float allowedDistance = 5f + Math.Clamp(PlayerRb.velocity.magnitude, 0f, 15f) / 4f;

        int number = 0;
        GameObject assistedEnemy = null;
        foreach (GameObject enemy in enemiesNearPlayer)
        {
            float angle = Vector3.Angle(PlayerRb.transform.forward, enemy.transform.position - PlayerRb.transform.position);
            bool isInAngle = angle < 40f;
            Vector3 dir = (enemy.transform.position - PlayerRb.transform.position).normalized;
            Physics.Raycast(PlayerRb.transform.position + dir / 2f, dir, out RaycastHit hit, allowedDistance, GameManager._instance.LayerMaskForVisible);
            if (hit.collider != null && GetParent(hit.collider.transform) == enemy.transform && isInAngle)
            {
                assistedEnemy = enemy;
                number++;
            }
        }
        if (number == 1) return assistedEnemy;
        return null;
    }
    
    public void ArrangeUI(float stamina, float maxStamina, bool isHookAvailable, IThrowableItem CurrentThrowable, IThrowableItem NextThrowable, IThrowableItem BeforeThrowable)
    {
        if(stamina < 8f)
        {
            StaminaBar.color = Color.red;
        }
        else
        {
            StaminaBar.color = _originalStaminaColor;
        }

        StaminaBar.fillAmount = Mathf.Lerp(StaminaBar.fillAmount, stamina / maxStamina, Time.deltaTime * 7.5f);
        SpeedText.text = PlayerRb.velocity.magnitude.ToString("n0") + " m/s";
        FrameRate.text = GetAvarageFrameRate().ToString();

        ArrangeThrowableUI(this.CurrentThrowable, CurrentThrowable);
        ArrangeThrowableUI(this.NextThrowable, NextThrowable);
        ArrangeThrowableUI(this.BeforeThrowable, BeforeThrowable);
        
    }
    public void ArrangeBossUI(float stamina, float maxStamina)
    {
        if (BossStaminaBar == null) return;

        BossStaminaBar.fillAmount = Mathf.Lerp(BossStaminaBar.fillAmount, stamina / maxStamina, Time.deltaTime * 7.5f);
    }
    private void ArrangeThrowableUI(Image objectsImage, IThrowableItem item)
    {
        if (item == null)
        {
            objectsImage.sprite = PrefabHolder._instance.EmptyImage;
            objectsImage.enabled = false;
            objectsImage.transform.parent.GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
        else
        {
            objectsImage.transform.parent.GetComponentInChildren<TextMeshProUGUI>().text = item.CountInterface.ToString();
            objectsImage.enabled = true;
            switch (item)
            {
                case Knife k:
                    objectsImage.sprite = PrefabHolder._instance.KnifeImage;
                    objectsImage.color = ThrowableColor;
                    break;
                case Bomb b:
                    objectsImage.sprite = PrefabHolder._instance.BombImage;
                    objectsImage.color = Color.red;
                    break;
                case Smoke s:
                    objectsImage.sprite = PrefabHolder._instance.SmokeImage;
                    objectsImage.color = Color.green;
                    break;
                case Shuriken s:
                    objectsImage.sprite = PrefabHolder._instance.ShurikenImage;
                    objectsImage.color = ThrowableColor;
                    break;
                case Glass g:
                    objectsImage.sprite = PrefabHolder._instance.GlassImage;
                    objectsImage.color = ThrowableColor;
                    break;
                case Stone s:
                    objectsImage.sprite = PrefabHolder._instance.StoneImage;
                    objectsImage.color = ThrowableColor;
                    break;
                default:
                    break;
            }
        }
    }
    private int GetAvarageFrameRate()
    {
        if (_frameRates.Count >= _frameRateCountForAvarage)
        {
            _frameRates.Dequeue();
        }
        _frameRates.Enqueue((int)(1f / Time.deltaTime));

        int sum = 0;
        foreach (var item in _frameRates)
        {
            sum += item;
        }
        return sum / _frameRateCountForAvarage;
    }
    public void EnterCutscene(string cutsceneName)
    {
        if (isOnCutscene) return;


        _defaultCamera.SetActive(false);
        CutsceneCameras.SetActive(true);
        MainCamera = CutsceneCameras;

        PlayerHands.SetActive(false);

        StopScreen.SetActive(false);
        StopScreen.transform.Find("Options").gameObject.SetActive(true);
        StopScreen.transform.Find("OptionsScreen").gameObject.SetActive(false);

        InGameScreen.SetActive(false);
        PassCutsceneButton.SetActive(false);
        Time.timeScale = 1f;
        SoundManager._instance.ContinueMusic();
        SoundManager._instance.ContinueAllSound();
        isGameStopped = false;
        isOnCutscene = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        CutsceneController._instance.PlayCutscene(cutsceneName);
    }
    public void ExitCutscene()
    {
        if (SceneManager.GetActiveScene().buildIndex == Boss1LevelIndex)
        {
            CutsceneController._instance.SIGNALBoss1EyesOpen();
            CutsceneController._instance.SIGNALBoss1End();
        }
        else if (SceneManager.GetActiveScene().buildIndex == Boss2LevelIndex)
        {

        }
        else if (SceneManager.GetActiveScene().buildIndex == Boss3LevelIndex)
        {

        }

        Instantiate(WaitingScreenPrefab, InGameScreen.transform.parent);
        PlayerHands.SetActive(true);

        _defaultCamera.SetActive(true);
        CutsceneCameras.SetActive(false);
        MainCamera = _defaultCamera;

        CloseStopScreen();
        isOnCutscene = false;
        _bornEvent?.Invoke();
    }
    public void ActivatePassageToNextScene()
    {
        if (ToNextSceneObject == null || ToNextSceneObject.GetComponentInChildren<BoxCollider>().enabled) return;
        ToNextSceneObject.transform.parent.GetComponent<Animator>().Play("NextSceneDoorOpen");
        ToNextSceneObject.GetComponentInChildren<BoxCollider>().enabled = true;
        ToNextSceneObject.GetComponentInChildren<Light>().enabled = true;
    }
    public void ActivatePassageToNextSceneFromBoss()
    {
        if (ToNextSceneObject == null) return;

        ActivatePassageToNextScene();
    }
    public void OpenBossUI()
    {
        BossUI.SetActive(true);
    }
    public void CloseBossUI()
    {
        BossUI.SetActive(false);
    }
    private void OpenStopScreen()
    {
        StopScreen.SetActive(true);
        InGameScreen.SetActive(false);
        PassCutsceneButton.SetActive(false);
        Time.timeScale = 0f;
        SoundManager._instance.PauseMusic();
        SoundManager._instance.PauseAllSound();
        isGameStopped = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void CloseStopScreen()
    {
        StopScreen.SetActive(false);
        StopScreen.transform.Find("Options").gameObject.SetActive(true);
        StopScreen.transform.Find("OptionsScreen").gameObject.SetActive(false);

        PassCutsceneButton.SetActive(false);
        InGameScreen.SetActive(true);
        Time.timeScale = 1f;
        SoundManager._instance.ContinueMusic();
        SoundManager._instance.ContinueAllSound();
        isGameStopped = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void CloseTutorialVideo()
    {
        if (TutorialTextUI.transform.Find("VideoDual_Right").gameObject.activeSelf)
        {
            TutorialTextUI.transform.Find("VideoDual_Right").gameObject.SetActive(false);
            TutorialTextUI.transform.Find("VideoDual_Left").gameObject.SetActive(false);

            TutorialTextUI.transform.Find("VideoDual_Right").GetComponent<VideoPlayer>().Stop();
            TutorialTextUI.transform.Find("VideoDual_Left").GetComponent<VideoPlayer>().Stop();

            TutorialTextUI.transform.Find("VideoDual_Right").GetComponent<VideoPlayer>().clip = null;
            TutorialTextUI.transform.Find("VideoDual_Left").GetComponent<VideoPlayer>().clip = null;
        }
        else
        {
            TutorialTextUI.transform.Find("VideoSingle").gameObject.SetActive(false);
            TutorialTextUI.transform.Find("VideoSingle").GetComponent<VideoPlayer>().Stop();
            TutorialTextUI.transform.Find("VideoSingle").GetComponent<VideoPlayer>().clip = null;
        }

        TutorialTextUI.SetActive(false);
        CloseStopScreen();
    }
    public void OpenOptions()
    {
        StopScreen.transform.Find("Options").gameObject.SetActive(false);
        StopScreen.transform.Find("OptionsScreen").gameObject.SetActive(true);
    }
    public void Die()
    {
        if (isPlayerDead) return;
       
        isPlayerDead = true;
        StopScreen.SetActive(false);
        StopScreen.transform.Find("Options").gameObject.SetActive(true);
        StopScreen.transform.Find("OptionsScreen").gameObject.SetActive(false);

        if (_slowTimeCoroutine != null)
            StopCoroutine(_slowTimeCoroutine);

        InGameScreen.SetActive(false);
        DeathScreen.SetActive(true);
        StartCoroutine(DieTimeScaleArrange());
    }
    private IEnumerator DieTimeScaleArrange()
    {
        while (Time.timeScale > 0.2f) 
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, 0f, Time.deltaTime * 2f);
            yield return null;
        }
        Time.timeScale = 0.2f;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public void SlowTime(float waitTime = 0f)
    {
        if (_slowTimeCoroutine != null)
            StopCoroutine(_slowTimeCoroutine);

        if (waitTime == 0)
            _slowTimeCoroutine = StartCoroutine(SlowTimeCoroutine());
        else
            _slowTimeCoroutine = StartCoroutine(SlowTimeCoroutine(waitTime));
    }
    private IEnumerator SlowTimeCoroutine()
    {
        IsTimeSlowed = true;
        GameManager._instance.BlurVolume.enabled = true;

        SoundManager._instance.PlaySound(SoundManager._instance.TimeSlowEnter, transform.position, 0.5f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        SoundManager._instance.SlowDownMusic();
        SoundManager._instance.SlowDownAllSound();
        while (Time.timeScale > 0.3f || isGameStopped)
        {
            if (!isGameStopped)
            {
                Time.timeScale = Mathf.Lerp(Time.timeScale, 0.25f, Time.deltaTime * 12f);
                ChromaticVolume.weight = Mathf.Lerp(ChromaticVolume.weight, 1f, Time.deltaTime * 6f);
            }
            yield return null;
        }
        ChromaticVolume.weight = 1f;
        Time.timeScale = 0.25f;

        while (!TimeStopEndSignal)
        {
            yield return null;
        }
        TimeStopEndSignal = false;
        SoundManager._instance.PlaySound(SoundManager._instance.TimeSlowExit, transform.position, 0.25f, false, UnityEngine.Random.Range(0.93f, 1.07f));

        while (Time.timeScale < 0.95f)
        {
            if (!isGameStopped)
            {
                Time.timeScale = Mathf.Lerp(Time.timeScale, 1f, Time.deltaTime * 12f);
                ChromaticVolume.weight = Mathf.Lerp(ChromaticVolume.weight, 0f, Time.deltaTime * 8f);
            }
            yield return null;
        }

        GameManager._instance.BlurVolume.enabled = false;
        ChromaticVolume.weight = 0f;
        Time.timeScale = 1f;
        SoundManager._instance.UnSlowDownMusic();
        SoundManager._instance.UnSlowDownAllSound();

        IsTimeSlowed = false;
    }
    private IEnumerator SlowTimeCoroutine(float waitTime)
    {
        IsTimeSlowed = true;
        GameManager._instance.BlurVolume.enabled = false;

        SoundManager._instance.PlaySound(SoundManager._instance.TimeSlowEnter, transform.position, 0.5f, false, UnityEngine.Random.Range(0.93f, 1.07f));
        SoundManager._instance.SlowDownMusic();
        SoundManager._instance.SlowDownAllSound();
        while (Time.timeScale > 0.15f || isGameStopped)
        {
            if (!isGameStopped)
            {
                Time.timeScale = Mathf.Lerp(Time.timeScale, 0.1f, Time.deltaTime * 12.5f);
                ChromaticVolume.weight = Mathf.Lerp(ChromaticVolume.weight, 1f, Time.deltaTime * 10f);
            }
            yield return null;
        }
        ChromaticVolume.weight = 1f;
        Time.timeScale = 0.1f;
        
        yield return new WaitForSecondsRealtime(waitTime);
        SoundManager._instance.PlaySound(SoundManager._instance.TimeSlowExit, transform.position, 0.6f, false, UnityEngine.Random.Range(0.93f, 1.07f));

        while (Time.timeScale < 0.95f)
        {
            if (!isGameStopped)
            {
                Time.timeScale = Mathf.Lerp(Time.timeScale, 1f, Time.deltaTime * 21f);
                ChromaticVolume.weight = Mathf.Lerp(ChromaticVolume.weight, 0f, Time.deltaTime * 10f);
            }
            yield return null;
        }

        ChromaticVolume.weight = 0f;
        Time.timeScale = 1f;
        SoundManager._instance.UnSlowDownMusic();
        SoundManager._instance.UnSlowDownAllSound();

        IsTimeSlowed = false;
    }
}
