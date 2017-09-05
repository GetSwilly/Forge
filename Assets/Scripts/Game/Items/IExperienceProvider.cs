using UnityEngine;
using System.Collections;

public interface IExperienceProvider {
    
    bool IsUsable { get; }

    float ExperienceValue { get; }

    ProviderActivationType ActivationType { get; }
}
