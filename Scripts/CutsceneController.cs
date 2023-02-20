using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
    List<GameObject> _cutscenes;
    GameObject _currentCutscene;
    private void Awake()
    {
        _cutscenes = new List<GameObject>();
        foreach (Transform item in GameManager._instance.CutsceneCamerasGetter.transform)
        {
            _cutscenes.Add(item.gameObject);
        }
    }

    public void PlayCutscene(string cutsceneName)
    {
        foreach (var cutsceneObj in _cutscenes)
        {
            if (cutsceneObj.name == cutsceneName)
            {
                _currentCutscene = cutsceneObj;
                cutsceneObj.SetActive(true);
                cutsceneObj.GetComponent<PlayableDirector>().Play();
                return;
            }
        }
        Debug.LogError("Cutscene Name not found...");
        GameManager._instance.ExitCutscene();
    }
    private void OnDisable()
    {
        if (_currentCutscene != null)
        {
            _currentCutscene.SetActive(false);
            if (_currentCutscene.GetComponent<PlayableDirector>().state == PlayState.Playing)
                _currentCutscene.GetComponent<PlayableDirector>().Stop();
        }
    }
}
