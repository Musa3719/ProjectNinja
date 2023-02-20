using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    public enum Skills
    {
        Teleport,
        Mirror
    }

    [SerializeField]
    private Skills _skill;

    private Image _image;
    private float _startTime;
    private bool _isCounting;

    private void Awake()
    {
        _image = transform.Find("SkillWaitImage").GetComponent<Image>();
        _image.fillAmount = 1f;
    }
    public void StartCountdown()
    {
        _startTime = Time.time;
        _isCounting = true;
        _image.enabled = true;
    }
    private void Update()
    {
        if (!_isCounting) return;

        

        switch (_skill)
        {
            case Skills.Teleport:
                if ((Time.time - _startTime) >= GameManager._instance.TeleportAvailableTimeAfterHologram)
                {
                    _isCounting = false;
                    _image.enabled = false;
                }
                _image.fillAmount = (1 - (Time.time - _startTime) / GameManager._instance.TeleportAvailableTimeAfterHologram);
                break;
            case Skills.Mirror:
                if ((Time.time - _startTime) >= GameManager._instance.InvertedMirrorFunctionalTime)
                {
                    _isCounting = false;
                    _image.enabled = false;
                }
                _image.fillAmount = (1 - (Time.time - _startTime) / GameManager._instance.InvertedMirrorFunctionalTime);
                break;
            default:
                break;
        }
    }
}
