using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    struct TransformWithTime
    {
        public float time;
        public Vector3 pos;

        public TransformWithTime(float time, Vector3 pos)
        {
            this.time = time;
            this.pos = pos;
        }
    }

    [SerializeField] private Transform _transform;

    private List<TransformWithTime> _list;
    private void Awake()
    {
        _list = new List<TransformWithTime>();
    }

    private void Update()
    {
        _list.Add(new TransformWithTime(Time.time, _transform.position));
        if (_list.Count > 500)
            _list.RemoveAt(0);
        transform.position = GetPastPosition(0.15f);
    }
    private Vector3 GetPastPosition(float second)
    {
        for (int i = _list.Count - 1; i >= 0; i--)
        {
            if (Time.time - _list[i].time > second)
            {
                return _list[i].pos;
            }
        }
        return _list[0].pos;
    }
}
