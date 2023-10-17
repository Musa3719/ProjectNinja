using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagForAnim : MonoBehaviour
{
    private Coroutine _closeFlagCoroutine;
    private void Awake()
    {
        GetComponent<Animator>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.gameObject != null) && other.gameObject.CompareTag("Player") && GameManager._instance.IsPlayerOnWall)
        {
            GetComponent<Animator>().enabled = true;
            GetComponent<Animator>().Play("ToWall");

            if (_closeFlagCoroutine != null)
                StopCoroutine(_closeFlagCoroutine);
            _closeFlagCoroutine = StartCoroutine(CloseFlagCoroutine());
        }
    }
    private IEnumerator CloseFlagCoroutine()
    {
        yield return new WaitForSeconds(5f);
        GetComponent<Animator>().enabled = false;
    }
}
