using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKillObject
{
    public GameObject Owner { get; }
    public void Kill(IKillable killable, Vector3 dir, float killersVelocityMagnitude, IKillObject killer);
}