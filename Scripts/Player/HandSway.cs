using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSway : MonoBehaviour
{

    bool canSway = true;

    public float Amount = 0.025f;
    public float maxAmount = 0.025f;
    public float SmoothAmount = 2;

    private Vector3 initialPositon;

    // Start is called before the first frame update
    void Start()
    {
        initialPositon = transform.localPosition;
        canSway = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerCombat._instance.IsDead) return;

        if (canSway == true)
        {
            float movementX = -InputHandler.GetAxis("Mouse X") * Amount;
            float movementY = -InputHandler.GetAxis("Mouse Y") * Amount;

            if (PlayerCombat._instance._IsAttacking)
            {
                movementX /= 2f;
                movementY /= 2f;
            }

            movementX = Mathf.Clamp(movementX, -maxAmount, maxAmount);
            movementY = Mathf.Clamp(movementY, -maxAmount, maxAmount);

            Vector3 finalPosition = new Vector3(movementX, movementY, 0);
            transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPositon, Time.deltaTime * SmoothAmount);
        }

    }
}