using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagForAnim : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != null && other.gameObject.CompareTag("Player") && GameManager._instance.IsPlayerOnWall)
        {
            GetComponent<Animator>().Play("ToWall");
        }
    }
}
