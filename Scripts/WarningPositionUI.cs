using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningPositionUI : MonoBehaviour
{
    private RectTransform RectTransform;
    private Color color;
    private Image image;

    public Transform TargetTransform;
    public bool IsAllowedToSetColor;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        color = Color.black;
        image = GetComponent<Image>();
        IsAllowedToSetColor = true;
    }

    void Update()
    {
        if (TargetTransform == null) return;
        if (color == Color.black) color = GetComponent<Image>().color;


        Vector2 tempPlayerPos = new Vector2(GameManager._instance.PlayerRb.transform.position.x, GameManager._instance.PlayerRb.transform.position.z);
        Vector2 tempTargetPos = new Vector2(TargetTransform.position.x, TargetTransform.position.z);
        Vector2 tempCamForward = new Vector2(GameManager._instance.MainCamera.transform.forward.x, GameManager._instance.MainCamera.transform.forward.z);

        Vector2 direction = (tempTargetPos - tempPlayerPos).normalized;

        float angle = Vector2.SignedAngle(tempCamForward, direction);

        RectTransform.anchoredPosition = Vector2FromAngle(angle + 90f) * 450f;

        if (IsAllowedToSetColor)
            image.color = new Color(color.r, color.g, color.b, (60f - (tempTargetPos - tempPlayerPos).magnitude) / 60f);
    }
    public Vector2 Vector2FromAngle(float a)
    {
        a *= Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(a), Mathf.Sin(a));
    }
}
