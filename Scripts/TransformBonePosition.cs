using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformBonePosition : MonoBehaviour
{
    [SerializeField]
    private Transform _targetTransform;

    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private void Awake()
    {
        skinnedMeshRenderers = transform.parent.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    public void TransformPosition()
    {
        StartCoroutine(TransformPositionCoroutine());
    }
    IEnumerator TransformPositionCoroutine()
    {
        yield return new WaitForSeconds(7f);

        Vector3 distance = _targetTransform.position - transform.position;
        transform.parent.parent.position += distance;
        _targetTransform.position -= distance;

        foreach (SkinnedMeshRenderer mesh in skinnedMeshRenderers)
        {
            mesh.updateWhenOffscreen = false;
        }
    }
}
