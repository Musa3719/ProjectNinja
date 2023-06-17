using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBobber : MonoBehaviour
{
    public Vector3 bobOffset;

    private float timer = 0.0f;
    float bobbingSpeed = 0.12f;
    float bobbingAmount = 0.012f;
    const float bobbingIdleMultiplier = 0.4f;
    const float bobbingSpeedIdleMultiplier = 0.5f;
    float midpoint = 0f;

    void Update()
    {
        if (GameManager._instance.isGameStopped || GameManager._instance.isOnCutscene || GameManager._instance.isPlayerDead)
        {
            bobOffset = Vector3.zero;
            return;
        }

        bobbingAmount = 0.05f + (Mathf.Clamp(PlayerStateController._instance._rb.velocity.magnitude, 0.1f, 14f) - 7f) / 180f;
        bobbingSpeed = 0.13f + (Mathf.Clamp(PlayerStateController._instance._rb.velocity.magnitude, 0.1f, 14f) + 5f) / 120f;

        var moveState = PlayerStateController._instance._playerState as PlayerStates.Movement;
        if (moveState != null && moveState._isCrouching)
        {
            bobbingAmount /= 2.5f;
            bobbingSpeed /= 2.5f;
        }

        float waveslice = 0.0f;
        float horizontal = InputHandler.GetAxis("Horizontal");
        float vertical = InputHandler.GetAxis("Vertical");

        Vector3 cSharpConversion = transform.localPosition;

        if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)
        {
            //timer = 0.0f;
            vertical = 1f;
            bobbingAmount *= bobbingIdleMultiplier;
            bobbingSpeed *= bobbingSpeedIdleMultiplier;
        }

        //these were in the else before
        waveslice = Mathf.Sin(timer);
        timer = timer + bobbingSpeed;
        if (timer > Mathf.PI * 2)
        {
            timer = timer - (Mathf.PI * 2);
        }

        if (waveslice != 0)
        {
            float translateChange = waveslice * bobbingAmount;
            float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
            totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
            translateChange = totalAxes * translateChange;
            cSharpConversion.y = midpoint + translateChange;
        }
        else
        {
            cSharpConversion.y = midpoint;
        }

        bobOffset = new Vector3(0f, cSharpConversion.y, 0f);
    }



}