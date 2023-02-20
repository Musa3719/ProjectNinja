using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBlade : MonoBehaviour
{
    private float _speed;
    private float _lastSpeed;
    private float _lastZAngle;

    void LateUpdate()
    {
        if (GameManager._instance.isGameStopped) { transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, _lastZAngle); return; }

        _lastSpeed = _speed;
        _speed = Mathf.Clamp(GameManager._instance.PlayerRb.velocity.magnitude, 0.25f, 200f) * 50f;
        if (GameManager._instance.PlayerRb.velocity.magnitude > GameManager._instance.PlayerRunningSpeed + 1f) _speed *= 1.5f;
        if (GameManager._instance.isPlayerAttacking) _speed *= 10f;

        Vector2 tempDir = new Vector2(GameManager._instance.PlayerRb.transform.forward.x, GameManager._instance.PlayerRb.transform.forward.z);
        Vector2 tempVel = new Vector2(GameManager._instance.PlayerRb.velocity.x, GameManager._instance.PlayerRb.velocity.z);
        float angle = Vector2.Angle(tempVel, tempDir);
        if (angle > 120f) _speed = -_speed;
        else if (angle > 75f) { float sAngle = Vector2.SignedAngle(tempVel, tempDir); _speed = sAngle <= 0f ? _speed : -_speed; }

        float lerpSpeed = 3f;
        if (GameManager._instance.PlayerRb.velocity.magnitude < 3f) lerpSpeed /= 1.25f;
        _speed = Mathf.Lerp(_lastSpeed, _speed, Time.deltaTime * lerpSpeed);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, _lastZAngle + Time.deltaTime * _speed);
        _lastZAngle = transform.localEulerAngles.z;

        GameManager._instance.BladeSpeed = _speed;
    }
}
