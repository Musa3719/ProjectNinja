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

    public Transform PlayerFootPos;
    public Transform PlayerRightHandHolder;

    public bool IsTimeSlowed;

    private Coroutine _slowTimeCoroutine;
    private Coroutine _effectPlayerByDarkCoroutine;
    private Coroutine _playerAttackHandleCoroutine;
    private Coroutine _disableWarningUICoroutine;
    private Coroutine _tutorialTextCoroutine;
    private Coroutine _graphicsBackToNormalCoroutine;
    private Coroutine _openInvincibleScreen;

    public bool isGameStopped { get; set; }
    public bool isOnCutscene { get; private set; }
    public bool isPlayerDead { get; private set; }
    public bool isPlayerAttacking { get; private set; }

    public bool IsPlayerOnWall;

    public const int TeleportActivatedLevelIndex = 17;
    public const int IceActivatedLevelIndex = 9;
    //public const int InvertedMirrorActivatedLevelIndex = 17;
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

    public Color _AvailableColor;
    public Color _NotAvailableColor;
    public Color _InUseColor;
    public Color _InUseWaitingColor;

    public float InvertedMirrorFunctionalTime { get; private set; }
    public float TeleportAvailableTimeAfterHologram { get; private set; }

    public GameObject TeleportSkillUIGetter => TeleportSkill;
    public GameObject IceSkillUIGetter => IceSkill;
    public GameObject InvertedMirrorSkillUIGetter => InvertedMirrorSkill;
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
    public GameObject CanDoWallMovementUI;
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
    public Volume BlurVolume { get; private set; }
    private Volume ChromaticVolume;
    [SerializeField]
    private GameObject TeleportSkill;
    [SerializeField]
    private GameObject IceSkill;
    [SerializeField]
    private GameObject InvertedMirrorSkill;
    public GameObject NewspaperUI;
    public GameObject PaintingUI;
    public GameObject MaxSpeedCounterUI;
    private GameObject CutsceneCameras;
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
    private TextMeshProUGUI StateText;
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

    [HideInInspector]
    public Rigidbody PlayerRb;
    public float PlayerLastSpeed;
    public Transform PlayerRightHandTransform;
    public Transform PlayerLeftHandTransform;
    public bool IsLeftThrowing { get; set; }
    public float PlayerRunningSpeed { get; set; }

    public Painting lookingToPainting { get; set; }

    public GameObject ArrowPrefab;
    public GameObject BulletPrefab;
    public GameObject TeleportSpotPrefab;
    public GameObject InvertedMirrorPrefab;
    public GameObject SmokePrefab;

    private List<HDAdditionalLightData> MixedLights;

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

    public Dictionary<string, string> AttackNameToPrepareName;

    public Dictionary<string, string> AttackNameToPrepareNameBoss;

    public Dictionary<string, float> Enemy1AttackNameToHitOpenTime;
    public Dictionary<string, float> Enemy2AttackNameToHitOpenTime;
    public Dictionary<string, float> Enemy3AttackNameToHitOpenTime;
    public Dictionary<string, float> Enemy4AttackNameToHitOpenTime;
    public Dictionary<string, float> Enemy5AttackNameToHitOpenTime;
    public Dictionary<string, float> KatanaAttackNameToHitOpenTime;

    public Dictionary<string, float> NameToHitOpenTimeBoss1;
    public Dictionary<string, float> NameToHitOpenTimeBoss2;
    public Dictionary<string, float> NameToHitOpenTimeBoss3;

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

    public float BladeSpeed;

    public float RunSpeedAdditionActiveTime;

    public bool TimeStopEndSignal;

    public bool IsPlayerInSmoke { get; set; }
    public bool IsFollowPlayerTriggered { get; set; }

    public GameObject BossPhaseCounterBetweenScenes { get; private set; }
    public GameObject LevelNumberObject { get; private set; }

    private NextSceneHandler _nextSceneHandler;
    private int _lastTutorialTextNumber;

    public event Action _isGroundedWorkedThisFrameEvent;
    public event Action<float> _staminaGainEvent;
    public event Action<string, float> _playAnimEvent;
    public Action<Vector3> _pushEvent;
    public event Action _bornEvent;
    public event Action _enemyDiedEvent;

    [HideInInspector]public CustomPassVolume CustomPassForHands;
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
    private void Awake()
    {
        //Application.targetFrameRate = 10;
        _instance = this;
        CutsceneCameras = GameObject.FindGameObjectWithTag("CutsceneCameras");
        CutsceneCameras.SetActive(false);
        BlurVolume = GameObject.FindGameObjectWithTag("Volume").transform.Find("Blur").GetComponent<Volume>();
        ChromaticVolume = GameObject.FindGameObjectWithTag("Volume").transform.Find("Chromatic").GetComponent<Volume>();
        BlurVolume.enabled = false;
        ArrangeGraphicsToLow();
        
        GameObject[] carpets = GameObject.FindGameObjectsWithTag("Carpet");
        foreach (var item in carpets)
        {
            item.GetComponent<MeshRenderer>().material = CarpetLowSetting;
        }

        _AvailableColor = new Color(77f / 255f, 156f / 255f, 70f / 255f, 1f);
        _NotAvailableColor = new Color(114f / 255f, 44f / 255f, 44f / 255f, 1f);
        _InUseColor = new Color(47f / 255f, 144f / 255f, 188f / 255f, 1f);
        _InUseWaitingColor = new Color(181f / 255f, 181f / 255f, 58f / 255f, 1f);

        MidScreenDotMovementColor = new Color(195f / 255f, 195f / 255f, 195f / 255f, 0.8f);
        MidScreenDotOnWallColor = new Color(255f / 255f, 0f / 255f, 217f / 255f, 204f/255f);

        ThrowableColor = new Color(219f / 255f, 219f / 255f, 219f / 255f, 219f / 255f);

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
        MainCamera = Camera.main.gameObject;
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

        CheckForSkillsOpen();

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
            else if (_nextSceneHandler._isInInteract)
            {
                //nothing
            }
            else if (lookingToPainting != null)
            {
                //lookingToPainting.ClosePainting();
                lookingToPainting = null;
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
    }
    private void ClothRender()
    {
        _clothTimer += Time.deltaTime;
        if (_clothTimer < 0.5f) return;
        _clothTimer = 0f;

        SortCloths();
        int numberOfCloths = 0;
        switch (Options._instance.Quality)
        {
            case 0:
                numberOfCloths = 3;
                break;
            case 1:
                numberOfCloths = 2;
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
        List<Cloth> tempList = new List<Cloth>();
        int count = Cloths.Count;
        for (int i = 0; i < count; i++)
        {
            float minLenght = 1000f;
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
            tempList.Add(tempCloth);
            Cloths.Remove(tempCloth);
        }
        Cloths = tempList;
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
            else if ((light.transform.position - GameManager._instance.PlayerRb.transform.position).magnitude < 15f * distanceMultiplier && Time.realtimeSinceStartup - light.GetComponent<FloatHolder>().Value >= 0.07f)
                isGoingToRender = true;
            else if ((light.transform.position - GameManager._instance.PlayerRb.transform.position).magnitude < 20f * distanceMultiplier && Time.realtimeSinceStartup - light.GetComponent<FloatHolder>().Value >= 0.12f)
                isGoingToRender = true;
            else if ((light.transform.position - GameManager._instance.PlayerRb.transform.position).magnitude < 30f * distanceMultiplier && Time.realtimeSinceStartup - light.GetComponent<FloatHolder>().Value >= 0.2f)
                isGoingToRender = true;
            else if (Time.realtimeSinceStartup - light.GetComponent<FloatHolder>().Value >= 0.5f)
                isGoingToRender = true;

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
        Transform lights = GameObject.Find("Level").transform.Find("Lights");
        foreach (Transform light in lights)
        {
            if (light.name != "Sky and Fog Volume" && light.Find("Lights") != null && light.Find("Lights").GetComponentInChildren<Light>() != null && light.Find("Lights").Find("Mixed") != null)
            {
                MixedLights.Add(light.Find("Lights").GetComponentInChildren<HDAdditionalLightData>());
            }
        }
    }
    public void ArrangeGraphicsToLow()
    {
        if (PlayerPrefs.GetInt("Quality") == 2 && GameObject.Find("Level") != null)
        {
            if (_graphicsBackToNormalCoroutine != null)
                StopCoroutine(_graphicsBackToNormalCoroutine);

            
            //GameObject.Find("Level").transform.Find("ReflectionProbs").gameObject.SetActive(false);
            //GameObject.Find("Level").transform.Find("ReflectionProbs").gameObject.SetActive(true);

            Transform lights = GameObject.Find("Level").transform.Find("Lights");
            foreach (Transform light in lights)
            {
                if (light.name == "Sky and Fog Volume")
                {
                    light.GetComponent<Volume>().profile = LowSettingsVolume;
                }
                else if (light.Find("Lights") != null)
                {
                    light.GetComponentInChildren<HDAdditionalLightData>().affectsVolumetric = false;
                }

            }
        }
    }
    public void GraphicsBackToNormal()
    {
        if (_graphicsBackToNormalCoroutine != null)
            StopCoroutine(_graphicsBackToNormalCoroutine);
        _graphicsBackToNormalCoroutine = StartCoroutine(GraphicsBackToNormalCoroutine());
    }
    private IEnumerator GraphicsBackToNormalCoroutine()
    {
        Transform lights = GameObject.Find("Level").transform.Find("Lights");
        foreach (Transform light in lights)
        {
            if (light.name == "Sky and Fog Volume")
            {
                light.GetComponent<Volume>().profile = NormalSettingsVolume;
            }
            else if (light.Find("Lights") != null)
            {
                light.GetComponentInChildren<HDAdditionalLightData>().affectsVolumetric = true;
            }
            yield return null;
        }
        //GameObject.Find("Level").transform.Find("ReflectionProbs").gameObject.SetActive(false);
        //GameObject.Find("Level").transform.Find("ReflectionProbs").gameObject.SetActive(true);
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
        AttackNameToPrepareName = new Dictionary<string, string> { ["Attack1"] = "Empty", ["Attack2"] = "ReadyFor2", ["Attack3"] = "ReadyFor3", ["Attack4"] = "ReadyFor4", ["Attack5"] = "ReadyFor5", ["Attack6"] = "ReadyFor6" };

        AttackNameToPrepareNameBoss = new Dictionary<string, string>
        {
            ["Attack1"] = "Empty",
            ["Attack2"] = "ReadyFor2",
            ["Attack3"] = "ReadyFor3",
            ["Attack4"] = "Empty",
            ["Attack5"] = "ReadyFor5",
            ["Attack6"] = "ReadyFor6"
        ,
            ["Attack7"] = "Empty",
            ["Attack8"] = "ReadyFor8",
            ["Attack9"] = "ReadyFor9",
            ["Attack10"] = "Empty"
        };

        Enemy1AnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 0.6f, ["Attack2"] = 1.1f, ["Attack3"] = 1.2f, ["Attack4"] = 0.85f, ["Attack5"] = 1f, ["Attack6"] = 0.85f };
        Enemy2AnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 0.6f, ["Attack2"] = 1.1f, ["Attack3"] = 1.2f, ["Attack4"] = 0.85f, ["Attack5"] = 1f, ["Attack6"] = 0.85f };
        Enemy3AnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 0.6f, ["Attack2"] = 1.1f, ["Attack3"] = 1.2f, ["Attack4"] = 0.85f, ["Attack5"] = 1f, ["Attack6"] = 0.85f };
        Enemy4AnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 1.35f, ["Attack2"] = 1.15f, ["Attack3"] = 1f, ["Attack4"] = 0.75f, ["Attack5"] = 0.75f, ["Attack6"] = 0.75f };
        Enemy5AnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 1.35f, ["Attack2"] = 1.15f, ["Attack3"] = 1f, ["Attack4"] = 0.75f, ["Attack5"] = 0.75f, ["Attack6"] = 0.75f };
        KatanaAnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 0.65f * 4.5f, ["Attack2"] = 0.65f * 4.5f, ["Attack3"] = 0.65f * 4.5f, ["Attack4"] = 0.65f * 4.5f, ["Attack5"] = 0.65f * 4.5f, ["Attack6"] = 0.65f * 4.5f };

        Boss1AnimNameToSpeed = new Dictionary<string, float> { ["JumpAttack"] = 1f, ["Attack1"] = 1f, ["Attack2"] = 1f, ["Attack3"] = 1f, ["Attack4"] = 0.85f, ["Attack5"] = 0.8f, ["Attack6"] = 1.15f, ["Attack7"] = 1.2f, ["Attack8"] = 1f, ["Attack9"] = 1f, ["Attack10"] = 1.15f };
        Boss2AnimNameToSpeed = new Dictionary<string, float> { ["JumpAttack"] = 1f, ["Attack1"] = 1f, ["Attack2"] = 1f, ["Attack3"] = 1f, ["Attack4"] = 0.85f, ["Attack5"] = 0.8f, ["Attack6"] = 1.15f, ["Attack7"] = 1.2f, ["Attack8"] = 1f, ["Attack9"] = 1f, ["Attack10"] = 1.15f };
        Boss3AnimNameToSpeed = new Dictionary<string, float> { ["JumpAttack"] = 1f, ["Attack1"] = 1f, ["Attack2"] = 1f, ["Attack3"] = 1f, ["Attack4"] = 0.85f, ["Attack5"] = 0.8f, ["Attack6"] = 1.15f, ["Attack7"] = 1.2f, ["Attack8"] = 1f, ["Attack9"] = 1f, ["Attack10"] = 1.15f };


        Enemy1AttackNameToHitOpenTime = new Dictionary<string, float> { ["Attack1"] = GetAttackAnimationOpenTime("Attack1", Enemy_1_Animator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", Enemy_1_Animator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", Enemy_1_Animator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", Enemy_1_Animator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", Enemy_1_Animator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", Enemy_1_Animator) };
        Enemy2AttackNameToHitOpenTime = new Dictionary<string, float> { ["Attack1"] = GetAttackAnimationOpenTime("Attack1", Enemy_2_Animator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", Enemy_2_Animator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", Enemy_2_Animator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", Enemy_2_Animator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", Enemy_2_Animator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", Enemy_2_Animator) };
        Enemy3AttackNameToHitOpenTime = new Dictionary<string, float> { ["Attack1"] = GetAttackAnimationOpenTime("Attack1", Enemy_3_Animator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", Enemy_3_Animator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", Enemy_3_Animator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", Enemy_3_Animator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", Enemy_3_Animator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", Enemy_3_Animator) };
        Enemy4AttackNameToHitOpenTime = new Dictionary<string, float> { ["Attack1"] = GetAttackAnimationOpenTime("Attack1", Enemy_4_Animator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", Enemy_4_Animator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", Enemy_4_Animator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", Enemy_4_Animator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", Enemy_4_Animator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", Enemy_4_Animator) };
        Enemy5AttackNameToHitOpenTime = new Dictionary<string, float> { ["Attack1"] = GetAttackAnimationOpenTime("Attack1", Enemy_5_Animator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", Enemy_5_Animator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", Enemy_5_Animator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", Enemy_5_Animator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", Enemy_5_Animator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", Enemy_5_Animator) };
        KatanaAttackNameToHitOpenTime = new Dictionary<string, float> { ["Attack1"] = GetAttackAnimationOpenTime("Attack1", KatanaAnimator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", KatanaAnimator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", KatanaAnimator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", KatanaAnimator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", KatanaAnimator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", KatanaAnimator) };

        NameToHitOpenTimeBoss1 = new Dictionary<string, float> { ["JumpAttack"] = GetAttackAnimationOpenTime("JumpAttack", Boss1Animator, 0.45f), ["Attack1"] = GetAttackAnimationOpenTime("Attack1", Boss1Animator, 0.45f), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", Boss1Animator, 0.45f), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", Boss1Animator, 0.45f), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", Boss1Animator, 0.45f), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", Boss1Animator, 0.45f), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", Boss1Animator, 0.45f), ["Attack7"] = GetAttackAnimationOpenTime("Attack7", Boss1Animator, 0.45f), ["Attack8"] = GetAttackAnimationOpenTime("Attack8", Boss1Animator, 0.45f), ["Attack9"] = GetAttackAnimationOpenTime("Attack9", Boss1Animator, 0.45f), ["Attack10"] = GetAttackAnimationOpenTime("Attack10", Boss1Animator, 0.45f) };
        NameToHitOpenTimeBoss2 = new Dictionary<string, float> { ["JumpAttack"] = GetAttackAnimationOpenTime("JumpAttack", Boss2Animator, 0.45f), ["Attack1"] = GetAttackAnimationOpenTime("Attack1", Boss2Animator, 0.45f), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", Boss2Animator, 0.45f), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", Boss2Animator, 0.45f), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", Boss2Animator, 0.45f), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", Boss2Animator, 0.45f), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", Boss2Animator, 0.45f), ["Attack7"] = GetAttackAnimationOpenTime("Attack7", Boss2Animator, 0.45f), ["Attack8"] = GetAttackAnimationOpenTime("Attack8", Boss2Animator, 0.45f), ["Attack9"] = GetAttackAnimationOpenTime("Attack9", Boss2Animator, 0.45f), ["Attack10"] = GetAttackAnimationOpenTime("Attack10", Boss2Animator, 0.45f) };
        NameToHitOpenTimeBoss3 = new Dictionary<string, float> { ["JumpAttack"] = GetAttackAnimationOpenTime("JumpAttack", Boss3Animator, 0.45f), ["Attack1"] = GetAttackAnimationOpenTime("Attack1", Boss3Animator, 0.45f), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", Boss3Animator, 0.45f), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", Boss3Animator, 0.45f), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", Boss3Animator, 0.45f), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", Boss3Animator, 0.45f), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", Boss3Animator, 0.45f), ["Attack7"] = GetAttackAnimationOpenTime("Attack7", Boss3Animator, 0.45f), ["Attack8"] = GetAttackAnimationOpenTime("Attack8", Boss3Animator, 0.45f), ["Attack9"] = GetAttackAnimationOpenTime("Attack9", Boss3Animator, 0.45f), ["Attack10"] = GetAttackAnimationOpenTime("Attack10", Boss3Animator, 0.45f) };
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
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");

        PlayerRb.GetComponent<Rigidbody>().velocity = Vector3.zero;
        if (boss != null)
            boss.GetComponent<Rigidbody>().velocity = Vector3.zero;
        foreach (var enemy in enemies)
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

    public void CallForAction(Action action, float time)
    {
        StartCoroutine(CallForActionCoroutine(action, time));
    }
    private IEnumerator CallForActionCoroutine(Action action, float time)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }

    public void ActivateWarningUI()
    {
        foreach (var enemy in GameManager._instance.allEnemies)
        {
            GameObject warning = Instantiate(WarningUIPrefab, WarningUIParent.transform);
            warning.GetComponent<WarningPositionUI>().TargetTransform = enemy.transform;
            warning.GetComponent<Image>().color = Color.red;
        }
        foreach (var projectile in Projectiles)
        {
            StartCoroutine(CheckForOneProjectileWarning(projectile));
        }
    }
    private IEnumerator CheckForOneProjectileWarning(GameObject projectile)
    {
        float firstDistance = (projectile.transform.position - PlayerRb.transform.position).magnitude;
        yield return new WaitForSeconds(0.05f);
        if (projectile == null) yield break;
        float secondDistance = (projectile.transform.position - PlayerRb.transform.position).magnitude;

        if (firstDistance < secondDistance)
        {
            GameObject warning = Instantiate(WarningUIPrefab, WarningUIParent.transform);
            warning.GetComponent<WarningPositionUI>().TargetTransform = projectile.transform;
            warning.GetComponent<Image>().color = Color.blue;
        }
    }
    public void DisableWarningUI()
    {
        if (WarningUIParent.transform.childCount == 0) return;

        if (_disableWarningUICoroutine != null)
            StopCoroutine(_disableWarningUICoroutine);
        _disableWarningUICoroutine = StartCoroutine(DisableWarningUICoroutine());
    }
    private IEnumerator DisableWarningUICoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        foreach (Transform item in WarningUIParent.transform)
        {
            item.GetComponent<WarningPositionUI>().IsAllowedToSetColor = false;
        }

        float startTime = Time.time;
        while (Time.time < startTime + 1f)
        {
            foreach (Transform item in WarningUIParent.transform)
            {
                Color color = item.GetComponent<Image>().color;
                item.GetComponent<Image>().color = new Color(color.r, color.g, color.b, color.a - Time.deltaTime * 1.5f);
            }
            yield return null;
        }

        while (WarningUIParent.transform.childCount > 0)
        {
            Destroy(WarningUIParent.transform.GetChild(0).gameObject);
            yield return null;
        }
    }
    public void SIGNALOpenLockAndFightSound()
    {
        SoundManager._instance.PlaySound(SoundManager._instance.DoorKeyUsed, MainCamera.transform.position, 0.5f, false, 1f);
        CallForAction(() => { if (!isOnCutscene) return; SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Cuts), MainCamera.transform.position, 0.12f, false, 0.8f);}, 1f);
        CallForAction(() => { if (!isOnCutscene) return; SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Cuts), MainCamera.transform.position, 0.12f, false, 0.8f);}, 2.5f);
        CallForAction(() => { if (!isOnCutscene) return; SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Cuts), MainCamera.transform.position, 0.12f, false, 0.8f);}, 3.2f);
        CallForAction(() => { if (!isOnCutscene) return; SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Cuts), MainCamera.transform.position, 0.12f, false, 1.1f);}, 4.5f);
    }
    public void SIGNALBoss1Enter()
    {
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        boss.transform.position = new Vector3(-15.284f, 3.925f, 45.679f);
        boss.transform.Find("Model").Find("Armature").Find("RL_BoneRoot").Find("CC_Base_Hip").Find("CC_Base_Waist").Find("CC_Base_Spine01").Find("CC_Base_Spine02").Find("CC_Base_NeckTwist01").Find("CC_Base_NeckTwist02").Find("CC_Base_Head").Find("EyeObj").gameObject.SetActive(false);
        boss.transform.Find("Model").GetComponent<Animator>().Play("Cutscene");
        boss.GetComponent<NavMeshAgent>().enabled = false;
        boss.transform.Find("Model").Find("AnimationRigging").GetComponent<Rig>().weight = 0f;
    }
    public void SIGNALBoss1EyesOpen()
    {
        GameObject.FindGameObjectWithTag("Boss").transform.Find("Model").Find("Armature").Find("RL_BoneRoot").Find("CC_Base_Hip").Find("CC_Base_Waist").Find("CC_Base_Spine01").Find("CC_Base_Spine02").Find("CC_Base_NeckTwist01").Find("CC_Base_NeckTwist02").Find("CC_Base_Head").Find("EyeObj").gameObject.SetActive(true);
    }
    public void SIGNALBoss1End()
    {
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        boss.transform.Find("Model").Find("Armature").Find("RL_BoneRoot").Find("CC_Base_Hip").position = new Vector3(-0.0003641245f, 0.03000933f, 0.9237275f);
        boss.transform.position = new Vector3(-11.726f, 3.411f, 45.679f);

        boss.GetComponent<NavMeshAgent>().enabled = true;
        boss.transform.Find("Model").Find("AnimationRigging").GetComponent<Rig>().weight = 1f;
    }
    public void TriggerTutorialText(string text, int number)
    {
        string newText = "";
        int newLineCounter = 1;
        for (int i = 0; i < text.Length; i++)
        {
            if (i > 75 * newLineCounter && text[i] == ' ')
            {
                newLineCounter++;
                newText += "\n";
            }
            else
                newText += text[i];
        }

        if (_lastTutorialTextNumber == number) return;

        _lastTutorialTextNumber = number;
        TutorialTextUI.SetActive(true);
        SoundManager._instance.PlaySound(SoundManager._instance.TutorialText, MainCamera.transform.position, 0.12f, false, 0.8f);
        TutorialTextUI.GetComponent<TextMeshProUGUI>().text = newText;
        TextMeshProUGUI childText = TutorialTextUI.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        childText.text = newText;

        var color = childText.color;
        childText.color = new Color(color.r, color.g, color.b, 0f);
        color = TutorialTextUI.GetComponentInChildren<Image>().color;
        TutorialTextUI.GetComponentInChildren<Image>().color = new Color(color.r, color.g, color.b, 0f);

        if (_tutorialTextCoroutine != null)
            StopCoroutine(_tutorialTextCoroutine);
        _tutorialTextCoroutine = StartCoroutine(TutorialTextCoroutine(1f, childText));
    }
    private IEnumerator TutorialTextCoroutine(float targetTransparency, TextMeshProUGUI childText)
    {
        float startTime = Time.time;
        while (startTime + 2f > Time.time)
        {
            var color = childText.color;
            childText.color = Color.Lerp(color, new Color(color.r, color.g, color.b, targetTransparency), Time.deltaTime * 5f);
            color = TutorialTextUI.GetComponentInChildren<Image>().color;
            TutorialTextUI.GetComponentInChildren<Image>().color = Color.Lerp(color, new Color(color.r, color.g, color.b, targetTransparency * 0.85f), Time.deltaTime * 5f);
            yield return null;
        }

        if (targetTransparency == 0f) TutorialTextUI.SetActive(false);
    }
    public void CloseTutorialUI()
    {
        if (_tutorialTextCoroutine != null)
            StopCoroutine(_tutorialTextCoroutine);
        _tutorialTextCoroutine = StartCoroutine(TutorialTextCoroutine(0f, TutorialTextUI.transform.Find("Text").GetComponent<TextMeshProUGUI>()));
    }
    

    public void EnemyDied(bool isKilledByPlayer)
    {
        if (isPlayerDead) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        int numberOfEnemies = 0;
        foreach (var enemy in enemies)
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
            if (_openInvincibleScreen != null)
                StopCoroutine(_openInvincibleScreen);
            _openInvincibleScreen = StartCoroutine(OpenInvincibleScreen());
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
    private void CheckForSkillsOpen()
    {
        return;
        if (SceneManager.GetActiveScene().buildIndex >= TeleportActivatedLevelIndex)
        {
            isTeleportSkillOpen = true;
            TeleportSkill.SetActive(true);
            TeleportSkill.GetComponent<Image>().color = _AvailableColor;
        }
        /*if (SceneManager.GetActiveScene().buildIndex >= InvertedMirrorActivatedLevelIndex)
        {
            isInvertedMirrorSkillOpen = true;
            InvertedMirrorSkill.SetActive(true);
            InvertedMirrorSkill.GetComponent<Image>().color = _AvailableColor;
        }*/
        if (SceneManager.GetActiveScene().buildIndex >= IceActivatedLevelIndex)
        {
            isIceSkillOpen = true;
            IceSkill.SetActive(true);
            IceSkill.GetComponent<Image>().color = _AvailableColor;
        }
    }
    public void PlayerAttackHandle()
    {
        if (_playerAttackHandleCoroutine != null)
            StopCoroutine(_playerAttackHandleCoroutine);
        _playerAttackHandleCoroutine = StartCoroutine(PlayerAttackHandleCoroutine());
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
        if (_effectPlayerByDarkCoroutine != null)
            StopCoroutine(_effectPlayerByDarkCoroutine);
        _effectPlayerByDarkCoroutine = StartCoroutine(EffectPlayerByDarkCoroutine());
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
        GameObject nearestEnemy = GetNearestAliveEnemy();
        if (nearestEnemy == null) return null;

        bool isInAngle = Vector3.Angle(PlayerRb.transform.forward, nearestEnemy.transform.position - PlayerRb.transform.position) < 33f;
        if ((nearestEnemy.transform.position - PlayerRb.transform.position).magnitude < 5f + Math.Clamp(PlayerRb.velocity.magnitude, 0f, 15f) / 4f && isInAngle)
        {
            return nearestEnemy;
        }
        return null;
    }
    private GameObject GetNearestAliveEnemy()
    {
        if (enemiesNearPlayer == null || enemiesNearPlayer.Count == 0) return null;

        GameObject nearestEnemy = null;
        foreach (GameObject enemy in enemiesNearPlayer)
        {
            if (nearestEnemy == null) nearestEnemy = enemy;

            if ((enemy.transform.position - PlayerRb.transform.position).magnitude < ((nearestEnemy.transform.position - PlayerRb.transform.position).magnitude))
            {
                nearestEnemy = enemy;
            }
        }
        return nearestEnemy;
    }
    public void ArrangeUI(float stamina, float maxStamina, string playerState, bool isHookAvailable, IThrowableItem CurrentThrowable, IThrowableItem NextThrowable, IThrowableItem BeforeThrowable)
    {
        if(stamina < 11f)//11 for block
        {
            StaminaBar.color = Color.red;
        }
        else
        {
            StaminaBar.color = _originalStaminaColor;
        }

        StaminaBar.fillAmount = Mathf.Lerp(StaminaBar.fillAmount, stamina / maxStamina, Time.deltaTime * 7.5f);
        SpeedText.text = PlayerRb.velocity.magnitude.ToString("n0") + " m/s";
        StateText.text = playerState;
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
                    objectsImage.color = ThrowableColor;
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

        MainCamera.SetActive(false);
        CutsceneCameras.SetActive(true);

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

        CutsceneCameras.GetComponent<CutsceneController>().PlayCutscene(cutsceneName);
    }
    public void ExitCutscene()
    {
        if (SceneManager.GetActiveScene().buildIndex == Boss1LevelIndex)
        {
            SIGNALBoss1EyesOpen();
            SIGNALBoss1End();
        }
        else if (SceneManager.GetActiveScene().buildIndex == Boss2LevelIndex)
        {

        }
        else if (SceneManager.GetActiveScene().buildIndex == Boss3LevelIndex)
        {

        }

        Instantiate(WaitingScreenPrefab, InGameScreen.transform.parent);
        PlayerHands.SetActive(true);

        MainCamera.SetActive(true);
        CutsceneCameras.SetActive(false);

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
    public void OpenOptions()
    {
        StopScreen.transform.Find("Options").gameObject.SetActive(false);
        StopScreen.transform.Find("OptionsScreen").gameObject.SetActive(true);
    }
    public void Die()
    {
        if (isPlayerDead) return;
        AsyncOperation loader = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        loader.allowSceneActivation = false;
        isPlayerDead = true;
        StopScreen.SetActive(false);
        StopScreen.transform.Find("Options").gameObject.SetActive(true);
        StopScreen.transform.Find("OptionsScreen").gameObject.SetActive(false);

        if (_slowTimeCoroutine != null)
            StopCoroutine(_slowTimeCoroutine);

        InGameScreen.SetActive(false);
        DeathScreen.SetActive(true);
        StartCoroutine(DieTimeScaleArrange(loader));
    }
    private IEnumerator DieTimeScaleArrange(AsyncOperation loader)
    {
        while (Time.timeScale > 0.2f) 
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, 0f, Time.deltaTime * 2f);
            yield return null;
        }
        Time.timeScale = 0.2f;
        loader.allowSceneActivation = true;
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
            float staminaUse = 15f * (Time.timeScale == 0 ? 0f : Time.unscaledDeltaTime);
            PlayerGainStamina(-staminaUse);
            yield return null;
        }
        TimeStopEndSignal = false;

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
        DisableWarningUI();

        IsTimeSlowed = false;
    }
    private IEnumerator SlowTimeCoroutine(float waitTime)
    {
        IsTimeSlowed = true;

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
        DisableWarningUI();

        IsTimeSlowed = false;
    }
}
