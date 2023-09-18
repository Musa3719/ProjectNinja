using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputHandler : MonoBehaviour
{
    public static bool _isAllowedToInput;

    public static Dictionary<string, ButtonControl> ChangedKeys;

    //private Coroutine _waitForKeyCoroutine;

    private void Awake()
    {
        ChangedKeys = new Dictionary<string, ButtonControl>();
    }
    private static bool _KeyboardBinding(string inputName, bool isCheckingButtonDown)
    {
        ButtonControl key = null;
        switch (inputName)
        {
            /*case "PastLevelDebug":
                if (Keyboard.current == null) return false;
                key = ChangedKeys.ContainsKey("PastLevelDebug") ? ChangedKeys["PastLevelDebug"] : Keyboard.current.lKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;*/
            case "Fire1":
                if (Mouse.current == null) return false;
                key = ChangedKeys.ContainsKey("Fire1") ? ChangedKeys["Fire1"] : Mouse.current.leftButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Run":
                if (Keyboard.current == null) return false;
                key = ChangedKeys.ContainsKey("Run") ? ChangedKeys["Run"] : Keyboard.current.leftShiftKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Esc":
                if (Keyboard.current == null) return false;
                key = ChangedKeys.ContainsKey("Esc") ? ChangedKeys["Esc"] : Keyboard.current.escapeKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Hook":
                if (Mouse.current == null) return false;
                key = ChangedKeys.ContainsKey("Hook") ? ChangedKeys["Hook"] : Mouse.current.middleButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "WeaponChange":
                if (Keyboard.current == null) return false;
                key = ChangedKeys.ContainsKey("Hook") ? ChangedKeys["Hook"] : Keyboard.current.eKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Jump":
                if (Keyboard.current == null) return false;
                key = ChangedKeys.ContainsKey("Jump") ? ChangedKeys["Jump"] : Keyboard.current.spaceKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Crouch":
                if (Keyboard.current == null) return false;
                key = ChangedKeys.ContainsKey("Crouch") ? ChangedKeys["Crouch"] : Keyboard.current.leftCtrlKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Dodge":
                if (Keyboard.current == null) return false;
                key = ChangedKeys.ContainsKey("Dodge") ? ChangedKeys["Dodge"] : Keyboard.current.altKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            /*case "LeaveWall":
                if (Keyboard.current == null) return false;
                return isCheckingButtonDown ? Keyboard.current.xKey.wasPressedThisFrame : Keyboard.current.xKey.isPressed;*/
            case "Throw":
                if (Keyboard.current == null) return false;
                key = ChangedKeys.ContainsKey("Throw") ? ChangedKeys["Throw"] : Keyboard.current.qKey;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Block":
                if (Mouse.current == null) return false;
                key = ChangedKeys.ContainsKey("Block") ? ChangedKeys["Block"] : Mouse.current.rightButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Teleport":
                if (Keyboard.current == null) return false;
                key = ChangedKeys.ContainsKey("Teleport") ? ChangedKeys["Teleport"] : Keyboard.current.digit2Key;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "IceSkill":
                if (Keyboard.current == null) return false;
                key = ChangedKeys.ContainsKey("IceSkill") ? ChangedKeys["IceSkill"] : Keyboard.current.digit1Key;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "InvertedMirror":
                if (Keyboard.current == null) return false;
                key = ChangedKeys.ContainsKey("InvertedMirror") ? ChangedKeys["InvertedMirror"] : Keyboard.current.digit3Key;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Stamina":
                if (Keyboard.current == null) return false;
                key = ChangedKeys.ContainsKey("Stamina") ? ChangedKeys["Stamina"] : Keyboard.current.rKey;
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
                key = ChangedKeys.ContainsKey("Fire1") ? ChangedKeys["Fire1"] : Gamepad.current.rightTrigger;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Run":
                key = ChangedKeys.ContainsKey("Run") ? ChangedKeys["Run"] : Gamepad.current.leftStickButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Esc":
                key = ChangedKeys.ContainsKey("Esc") ? ChangedKeys["Esc"] : Gamepad.current.startButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Hook":
                key = ChangedKeys.ContainsKey("Hook") ? ChangedKeys["Hook"] : Gamepad.current.leftShoulder;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "WeaponChange":
                key = ChangedKeys.ContainsKey("Hook") ? ChangedKeys["Hook"] : Gamepad.current.rightShoulder;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Jump":
                key = ChangedKeys.ContainsKey("Jump") ? ChangedKeys["Jump"] : Gamepad.current.aButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Crouch":
                key = ChangedKeys.ContainsKey("Crouch") ? ChangedKeys["Crouch"] : Gamepad.current.yButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Dodge":
                key = ChangedKeys.ContainsKey("Dodge") ? ChangedKeys["Dodge"] : Gamepad.current.bButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            /*case "LeaveWall":
                return isCheckingButtonDown ? Gamepad.current.rightStickButton.wasPressedThisFrame : Gamepad.current.rightStickButton.isPressed;*/
            case "Throw":
                key = ChangedKeys.ContainsKey("Throw") ? ChangedKeys["Throw"] : Gamepad.current.dpad.up;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Block":
                key = ChangedKeys.ContainsKey("Block") ? ChangedKeys["Block"] : Gamepad.current.leftTrigger;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Teleport":
                key = ChangedKeys.ContainsKey("Teleport") ? ChangedKeys["Teleport"] : Gamepad.current.dpad.down;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "IceSkill":
                key = ChangedKeys.ContainsKey("IceSkill") ? ChangedKeys["IceSkill"] : Gamepad.current.dpad.right;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "InvertedMirror":
                key = ChangedKeys.ContainsKey("InvertedMirror") ? ChangedKeys["InvertedMirror"] : Gamepad.current.dpad.left;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
            case "Stamina":
                key = ChangedKeys.ContainsKey("ThStaminarow") ? ChangedKeys["Stamina"] : Gamepad.current.xButton;
                return isCheckingButtonDown ? key.wasPressedThisFrame : key.isPressed;
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
        if (Mouse.current == null)
        {
            //value += Gamepad.current.selectButton.wasPressedThisFrame ? 1f : 0f;
            //return value;
            return Gamepad.current.dpad.left.wasPressedThisFrame ? 1f : (Gamepad.current.dpad.right.wasPressedThisFrame ? -1f : 0f);
        }

        if(Gamepad.current == null)
        {
            value -= Mouse.current.scroll.ReadValue().y;
            return value;
        }

        value -= Mouse.current.scroll.ReadValue().y;
        value += Gamepad.current.selectButton.wasPressedThisFrame ? 1f : 0f;
        return value;
    }
    public static float GetAxis(string str)
    {
        if (!_isAllowedToInput) return 0f;

        if ((Keyboard.current == null || Mouse.current == null) && Gamepad.current == null) return 0f;

        if (Time.deltaTime > 0.33f) return 0f;

        if(Keyboard.current == null || Mouse.current == null)
        {
            switch (str)
            {
                case "Horizontal":
                    float horizontal = 0f;
                    horizontal += Gamepad.current.leftStick.right.isPressed ? 1f : 0f;
                    horizontal += Gamepad.current.leftStick.left.isPressed ? -1f : 0f;
                    return horizontal;
                case "Vertical":
                    float vertical = 0f;
                    vertical += Gamepad.current.leftStick.up.isPressed ? 1f : 0f;
                    vertical += Gamepad.current.leftStick.down.isPressed ? -1f : 0f;
                    return vertical;
                case "Mouse X":
                    float x = 0f;
                    x += Gamepad.current.rightStick.ReadValue().x;
                    return x;
                case "Mouse Y":
                    float y = 0f;
                    y += Gamepad.current.rightStick.ReadValue().y;
                    return y;
                default:
                    Debug.LogError("WrongAxis");
                    return 0f;
            }
        }

        if (Gamepad.current == null)
        {
            switch (str)
            {
                case "Horizontal":
                    float horizontal = 0f;
                    horizontal += Keyboard.current.dKey.isPressed ? 1f : 0f;
                    horizontal += Keyboard.current.aKey.isPressed ? -1f : 0f;
                    return horizontal;
                case "Vertical":
                    float vertical = 0f;
                    vertical += Keyboard.current.wKey.isPressed ? 1f : 0f;
                    vertical += Keyboard.current.sKey.isPressed ? -1f : 0f;
                    return vertical;
                case "Mouse X":
                    float x = 0f;
                    x += Mouse.current.delta.ReadValue().x;
                    return x;
                case "Mouse Y":
                    float y = 0f;
                    y += Mouse.current.delta.ReadValue().y;
                    return y;
                default:
                    Debug.LogError("WrongAxis");
                    return 0f;
            }
        }

        switch (str)
        {
            case "Horizontal":
                float horizontal = 0f;
                horizontal += Keyboard.current.dKey.isPressed ? 1f : 0f;
                horizontal += Keyboard.current.aKey.isPressed ? -1f : 0f;
                horizontal += Gamepad.current.leftStick.right.isPressed ? 1f : 0f;
                horizontal += Gamepad.current.leftStick.left.isPressed ? -1f : 0f;
                return horizontal;
            case "Vertical":
                float vertical = 0f;
                vertical += Keyboard.current.wKey.isPressed ? 1f : 0f;
                vertical += Keyboard.current.sKey.isPressed ? -1f : 0f;
                vertical += Gamepad.current.leftStick.up.isPressed ? 1f : 0f;
                vertical += Gamepad.current.leftStick.down.isPressed ? -1f : 0f;
                return vertical;
            case "Mouse X":
                float x = 0f;
                x += Mouse.current.delta.ReadValue().x;
                x += Gamepad.current.rightStick.ReadValue().x;
                return x;
            case "Mouse Y":
                float y = 0f;
                y += Mouse.current.delta.ReadValue().y;
                y += Gamepad.current.rightStick.ReadValue().y;
                return y;
            default:
                Debug.LogError("WrongAxis");
                return 0f;
        }
    }
    /*public void ResetKeys()
    {
        ChangedKeys = new Dictionary<string, ButtonControl>();
        //update ui
        //reset save
    }
    public void ChangeKey(string buttonName)
    {
        if (_waitForKeyCoroutine != null)
            StopCoroutine(_waitForKeyCoroutine);
        _waitForKeyCoroutine = StartCoroutine(WaitForKeyCoroutine(buttonName));
    }
    private IEnumerator WaitForKeyCoroutine(string buttonName)
    {
        GameManager._instance.StopScreen.transform.Find("OptionsScreen").Find("PressAKey").gameObject.SetActive(true);
        float startTime = Time.time;
        while (startTime + 10f > Time.time)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                ChangeKeyBinding(buttonName, Keyboard.current.anyKey as ButtonControl);
                //update ui
                Debug.Log((Keyboard.current. as ButtonControl));
                break;
            }
            yield return null;
        }
        GameManager._instance.StopScreen.transform.Find("OptionsScreen").Find("PressAKey").gameObject.SetActive(false);
    }
    public void ChangeKeyBinding(string buttonName, ButtonControl newButton)
    {
        if (newButton == null)
        {
            Debug.LogError("Button value is null");
            return;
        }
        else if (IsDeleteKey(newButton) && ChangedKeys.ContainsKey(buttonName))
        {
            ChangedKeys.Remove(buttonName);
        }

        else if (ChangedKeys.ContainsKey(buttonName))
        {
            ChangedKeys[buttonName] = newButton;
        }
        else
        {
            ChangedKeys.Add(buttonName, newButton);

        }
        //save
    }
    public static string GetString<K, V>(IDictionary<K, V> dict)
    {
        var sb = new StringBuilder();
        foreach (var pair in dict)
        {
            sb.Append(String.Format("[{0}:{1}] ", pair.Key, pair.Value));
        }
        return sb.ToString();
    }
    private bool IsDeleteKey(ButtonControl newButton)
    {
        if (Keyboard.current != null && newButton == Keyboard.current.deleteKey)
            return true;
        if (Gamepad.current != null && newButton == Gamepad.current.leftStickButton)
            return true;
        return false;
    }*/
}
