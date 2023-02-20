using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class InvertedMirror : MonoBehaviour
{
    private bool _isPlayerGainedStaminaBefore;
    private void OnTriggerEnter(Collider other)
    {
        if(other!=null && other.gameObject!=null && other.GetComponent<Rigidbody>() != null && !other.CompareTag("HitBox"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            rb.transform.localEulerAngles = new Vector3(rb.transform.localEulerAngles.x, transform.localEulerAngles.y, rb.transform.localEulerAngles.z);
            Vector3 speedWithoutY = rb.velocity;
            speedWithoutY.y = 0f;
            float yVelocity = rb.velocity.y;
            rb.velocity = speedWithoutY.magnitude * 1.5f * rb.transform.forward + Vector3.up * yVelocity;

            if (other.CompareTag("Player"))
            {
                //VFX and Sound
                GameManager._instance.PlayerPlayAnim("SkillUse", 0.4f);

                if (!_isPlayerGainedStaminaBefore)
                {
                    _isPlayerGainedStaminaBefore = true;
                    GameManager._instance.PlayerGainStamina(60f);
                }
            }
            
        }
    }
}
