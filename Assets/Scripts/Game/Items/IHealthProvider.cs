using UnityEngine;
using System.Collections;

public interface IHealthProvider {

    bool IsUsable { get; }

    float HealthValue { get; }

    ProviderActivationType ActivationType { get; }
}
