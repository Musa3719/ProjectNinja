using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    void DoState(Rigidbody rb);
    void DoStateLateUpdate(Rigidbody rb);
    void DoStateFixedUpdate(Rigidbody rb);
}
