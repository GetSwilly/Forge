using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
public class PolyWater : MonoBehaviour
{
    Vector3 _waveSource1 = new Vector3(2.0f, 0.0f, 2.0f);
    public float WaveSpeed = -0.3f;
    public float WaveHeight = 0.48f;
    public float WavePattern = 0.62f;
    public float WavePatternChangeRate = 0f;
    public bool EdgeBlend = true;
    public bool ForceFlatShading = true;
    Mesh _mesh;
    Vector3[] _verts;
    float[] vertHeights;

    public bool shouldRecalculateCollider = false;

    [SerializeField]
    float updateTime = 0.025f;


    MeshFilter _filter;
    MeshCollider _collider;

    void Awake()
    {
        _filter = GetComponent<MeshFilter>();
        _collider = GetComponent<MeshCollider>();
    }
    void Start()
    {
        Camera.main.depthTextureMode |= DepthTextureMode.Depth;
        
        MakeMeshLowPoly(_filter);

        StartCoroutine(UpdateWater());
    }

    IEnumerator UpdateWater()
    {
        while (true)
        {

            yield return new WaitForSeconds(updateTime);


            float _patternDelta = WavePatternChangeRate * Time.deltaTime;
            _patternDelta *= Random.value <= .5f ? -1f : 1f;
            WavePattern += _patternDelta;


            CalcWave();
            SetEdgeBlend();
        }
    }

    MeshFilter MakeMeshLowPoly(MeshFilter mf)
    {
        _mesh = mf.mesh;
        var oldVerts = _mesh.vertices;
        var triangles = _mesh.triangles;
        var vertices = new Vector3[triangles.Length];
        vertHeights = new float[triangles.Length];
        for (var i = 0; i < triangles.Length; i++)
        {
            vertices[i] = oldVerts[triangles[i]];
            triangles[i] = i;

            vertHeights[i] = vertices[i].y;
        }
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        _verts = _mesh.vertices;
        return mf;
    }

    void SetEdgeBlend()
    {
        if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
        {
            EdgeBlend = false;
        }
        if (EdgeBlend)
        {
            Shader.EnableKeyword("WATER_EDGEBLEND_ON");
            if (Camera.main)
            {
                Camera.main.depthTextureMode |= DepthTextureMode.Depth;
            }
        }
        else
        {
            Shader.DisableKeyword("WATER_EDGEBLEND_ON");
        }
    }

    void CalcWave()
    {
        if (_verts == null)
            return;


        for (var i = 0; i < _verts.Length; i++)
        {
            var v = _verts[i];
            v.y = vertHeights[i];  // 0.0f;
            var dist = Vector3.Distance(v, _waveSource1);
            dist = (dist % WavePattern) / WavePattern;
            v.y = WaveHeight * Mathf.Sin(Time.time * Mathf.PI * 2.0f * WaveSpeed
            + (Mathf.PI * 2.0f * dist));
            v.y += vertHeights[i];
            _verts[i] = v;
        }
        _mesh.vertices = _verts;
        _mesh.RecalculateNormals();
        _mesh.MarkDynamic();

        _filter.mesh = _mesh;

        if (shouldRecalculateCollider && _collider != null)
        {
            _collider.sharedMesh = _mesh;
        }
    }
}