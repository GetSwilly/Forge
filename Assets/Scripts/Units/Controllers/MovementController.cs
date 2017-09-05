﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour {

    static readonly float GROUND_CHECK_DISTANCE = .6f;
    static readonly float SLOPE_CHECK_HEIGHT = 0.05f;

    [Tooltip("Should show debug information?")]
    [SerializeField]
    bool showDebug = false;


    [Tooltip("Speed of movement")]
    [SerializeField]
    DeviatingFloat m_Speed;

    float speedValue;
	float additionalSpeedMultiplier = 0f;


    //Move with different speed based on direction of movement
    [Tooltip("Ratio applied to speed based on direction of travel in relation of angle to directly forward.")]
    [SerializeField]
    AnimationCurve directionalSpeedupCurve = AnimationCurve.Linear(0f, 1f, 180f, .5f);

    //Strength of returning to zero movement
    [Tooltip("Ratio applied to speed whenever zero movement is desired.")]
    [SerializeField]
    [Range(0.25f, 5f)]
    float stoppingPower = 1.5f;


    [Tooltip("Speed at which object is capable of rotating.")]
    [SerializeField]
    [Range(0f, 10f)]
    float rotationSpeed = 5f;

    //Time between steps
    [Tooltip("Time delay between object's steps. Object is not capable of applied movement during this time.")]
    [SerializeField]
    float stepTime = 0f;
    bool canStep = true;

 
    [SerializeField]
    LayerMask groundMask;


	protected Transform m_Transform;
	Rigidbody m_Rigidbody;

    List<float> speedMultipliers = new List<float>();
    float totalSpeedMultiplier = 1f;

    List<float> rotationMultipliers = new List<float>();
    float totalRotationMultiplier = 1f;


    Vector3 bounds;
    float maxBoundsValue;


	public virtual void Awake ()
    {
		m_Transform = this.GetComponent<Transform>();
		m_Rigidbody = this.GetComponent<Rigidbody>();

        
		speedValue = (float)Utilities.GetRandomGaussian(m_Speed);
	}
    void Start()
    {
        bounds = Utilities.CalculateObjectBounds(gameObject, false);
        maxBoundsValue = Mathf.Max(bounds.x, bounds.z);
    }

    IEnumerator StepWait()
    {
        canStep = false;
        yield return new WaitForSeconds(stepTime);
        canStep = true;
    }

    public void MoveInLocalDirection(Vector3 localDir)
    {
        Move(m_Transform.TransformDirection(localDir));
    }

	public virtual void Move(Vector3 moveDir)
    {
        if (!canStep || !this.enabled)
            return;
        

		if(moveDir.magnitude == 0)
			return;
       
        Vector3 originPos = m_Transform.position + (Vector3.down * (bounds.y / 2f)) + (Vector3.up * SLOPE_CHECK_HEIGHT);

       if (showDebug)
            {
                Debug.DrawRay(originPos, moveDir.normalized * GROUND_CHECK_DISTANCE, Color.red);
            }

        if (moveDir.magnitude > 1)
			moveDir.Normalize();

        if (showDebug)
            Debug.DrawLine(m_Transform.position, m_Transform.position + (moveDir * 3f), Color.cyan);




        Vector3 velVector = m_Rigidbody.velocity;
        velVector.y = 0;
        velVector.Normalize();

        float dirSpeedup = directionalSpeedupCurve.Evaluate(Vector3.Angle(m_Transform.forward, moveDir));
        float desiredAcceleration = Utilities.IsInOppositeDirection(moveDir, velVector) ? Speed * stoppingPower : Speed;

        Vector3 forceVector = moveDir.normalized * desiredAcceleration * dirSpeedup * Time.deltaTime;

		m_Rigidbody.AddForce(forceVector, ForceMode.Acceleration);

        if(stepTime > 0)
           StartCoroutine(StepWait());
	}
    

    public void RotateTowards(Transform t)
    {
        RotateTowards(t, 1f);
    }
    public void RotateTowards(Transform t, float _multiplier)
    {
		if(t != null)
			RotateTowards(t.position, _multiplier);
	}
    public void RotateTowards(Vector3 lookAtPosition)
    {
        RotateTowards(lookAtPosition, 1f);
    }
	public void RotateTowards(Vector3 lookAtPosition, float _multiplier)
    {
        if (!this.enabled)
            return;


		Vector3 dispVector = lookAtPosition - m_Transform.position;

        /*
        if (showDebug)
        {
            Debug.Log(string.Format("Rotate towards {0}. Distance {1}.", lookAtPosition, dispVector.magnitude));
            Debug.DrawLine(myTransform.position, lookAtPosition, Color.yellow);
        }
        */

		if (dispVector.magnitude <= maxBoundsValue)
			return;

		//Quaternion desiredQ = Quaternion.LookRotation(Vector3.forward, lookAtPosition - myTransform.position);

		//myTransform.rotation =  Quaternion.Slerp(myTransform.rotation, desiredQ, rotationSmoothing);

		Quaternion desiredQ  = Quaternion.LookRotation(lookAtPosition - m_Transform.position);

		m_Transform.rotation = Quaternion.RotateTowards(m_Transform.rotation, desiredQ, RotationSpeed * _multiplier);// * Time.deltaTime);
	}


	public void MoveTowards(Vector3 targetPosition)
    {
		RotateTowards(targetPosition);
        Move(targetPosition - m_Transform.position);
	}




    public void AddSpeedMultiplier(float _multiplier)
    {
        if (Mathf.Approximately(_multiplier, 1f))
            return;

        speedMultipliers.Add(Mathf.Abs(_multiplier));
        totalSpeedMultiplier *= _multiplier;
    }
    public void RemoveSpeedMultiplier(float _multiplier)
    {
        if(speedMultipliers.Count == 1 && Mathf.Approximately(speedMultipliers[0], _multiplier))
        {
            speedMultipliers.Clear();
            totalSpeedMultiplier = 1f;
            return;
        }



        int index = -1;

        for (int i = 0; i < speedMultipliers.Count; i++)
        {
            if (Mathf.Approximately(speedMultipliers[i], _multiplier))
            {
                index = i;
                break;
            }
        }


        if (index == -1)
            return;

        speedMultipliers.RemoveAt(index);

        if (_multiplier == 0)
        {

            totalSpeedMultiplier = 1f;


            for (int i = 0; i < speedMultipliers.Count; i++)
            {
                totalSpeedMultiplier *= speedMultipliers[i];
            }
        }
        else
        {
            totalSpeedMultiplier /= _multiplier;
        }

    }

    public void AddRotationMultiplier(float _multiplier)
    {
        if(Mathf.Approximately(_multiplier, 1f))
            return;

        rotationMultipliers.Add(Mathf.Abs(_multiplier));
        totalRotationMultiplier *= _multiplier;
    }
    public void RemoveRotationMultiplier(float _multiplier)
    {
        if (rotationMultipliers.Count == 1 && Mathf.Approximately(rotationMultipliers[0],_multiplier))
        {
            rotationMultipliers.Clear();
            totalRotationMultiplier = 1f;
            return;
        }


        int index = -1;

        for (int i = 0; i < rotationMultipliers.Count; i++)
        {
            if (Mathf.Approximately(rotationMultipliers[i], _multiplier))
            {
                index = i;
                break;
            }
        }


        if (index == -1)
            return;

        rotationMultipliers.RemoveAt(index);

        if (_multiplier == 0)
        {

            totalRotationMultiplier = 1f;


            for (int i = 0; i < rotationMultipliers.Count; i++)
            {
                totalRotationMultiplier *= rotationMultipliers[i];
            }
        }
        else
        {
            totalRotationMultiplier /= _multiplier;
        }

    }

   

    #region Accessors

    public float Speed
    {
		get
        {
          return speedValue * (1f + AdditionalSpeedMultiplier) * SpeedMultiplier;
        }
	}
	public float SpeedMultiplier
    {
		get { return totalSpeedMultiplier; }
        set { totalSpeedMultiplier = value; }
	}
    

	public float RotationSpeed
    {
		get { return rotationSpeed * RotationMultiplier; }
	}
	public float RotationMultiplier
    {
		get { return totalRotationMultiplier; }
		set { totalRotationMultiplier = value; }
	}

    public float AdditionalSpeedMultiplier
    {
        get { return additionalSpeedMultiplier; }
        set { additionalSpeedMultiplier = value; }
    }
	public Vector3 Velocity
    {
		get
        {
            if (m_Rigidbody != null)
                return m_Rigidbody.velocity;

            return Vector3.zero;
        }
	}

	#endregion




    void OnValidate()
    {
        m_Speed.Mean = Mathf.Max(0f, m_Speed.Mean);
        stepTime = Mathf.Max(0f, stepTime);

        Utilities.ValidateCurve_Times(directionalSpeedupCurve, 0f, 180f);
    }


    void OnDrawGizmos()
    {
        if (showDebug && m_Transform != null)
        {
            Vector3 vel = Velocity;
            vel.y = 0;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(m_Transform.position, m_Transform.position + vel);
        }
    }
}
