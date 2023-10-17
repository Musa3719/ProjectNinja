using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereTrap : MonoBehaviour, ITrap, IKillObject
{
    public bool _Activated { get; set; }
    [SerializeField]
    private bool _isStatic;

    [SerializeField]
    private bool _checkForActivate;
    public bool _CheckForActivate { get { return _checkForActivate; } set { _checkForActivate = value; } }

    public GameObject Owner => gameObject;

    void Update()
    {
        if (!_Activated && _CheckForActivate && !_isStatic)
        {
            Activate();
        }
    }
    public void Activate()
    {
        GetComponent<Rigidbody>().isKinematic = false;
        _Activated = true;
    }

    public void Kill(IKillable killable, Vector3 dir, float killersVelocityMagnitude, IKillObject killer)
    {
        killable.Die(dir, killersVelocityMagnitude, killer);
    }
   
    private void OnTriggerEnter(Collider other)
    {
        if ((_Activated || _isStatic) && other != null && other.CompareTag("HitBox"))
        {
            Kill(GameManager._instance.GetHitBoxIKillable(other), GetComponent<Rigidbody>().velocity.normalized, GetComponent<Rigidbody>().velocity.magnitude, this);
        }
    }
}
