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
        GameManager._instance.CallForAction(() => decalProjector.decalLayerMask = DecalLayerEnum.DecalLayerDefault, 2f);
    }

    private void LateUpdate()
    {
        transform.position = FollowingTransform.position + LocalPosition.x * FollowingTransform.right + LocalPosition.y * FollowingTransform.up + +LocalPosition.z * FollowingTransform.forward;
        decalProjector.fadeFactor = Mathf.Lerp(decalProjector.fadeFactor, 1f, Time.unscaledDeltaTime * 4f);
    }
}
