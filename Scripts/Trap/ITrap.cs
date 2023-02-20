using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface ITrap
{
    public bool _Activated { get; set; }
    public bool _CheckForActivate { get; set; }
    void Activate();
}
