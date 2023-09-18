using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.AI;

public class CutsceneController : MonoBehaviour
{
    List<GameObject> _cutscenes;
    GameObject _currentCutscene;
    Vector3 _bossStartPos;
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
        if (GameObject.FindGameObjectWithTag("Boss") != null)
        {
            _bossStartPos = GameObject.FindGameObjectWithTag("Boss").transform.position;
        }
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
        Debug.LogWarning("Cutscene Name not found...");
        GameManager._instance.ExitCutscene();
    }
    private void OnDisable()
    {
        if (_currentCutscene != null)
        {
            if (GameObject.FindGameObjectWithTag("Boss") != null)
            {
                if (GameObject.FindGameObjectWithTag("Boss").name == "Boss_1")
                    GameManager._instance.SIGNALBoss1End();
            }

            _currentCutscene.SetActive(false);
            if (_currentCutscene.GetComponent<PlayableDirector>().state == PlayState.Playing)
                _currentCutscene.GetComponent<PlayableDirector>().Stop();
        }
    }
    public void Boss1Pos()
    {
        StartCoroutine(Boss1PosCoroutine());
    }
    private IEnumerator Boss1PosCoroutine()
    {
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        Vector3 targetPos;

        float startTime = Time.time;
        while (startTime + 0.7f > Time.time)
        {
            targetPos = new Vector3(boss.transform.position.x, 3.411f, boss.transform.position.z);
            boss.transform.position = Vector3.Lerp(boss.transform.position, targetPos, Time.deltaTime * 1f);
            yield return null;
        }
    }
}
