using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleTrap : MonoBehaviour, ITrap, IKillObject
{
    public bool _Activated { get; set; }

    [SerializeField]
    private bool _checkForActivate;
    public bool _CheckForActivate { get { return _checkForActivate; } set { _checkForActivate = value; } }
    public GameObject Owner => gameObject;


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

    public void Kill(IKillable killable, Vector3 dir, float killersVelocityMagnitude, IKillObject killer)
    {
        killable.Die(dir, killersVelocityMagnitude, killer, true);
    }
    
    private void OnTriggerEnter(Collider other)
    {

        if (other.transform.parent != null && other.transform.parent.CompareTag("Wolf"))
        {
            other.transform.parent.GetComponent<Wolf>().Die(Vector3.zero, 0f, this);
            return;
        }

        if (_Activated && other != null && other.CompareTag("HitBox"))
        {
            Kill(GameManager._instance.GetHitBoxIKillable(other), Vector3.zero, 0f, this);
        }
    }

}
