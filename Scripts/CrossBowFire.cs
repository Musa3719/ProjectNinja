using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossBowFire : MonoBehaviour
{
    [SerializeField] private Transform _startTransform;
    [SerializeField] private AudioClip _fireSound;
    private float _lastAttackTime = 0f;
    public void FireArrow()
    {
        if (_lastAttackTime + 1f < Time.time)
        {
            if (Steamworks.SteamClient.IsValid && !new Achievement("CrossbowAchievement").State)
                new Achievement("CrossbowAchievement").Trigger();

            SoundManager._instance.PlaySound(_fireSound, transform.position, 0.5f, false, Random.Range(0.9f, 1.1f));
            _lastAttackTime = Time.time;
            Invoke("FireForWait", 0.15f);
        }
    }
    private void FireForWait()
    {
        Vector3 forward = -transform.forward;
        GameObject arrow = Instantiate(GameManager._instance.ArrowPrefab, _startTransform.position, Quaternion.identity);
        arrow.GetComponentInChildren<Projectile>().WhenTriggered = arrow.GetComponentInChildren<Projectile>().WhenTriggeredForKnife;
        arrow.GetComponentInChildren<Projectile>().IgnoreCollisionCollider = GetComponent<Collider>();
        arrow.GetComponentInChildren<Projectile>().isTrap = true;
        arrow.GetComponentInChildren<Rigidbody>().velocity = forward * 35f;
        arrow.transform.forward = forward;
    }
}
