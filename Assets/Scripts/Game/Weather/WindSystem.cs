using UnityEngine;
using System.Collections;

[System.Serializable]
public class WindSystem {

    [SerializeField]
    AnimationCurve windSpeedCurve = AnimationCurve.Linear(0f, 0f, 1f, 10f);

    [SerializeField]
    AnimationCurve windStrengthCurve = AnimationCurve.Linear(0f, 0f, 1f, 10f);

    [SerializeField]
    AnimationCurve windSizeCurve = AnimationCurve.Linear(0f, 0f, 1f, 10f);

    [SerializeField]
    AnimationCurve windVariationCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);


    [SerializeField]
    GameObject breezePrefab;


    [SerializeField]
    Vector3 spawnArea = new Vector3(50, 10, 50);


    [SerializeField]
    float spawnHeight = 25f;

    [SerializeField]
    float spawnInterval = 5f;
    float spawnTimer;
    [SerializeField]
    float breezeLifetime = 20f;


    float currentWindValue;
    Vector3 currentWind;



    ObjectPooler breezePool;
    Transform myTransform;

    public void Setup(Transform _transform)
    {
        myTransform = _transform;


        GameObject _object = new GameObject("Breezes");
        _object.transform.parent = myTransform;
        _object.transform.localPosition = Vector3.zero;


        breezePool = myTransform.gameObject.AddComponent<ObjectPooler>();
        breezePool.Parent = _object.transform;
        breezePool.PooledObject = breezePrefab;
        breezePool.PoolLength = 10;
        breezePool.Initialize();
    }




    public void Initialize()
    {
        spawnTimer = 0;
        CalculateWind();

        Vector3 _size = A_Star_Pathfinding.Instance.WorldSize;
        spawnArea.x = _size.x / 2f;
        spawnArea.z = _size.z / 2f;
       
    }
    public void Terminate()
    {
        breezePool.ClearPool();
    }



    void CalculateWind()
    {
        currentWindValue = UnityEngine.Random.value;

        currentWind = UnityEngine.Random.onUnitSphere;
        currentWind.y = 0;

        currentWind = currentWind.normalized;// * windStrengthCurve.Evaluate(UnityEngine.Random.Range(0f,1f));
    }





    public void Update(float deltaTime)
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnInterval)
            return;

        spawnTimer = 0;



        Vector3 offsetVector = new Vector3(UnityEngine.Random.Range(-spawnArea.x, spawnArea.x), spawnHeight + UnityEngine.Random.Range(-spawnArea.y, spawnArea.y), UnityEngine.Random.Range(-spawnArea.z, spawnArea.z));
        Vector3 spawnPos = myTransform.position + offsetVector;

        GameObject obj = breezePool.GetPooledObject();

        obj.transform.eulerAngles = new Vector3(0f, (float)Random.value * 360f, 0f);
        obj.transform.position = spawnPos;
        obj.SetActive(true);

        Breeze _breeze = obj.GetComponent<Breeze>();

        if(_breeze == null)
        {
            obj.SetActive(false);
            return;
        }


        _breeze.Initialize(BreezeSize, BreezeVector, BreezeForce, breezeLifetime);
    }


    public Vector3 BreezeVector
    {
        get { return currentWind.normalized * windSpeedCurve.Evaluate(currentWindValue) * windVariationCurve.Evaluate(UnityEngine.Random.value); }
    }
    public float BreezeForce
    {
        get { return windStrengthCurve.Evaluate(currentWindValue) * windVariationCurve.Evaluate(UnityEngine.Random.value); }
    }
    public float BreezeSize
    {
        get { return windSizeCurve.Evaluate(currentWindValue) * windVariationCurve.Evaluate(UnityEngine.Random.value); }
    }

    public Vector3 Wind
    {
        get { return currentWind; }
    }
}
