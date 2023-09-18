using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyCore : MonoBehaviour
{
    private void Awake()
    {
        Transform[] childs = new Transform[7];

        int i = 0;
        foreach (Transform child in transform)
        {
            childs[i] = child;
            i++;
        }
        foreach (Transform child in childs)
        {
            child.parent = null;
        }
        Destroy(gameObject);
    }
}
