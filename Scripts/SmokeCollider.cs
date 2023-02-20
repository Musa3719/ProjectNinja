using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SmokeCollider : MonoBehaviour
{
    [SerializeField]
    private float _MaxScale;
    [SerializeField]
    private float _GrowthByTime;
    [SerializeField]
    private float _LifeTime;

    private float _startTime;
    private void Awake()
    {
        _startTime = Time.time;
        transform.parent.transform.position = new Vector3(transform.parent.transform.position.x, Random.Range(-0.3f, 0.5f), transform.parent.transform.position.z);
    }

    void Update()
    {
        if (transform.localScale.x < _MaxScale)
        {
            //transform.localScale += Vector3.one * _GrowthByTime / transform.localScale.x * Time.deltaTime;
        }

        if (Time.time - _startTime >= _LifeTime)
        {
            foreach (var item in transform.parent.GetComponentsInChildren<VisualEffect>())
            {
                item.Stop();
            }
            Destroy(transform.parent.gameObject, 3f);
        }
    }
    
}
