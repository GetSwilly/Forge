using UnityEngine;
using System.Collections;

namespace SensorToolkit
{
    /*
     * A paramemtric shape for creating field of view cones that work with the trigger sensor. Requires a MeshCollider
     * component on the same gameobject. When the script starts it will dynamically create a mesh for the fov cone and
     * assign it to this MeshCollider component.
     */
    [RequireComponent(typeof(MeshCollider))]
    [ExecuteInEditMode]
    public class FOVCollider : MonoBehaviour
    {
        // The length of the field of view cone in world units.
        public float Length = 5f;

        // The size of the field of view cones base in world units.
        public float BaseSize = 0.5f;

        // The arc angle of the fov cone.
        [Range(1f, 180f)]
        public float FOVAngle = 90f;

        // The elevation angle of the cone.
        [Range(1f, 180f)]
        public float ElevationAngle = 90f;

        // The number of vertices used to approximate the arc of the fov cone. Ideally this should be as low as possible.
        [Range(0, 8)]
        public int Resolution = 0;

        MeshCollider mc;
        Vector3[] pts;
        int[] triangles;

        void OnEnable()
        {
            mc = GetComponent<MeshCollider>();
            CreateCollider();
        }

        void OnValidate()
        {
            Length = Mathf.Max(0f, Length);
            BaseSize = Mathf.Max(0f, BaseSize);
        }

        public void CreateCollider()
        {
            pts = new Vector3[4 + (2+Resolution)*(2+Resolution)];
            // Unity doesn't actually need all the triangles to cook the convex hull, so only add enough to make it happy.
            triangles = new int[2*3];

            // Base points
            pts[0] = new Vector3(-BaseSize / 2f, -BaseSize / 2f, 0f); // Bottom Left
            pts[1] = new Vector3(BaseSize / 2f, -BaseSize / 2f, 0f);  // Bottom Right
            pts[2] = new Vector3(BaseSize / 2f, BaseSize / 2f, 0f);   // Top Right
            pts[3] = new Vector3(-BaseSize / 2f, BaseSize / 2f, 0f);  // Top Left
            triangles[0] = 2; triangles[1] = 1; triangles[2] = 0; triangles[3] = 3; triangles[4] = 2; triangles[5] = 0;

            for (int y = 0; y < 2+Resolution; y++)
            {
                for (int x = 0; x < 2+Resolution; x++)
                {
                    int i = 4 + y * (2 + Resolution) + x;
                    float ay = Mathf.Lerp(-FOVAngle / 2f, FOVAngle / 2f, (float)x / (float)(Resolution + 1));
                    float ax = Mathf.Lerp(-ElevationAngle / 2f, ElevationAngle / 2f, (float)y / (float)(Resolution + 1));
                    Vector3 p = Quaternion.Euler(ax, ay, 0f) * Vector3.forward * Length;
                    pts[i] = p;
                }
            }

            Mesh mesh = new Mesh();
            releaseMesh();
            mc.sharedMesh = mesh;
            mesh.vertices = pts;
            mesh.triangles = triangles;
            mesh.name = "FOVColliderPoints";
            mc.convex = true;
            mc.isTrigger = true;
        }

        void releaseMesh()
        {
            if (mc.sharedMesh != null && mc.sharedMesh.name == "FOVColliderPoints")
            {
                DestroyImmediate(mc.sharedMesh, true);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            foreach(Vector3 p in pts)
            {
                Gizmos.DrawSphere(transform.TransformPoint(p), 0.1f);
            }
        }
    }
}