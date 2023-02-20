using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKillObject
{
    public void Kill(IKillable killable, Vector3 dir, float killersVelocityMagnitude);
}