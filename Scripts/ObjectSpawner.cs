using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject Prefab;
    [SerializeField] private float CheckFrequency;
    [SerializeField] private float Chance;
    private float _counter;

    private void Update()
    {
        _counter += Time.deltaTime;
        if (_counter > CheckFrequency)
        {
            if (Random.Range(0, (1f / Chance) - 1) == 0)
            {
                Instantiate(Prefab, transform.position, Quaternion.identity);
            }
            _counter = 0f;
        }
    }
}
