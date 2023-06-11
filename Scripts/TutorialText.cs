using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialText : MonoBehaviour
{
    [SerializeField]
    private int _number;
    private void OnTriggerEnter(Collider other)
    {
        if(other!=null && other.CompareTag("Player"))
        {
            if (_number == -1)
                GameManager._instance.CloseTutorialUI();
            else
                GameManager._instance.TriggerTutorialText(GetTextForKeyboardOrController(_number), _number);
        }
    }
    
    private string GetTextForKeyboardOrController(int s)
    {
        if (Gamepad.current == null && Keyboard.current != null)
            return Localization._instance.Tutorial[1].Split('\n')[_number];
        else if (Gamepad.current != null)
            return Localization._instance.Tutorial[0].Split('\n')[_number];
        Debug.LogError("KeyboardOrGamepadNotFound");
        return "";
    }
}
