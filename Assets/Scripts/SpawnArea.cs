using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnArea : MonoBehaviour {

    private static readonly float MinimumAreaDimension = 1f;

    [SerializeField]
    Vector3 area;

    Transform m_Transform;

    void Awake()
    {
        m_Transform = GetComponent<Transform>();
    }


    public Vector3 GetSpawnPoint()
    {
        Vector3 offsetVector = Vector3.zero;
        offsetVector.x += Random.Range(-area.x, area.x) /2f;
        offsetVector.y += Random.Range(-area.y, area.y) / 2f;
        offsetVector.z += Random.Range(-area.z, area.z) / 2f;

        Vector3 spawnPoint = m_Transform.position + (m_Transform.right * offsetVector.x) + (m_Transform.forward * offsetVector.z);

        return spawnPoint;
    }
    public GameObject SpawnObject(GameObject spawnObj, Transform spawnParent)
    {
        Vector3 spawnPoint = GetSpawnPoint();

        GameObject newObj = Instantiate(spawnObj, spawnParent) as GameObject;
        newObj.transform.position = spawnPoint;
        newObj.transform.rotation = m_Transform.rotation;
        newObj.SetActive(true);

        return newObj;
    }


    void OnValidate()
    {
        area.x = Mathf.Clamp(area.x, MinimumAreaDimension, area.x);
        area.y = Mathf.Clamp(area.y, MinimumAreaDimension, area.y);
        area.z = Mathf.Clamp(area.z, MinimumAreaDimension, area.z);

        BoxCollider box = GetComponent<BoxCollider>();
        if(box != null)
        {
            box.size = area;
        }
    }

    //void OnDrawGizmos()
    //{
    //    Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
    //   Gizmos.matrix *= rotationMatrix;
    //    Gizmos.DrawWireCube(transform.position, transform.localScale);
    //}
}
