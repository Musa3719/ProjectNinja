using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereTrap : MonoBehaviour, ITrap, IKillObject
{
    public bool _Activated { get; set; }

    [SerializeField]
    private bool _checkForActivate;
    public bool _CheckForActivate { get { return _checkForActivate; } set { _checkForActivate = value; } }


    void Update()
    {
        if (!_Activated && _CheckForActivate)
        {
            Activate();
        }
    }
    public void Activate()
    {
        GetComponent<Rigidbody>().isKinematic = false;
        _Activated = true;
    }

    public void Kill(IKillable killable, Vector3 dir, float killersVelocityMagnitude)
    {
        killable.Die(dir, killersVelocityMagnitude);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (_Activated && collision.collider != null && collision.collider.CompareTag("HitBox"))
        {
            Kill(GameManager._instance.GetHitBoxIKillable(collision.collider), Vector3.zero, 0f);
        }
    }
}
