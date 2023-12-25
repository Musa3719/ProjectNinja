using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NailTrap : MonoBehaviour, ITrap
{
    public bool _Activated { get; set; }
    public bool _CheckForActivate { get; set; }
    private bool _isOpenTrapCoroutineActive;
    private Coroutine _openTrapCoroutine;

    public void Activate()
    {
        if (_openTrapCoroutine != null)
            StopCoroutine(_openTrapCoroutine);
        _openTrapCoroutine = StartCoroutine(OpenTrap());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.CompareTag("HitBox"))
        {
            if (!_isOpenTrapCoroutineActive)
                Activate();
        }
    }
    private IEnumerator OpenTrap()
    {
        _isOpenTrapCoroutineActive = true;
        yield return new WaitForSeconds(0.525f);

        Transform childTransform = transform.Find("WalkTrapBlade");
        Transform childTransform2 = transform.Find("NailTrapKiller");

        //open
        while (childTransform.localPosition.y <= 0.32f || childTransform2.localPosition.y <= 1.1f)
        {
            if(childTransform.localPosition.y <= 0.32f)
                childTransform.localPosition = new Vector3(childTransform.localPosition.x, Mathf.Lerp(childTransform.localPosition.y, 0.4f, Time.deltaTime * 25f), childTransform.localPosition.z);
            if(childTransform2.localPosition.y <= 1.4f)
                childTransform2.localPosition = new Vector3(childTransform2.localPosition.x, Mathf.Lerp(childTransform2.localPosition.y, 1.5f, Time.deltaTime * 25f), childTransform2.localPosition.z);
            yield return null;
        }
        childTransform.localPosition = new Vector3(childTransform.localPosition.x, 0.4f, childTransform.localPosition.z);
        childTransform2.localPosition = new Vector3(childTransform2.localPosition.x, 1.5f, childTransform2.localPosition.z);

        //wait for close
        yield return new WaitForSeconds(0.45f);
        _isOpenTrapCoroutineActive = false;

        //close it
        while (childTransform.localPosition.y >= -2f || childTransform2.localPosition.y >= 0.22f)
        {
            if(childTransform.localPosition.y >= -2f)
                childTransform.localPosition = new Vector3(childTransform.localPosition.x, Mathf.Lerp(childTransform.localPosition.y, -2.15f, Time.deltaTime * 4f), childTransform.localPosition.z);
            if(childTransform2.localPosition.y >= 0.22f)
                childTransform2.localPosition = new Vector3(childTransform2.localPosition.x, Mathf.Lerp(childTransform2.localPosition.y, 0.09f, Time.deltaTime * 25f), childTransform2.localPosition.z);
            yield return null;
        }
        childTransform.localPosition = new Vector3(childTransform.localPosition.x, -2.06f, childTransform.localPosition.z);
        childTransform2.localPosition = new Vector3(childTransform2.localPosition.x, 0.15f, childTransform2.localPosition.z);
    }
}
