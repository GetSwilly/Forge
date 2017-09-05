using UnityEngine;
using System.Collections;

public interface IMovementAffector {

    void EnableMovementEffects();

    void DisableMovementEffects();



    float SpeedEffect { get; }
    float RotationEffect { get; }
}
