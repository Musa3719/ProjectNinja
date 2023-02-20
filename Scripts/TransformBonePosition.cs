using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformBonePosition : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshRenderer _mesh;
    [SerializeField]
    private Transform _targetTransform;

    public void TransformPosition()
    {
        StartCoroutine(TransformPositionCoroutine());
    }
    IEnumerator TransformPositionCoroutine()
    {
        yield return new WaitForSeconds(7f);

        Vector3 distance = _targetTransform.position - transform.position;
        transform.position += distance;
        _targetTransform.position -= distance;

        _mesh.updateWhenOffscreen = false;
    }
}
