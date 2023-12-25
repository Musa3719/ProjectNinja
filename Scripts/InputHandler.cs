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
        InputHandler._isAllowedToInput = true;
    }

    private void Update()
    {
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

        if (Mouse.current == null && Gamepad.current == null) return 0f;

        float value = 0f;
        if (Gamepad.current != null)
        {
            value += Gamepad.current.dpad.left.wasPressedThisFrame ? 1f : (Gamepad.current.dpad.right.wasPressedThisFrame ? -1f : 0f);
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

        if ((Keyboard.current == null || Mouse.current == null) && Gamepad.current == null) return 0f;

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

        if (horizontal != 0f) return horizontal;
        if (vertical != 0f) return vertical;
        if (x != 0f) return x;
        if (y != 0f) return y;
        return 0f;
        
    }
}
