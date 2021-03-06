﻿using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Gun))]
public class Turret : ForgeableObject
{

    [SerializeField]
    float minimumFireAngle = 1f;

    [SerializeField]
    float rotationSmoothing = 1f;

    List<GameObject> nearbyTargets = new List<GameObject>();

    GameObject target;
    Gun m_Gun;

    protected override void Awake()
    {
        base.Awake();

        m_Gun = GetComponent<Gun>();
    }


    void Update()
    {
        if (target == null)
        {
            SelectTarget();
        }
        else if (!target.gameObject.activeInHierarchy)
        {
            RemoveTarget(target);
            SelectTarget();
        }

        if (target == null)
            return;

        Vector3 targetVector = target.transform.position - m_Pivot.position;

        Quaternion targetRotation = Quaternion.LookRotation(targetVector);
        m_Pivot.rotation = Quaternion.Slerp(m_Pivot.rotation, targetRotation, RotationSmoothing * Time.deltaTime);


        if (Vector3.Angle(m_Pivot.forward, targetVector) <= MinimumFireAngle)
        {
            if (showDebug)
            {
                Debug.DrawRay(m_Pivot.position, m_Pivot.forward * 10f, Color.yellow);
            }


            Fire();
        }


        if (showDebug)
        {
            Debug.DrawLine(m_Pivot.position, target.transform.position);
        }
    }


    public void Fire()
    {
        m_Gun.ActivatePrimary();
    }

    public void SelectTarget()
    {
        if (target != null || nearbyTargets.Count == 0)
            return;

        target = nearbyTargets[0];
    }

    void AddTarget(GameObject _target)
    {
        if (_target == null || !_target.activeInHierarchy || nearbyTargets.Contains(_target))
            return;

        nearbyTargets.Add(_target);
    }
    void RemoveTarget(GameObject _target)
    {
        if (_target == null)
            return;

        nearbyTargets.Remove(_target);

        if (_target == target)
        {
            target = null;
        }
    }

    #region Accessors
    
    public float MinimumFireAngle
    {
        get { return minimumFireAngle; }
        private set { minimumFireAngle = Mathf.Clamp(value, 0f, 180f); }
    }
    public float RotationSmoothing
    {
        get { return rotationSmoothing; }
        private set { rotationSmoothing = Mathf.Clamp(value, 0f, value); }
    }

    #endregion


    protected override void SightMaintained(GameObject obj)
    {
        Team teamMember = obj.GetComponent<Team>();

        if (teamMember != null && m_Team.IsEnemy(teamMember))
        {
            AddTarget(obj);
        }
    }
    protected override void SightLost(GameObject obj)
    {
        Team teamMember = obj.GetComponent<Team>();

        if (teamMember != null && m_Team.IsEnemy(teamMember))
        {
            RemoveTarget(obj);
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();
       
        MinimumFireAngle = MinimumFireAngle;
        RotationSmoothing = RotationSmoothing;
    }
}
