using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlaneSoundType
{
    Metal,
    TechMetal,
    ThinMetal,
    Glass,
    Marble,
    Brick,
    Stone,
    Dirt,
    Grass,
    Wood,
    Fabric,
    Water,
    Ice
}
public class PlaneSound : MonoBehaviour
{
    [SerializeField]
    private PlaneSoundType _planeSoundType;

    public PlaneSoundType PlaneSoundType => _planeSoundType;
}
