using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private GameObject PressAButtonText;

    public static bool _isAllowedToInput;
    public static Dictionary<string, KeyCode> ChangedKeys;
    private Coroutine _waitForKeyCoroutine;

    public static int _lastTimeUsedSelectItemJoystick;
    public static int _lastTimeUsedThrowJoystick;

    private void Awake()
    {
        ChangedKeys = new Dictionary<string, KeyCode>();
        if (Gamepad.current == null && Joystick.current != null)
            StartJoystick();
    }
    private void Update()
    {
        if (SceneController._instance.SceneBuildIndex == 0)
            ArrangeButtonPressedUI();
        if (SceneController._instance.SceneBuildIndex == 0 || (GameManager._instance != null && GameManager._instance.isGameStopped) || (GameManager._instance != null && GameManager._instance.isOnCutscene))
        {
            MoveCursorWithGamepads();
            CheckCursorPressed();
        }
    }
    public static float AxisDown(ref int lastTimeUsed, string axis)
    {
        if (Input.GetAxisRaw(axis) != 0)
        {
            if (lastTimeUsed == -1 || lastTimeUsed == Time.frameCount)
            {
                lastTimeUsed = Time.frameCount;
                return Input.GetAxisRaw(axis);
            }
        }
        else
        {
            lastTimeUsed = -1;
        }
        return 0f;
    }
    
    private void MoveCursorWithGamepads()
    {
        float x = 0f, y = 0;
        if (Gamepad.current != null)
        {
            x += (Gamepad.current.leftStick.ReadValue().x) * 7f * Time.unscaledDeltaTime * 60f * Options._instance.MouseSensitivity;
            y += (Gamepad.current.leftStick.ReadValue().y) * 7f * Time.unscaledDeltaTime * 60f * Options._instance.MouseSensitivity;
        }
        if (Joystick.current != null)
        {
            x += Input.GetAxis("HorizontalJoystick") * 7f * Time.unscaledDeltaTime * 60f * Options._instance.MouseSensitivity;
            y += Input.GetAxis("VerticalJoystick") * 7f * Time.unscaledDeltaTime * 60f * Options._instance.MouseSensitivity;
        }
        if (Mouse.current == null)
        {
            Debug.LogError("Mouse Current NULL");
            return;
        }
        if (x == 0f && y == 0f) return;

        var cam = GameManager._instance == null ? Camera.main.gameObject : GameManager._instance.MainCamera;
        var view = cam.GetComponent<Camera>().ScreenToViewportPoint(Input.mousePosition);
        var isOutside = view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
        if (!isOutside)
            Mouse.current.WarpCursorPosition(Mouse.current.position.ReadValue() + new Vector2(x, y));
    }
    private void CheckCursorPressed()
    {
        if (SceneController._instance.SceneBuildIndex == 0 && PressAButtonText.activeInHierarchy) return;

        if (Gamepad.current != null)
        {
            bool isPressed = Gamepad.current.crossButton.wasPressedThisFrame;
            if (isPressed)
            {
                Click();
                return;
            }
        }
            
        if (Joystick.current != null)
        {
            bool isPressed = Input.GetKeyDown(ChangedKeys["Jump"]);
            if (isPressed)
            {
                Click();
                return;
            }
        }
    }
    private void Click()
    {
        Mouse.current.CopyState<MouseState>(out var mouseState);
        mouseState.WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Left, true);
        InputState.Change(Mouse.current, mouseState, InputUpdateType.Dynamic);
        StartCoroutine(ClickCoroutine());
    }
    /// <summary>
    /// For Mouse Update
    /// </summary>
    private IEnumerator ClickCoroutine()
    {
        Mouse.current.WarpCursorPosition(Mouse.current.position.ReadValue() + new Vector2(1, 1));
        yield return null;
        Mouse.current.WarpCursorPosition(Mouse.current.position.ReadValue() - new Vector2(1, 1));
    }
    private void ArrangeButtonPressedUI()
    {
        if (SceneController._instance.SceneBuildIndex == 0 && Joystick.current != null && Options._instance.JoystickUI.activeInHierarchy && !PressAButtonText.activeInHierarchy)
        {
            if (GetAnyJoystickKey())
            {
                string name = KeyToNameFromDict(GetJoystickKey());
                string outputName = "";
                switch (name)
                {
                    case "Fire1":
                        outputName = GetLocalizedButtonName("R1");
                        break;
                    case "Run":
                        outputName = GetLocalizedButtonName("Left Analog Button");
                        break;
                    case "Esc":
                        outputName = GetLocalizedButtonName("Menu");
                        break;
                    case "MiddleMouse":
                        outputName = GetLocalizedButtonName("R2");
                        break;
                    case "WeaponChange":
                        outputName = GetLocalizedButtonName("L2");
                        break;
                    case "Jump":
                        outputName = GetLocalizedButtonName("Cross");
                        break;
                    case "Crouch":
                        outputName = GetLocalizedButtonName("Triangle");
                        break;
                    case "Dodge":
                        outputName = GetLocalizedButtonName("Circle");
                        break;
                    case "Block":
                        outputName = GetLocalizedButtonName("L1");
                        break;
                    case "Stamina":
                        outputName = GetLocalizedButtonName("Square");
                        break;
                    default:
                        outputName = GetLocalizedButtonName("Button not attached to any action");
                        break;
                }
                Options._instance.JoystickUI.transform.Find("ControllerImage").Find("PressedButton").GetComponent<TextMeshProUGUI>().text = outputName;
            }
        }
    }
    private string GetLocalizedButtonName(string buttonName)
    {
        if (Localization._instance._ActiveLanguage == Language.EN) return buttonName;

        switch (buttonName)
        {
            case "R1":
                return buttonName;
            case "Left Analog Button":
                if (Localization._instance._ActiveLanguage == Language.SC) return "左模拟键";
                if (Localization._instance._ActiveLanguage == Language.JP) return "左アナログキー";
                if (Localization._instance._ActiveLanguage == Language.TR) return "Sol Analog Tuşu";
                break;
            case "Menu":
                if (Localization._instance._ActiveLanguage == Language.SC) return "菜单按钮";
                if (Localization._instance._ActiveLanguage == Language.JP) return "メニューボタン";
                if (Localization._instance._ActiveLanguage == Language.TR) return "Menü";
                break;
            case "R2":
                return buttonName;
            case "L2":
                return buttonName;
            case "Cross":
                if (Localization._instance._ActiveLanguage == Language.SC) return "叉";
                if (Localization._instance._ActiveLanguage == Language.JP) return "クロス";
                if (Localization._instance._ActiveLanguage == Language.TR) return "Çarpı";
                break;
            case "Triangle":
                if (Localization._instance._ActiveLanguage == Language.SC) return "三角形";
                if (Localization._instance._ActiveLanguage == Language.JP) return "三角形";
                if (Localization._instance._ActiveLanguage == Language.TR) return "Üçgen";
                break;
            case "Circle":
                if (Localization._instance._ActiveLanguage == Language.SC) return "圆圈";
                if (Localization._instance._ActiveLanguage == Language.JP) return "丸";
                if (Localization._instance._ActiveLanguage == Language.TR) return "Yuvarlak";
                break;
            case "Up D-Pad":
                if (Localization._instance._ActiveLanguage == Language.SC) return "向上箭头键";
                if (Localization._instance._ActiveLanguage == Language.JP) return "上矢印キー";
                if (Localization._instance._ActiveLanguage == Language.TR) return "Yukarı D-Pad";
                break;
            case "L1":
                return buttonName;
            case "Square":
                if (Localization._instance._ActiveLanguage == Language.SC) return "正方形";
                if (Localization._instance._ActiveLanguage == Language.JP) return "四角";
                if (Localization._instance._ActiveLanguage == Language.TR) return "Kare";
                break;
            case "Button not attached to any action":
                if (Localization._instance._ActiveLanguage == Language.SC) return "按钮未分配给任何键";
                if (Localization._instance._ActiveLanguage == Language.JP) return "ボタンがどのキーにも割り当てられていません";
                if (Localization._instance._ActiveLanguage == Language.TR) return "Buton Herhangi Bir Tuşa Atanmadı";
                break;
        }
        return "";
    }
    private void StartJoystick()
    {
        ChangedKeys["Fire1"] = PlayerPrefs.GetInt("Fire1", -1) != -1 ? IntToKey(PlayerPrefs.GetInt("Fire1")) : KeyCode.JoystickButton7;
        ChangedKeys["Run"] = PlayerPrefs.GetInt("Run", -1) != -1 ? IntToKey(PlayerPrefs.GetInt("Run")) : KeyCode.JoystickButton10;
        ChangedKeys["Esc"] = PlayerPrefs.GetInt("Esc", -1) != -1 ? IntToKey(PlayerPrefs.GetInt("Esc")) : KeyCode.JoystickButton9;
        ChangedKeys["MiddleMouse"] = PlayerPrefs.GetInt("MiddleMouse", -1) != -1 ? IntToKey(PlayerPrefs.GetInt("MiddleMouse")) : KeyCode.JoystickButton5;
        ChangedKeys["WeaponChange"] = PlayerPrefs.GetInt("WeaponChange", -1) != -1 ? IntToKey(PlayerPrefs.GetInt("WeaponChange")) : KeyCode.JoystickButton4;
        ChangedKeys["Jump"] = PlayerPrefs.GetInt("Jump", -1) != -1 ? IntToKey(PlayerPrefs.GetInt("Jump")) : KeyCode.JoystickButton2;
        ChangedKeys["Crouch"] = PlayerPrefs.GetInt("Crouch", -1) != -1 ? IntToKey(PlayerPrefs.GetInt("Crouch")) : KeyCode.JoystickButton0;
        ChangedKeys["Dodge"] = PlayerPrefs.GetInt("Dodge", -1) != -1 ? IntToKey(PlayerPrefs.GetInt("Dodge")) : KeyCode.JoystickButton1;
        ChangedKeys["Block"] = PlayerPrefs.GetInt("Block", -1) != -1 ? IntToKey(PlayerPrefs.GetInt("Block")) : KeyCode.JoystickButton6;
        ChangedKeys["Stamina"] = PlayerPrefs.GetInt("Stamina", -1) != -1 ? IntToKey(PlayerPrefs.GetInt("Stamina")) : KeyCode.JoystickButton3;

        if (PlayerPrefs.GetInt("Fire1", -1) == -1)
            Options._instance.OpenJoystickArrangementNeedText();
    }
    public void ResetJoystickSettigs()
    {
        ChangedKeys["Fire1"] = KeyCode.JoystickButton7;
        PlayerPrefs.SetInt("Fire1", 7);
        ChangedKeys["Run"] = KeyCode.JoystickButton10;
        PlayerPrefs.SetInt("Run", 10);
        ChangedKeys["Esc"] = KeyCode.JoystickButton9;
        PlayerPrefs.SetInt("Esc", 9);
        ChangedKeys["MiddleMouse"] = KeyCode.JoystickButton5;
        PlayerPrefs.SetInt("MiddleMouse", 5);
        ChangedKeys["WeaponChange"] = KeyCode.JoystickButton4;
        PlayerPrefs.SetInt("WeaponChange", 4);
        ChangedKeys["Jump"] = KeyCode.JoystickButton2;
        PlayerPrefs.SetInt("Jump", 2);
        ChangedKeys["Crouch"] = KeyCode.JoystickButton0;
        PlayerPrefs.SetInt("Crouch", 0);
        ChangedKeys["Dodge"] = KeyCode.JoystickButton1;
        PlayerPrefs.SetInt("Dodge", 1);
        ChangedKeys["Block"] = KeyCode.JoystickButton6;
        PlayerPrefs.SetInt("Block", 6);
        ChangedKeys["Stamina"] = KeyCode.JoystickButton3;
        PlayerPrefs.SetInt("Stamina", 3);

    }
    public void ChangeJoystickTrigger(string name)
    {
        if (_waitForKeyCoroutine != null)
            StopCoroutine(_waitForKeyCoroutine);
        _waitForKeyCoroutine = StartCoroutine(WaitForKeyCoroutine(name));
    }
    private IEnumerator WaitForKeyCoroutine(string name)
    {
        KeyCode key;
        PressAButtonText.SetActive(true);
        while (!GetAnyJoystickKey())
        {
            yield return null;
        }
        key = GetJoystickKey();
        ChangeJoystickButton(name, key);
        PressAButtonText.SetActive(false);
    }
    private void ChangeJoystickButton(string name, KeyCode key)
    {
        if (ChangedKeys.ContainsValue(key))
        {
            ChangedKeys[KeyToNameFromDict(key)] = ChangedKeys[name];
            PlayerPrefs.SetInt(KeyToNameFromDict(key), KeyToInt(ChangedKeys[name]));
        }

        ChangedKeys[name] = key;
        PlayerPrefs.SetInt(name, KeyToInt(key));
    }
    private bool GetAnyJoystickKey()
    {
        if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.JoystickButton2) ||
            Input.GetKeyDown(KeyCode.JoystickButton3) || Input.GetKeyDown(KeyCode.JoystickButton4) || Input.GetKeyDown(KeyCode.JoystickButton5) || Input.GetKeyDown(KeyCode.JoystickButton6) ||
            Input.GetKeyDown(KeyCode.JoystickButton7) || Input.GetKeyDown(KeyCode.JoystickButton8) || Input.GetKeyDown(KeyCode.JoystickButton9) || Input.GetKeyDown(KeyCode.JoystickButton10) ||
            Input.GetKeyDown(KeyCode.JoystickButton11) || Input.GetKeyDown(KeyCode.JoystickButton12) || Input.GetKeyDown(KeyCode.JoystickButton13) || Input.GetKeyDown(KeyCode.JoystickButton14) ||
            Input.GetKeyDown(KeyCode.JoystickButton15) || Input.GetKeyDown(KeyCode.JoystickButton16) || Input.GetKeyDown(KeyCode.JoystickButton17) || Input.GetKeyDown(KeyCode.JoystickButton18) ||
            Input.GetKeyDown(KeyCode.JoystickButton19))
            return true;
        return false;
    }
    private KeyCode GetJoystickKey()
    {
        if (Input.GetKeyDown(KeyCode.JoystickButton0)) return KeyCode.JoystickButton0;
        if (Input.GetKeyDown(KeyCode.JoystickButton1)) return KeyCode.JoystickButton1;
        if (Input.GetKeyDown(KeyCode.JoystickButton2)) return KeyCode.JoystickButton2;
        if (Input.GetKeyDown(KeyCode.JoystickButton3)) return KeyCode.JoystickButton3;
        if (Input.GetKeyDown(KeyCode.JoystickButton4)) return KeyCode.JoystickButton4;
        if (Input.GetKeyDown(KeyCode.JoystickButton5)) return KeyCode.JoystickButton5;
        if (Input.GetKeyDown(KeyCode.JoystickButton6)) return KeyCode.JoystickButton6;
        if (Input.GetKeyDown(KeyCode.JoystickButton7)) return KeyCode.JoystickButton7;
        if (Input.GetKeyDown(KeyCode.JoystickButton8)) return KeyCode.JoystickButton8;
        if (Input.GetKeyDown(KeyCode.JoystickButton9)) return KeyCode.JoystickButton9;
        if (Input.GetKeyDown(KeyCode.JoystickButton10)) return KeyCode.JoystickButton10;
        if (Input.GetKeyDown(KeyCode.JoystickButton11)) return KeyCode.JoystickButton11;
        if (Input.GetKeyDown(KeyCode.JoystickButton12)) return KeyCode.JoystickButton12;
        if (Input.GetKeyDown(KeyCode.JoystickButton13)) return KeyCode.JoystickButton13;
        if (Input.GetKeyDown(KeyCode.JoystickButton14)) return KeyCode.JoystickButton14;
        if (Input.GetKeyDown(KeyCode.JoystickButton15)) return KeyCode.JoystickButton15;
        if (Input.GetKeyDown(KeyCode.JoystickButton16)) return KeyCode.JoystickButton16;
        if (Input.GetKeyDown(KeyCode.JoystickButton17)) return KeyCode.JoystickButton17;
        if (Input.GetKeyDown(KeyCode.JoystickButton18)) return KeyCode.JoystickButton18;
        if (Input.GetKeyDown(KeyCode.JoystickButton19)) return KeyCode.JoystickButton19;
        return KeyCode.None;
    }
    
    private int KeyToInt(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.JoystickButton0:
                return 0;
            case KeyCode.JoystickButton1:
                return 1;
            case KeyCode.JoystickButton2:
                return 2;
            case KeyCode.JoystickButton3:
                return 3;
            case KeyCode.JoystickButton4:
                return 4;
            case KeyCode.JoystickButton5:
                return 5;
            case KeyCode.JoystickButton6:
                return 6;
            case KeyCode.JoystickButton7:
                return 7;
            case KeyCode.JoystickButton8:
                return 8;
            case KeyCode.JoystickButton9:
                return 9;
            case KeyCode.JoystickButton10:
                return 10;
            case KeyCode.JoystickButton11:
                return 11;
            case KeyCode.JoystickButton12:
                return 12;
            case KeyCode.JoystickButton13:
                return 13;
            case KeyCode.JoystickButton14:
                return 14;
            case KeyCode.JoystickButton15:
                return 15;
            case KeyCode.JoystickButton16:
                return 16;
            case KeyCode.JoystickButton17:
                return 17;
            case KeyCode.JoystickButton18:
                return 18;
            case KeyCode.JoystickButton19:
                return 19;
            default:
                Debug.LogError("Button Not Valid");
                return 0;
        }
    }
    private KeyCode IntToKey(int i)
    {
        switch (i)
        {
            case 0:
                return KeyCode.JoystickButton0;
            case 1:
                return KeyCode.JoystickButton1;
            case 2:
                return KeyCode.JoystickButton2;
            case 3:
                return KeyCode.JoystickButton3;
            case 4:
                return KeyCode.JoystickButton4;
            case 5:
                return KeyCode.JoystickButton5;
            case 6:
                return KeyCode.JoystickButton6;
            case 7:
                return KeyCode.JoystickButton7;
            case 8:
                return KeyCode.JoystickButton8;
            case 9:
                return KeyCode.JoystickButton9;
            case 10:
                return KeyCode.JoystickButton10;
            case 11:
                return KeyCode.JoystickButton11;
            case 12:
                return KeyCode.JoystickButton12;
            case 13:
                return KeyCode.JoystickButton13;
            case 14:
                return KeyCode.JoystickButton14;
            case 15:
                return KeyCode.JoystickButton15;
            case 16:
                return KeyCode.JoystickButton16;
            case 17:
                return KeyCode.JoystickButton17;
            case 18:
                return KeyCode.JoystickButton18;
            case 19:
                return KeyCode.JoystickButton19;
            default:
                Debug.LogError("Button Not Valid");
                return KeyCode.JoystickButton0;
        }
    }
    private string KeyToNameFromDict(KeyCode key)
    {
        return ChangedKeys.FirstOrDefault(x => x.Value == key).Key;
    }
    private static bool _KeyboardBinding(string inputName, bool isCheckingButtonDown)
    {
        if (Keyboard.current == null) return false;

        ButtonControl key = null;
        switch (inputName)
        {
            /*case "PastLevelDebug":
                if (Keyboard.current == null) return false;
                key = ChangedKeys.ContainsKey("PastLevelDebug") ? ChangedKeys["PastLevelDebug"] : Keyboard.current.lKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;*/
            case "Fire1":
                if (Mouse.current == null) return false;
                key = Mouse.current.leftButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Run":
                if (Keyboard.current == null) return false;
                key = Keyboard.current.leftShiftKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Esc":
                if (Keyboard.current == null) return false;
                key = Keyboard.current.escapeKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "MiddleMouse":
                if (Mouse.current == null) return false;
                key = Mouse.current.middleButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "WeaponChange":
                if (Keyboard.current == null) return false;
                key = Keyboard.current.eKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Jump":
                if (Keyboard.current == null) return false;
                key = Keyboard.current.spaceKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Crouch":
                if (Keyboard.current == null) return false;
                key = Keyboard.current.leftCtrlKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Dodge":
                if (Keyboard.current == null) return false;
                key = Keyboard.current.altKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            /*case "LeaveWall":
                if (Keyboard.current == null) return false;
                return isCheckingButtonDown ? Keyboard.current.xKey.wasPressedThisFrame : Keyboard.current.xKey.isPressed;*/
            case "Throw":
                if (Keyboard.current == null) return false;
                key = Keyboard.current.qKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Block":
                if (Mouse.current == null) return false;
                key = Mouse.current.rightButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Stamina":
                if (Keyboard.current == null) return false;
                key = Keyboard.current.rKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            default:
                return false;
        }
    }
    private static bool _GamepadBinding(string inputName, bool isCheckingButtonDown)
    {
        if (Gamepad.current == null && Joystick.current != null) return _JoystickBinding(inputName, isCheckingButtonDown);

        if (Gamepad.current == null) return false;

        ButtonControl key = null;
        switch (inputName)
        {
            case "Fire1":
                key = Gamepad.current.rightTrigger;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Run":
                key = Gamepad.current.leftStickButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Esc":
                key = Gamepad.current.startButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "MiddleMouse":
                key = Gamepad.current.rightShoulder;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "WeaponChange":
                key = Gamepad.current.leftShoulder;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Jump":
                key = Gamepad.current.aButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Crouch":
                key = Gamepad.current.yButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Dodge":
                key = Gamepad.current.bButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            /*case "LeaveWall":
                return isCheckingButtonDown ? Gamepad.current.rightStickButton.wasPressedThisFrame : Gamepad.current.rightStickButton.isPressed;*/
            case "Throw":
                key = Gamepad.current.dpad.up;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Block":
                key = Gamepad.current.leftTrigger;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Stamina":
                key = Gamepad.current.xButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            default:
                return false;
        }
    }
    private static bool _JoystickBinding(string inputName, bool isCheckingButtonDown)
    {
        if (ChangedKeys.Count == 0) return false;

        KeyCode key;
        switch (inputName)
        {
            case "Fire1":
                key = ChangedKeys["Fire1"];
                return isCheckingButtonDown ? Input.GetKeyDown(key) : Input.GetKey(key);
            case "Run":
                key = ChangedKeys["Run"];
                return isCheckingButtonDown ? Input.GetKeyDown(key) : Input.GetKey(key);
            case "Esc":
                key = ChangedKeys["Esc"];
                return isCheckingButtonDown ? Input.GetKeyDown(key) : Input.GetKey(key);
            case "MiddleMouse":
                key = ChangedKeys["MiddleMouse"];
                return isCheckingButtonDown ? Input.GetKeyDown(key) : Input.GetKey(key);
            case "WeaponChange":
                key = ChangedKeys["WeaponChange"];
                return isCheckingButtonDown ? Input.GetKeyDown(key) : Input.GetKey(key);
            case "Jump":
                key = ChangedKeys["Jump"];
                return isCheckingButtonDown ? Input.GetKeyDown(key) : Input.GetKey(key);
            case "Crouch":
                key = ChangedKeys["Crouch"];
                return isCheckingButtonDown ? Input.GetKeyDown(key) : Input.GetKey(key);
            case "Dodge":
                key = ChangedKeys["Dodge"];
                return isCheckingButtonDown ? Input.GetKeyDown(key) : Input.GetKey(key);
            case "Throw":
                return AxisDown(ref _lastTimeUsedThrowJoystick, "ThrowJoystick") > 0f;
            case "Block":
                key = ChangedKeys["Block"];
                return isCheckingButtonDown ? Input.GetKeyDown(key) : Input.GetKey(key);
            case "Stamina":
                key = ChangedKeys["Stamina"];
                return isCheckingButtonDown ? Input.GetKeyDown(key) : Input.GetKey(key);
            default:
                return false;
        }
    }

    public static bool GetButtonDown(string str)
    {
        if (!_isAllowedToInput) return false;

        if (_KeyboardBinding(str, true) || _GamepadBinding(str, true))
            return true;
        return false;
    }
    public static bool GetButton(string str)
    {
        if (!_isAllowedToInput) return false;

        if (_KeyboardBinding(str, false) || _GamepadBinding(str, false))
            return true;
        return false;
    }

    public static float GetScrollForItems()
    {
        if (!_isAllowedToInput) return 0f;

        if (Mouse.current == null && Gamepad.current == null && Joystick.current == null) return 0f;

        float value = 0f;
        if (Gamepad.current != null)
        {
            value += Gamepad.current.dpad.left.wasPressedThisFrame ? 1f : (Gamepad.current.dpad.right.wasPressedThisFrame ? -1f : 0f);
        }
        if (Joystick.current != null)
        {
            value += InputHandler.AxisDown(ref InputHandler._lastTimeUsedSelectItemJoystick, "SelectItemJoystick");
        }
        if(Mouse.current != null)
        {
            value -= Mouse.current.scroll.ReadValue().y;
        }
        return value;
    }
    public static float GetAxis(string str)
    {
        if (!_isAllowedToInput) return 0f;

        if ((Keyboard.current == null || Mouse.current == null) && Gamepad.current == null && Joystick.current == null) return 0f;

        if (Time.deltaTime > 0.33f) return 0f;

        float horizontal = 0f, vertical = 0f, x = 0f, y = 0f;

        if(Gamepad.current != null)
        {
            switch (str)
            {
                case "Horizontal":
                    horizontal += Gamepad.current.leftStick.right.isPressed ? 1f : 0f;
                    horizontal += Gamepad.current.leftStick.left.isPressed ? -1f : 0f;
                    break;
                case "Vertical":
                    vertical += Gamepad.current.leftStick.up.isPressed ? 1f : 0f;
                    vertical += Gamepad.current.leftStick.down.isPressed ? -1f : 0f;
                    break;
                case "Mouse X":
                    x += (Gamepad.current.rightStick.ReadValue().x) * 65f;
                    break;
                case "Mouse Y":
                    y += (Gamepad.current.rightStick.ReadValue().y) * 65f;
                    break;
                default:
                    Debug.LogError("WrongAxis");
                    break;
            }
        }

        if (Keyboard.current != null && Mouse.current != null)
        {
            switch (str)
            {
                case "Horizontal":
                    horizontal += Keyboard.current.dKey.isPressed ? 1f : 0f;
                    horizontal += Keyboard.current.aKey.isPressed ? -1f : 0f;
                    break;
                case "Vertical":
                    vertical += Keyboard.current.wKey.isPressed ? 1f : 0f;
                    vertical += Keyboard.current.sKey.isPressed ? -1f : 0f;
                    break;
                case "Mouse X":
                    x += Mouse.current.delta.ReadValue().x;
                    break;
                case "Mouse Y":
                    y += Mouse.current.delta.ReadValue().y;
                    break;
                default:
                    Debug.LogError("WrongAxis");
                    break;
            }
        }

        if (Joystick.current != null)
        {
            switch (str)
            {
                case "Horizontal":
                    horizontal += Input.GetAxis("HorizontalJoystick");
                    break;
                case "Vertical":
                    vertical += Input.GetAxis("VerticalJoystick");
                    break;
                case "Mouse X":
                    x += Input.GetAxis("HorizontalJoystick2") * 65f;
                    break;
                case "Mouse Y":
                    y += Input.GetAxis("VerticalJoystick2") * 65f;
                    break;
                default:
                    Debug.LogError("WrongAxis");
                    break;
            }
        }

        if (horizontal != 0f) return horizontal;
        if (vertical != 0f) return vertical;
        if (x != 0f) return x;
        if (y != 0f) return y;
        return 0f;
        
    }
}
