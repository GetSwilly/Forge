using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IForgeable {

    void Initialize(ForgeSite activator);
    void Initialize(ForgeSite activator, ITeamMember team);

    GameObject GameObject { get; }
    string Name { get; set; }
}
