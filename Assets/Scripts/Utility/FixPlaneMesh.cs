using UnityEngine;
using System.Collections;

public class FixPlaneMesh : MonoBehaviour {

	void Start()
    {
        Recalculate();
    }
    void Recalculate()
    {
        MeshFilter _filter = GetComponent<MeshFilter>();
        Mesh _mesh = _filter.sharedMesh;

       Vector3[] _verts = _mesh.vertices;

        for (int i = 0; i < _verts.Length; i++)
        {
            Vector3 v = _verts[i];
            v.y = 0.0f;
            _verts[i] = v;
        }
        _mesh.vertices = _verts;

        GetComponent<MeshFilter>().sharedMesh = _mesh;
    }
}
