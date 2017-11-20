using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{

    [SerializeField]
    Properties m_Properties;

    [SerializeField]
    float strength;

    [SerializeField]
    float duration;

    [SerializeField]
    bool useZAxis = false;

    [SerializeField]
    bool testShake = false;

    [SerializeField]
    AnimationCurve strengthOverTime = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    const float maxAngle = 10f;
    IEnumerator currentShakeCoroutine;

    Vector3 baseLocalPosition;

    public static CameraShake Instance { get; private set; }
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }

        Instance = this;
    }
    void Start()
    {
        baseLocalPosition = this.transform.localPosition;
    }
    void Update()
    {
        if (testShake)
        {
            testShake = false;
            StartShake();
        }
    }

    public void StartShake()
    {
        StopAllCoroutines();

        StartCoroutine(ShakeRoutine());
        //StartShake(m_Properties);
    }
    public void StartShake(Properties properties)
    {
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
        }

        currentShakeCoroutine = Shake(properties);
        StartCoroutine(currentShakeCoroutine);
    }

    IEnumerator Shake(Properties properties)
    {
        float completionPercent = 0;
        float movePercent = 0;

        float angle_radians = properties.Angle * Mathf.Deg2Rad - Mathf.PI;
        Vector3 previousWaypoint = baseLocalPosition;
        Vector3 currentWaypoint = Vector3.zero;
        float moveDistance = 0;
        float speed = 0;

        Quaternion targetRotation = Quaternion.identity;
        Quaternion previousRotation = Quaternion.identity;

        do
        {
            if (movePercent >= 1 || completionPercent == 0)
            {
                float dampingFactor = DampingCurve(completionPercent, properties.DampingPercent);
                float noiseAngle = (Random.value - .5f) * Mathf.PI;
                angle_radians += Mathf.PI + noiseAngle * properties.NoisePercent;
                currentWaypoint = new Vector3(Mathf.Cos(angle_radians), Mathf.Sin(angle_radians)) * properties.Strength * dampingFactor;
                previousWaypoint = transform.localPosition;
                moveDistance = Vector3.Distance(currentWaypoint, previousWaypoint);

                targetRotation = Quaternion.Euler(new Vector3(currentWaypoint.y, currentWaypoint.x).normalized * properties.RotationPercent * dampingFactor * maxAngle);
                previousRotation = transform.localRotation;

                speed = Mathf.Lerp(properties.MinSpeed, properties.MaxSpeed, dampingFactor);

                movePercent = 0;
            }

            completionPercent += Time.deltaTime / properties.Duration;
            movePercent += Time.deltaTime / moveDistance * speed;
            transform.localPosition = Vector3.Lerp(previousWaypoint, currentWaypoint, movePercent);
            transform.localRotation = Quaternion.Slerp(previousRotation, targetRotation, movePercent);


            yield return null;
        } while (moveDistance > 0);
    }

    float DampingCurve(float x, float dampingPercent)
    {
        x = Mathf.Clamp01(x);
        float a = Mathf.Lerp(2, .25f, dampingPercent);
        float b = 1 - Mathf.Pow(x, a);
        return b * b * b;
    }

    IEnumerator ShakeRoutine()
    {
        float timer = 0f;

        Vector3 targetPosition = transform.position;

        Vector3 targetUnitSphere = Vector3.zero;


        do
        {
            Vector3 shakeDirection = Random.onUnitSphere;

            if (!useZAxis)
            {
                shakeDirection.z = 0f;
            }

            shakeDirection.Normalize();

            float strengthAmount = strengthOverTime.Evaluate(timer / duration) * Time.deltaTime;

            targetUnitSphere += shakeDirection * strengthAmount;

            if(targetUnitSphere.magnitude > 1f)
            {
                targetUnitSphere.Normalize();
            }

            Vector3 returnVector = -targetUnitSphere * (1f-strengthAmount);

            targetUnitSphere += returnVector;

            Vector3 moveDir = (transform.forward * targetUnitSphere.z) + (transform.right * targetUnitSphere.x) + (transform.up * targetUnitSphere.y);
            moveDir *= strength;

            transform.localPosition = baseLocalPosition + moveDir;// (targetUnitSphere * strength);

            yield return null;
            timer += Time.deltaTime;

        } while (timer <= duration);

        transform.localPosition = baseLocalPosition;
    }

    void OnValidate()
    {
        m_Properties.Validate();
    }

    [System.Serializable]
    public class Properties
    {
        [SerializeField]
        float angle;

        [SerializeField]
        float strength;

        [SerializeField]
        float maxSpeed;

        [SerializeField]
        float minSpeed;

        [SerializeField]
        float duration;

        [SerializeField]
        [Range(0, 1)]
        float noisePercent;

        [SerializeField]
        [Range(0, 1)]
        float dampingPercent;

        [SerializeField]
        [Range(0, 1)]
        float rotationPercent;

        public Properties(float angle, float strength, float speed, float duration, float noisePercent, float dampingPercent, float rotationPercent)
        {
            Angle = angle;
            Strength = strength;
            MaxSpeed = speed;
            Duration = duration;
            NoisePercent = Mathf.Clamp01(noisePercent);
            DampingPercent = Mathf.Clamp01(dampingPercent);
            RotationPercent = Mathf.Clamp01(rotationPercent);
        }

        #region Accessors

        public float Angle
        {
            get { return angle; }
            private set { angle = Mathf.Clamp(value, 0f, value); }
        }

        public float Strength
        {
            get { return strength; }
            private set { strength = Mathf.Clamp(value, 0f, value); }
        }

        public float MaxSpeed
        {
            get { return maxSpeed; }
            private set { maxSpeed = Mathf.Clamp(value, MinSpeed, value); }
        }

        public float MinSpeed
        {
            get { return minSpeed; }
            private set { minSpeed = Mathf.Clamp(value, 0f, value); }
        }

        public float Duration
        {
            get { return duration; }
            private set { duration = Mathf.Clamp(value, 0f, value); }
        }

        public float NoisePercent
        {
            get { return noisePercent; }
            private set { noisePercent = Mathf.Clamp01(value); }
        }

        public float DampingPercent
        {
            get { return dampingPercent; }
            private set { dampingPercent = Mathf.Clamp01(value); }
        }

        public float RotationPercent
        {
            get { return rotationPercent; }
            private set { rotationPercent = Mathf.Clamp01(value); }
        }

        #endregion

        public void Validate()
        {
            Angle = Angle;
            Strength = Strength;
            MaxSpeed = MaxSpeed;
            MinSpeed = MinSpeed;
            Duration = Duration;
        }
    }
}