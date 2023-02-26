using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;
    [HideInInspector]
    public GameObject MainCamera;

    private Coroutine _slowTimeCoroutine;
    private Coroutine _effectPlayerByDarkCoroutine;
    private Coroutine _playerAttackHandleCoroutine;
    private Coroutine _disableWarningUICoroutine;

    public bool isGameStopped { get; set; }
    public bool isOnCutscene { get; private set; }
    public bool isPlayerDead { get; private set; }
    public bool isPlayerAttacking { get; private set; }

    public const int TeleportActivatedLevelIndex = 6;
    public const int IceActivatedLevelIndex = 11;
    public const int InvertedMirrorActivatedLevelIndex = 17;
    public const int FasterMovementActivatedLevelIndex = 23;//use it
    public bool _isInBossLevel { get; private set; }

    public const int Boss1LevelIndex = 1;//5
    public const int Boss2LevelIndex = 10;
    public const int Boss3LevelIndex = 16;
    public const int Boss4LevelIndex = 22;
    public const int Boss5LevelIndex = 28;
    public const int Boss6LevelIndex = 29;

    public const int Level1Index = 1;
    public const int Level2Index = 6;
    public const int Level3Index = 11;
    public const int Level4Index = 17;
    public const int Level5Index = 23;
    public const int LastLevelIndex = 30;

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
    public LayerMask MirrorRayLayer;


    public Color MidScreenDotMovementColor;
    public Color MidScreenDotOnWallColor;

    public Color ThrowableColor;

    [SerializeField]
    public GameObject GlassBrokenPrefab;
    [SerializeField]
    private GameObject WarningUIPrefab;
    [SerializeField]
    private GameObject WarningUIParent;
    public Image MidScreenDot;
    [SerializeField]
    private GameObject TeleportSkill;
    [SerializeField]
    private GameObject IceSkill;
    [SerializeField]
    private GameObject InvertedMirrorSkill;
    public HookTimerUI HookTimerUI;
    public GameObject NewspaperUI;
    public GameObject PaintingUI;
    [SerializeField]
    private GameObject CutsceneCameras;
    [SerializeField]
    private GameObject DeathScreen;
    [SerializeField]
    private GameObject StopScreen;
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
    private Toggle IsHookAvailableToggle;
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

    public List<GameObject> Projectiles { get; private set; }

    [HideInInspector]
    public Rigidbody PlayerRb;
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


    [SerializeField]
    private RuntimeAnimatorController SwordAnimator;
    [SerializeField]
    private RuntimeAnimatorController AxeAnimator;
    [SerializeField]
    private RuntimeAnimatorController HalberdAnimator;
    [SerializeField]
    private RuntimeAnimatorController HammerAnimator;
    [SerializeField]
    private RuntimeAnimatorController MaceAnimator;
    [SerializeField]
    private RuntimeAnimatorController KatanaAnimator;

    [SerializeField]
    private RuntimeAnimatorController Boss1Animator;
    public RuntimeAnimatorController Boss1AnimatorGetter => Boss1Animator;
    [SerializeField]
    private RuntimeAnimatorController Boss2Animator;
    public RuntimeAnimatorController Boss2AnimatorGetter => Boss2Animator;
    [SerializeField]
    private RuntimeAnimatorController Boss31Animator;
    public RuntimeAnimatorController Boss31AnimatorGetter => Boss31Animator;
    [SerializeField]
    private RuntimeAnimatorController Boss32Animator;
    public RuntimeAnimatorController Boss32AnimatorGetter => Boss32Animator;
    [SerializeField]
    private RuntimeAnimatorController Boss33Animator;
    public RuntimeAnimatorController Boss33AnimatorGetter => Boss33Animator;
    [SerializeField]
    private RuntimeAnimatorController Boss4Animator;
    public RuntimeAnimatorController Boss4AnimatorGetter => Boss4Animator;
    [SerializeField]
    private RuntimeAnimatorController Boss5Animator;
    public RuntimeAnimatorController Boss5AnimatorGetter => Boss5Animator;
    [SerializeField]
    private RuntimeAnimatorController Boss6Animator;
    public RuntimeAnimatorController Boss6AnimatorGetter => Boss6Animator;


    public List<GameObject> allEnemies { get; private set; }
    public List<GameObject> enemiesNearPlayer { get; private set; }
    public List<GameObject> tempListForNearPlayers { get; private set; }

    public Dictionary<string, string> AttackNameToPrepareName;

    public Dictionary<string, string> AttackNameToPrepareNameBoss;

    public Dictionary<string, float> SwordAttackNameToHitOpenTime;
    public Dictionary<string, float> AxeAttackNameToHitOpenTime;
    public Dictionary<string, float> HalberdAttackNameToHitOpenTime;
    public Dictionary<string, float> MaceAttackNameToHitOpenTime;
    public Dictionary<string, float> HammerAttackNameToHitOpenTime;
    public Dictionary<string, float> KatanaAttackNameToHitOpenTime;

    public Dictionary<string, float> NameToHitOpenTimeBoss1;
    public Dictionary<string, float> NameToHitOpenTimeBoss2;
    public Dictionary<string, float> NameToHitOpenTimeBoss31;
    public Dictionary<string, float> NameToHitOpenTimeBoss32;
    public Dictionary<string, float> NameToHitOpenTimeBoss33;
    public Dictionary<string, float> NameToHitOpenTimeBoss4;
    public Dictionary<string, float> NameToHitOpenTimeBoss5;
    public Dictionary<string, float> NameToHitOpenTimeBoss6;

    public Dictionary<string, float> SwordAnimNameToSpeed;
    public Dictionary<string, float> AxeAnimNameToSpeed;
    public Dictionary<string, float> HalberdAnimNameToSpeed;
    public Dictionary<string, float> MaceAnimNameToSpeed;
    public Dictionary<string, float> HammerAnimNameToSpeed;
    public Dictionary<string, float> KatanaAnimNameToSpeed;

    public Dictionary<string, float> Boss1AnimNameToSpeed;
    public Dictionary<string, float> Boss2AnimNameToSpeed;
    public Dictionary<string, float> Boss31AnimNameToSpeed;
    public Dictionary<string, float> Boss32AnimNameToSpeed;
    public Dictionary<string, float> Boss33AnimNameToSpeed;
    public Dictionary<string, float> Boss4AnimNameToSpeed;
    public Dictionary<string, float> Boss5AnimNameToSpeed;
    public Dictionary<string, float> Boss6AnimNameToSpeed;


    public List<GameObject> BloodDecalPrefabs;
    public GameObject HoleDecal;
    public GameObject DeathVFX;
    public GameObject HitSmokeVFX;
    public GameObject GunFireVFX;
    public List<GameObject> BloodVFX;
    public List<GameObject> ExplosionVFX;
    public List<GameObject> SparksVFX;
    public List<GameObject> ShiningSparksVFX;

    public int _swordAttackAnimCount;
    public int _axeAttackAnimCount;
    public int _halberdAttackAnimCount;
    public int _hammerAttackAnimCount;
    public int _maceAttackAnimCount;
    public int _katanaAttackAnimCount;


    public GameObject ToNextSceneObject;
    public float BossArenaGroundYPosition;

    public float BladeSpeed;

    public GameObject BossPhaseCounterBetweenScenes { get; private set; }
    public GameObject LevelNumberObject { get; private set; }

    private NextSceneHandler _nextSceneHandler;
    private int _bossKilledCountForBoss3;

    public event Action _isGroundedWorkedThisFrameEvent;
    public event Action<float> _staminaGainEvent;
    public event Action<string, float> _playAnimEvent;
    public Action<Vector3> _pushEvent;
    public event Action _bornEvent;

    public T GetRandomFromList<T>(List<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
    
    private void Awake()
    {
        _instance = this;

        _AvailableColor = new Color(77f / 255f, 156f / 255f, 70f / 255f, 1f);
        _NotAvailableColor = new Color(114f / 255f, 44f / 255f, 44f / 255f, 1f);
        _InUseColor = new Color(47f / 255f, 144f / 255f, 188f / 255f, 1f);
        _InUseWaitingColor = new Color(181f / 255f, 181f / 255f, 58f / 255f, 1f);

        MidScreenDotMovementColor = new Color(195f / 255f, 195f / 255f, 195f / 255f, 0.8f);
        MidScreenDotOnWallColor = new Color(255f / 255f, 0f / 255f, 217f / 255f, 204f/255f);

        ThrowableColor = new Color(219f / 255f, 219f / 255f, 219f / 255f, 219f / 255f);

        Projectiles = new List<GameObject>();

        ArrangePhaseCounter();
        ArrangeLevelNumber();
        ArrangeDicts();
        ArrangeIsInBossRoom();

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

        allEnemies = new List<GameObject>();

        _nextSceneHandler = ToNextSceneObject.GetComponent<NextSceneHandler>();

        CheckForSkillsOpen();

        StartCoroutine(ArrangeEnemiesNearPlayerList());
    }
    private void Start()
    {
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

        SwordAnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 1f, ["Attack2"] = 1.6f, ["Attack3"] = 1.9f, ["Attack4"] = 1.6f, ["Attack5"] = 1.6f, ["Attack6"] = 1f };
        AxeAnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 1f, ["Attack2"] = 1.6f, ["Attack3"] = 1.9f, ["Attack4"] = 1.6f, ["Attack5"] = 1.6f, ["Attack6"] = 1f };
        HalberdAnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 1f, ["Attack2"] = 1.6f, ["Attack3"] = 1.9f, ["Attack4"] = 1.6f, ["Attack5"] = 1.6f, ["Attack6"] = 1f };
        MaceAnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 1f, ["Attack2"] = 1.6f, ["Attack3"] = 1.9f, ["Attack4"] = 1.6f, ["Attack5"] = 1.6f, ["Attack6"] = 1f };
        HammerAnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 1f, ["Attack2"] = 1.6f, ["Attack3"] = 1.9f, ["Attack4"] = 1.6f, ["Attack5"] = 1.6f, ["Attack6"] = 1f };
        KatanaAnimNameToSpeed = new Dictionary<string, float> { ["Attack1"] = 0.65f * 6f, ["Attack2"] = 0.65f * 6f, ["Attack3"] = 0.65f * 6f, ["Attack4"] = 0.65f * 6f, ["Attack5"] = 0.65f * 6f, ["Attack6"] = 0.65f * 6f };

        Boss1AnimNameToSpeed = new Dictionary<string, float> { ["JumpAttack"] = 1f, ["Attack1"] = 1.3f, ["Attack2"] = 1.3f, ["Attack3"] = 1.4f, ["Attack4"] = 1.2f, ["Attack5"] = 1.1f, ["Attack6"] = 1.6f, ["Attack7"] = 1.5f, ["Attack8"] = 1.3f, ["Attack9"] = 1.4f, ["Attack10"] = 1.5f };
        Boss2AnimNameToSpeed = new Dictionary<string, float> { ["JumpAttack"] = 1f, ["Attack1"] = 1.3f, ["Attack2"] = 1.3f, ["Attack3"] = 1.4f, ["Attack4"] = 1.2f, ["Attack5"] = 1.1f, ["Attack6"] = 1.6f, ["Attack7"] = 1.5f, ["Attack8"] = 1.3f, ["Attack9"] = 1.4f, ["Attack10"] = 1.5f };
        Boss31AnimNameToSpeed = new Dictionary<string, float> { ["JumpAttack"] = 1f, ["Attack1"] = 1.3f, ["Attack2"] = 1.3f, ["Attack3"] = 1.4f, ["Attack4"] = 1.2f, ["Attack5"] = 1.1f, ["Attack6"] = 1.6f, ["Attack7"] = 1.5f, ["Attack8"] = 1.3f, ["Attack9"] = 1.4f, ["Attack10"] = 1.5f };
        Boss32AnimNameToSpeed = new Dictionary<string, float> { ["JumpAttack"] = 1f, ["Attack1"] = 1.3f, ["Attack2"] = 1.3f, ["Attack3"] = 1.4f, ["Attack4"] = 1.2f, ["Attack5"] = 1.1f, ["Attack6"] = 1.6f, ["Attack7"] = 1.5f, ["Attack8"] = 1.3f, ["Attack9"] = 1.4f, ["Attack10"] = 1.5f };
        Boss33AnimNameToSpeed = new Dictionary<string, float> { ["JumpAttack"] = 1f, ["Attack1"] = 1.3f, ["Attack2"] = 1.3f, ["Attack3"] = 1.4f, ["Attack4"] = 1.2f, ["Attack5"] = 1.1f, ["Attack6"] = 1.6f, ["Attack7"] = 1.5f, ["Attack8"] = 1.3f, ["Attack9"] = 1.4f, ["Attack10"] = 1.5f };
        Boss4AnimNameToSpeed = new Dictionary<string, float> { ["JumpAttack"] = 1f, ["Attack1"] = 1.3f, ["Attack2"] = 1.3f, ["Attack3"] = 1.4f, ["Attack4"] = 1.2f, ["Attack5"] = 1.1f, ["Attack6"] = 1.6f, ["Attack7"] = 1.5f, ["Attack8"] = 1.3f, ["Attack9"] = 1.4f, ["Attack10"] = 1.5f };
        Boss5AnimNameToSpeed = new Dictionary<string, float> { ["JumpAttack"] = 1f, ["Attack1"] = 1.3f, ["Attack2"] = 1.3f, ["Attack3"] = 1.4f, ["Attack4"] = 1.2f, ["Attack5"] = 1.1f, ["Attack6"] = 1.6f, ["Attack7"] = 1.5f, ["Attack8"] = 1.3f, ["Attack9"] = 1.4f, ["Attack10"] = 1.5f };
        Boss6AnimNameToSpeed = new Dictionary<string, float> { ["JumpAttack"] = 1f, ["Attack1"] = 1.3f, ["Attack2"] = 1.3f, ["Attack3"] = 1.4f, ["Attack4"] = 1.2f, ["Attack5"] = 1.1f, ["Attack6"] = 1.6f, ["Attack7"] = 1.5f, ["Attack8"] = 1.3f, ["Attack9"] = 1.4f, ["Attack10"] = 1.5f };


        SwordAttackNameToHitOpenTime = new Dictionary<string, float> { ["Attack1"] = GetAttackAnimationOpenTime("Attack1", SwordAnimator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", SwordAnimator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", SwordAnimator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", SwordAnimator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", SwordAnimator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", SwordAnimator) };
        AxeAttackNameToHitOpenTime = new Dictionary<string, float> { ["Attack1"] = GetAttackAnimationOpenTime("Attack1", AxeAnimator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", AxeAnimator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", AxeAnimator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", AxeAnimator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", AxeAnimator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", AxeAnimator) };
        HalberdAttackNameToHitOpenTime = new Dictionary<string, float> { ["Attack1"] = GetAttackAnimationOpenTime("Attack1", HalberdAnimator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", HalberdAnimator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", HalberdAnimator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", HalberdAnimator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", HalberdAnimator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", HalberdAnimator) };
        MaceAttackNameToHitOpenTime = new Dictionary<string, float> { ["Attack1"] = GetAttackAnimationOpenTime("Attack1", MaceAnimator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", MaceAnimator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", MaceAnimator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", MaceAnimator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", MaceAnimator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", MaceAnimator) };
        HammerAttackNameToHitOpenTime = new Dictionary<string, float> { ["Attack1"] = GetAttackAnimationOpenTime("Attack1", HammerAnimator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", HammerAnimator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", HammerAnimator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", HammerAnimator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", HammerAnimator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", HammerAnimator) };
        KatanaAttackNameToHitOpenTime = new Dictionary<string, float> { ["Attack1"] = GetAttackAnimationOpenTime("Attack1", KatanaAnimator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", KatanaAnimator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", KatanaAnimator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", KatanaAnimator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", KatanaAnimator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", KatanaAnimator) };

        NameToHitOpenTimeBoss1 = new Dictionary<string, float> { ["JumpAttack"] = GetAttackAnimationOpenTime("JumpAttack", Boss1Animator), ["Attack1"] = GetAttackAnimationOpenTime("Attack1", Boss1Animator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", Boss1Animator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", Boss1Animator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", Boss1Animator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", Boss1Animator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", Boss1Animator), ["Attack7"] = GetAttackAnimationOpenTime("Attack7", Boss1Animator), ["Attack8"] = GetAttackAnimationOpenTime("Attack8", Boss1Animator), ["Attack9"] = GetAttackAnimationOpenTime("Attack9", Boss1Animator), ["Attack10"] = GetAttackAnimationOpenTime("Attack10", Boss1Animator) };
        NameToHitOpenTimeBoss2 = new Dictionary<string, float> { ["JumpAttack"] = GetAttackAnimationOpenTime("JumpAttack", Boss2Animator), ["Attack1"] = GetAttackAnimationOpenTime("Attack1", Boss2Animator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", Boss2Animator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", Boss2Animator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", Boss2Animator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", Boss2Animator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", Boss2Animator), ["Attack7"] = GetAttackAnimationOpenTime("Attack7", Boss2Animator), ["Attack8"] = GetAttackAnimationOpenTime("Attack8", Boss2Animator), ["Attack9"] = GetAttackAnimationOpenTime("Attack9", Boss2Animator), ["Attack10"] = GetAttackAnimationOpenTime("Attack10", Boss2Animator) };
        NameToHitOpenTimeBoss31 = new Dictionary<string, float> { ["JumpAttack"] = GetAttackAnimationOpenTime("JumpAttack", Boss31Animator), ["Attack1"] = GetAttackAnimationOpenTime("Attack1", Boss31Animator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", Boss31Animator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", Boss31Animator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", Boss31Animator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", Boss31Animator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", Boss31Animator), ["Attack7"] = GetAttackAnimationOpenTime("Attack7", Boss31Animator), ["Attack8"] = GetAttackAnimationOpenTime("Attack8", Boss31Animator), ["Attack9"] = GetAttackAnimationOpenTime("Attack9", Boss31Animator), ["Attack10"] = GetAttackAnimationOpenTime("Attack10", Boss31Animator) };
        NameToHitOpenTimeBoss32 = new Dictionary<string, float> { ["JumpAttack"] = GetAttackAnimationOpenTime("JumpAttack", Boss32Animator), ["Attack1"] = GetAttackAnimationOpenTime("Attack1", Boss32Animator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", Boss32Animator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", Boss32Animator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", Boss32Animator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", Boss32Animator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", Boss32Animator), ["Attack7"] = GetAttackAnimationOpenTime("Attack7", Boss32Animator), ["Attack8"] = GetAttackAnimationOpenTime("Attack8", Boss32Animator), ["Attack9"] = GetAttackAnimationOpenTime("Attack9", Boss32Animator), ["Attack10"] = GetAttackAnimationOpenTime("Attack10", Boss32Animator) };
        NameToHitOpenTimeBoss33 = new Dictionary<string, float> { ["JumpAttack"] = GetAttackAnimationOpenTime("JumpAttack", Boss33Animator), ["Attack1"] = GetAttackAnimationOpenTime("Attack1", Boss33Animator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", Boss33Animator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", Boss33Animator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", Boss33Animator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", Boss33Animator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", Boss33Animator), ["Attack7"] = GetAttackAnimationOpenTime("Attack7", Boss33Animator), ["Attack8"] = GetAttackAnimationOpenTime("Attack8", Boss33Animator), ["Attack9"] = GetAttackAnimationOpenTime("Attack9", Boss33Animator), ["Attack10"] = GetAttackAnimationOpenTime("Attack10", Boss33Animator) };
        NameToHitOpenTimeBoss4 = new Dictionary<string, float> { ["JumpAttack"] = GetAttackAnimationOpenTime("JumpAttack", Boss4Animator), ["Attack1"] = GetAttackAnimationOpenTime("Attack1", Boss4Animator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", Boss4Animator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", Boss4Animator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", Boss4Animator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", Boss4Animator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", Boss4Animator), ["Attack7"] = GetAttackAnimationOpenTime("Attack7", Boss4Animator), ["Attack8"] = GetAttackAnimationOpenTime("Attack8", Boss4Animator), ["Attack9"] = GetAttackAnimationOpenTime("Attack9", Boss4Animator), ["Attack10"] = GetAttackAnimationOpenTime("Attack10", Boss4Animator) };
        NameToHitOpenTimeBoss5 = new Dictionary<string, float> { ["JumpAttack"] = GetAttackAnimationOpenTime("JumpAttack", Boss5Animator), ["Attack1"] = GetAttackAnimationOpenTime("Attack1", Boss5Animator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", Boss5Animator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", Boss5Animator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", Boss5Animator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", Boss5Animator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", Boss5Animator), ["Attack7"] = GetAttackAnimationOpenTime("Attack7", Boss5Animator), ["Attack8"] = GetAttackAnimationOpenTime("Attack8", Boss5Animator), ["Attack9"] = GetAttackAnimationOpenTime("Attack9", Boss5Animator), ["Attack10"] = GetAttackAnimationOpenTime("Attack10", Boss5Animator) };
        NameToHitOpenTimeBoss6 = new Dictionary<string, float> { ["JumpAttack"] = GetAttackAnimationOpenTime("JumpAttack", Boss6Animator), ["Attack1"] = GetAttackAnimationOpenTime("Attack1", Boss6Animator), ["Attack2"] = GetAttackAnimationOpenTime("Attack2", Boss6Animator), ["Attack3"] = GetAttackAnimationOpenTime("Attack3", Boss6Animator), ["Attack4"] = GetAttackAnimationOpenTime("Attack4", Boss6Animator), ["Attack5"] = GetAttackAnimationOpenTime("Attack5", Boss6Animator), ["Attack6"] = GetAttackAnimationOpenTime("Attack6", Boss6Animator), ["Attack7"] = GetAttackAnimationOpenTime("Attack7", Boss6Animator), ["Attack8"] = GetAttackAnimationOpenTime("Attack8", Boss6Animator), ["Attack9"] = GetAttackAnimationOpenTime("Attack9", Boss6Animator), ["Attack10"] = GetAttackAnimationOpenTime("Attack10", Boss6Animator) };

    }

    /// <returns>attack anim time multiplied by 0.35</returns>
    private float GetAttackAnimationOpenTime(string name, RuntimeAnimatorController animator)
    {
        float lenght = GetAnimationTime(name, animator);
        return lenght * 0.35f;
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
            case "Sword":
                return SwordAnimNameToSpeed;
            case "Axe":
                return AxeAnimNameToSpeed;
            case "Halberd":
                return HalberdAnimNameToSpeed;
            case "Mace":
                return MaceAnimNameToSpeed;
            case "Hammer":
                return HammerAnimNameToSpeed;
            case "Katana":
                return KatanaAnimNameToSpeed;
            case "Boss1":
                return Boss1AnimNameToSpeed;
            case "Boss2":
                return Boss2AnimNameToSpeed;
            case "Boss31":
                return Boss31AnimNameToSpeed;
            case "Boss32":
                return Boss32AnimNameToSpeed;
            case "Boss33":
                return Boss33AnimNameToSpeed;
            case "Boss4":
                return Boss4AnimNameToSpeed;
            case "Boss5":
                return Boss5AnimNameToSpeed;
            case "Boss6":
                return Boss6AnimNameToSpeed;
            default:
                return SwordAnimNameToSpeed;
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
        if (sceneNumber == Boss1LevelIndex || sceneNumber == Boss2LevelIndex || sceneNumber == Boss3LevelIndex || sceneNumber == Boss4LevelIndex || sceneNumber == Boss5LevelIndex || sceneNumber == Boss6LevelIndex)
        {
            CallForAction(OpenBossUI, 1f);
            _isInBossLevel = true;
        }
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

    public void EnemyDied()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            ActivatePassageToNextScene();
        }
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
        if (SceneManager.GetActiveScene().buildIndex >= TeleportActivatedLevelIndex)
        {
            isTeleportSkillOpen = true;
            TeleportSkill.SetActive(true);
            TeleportSkill.GetComponent<Image>().color = _AvailableColor;
        }
        if (SceneManager.GetActiveScene().buildIndex >= InvertedMirrorActivatedLevelIndex)
        {
            isInvertedMirrorSkillOpen = true;
            InvertedMirrorSkill.SetActive(true);
            InvertedMirrorSkill.GetComponent<Image>().color = _AvailableColor;
        }
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
    private void Update()
    {
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
                lookingToPainting = null;
                lookingToPainting.ClosePainting();
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
    }
    public void EffectPlayerByDark()
    {
        //sound and screen VFX
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
        while (startTime + 1.25 > Time.time)
        {
            image.color = new Color(0f, 0f, 0f, Mathf.Lerp(image.color.a, 247f / 255f, Time.deltaTime * 3.5f));
            yield return null;
        }
        yield return new WaitForSeconds(1f);
        startTime = Time.time;
        while (startTime + 3.5 > Time.time)
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
                if ((PlayerRb.position-enemy.transform.position).magnitude < 45f && !enemiesNearPlayer.Contains(enemy))
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
                if((PlayerRb.position - enemyNear.transform.position).magnitude >= 45f)
                {
                    enemiesNearPlayer.Remove(enemyNear);
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
   
    public void ArrangeUI(float stamina, string playerState, bool isHookAvailable, IThrowableItem CurrentThrowable, IThrowableItem NextThrowable, IThrowableItem BeforeThrowable)
    {
        StaminaBar.fillAmount = Mathf.Lerp(StaminaBar.fillAmount, stamina / 100f, Time.deltaTime * 7.5f);
        SpeedText.text = PlayerRb.velocity.magnitude.ToString("n0") + " m/s";
        StateText.text = playerState;
        IsHookAvailableToggle.isOn = isHookAvailable;
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
            objectsImage.transform.parent.GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
        else
        {
            objectsImage.transform.parent.GetComponentInChildren<TextMeshProUGUI>().text = item.CountInterface.ToString();

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

        StopScreen.SetActive(false);
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
        MainCamera.SetActive(true);
        CutsceneCameras.SetActive(false);

        CloseStopScreen();
        isOnCutscene = false;
        _bornEvent?.Invoke();
    }
    public void ActivatePassageToNextScene()
    {
        if (ToNextSceneObject == null) return;

        ToNextSceneObject.SetActive(true);
    }
    public void ActivatePassageToNextSceneFromBoss()
    {
        if (ToNextSceneObject == null) return;

        if (SceneManager.GetActiveScene().buildIndex != Boss3LevelIndex)
            ToNextSceneObject.SetActive(true);
        else
        {
            _bossKilledCountForBoss3++;
            if (_bossKilledCountForBoss3 >= 3)
                ToNextSceneObject.SetActive(true);
        }
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
        PassCutsceneButton.SetActive(false);
        InGameScreen.SetActive(true);
        Time.timeScale = 1f;
        SoundManager._instance.ContinueMusic();
        SoundManager._instance.ContinueAllSound();
        isGameStopped = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void Die()
    {
        AsyncOperation loader = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        loader.allowSceneActivation = false;
        isPlayerDead = true;
        StopScreen.SetActive(false);
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

    public void SlowTime(float waitTime = 0.1f)
    {
        if (_slowTimeCoroutine != null)
            StopCoroutine(_slowTimeCoroutine);
        _slowTimeCoroutine = StartCoroutine(SlowTimeCoroutine(waitTime));
    }
    private IEnumerator SlowTimeCoroutine(float waitTime)
    {
        SoundManager._instance.SlowDownMusic();
        SoundManager._instance.SlowDownAllSound();
        while (Time.timeScale > 0.1f || isGameStopped)
        {
            if(!isGameStopped)
                Time.timeScale = Mathf.Lerp(Time.timeScale, 0f, Time.deltaTime * 8f);
            yield return null;
        }

        Time.timeScale = 0.1f;
        
        yield return new WaitForSeconds(waitTime * Time.timeScale);
        
        while (Time.timeScale < 1f)
        {
            if(!isGameStopped)
                Time.timeScale = Mathf.Lerp(Time.timeScale, 1f, Time.deltaTime * 20f);
            yield return null;
        }

        Time.timeScale = 1f;
        SoundManager._instance.UnSlowDownMusic();
        SoundManager._instance.UnSlowDownAllSound();
        DisableWarningUI();
    }
}