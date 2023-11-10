using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothRender : MonoBehaviour
{
    private void Awake()
    {
        GameManager._instance.Cloths.Add(GetComponent<Cloth>());
    }
}
