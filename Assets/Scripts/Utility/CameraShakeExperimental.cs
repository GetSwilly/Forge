using UnityEngine;
using System.Collections;

public class CameraShakeExperimental : MonoBehaviour {



    [SerializeField]
    float shakeTime = .75f;

    [SerializeField]
    float shakeAmountMajor = 0.7f;
    [SerializeField]
    float shakeAmountMinor = 0.5f;
    [SerializeField]
    float shakeAmountMini = 0.1f;

    [SerializeField]
    bool isShaking = false;

    CameraFollow myFollow;
    Transform myTransform;

    [HideInInspector]
    public static CameraShakeExperimental Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            Destroy(this);

        Instance = this;

        myTransform = GetComponent<Transform>();

        myFollow = GetComponent<CameraFollow>();
    }

    public void Shake(float _amount, float _time)
    {
        if (isShaking)
            StopShaking();

        StartShaking(_amount, _time);
    }

    public void ShakeMajor()
    {
        Shake(shakeAmountMajor, shakeTime);
    }
    public void ShakeMinor()
    {
        Shake(shakeAmountMinor, shakeTime);
    }
    public void ShakeMini()
    {
        Shake(shakeAmountMini, shakeTime);
    }

    void StartShaking(float _amount, float _time)
    {
        //originalPos = camTransform.localPosition;
        isShaking = true;

        //myFollow.enabled = false;
        //myFollow.moveSmoothing /= 2;
        StartCoroutine(ShakeRoutine(_amount, _time));
    }
    void StopShaking()
    {
        StopAllCoroutines();
        //camTransform.localPosition = originalPos;
        isShaking = false;
        //timer = 0f;


        //myFollow.moveSmoothing *= 2;
        //myFollow.enabled = true;
    }

    /*	void Update(){
            if (isShaking) {
                if (timer > 0) {
                    float ratio = Mathf.Clamp01(timer / shakeTime);
                    Vector3 randomPos = Random.insideUnitSphere;
                    randomPos.z = 0;
                    Debug.Log(randomPos.ToString());
                    camTransform.localPosition = originalPos + (randomPos * shakeAmount * ratio);

                    timer -= Time.deltaTime;
                } else {
                    StopShaking();
                }
            }
        }*/

    IEnumerator ShakeRoutine(float _amount, float _time)
    {

        float shakeAmount = _amount;
        float timer = _time;


        while (isShaking && timer > 0)
        {
            float ratio = Mathf.Clamp01(timer / _time);
            Vector3 randomPos = Random.insideUnitSphere;

            randomPos = myFollow.DesiredPosition + (randomPos * shakeAmount * ratio);

            myTransform.position = randomPos;

            timer -= Time.deltaTime;

            yield return null;
        }

        StopShaking();
    }
}
