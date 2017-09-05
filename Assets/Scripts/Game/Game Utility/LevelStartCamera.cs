using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CameraFollow))]
public class LevelStartCamera : MonoBehaviour {

    static readonly float DROP_DELAY = .25f;
    static readonly Vector3 CAMERA_OFFSET = new Vector3(0f, 100f, 0f);
    static readonly float MINIMUM_STOP_DISTANCE = 1f;


    [SerializeField]
    float startHeight;

    [SerializeField]
    float waitDelay = 1f;

    [SerializeField]
    float stopDistance = 5f;


    [SerializeField]
    float m_MoveSpeed = 2;

    [SerializeField]
    float m_RotationSpeed = 10;

    Camera m_Cam;
    CameraFollow m_Follow;
    Transform m_Transform;

    void Awake()
    {
        m_Cam = GetComponent<Camera>();
        m_Follow = GetComponent<CameraFollow>();
        m_Transform = GetComponent<Transform>();
    }


    public void Initiate()
    {
        StartCoroutine(CameraDrop());
    }
    IEnumerator CameraDrop()
    {
        CameraShake _shake = GetComponent<CameraShake>();

        if(_shake != null)
        {
            _shake.enabled = false;
        }




        m_Cam.transform.position = LevelController.Instance.transform.position + (Vector3.up * startHeight);
        m_Cam.transform.rotation = Quaternion.Euler(90, 0, 0);
        Vector3 targetPosition = LevelController.Instance.EndGoalPosition + CAMERA_OFFSET;
        Transform originalTarget = m_Follow.TargetTransform;


        m_Follow.StoreVariables();
        


        m_Follow.MoveSpeed = 0f;
        m_Follow.RotationSpeed = 0f;

        yield return new WaitForSeconds(DROP_DELAY);

        m_Follow.MoveSpeed = m_MoveSpeed;
        m_Follow.RotationSpeed = m_RotationSpeed;

        m_Follow.CurrentFollowStyle = CameraFollow.FollowStyle.Position;
        m_Follow.TargetPosition = targetPosition;
        m_Follow.DesiredAngle = 90f;

        yield return null;
        while (Vector3.Distance(m_Transform.position, m_Follow.AssignedPosition) > StopDistance)
        {
            yield return null;
        }

        //Debug.Log("Reached Position 0");

        yield return new WaitForSeconds(waitDelay);

        m_Follow.CurrentFollowStyle = CameraFollow.FollowStyle.Target;
        m_Follow.CameraOffset = CAMERA_OFFSET;
        m_Follow.AimOffset = 0f;
        m_Follow.TargetTransform = originalTarget;

        yield return null;
        while (Vector3.Distance(m_Transform.position, m_Follow.AssignedPosition) > StopDistance)
        {
            yield return null;
        }
        //Debug.Log("Reached Position 1");

        yield return new WaitForSeconds(waitDelay);


        m_Follow.ResetVariables();

        if (_shake != null)
        {
            _shake.enabled = true;
        }
    }

    public float StopDistance
    {
        get { return stopDistance; }
        set
        {
            stopDistance = value;

            if (stopDistance < MINIMUM_STOP_DISTANCE)
            {
                stopDistance = MINIMUM_STOP_DISTANCE;
            }
        }
    }


    void OnValidate()
    {
        StopDistance = StopDistance;
    }


    //public void DropCamera()
    //{
    //    StartCoroutine(CameraDrop());
    //}

    //IEnumerator CameraDrop()
    //{
    //    float normalFOV = m_Cam.fieldOfView;
    //    m_Cam.fieldOfView = dropFOVStart;
    //    float fovDiff = m_Cam.fieldOfView - normalFOV;

    //    float normalFollow = MoveSpeed;
    //    MoveSpeed = 0f;

    //    m_Cam.transform.position = new Vector3(0, dropHeightStart, 0);
    //    m_Cam.transform.rotation = Quaternion.Euler(90, 0, 0);

    //    yield return new WaitForSeconds(DROP_DELAY);


    //    float timer = 0f;

    //    while (timer < dropTime)
    //    {
    //        timer += Time.deltaTime;

    //        m_Cam.fieldOfView = normalFOV + (fovDiff * (1f - (timer / dropTime)));

    //        //Debug.Log(cam.fieldOfView.ToString());


    //        MoveSpeed = normalFollow * (timer / dropTime);

    //        yield return null;
    //    }

    //    m_Cam.fieldOfView = normalFOV;
    //    MoveSpeed = normalFollow;
    //}
}
