using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTrap : MonoBehaviour, ITrap
{
    public bool _Activated { get; set; }

    [SerializeField]
    private bool _checkForActivate;
    public bool _CheckForActivate { get { return _checkForActivate; } set { _checkForActivate = value; GameManager._instance.CallForAction(() => { _checkForActivate = false; }, 0.1f); } }

    private float _waitForReActivateTime;
    private float _projectileSpeed;
    private void Awake()
    {
        _waitForReActivateTime = 3.5f;
        _projectileSpeed = 20f;
    }

    void Update()
    {
        if (!_Activated && _CheckForActivate)
        {
            Activate();
        }
    }
    public void Activate()
    {
        GameObject newArrow = Instantiate(GameManager._instance.ArrowPrefab, transform.position, Quaternion.identity);
        newArrow.GetComponentInChildren<Projectile>().IgnoreCollisionCollider = null;
        newArrow.GetComponentInChildren<Projectile>().WhenTriggered = newArrow.GetComponentInChildren<Projectile>().WhenTriggeredForKnife;
        newArrow.GetComponentInChildren<Projectile>().isTrap = true;
        newArrow.GetComponentInChildren<Rigidbody>().velocity = transform.forward * _projectileSpeed;
        newArrow.transform.forward = transform.forward;
        Destroy(newArrow, 10f);

        _Activated = true;
        transform.Find("Trigger").gameObject.SetActive(false);
        GameManager._instance.CallForAction(() => { _Activated = false; transform.Find("Trigger").gameObject.SetActive(true); }, _waitForReActivateTime);
    }
}
