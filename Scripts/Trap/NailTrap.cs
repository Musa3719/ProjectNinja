using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NailTrap : MonoBehaviour, ITrap
{
    public bool _Activated { get; set; }
    public bool _CheckForActivate { get; set; }

    private Coroutine OpenTrapCoroutine;

    public void Activate()
    {
        OpenTrapCoroutine = StartCoroutine(OpenTrap());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.CompareTag("HitBox"))
        {
            if (OpenTrapCoroutine == null)
                Activate();
        }
    }
    private IEnumerator OpenTrap()
    {
        yield return new WaitForSeconds(0.6f);

        while (transform.localPosition.y <= 0.42f)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(transform.localPosition.y, 0.5f, Time.deltaTime * 25f), transform.localPosition.z);
            yield return null;
        }
        transform.localPosition = new Vector3(transform.localPosition.x, 0.5f, transform.localPosition.z);

        //wait for close
        yield return new WaitForSeconds(1.2f);

        //close it
        while (transform.localPosition.y >= 0.22f)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(transform.localPosition.y, 0.15f, Time.deltaTime * 25f), transform.localPosition.z);
            yield return null;
        }
        transform.localPosition = new Vector3(transform.localPosition.x, 0.15f, transform.localPosition.z);
    }
}
