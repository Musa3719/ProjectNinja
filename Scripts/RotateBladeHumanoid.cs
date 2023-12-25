using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBladeHumanoid : MonoBehaviour
{
    [SerializeField]
    private bool _haveWeapon;

    private float _speed;
    private float _lastSpeed;
    private float _lerpSpeed;

    private Rigidbody _rb;
    private IKillable _killable;
    private void Awake()
    {
        _rb = GetParent(transform).GetComponent<Rigidbody>();
        _killable = GetParent(transform).GetComponent<IKillable>();
        _lerpSpeed = 3f;
    }
    void LateUpdate()
    {
        _lastSpeed = _speed;
        _speed = 1600f;
        if (_killable.AttackCollider != null && _killable.AttackCollider.activeInHierarchy) _speed *= 8f;
        else if (_rb.velocity.magnitude > 3f) _speed *= 2f;
        else if (_rb.velocity.magnitude > 7f) _speed *= 3f;
        else if (_rb.velocity.magnitude > 10f) _speed *= 5f;

        if (_haveWeapon) _speed /= 5f;

        _speed = Mathf.Lerp(_lastSpeed, _speed, Time.deltaTime * _lerpSpeed);
        transform.RotateAround(transform.position, transform.up, Time.deltaTime * _speed);
    }

    private Transform GetParent(Transform getParent)
    {
        while (getParent.parent != null)
            getParent = getParent.parent;
        return getParent;
    }

    public void DisableHaveWeapon()
    {
        _haveWeapon = false;
    }
}
