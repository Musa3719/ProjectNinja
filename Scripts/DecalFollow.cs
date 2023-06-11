using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class DecalFollow : MonoBehaviour
{
    public Transform FollowingTransform;
    public Vector3 LocalPosition;

    private DecalProjector decalProjector;
    private void Awake()
    {
        decalProjector = GetComponent<DecalProjector>();
    }

    private void LateUpdate()
    {
        transform.position = FollowingTransform.position + LocalPosition.x * FollowingTransform.right + LocalPosition.y * FollowingTransform.up + +LocalPosition.z * FollowingTransform.forward;
        decalProjector.fadeFactor = Mathf.Lerp(decalProjector.fadeFactor, 1f, Time.deltaTime * 0.25f);
    }
}
