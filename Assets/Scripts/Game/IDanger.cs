using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDanger {

    float GetRemainingLifetime();

    Vector3 Position { get; }
    Vector3 Direction { get; }
    
    LayerMask FriendlyMask { get; }

    int Power { get; }
}
