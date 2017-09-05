using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CloudSystem {

    static readonly int CLOUD_POOL_LENGTH = 3;


    [SerializeField]
    List<WeightedObjectOfGameObject> regularCloudPrefabs = new List<WeightedObjectOfGameObject>();

    [SerializeField]
    List<WeightedObjectOfGameObject> stormCloudPrefabs = new List<WeightedObjectOfGameObject>();

    List<WeightedObject<ObjectPooler>> regularCloudPoolers = new List<WeightedObject<ObjectPooler>>();
    List<WeightedObject<ObjectPooler>> stormCloudPoolers = new List<WeightedObject<ObjectPooler>>();

    [SerializeField]
    AnimationCurve stormChanceCurve = AnimationCurve.Linear(0f, 0f, 100f, 0f);

    [SerializeField]
    AnimationCurve cloudScaleCurve = AnimationCurve.Linear(0f, 1f, 100f, 1f);

    [SerializeField]
    AnimationCurve cloudScaleSigmaCurve = AnimationCurve.Linear(0f, 0f, 100f, 0f);


    [SerializeField]
    [Range(0f, 100f)]
    float stopStormThreshold = 50f;

    bool isStorming = false;



    [SerializeField]
    Vector3 spawnArea = new Vector3(50, 10, 50);


    [SerializeField]
    float spawnHeight = 25f;

    [SerializeField]
    float spawnInterval = 5f;
    float spawnTimer = 0f;

    [SerializeField]
    float cloudLifetime = 20f;

    [SerializeField]
    [Range(0.1f, 5f)]
    float fadeInTime = 2f;

    [SerializeField]
    [Range(0.1f, 5f)]
    float fadeOutTime = 2f;


   
    Transform myTransform;

    public void Setup(Transform _transform)
    {
        myTransform = _transform;



        //Create parent object
        GameObject parentObj = new GameObject("Clouds");
        parentObj.transform.parent = myTransform;
        parentObj.transform.localPosition = Vector3.zero;


        //Create regular cloud pools
        for(int i = 0; i < regularCloudPrefabs.Count; i++)
        {
            ObjectPooler _pooler = myTransform.gameObject.AddComponent<ObjectPooler>();
            _pooler.Parent = parentObj.transform;
            _pooler.PooledObject = regularCloudPrefabs[i].Item1;
            _pooler.PoolLength = CLOUD_POOL_LENGTH;

            _pooler.Initialize();

            regularCloudPoolers.Add(new WeightedObject<ObjectPooler>(_pooler, regularCloudPrefabs[i].Item2));
        }

        //Create storm cloud pools
        for (int i = 0; i < stormCloudPrefabs.Count; i++)
        {
            ObjectPooler _pooler = myTransform.gameObject.AddComponent<ObjectPooler>();
            _pooler.Parent = parentObj.transform;
            _pooler.PooledObject = stormCloudPrefabs[i].Item1;
            _pooler.PoolLength = CLOUD_POOL_LENGTH;

            _pooler.Initialize();

            stormCloudPoolers.Add(new WeightedObject<ObjectPooler>(_pooler, stormCloudPrefabs[i].Item2));
        }
    }


    public void Initialize()
    {
        spawnTimer = 0;

        Vector3 _size = A_Star_Pathfinding.Instance.WorldSize;
        spawnArea.x = _size.x / 2f;
        spawnArea.z = _size.z / 2f;
    }
    public void Terminate()
    {
        for (int i = 0; i < regularCloudPoolers.Count; i++)
            regularCloudPoolers[i].Item1.ClearPool();

        for (int i = 0; i < stormCloudPoolers.Count; i++)
            stormCloudPoolers[i].Item1.ClearPool();
    }


    public void Update(float deltaTime)
    {
        spawnTimer += deltaTime;

        if (spawnTimer < spawnInterval)
            return;

        spawnTimer = 0;



        if (isStorming)
        {
            isStorming = WeatherSystem.Instance.WeatherIntensity > stopStormThreshold;
        }
        else
        {
            isStorming = Random.value <= Random.Range(0f, stormChanceCurve.Evaluate(WeatherSystem.Instance.WeatherIntensity));
        }




        GameObject obj = null;

        if (isStorming && stormCloudPoolers.Count > 0)
        {
            obj = Utilities.WeightedSelection<ObjectPooler>(stormCloudPoolers.ToArray(), 0f).GetPooledObject();
        }
        else if (regularCloudPoolers.Count > 0)
        {
            obj = Utilities.WeightedSelection<ObjectPooler>(regularCloudPoolers.ToArray(), 0f).GetPooledObject();
        }

        if (obj == null)
            return;

        Vector3 offsetVector = new Vector3(UnityEngine.Random.Range(-spawnArea.x, spawnArea.x), spawnHeight + UnityEngine.Random.Range(-spawnArea.y, spawnArea.y), UnityEngine.Random.Range(-spawnArea.z, spawnArea.z));
        Vector3 spawnPos = myTransform.position + offsetVector;

        float scaleValue = (float)Utilities.GetRandomGaussian(cloudScaleCurve.Evaluate(WeatherSystem.Instance.WeatherIntensity), cloudScaleSigmaCurve.Evaluate(WeatherSystem.Instance.WeatherIntensity));
        Vector3 scale = Vector3.one * Mathf.Abs(scaleValue);

        obj.transform.localScale = scale;
        obj.transform.position = spawnPos;
        obj.SetActive(true);

        Cloud cScript = obj.GetComponent<Cloud>();

        if (cScript == null)
        {
            MonoBehaviour.Destroy(obj);
            return;
        }

        cScript.Initialize(cloudLifetime, fadeInTime, fadeOutTime);
    }
}
