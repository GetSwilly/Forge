using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAcquirableObject {

    event EventHandler ObjectAcquired;

   // void Acquire();
    void Drop();

    GameObject Object { get; }
}
