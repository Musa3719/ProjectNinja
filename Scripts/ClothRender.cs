using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothRender : MonoBehaviour
{
    private Cloth _cloth;
    private Transform _parent;
    private Collider _parentCollider;
    private void Awake()
    {
        _cloth = GetComponent<Cloth>();
        _parent = GetParent();
        _parentCollider = _parent.GetComponent<Collider>();
    }
    private void Update()
    {
        Ray ray = new Ray(_parent.position + _parent.transform.forward, (GameManager._instance.PlayerRb.transform.position - _parent.position).normalized);
        Physics.Raycast(ray, out RaycastHit hit, 12f, GameManager._instance.LayerMaskForVisibleWithSolidTransparent);
        if (_parentCollider.enabled && hit.collider != null && hit.collider.CompareTag("Player"))
        {
            if (!GameManager._instance.Cloths.Contains(_cloth))
                GameManager._instance.Cloths.Add(_cloth);
        }
        else
        {
            if (GameManager._instance.Cloths.Contains(_cloth))
                GameManager._instance.Cloths.Remove(_cloth);
        }
    }
    private Transform GetParent()
    {
        Transform parent = transform;
        while (parent.parent != null)
            parent = parent.parent;
        return parent;
    }
}
