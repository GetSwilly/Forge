using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ForgeSite))]
public class BuildMenuInflater : MenuInflater
{

    ForgeSite m_Site;

    protected override void Awake()
    {
        base.Awake();

        m_Site = GetComponent<ForgeSite>();
    }


    protected override void AddButtons()
    {
        throw new NotImplementedException();
    }
}
