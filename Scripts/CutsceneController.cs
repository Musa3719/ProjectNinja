using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class CutsceneController : MonoBehaviour
{
    public static CutsceneController _instance;

    private List<GameObject> _cutscenes;
    private GameObject _currentCutscene;
    private Vector3 _bossStartPos;
    private GameObject _bossEnterSound;

    private void Awake()
    {
        _instance = this;
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
            /*if (GameObject.FindGameObjectWithTag("Boss") != null)
            {
                if (GameObject.FindGameObjectWithTag("Boss").name == "Boss_1")
                    GameManager._instance.SIGNALBoss1End();
            }*/

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

    #region Signals
    public void SIGNALOpenLockAndFightSound()
    {
        SoundManager._instance.PlaySound(SoundManager._instance.DoorKeyUsed, GameManager._instance.MainCamera.transform.position, 0.5f, false, 1f);
        GameManager._instance.CallForAction(() => { if (!GameManager._instance.isOnCutscene) return; SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Crushs), GameManager._instance.MainCamera.transform.position, 0.12f, false, 0.8f); }, 1f);
        GameManager._instance.CallForAction(() => { if (!GameManager._instance.isOnCutscene) return; SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Crushs), GameManager._instance.MainCamera.transform.position, 0.12f, false, 0.8f); }, 2.5f);
        GameManager._instance.CallForAction(() => { if (!GameManager._instance.isOnCutscene) return; SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Crushs), GameManager._instance.MainCamera.transform.position, 0.12f, false, 0.8f); }, 3.2f);
        GameManager._instance.CallForAction(() => { if (!GameManager._instance.isOnCutscene) return; SoundManager._instance.PlaySound(SoundManager._instance.GetRandomSoundFromList(SoundManager._instance.Crushs), GameManager._instance.MainCamera.transform.position, 0.12f, false, 1.1f); }, 4.5f);
    }
    public void SIGNALBoss1Enter()
    {
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        boss.transform.position = new Vector3(-15.284f, 3.925f, 45.679f);
        boss.transform.Find("Model").Find("Armature").Find("RL_BoneRoot").Find("CC_Base_Hip").Find("CC_Base_Waist").Find("CC_Base_Spine01").Find("CC_Base_Spine02").Find("CC_Base_NeckTwist01").Find("CC_Base_NeckTwist02").Find("CC_Base_Head").Find("EyeObj").gameObject.SetActive(false);
        boss.transform.Find("Model").GetComponent<Animator>().Play("Cutscene");
        boss.GetComponent<NavMeshAgent>().enabled = false;
        boss.transform.Find("Model").Find("AnimationRigging").GetComponent<Rig>().weight = 0f;
        GameManager._instance.CallForAction(() => _bossEnterSound = SoundManager._instance.PlayBossEnterSound(SoundManager._instance.Boss1Enter), 0.5f);
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

        if (_bossEnterSound != null)
            Destroy(_bossEnterSound);
    }
    #endregion
}
