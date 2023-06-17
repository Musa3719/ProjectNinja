using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputHandler : MonoBehaviour
{
    public static bool _isAllowedToInput;
    private static bool _KeyboardBinding(string inputName, bool isCheckingButtonDown)
    {

        switch (inputName)
        {
            case "PastLevelDebug":
                if (Keyboard.current == null) return false;
                return isCheckingButtonDown ? Keyboard.current.lKey.wasPressedThisFrame : Keyboard.current.lKey.isPressed;
            case "Fire1":
                if (Mouse.current == null) return false;
                return isCheckingButtonDown ? Mouse.current.leftButton.wasPressedThisFrame : Mouse.current.leftButton.isPressed;
            case "Run":
                if (Keyboard.current == null) return false;
                return isCheckingButtonDown ? Keyboard.current.leftShiftKey.wasPressedThisFrame : Keyboard.current.leftShiftKey.isPressed;
            case "Esc":
                if (Keyboard.current == null) return false;
                return isCheckingButtonDown ? Keyboard.current.escapeKey.wasPressedThisFrame : Keyboard.current.escapeKey.isPressed;
            case "Hook":
                if (Keyboard.current == null) return false;
                return isCheckingButtonDown ? Keyboard.current.eKey.wasPressedThisFrame : Keyboard.current.eKey.isPressed;
            case "UpHook":
                if (Mouse.current == null) return false;
                return isCheckingButtonDown ? Mouse.current.middleButton.wasPressedThisFrame : Mouse.current.middleButton.isPressed;
            case "Jump":
                if (Keyboard.current == null) return false;
                return isCheckingButtonDown ? Keyboard.current.spaceKey.wasPressedThisFrame : Keyboard.current.spaceKey.isPressed;
            case "Crouch":
                if (Keyboard.current == null) return false;
                return isCheckingButtonDown ? Keyboard.current.leftCtrlKey.wasPressedThisFrame : Keyboard.current.leftCtrlKey.isPressed;
            case "Dodge":
                if (Keyboard.current == null) return false;
                return isCheckingButtonDown ? Keyboard.current.altKey.wasPressedThisFrame : Keyboard.current.altKey.isPressed;
            /*case "LeaveWall":
                if (Keyboard.current == null) return false;
                return isCheckingButtonDown ? Keyboard.current.xKey.wasPressedThisFrame : Keyboard.current.xKey.isPressed;*/
            case "Throw":
                if (Keyboard.current == null) return false;
                return isCheckingButtonDown ? Keyboard.current.qKey.wasPressedThisFrame : Keyboard.current.qKey.isPressed;
            case "Block":
                if (Mouse.current == null) return false;
                return isCheckingButtonDown ? Mouse.current.rightButton.wasPressedThisFrame : Mouse.current.rightButton.isPressed;
            case "Teleport":
                if (Keyboard.current == null) return false;
                return isCheckingButtonDown ? Keyboard.current.digit2Key.wasPressedThisFrame : Keyboard.current.digit2Key.isPressed;
            case "IceSkill":
                if (Keyboard.current == null) return false;
                return isCheckingButtonDown ? Keyboard.current.digit1Key.wasPressedThisFrame : Keyboard.current.digit1Key.isPressed;
            case "InvertedMirror":
                if (Keyboard.current == null) return false;
                return isCheckingButtonDown ? Keyboard.current.digit3Key.wasPressedThisFrame : Keyboard.current.digit3Key.isPressed;
            case "Stamina":
                if (Keyboard.current == null) return false;
                return isCheckingButtonDown ? Keyboard.current.rKey.wasPressedThisFrame : Keyboard.current.rKey.isPressed;
            default:
                return false;
        }
    }
    private static bool _GamepadBinding(string inputName, bool isCheckingButtonDown)
    {
        if (Gamepad.current == null) return false;

        switch (inputName)
        {
            case "Fire1":
                return isCheckingButtonDown ? Gamepad.current.rightTrigger.wasPressedThisFrame : Gamepad.current.rightTrigger.isPressed;
            case "Run":
                return isCheckingButtonDown ? Gamepad.current.leftStickButton.wasPressedThisFrame : Gamepad.current.leftStickButton.isPressed;
            case "Esc":
                return isCheckingButtonDown ? Gamepad.current.startButton.wasPressedThisFrame : Gamepad.current.startButton.isPressed;
            case "Hook":
                return isCheckingButtonDown ? Gamepad.current.rightShoulder.wasPressedThisFrame : Gamepad.current.rightShoulder.isPressed;
            case "UpHook":
                return isCheckingButtonDown ? Gamepad.current.leftShoulder.wasPressedThisFrame : Gamepad.current.leftShoulder.isPressed;
            case "Jump":
                return isCheckingButtonDown ? Gamepad.current.aButton.wasPressedThisFrame : Gamepad.current.aButton.isPressed;
            case "Crouch":
                return isCheckingButtonDown ? Gamepad.current.yButton.wasPressedThisFrame : Gamepad.current.yButton.isPressed;
            case "Dodge":
                return isCheckingButtonDown ? Gamepad.current.bButton.wasPressedThisFrame : Gamepad.current.bButton.isPressed;
            /*case "LeaveWall":
                return isCheckingButtonDown ? Gamepad.current.rightStickButton.wasPressedThisFrame : Gamepad.current.rightStickButton.isPressed;*/
            case "Throw":
                return isCheckingButtonDown ? Gamepad.current.dpad.up.wasPressedThisFrame : Gamepad.current.dpad.up.isPressed;
            case "Block":
                return isCheckingButtonDown ? Gamepad.current.leftTrigger.wasPressedThisFrame : Gamepad.current.leftTrigger.isPressed;
            case "Teleport":
                return isCheckingButtonDown ? Gamepad.current.dpad.down.wasPressedThisFrame : Gamepad.current.dpad.down.isPressed;
            case "IceSkill":
                return isCheckingButtonDown ? Gamepad.current.dpad.right.wasPressedThisFrame : Gamepad.current.dpad.right.isPressed;
            case "InvertedMirror":
                return isCheckingButtonDown ? Gamepad.current.dpad.left.wasPressedThisFrame : Gamepad.current.dpad.left.isPressed;
            case "Stamina":
                return isCheckingButtonDown ? Gamepad.current.xButton.wasPressedThisFrame : Gamepad.current.xButton.isPressed;
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
            return Gamepad.current.dpad.left.wasPressedThisFrame ? 1f : 0f;
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
}
