using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialText : MonoBehaviour
{
    [SerializeField]
    private int _number;

    private Coroutine _waitForTimeScale;

    private void OnTriggerEnter(Collider other)
    {
        if(other!=null && other.CompareTag("Player"))
        {
            if (GameManager._instance.isOnCutscene)
            {
                if (_waitForTimeScale != null)
                    StopCoroutine(_waitForTimeScale);
                _waitForTimeScale = StartCoroutine(WaitForTimeScale());
            }
            else
            {
                GameManager._instance.TriggerTutorialVideo(GetTextForKeyboardOrController(_number), _number);
            }
        }
    }
    private IEnumerator WaitForTimeScale()
    {
        while (GameManager._instance.isOnCutscene)
            yield return null;
        GameManager._instance.TriggerTutorialVideo(GetTextForKeyboardOrController(_number), _number);
    }
    
    private string GetTextForKeyboardOrController(int s)
    {
        if (Options._instance.ControllerForTutorial == 0)
            return Localization._instance.Tutorial[1].Split('\n')[_number];
        else
            return Localization._instance.Tutorial[0].Split('\n')[_number];
    }
}
