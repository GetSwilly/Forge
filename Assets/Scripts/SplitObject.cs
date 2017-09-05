using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplitObject : MonoBehaviour {

    static readonly float SPAWN_DELAY = 0.01f;

    [System.Serializable]
    struct SplitInfo
    {
        public float objectScale;
        public bool keepSplitScript;

        public Vector3 localSpawnPosition;
        public bool shouldSpawnRandom;
    }

    [SerializeField]
    List<SplitInfo> objectSplits = new List<SplitInfo>();

    [SerializeField]
    float splitDelay = 1f;


    [SerializeField]
    [Range(0f, 180f)]
    float randomSpawnAngle = 30f;

    [SerializeField]
    float spawnDistance = 0.1f;



    void OnEnable()
    {
        StartCoroutine(DelayedSplitRoutine());
    }
    void OnDisable()
    {
        StopAllCoroutines();
    }


    IEnumerator DelayedSplitRoutine()
    {

        yield return new WaitForSeconds(splitDelay);


        for(int i = 0; i < objectSplits.Count; i++)
        {
            GameObject newObject = (GameObject)Instantiate(this.gameObject);

            if(!objectSplits[i].keepSplitScript)
                Destroy(newObject.GetComponent<SplitObject>());

            newObject.transform.parent = transform.parent;
            newObject.transform.localScale *= objectSplits[i].objectScale;

            Vector3 spawnDir = Vector3.zero;
            if (objectSplits[i].shouldSpawnRandom)
            {
                float _angle = Random.Range(0, randomSpawnAngle);
                _angle *= Random.value <= 0.5f ? 1f : -1f;

                spawnDir = Quaternion.AngleAxis(_angle, transform.up) * transform.forward;
            }
            else
            {
                spawnDir = transform.TransformPoint(objectSplits[i].localSpawnPosition) - transform.position;
            }

            Vector3 spawnPos = transform.position + (spawnDir.normalized * spawnDistance);
            newObject.transform.position = spawnPos;
            newObject.SetActive(true);

            yield return new WaitForSeconds(SPAWN_DELAY);
        }
        yield return null;

        gameObject.SetActive(false);
    }
}
