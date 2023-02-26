using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleTrap : MonoBehaviour, ITrap, IKillObject
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
        transform.parent.parent.GetComponent<Animator>().Play("AngleTrapActivated");
        transform.parent.parent.GetComponent<Animator>().speed = 0.3f;
        _Activated = true;
    }

    public void Kill(IKillable killable, Vector3 dir, float killersVelocityMagnitude)
    {
        killable.Die(dir, killersVelocityMagnitude);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (_Activated && other != null && other.CompareTag("HitBox"))
        {
            Kill(GameManager._instance.GetHitBoxIKillable(other), Vector3.zero, 0f);
        }
    }

}