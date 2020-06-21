using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class PlanetMeshController : MonoBehaviour
{
    public int discSubdivisions = 64;
    public float separationWidth = 1.0f;
    public Material idleMaterial;
    public Material hoverMaterial;

    void Awake()
    {
        _Initialize();
        GenerateMesh();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0.0f;
        Vector3 planetPosition = transform.position;
        planetPosition.z = 0.0f;

        Material[] materials = m_meshRenderer.sharedMaterials;
        if (Vector3.Distance(mouseWorldPosition, planetPosition) <= m_planet.radius)
        {
            float segmentStep = Mathf.PI * 2.0f / m_planet.segments;
            float baseAngle = m_planet.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            Vector3 planetToMouse = mouseWorldPosition - planetPosition;

            for (int i = 0; i < m_planet.segments; ++i)
            {
                Vector3 beginDirection = new Vector3(Mathf.Cos(segmentStep * i + baseAngle), Mathf.Sin(segmentStep * i + baseAngle), 0.0f);
                float angle = Vector3.SignedAngle(beginDirection, planetToMouse, Vector3.forward) * Mathf.Deg2Rad;

                if (angle >= 0.0f && angle < segmentStep)
                {
                    materials[i] = hoverMaterial;
                }
                else
                {
                    materials[i] = idleMaterial;
                }
            }
        }
        else
        {
            for (int i = 0; i < materials.Length; ++i)
            {
                materials[i] = idleMaterial;
            }
        }
        m_meshRenderer.sharedMaterials = materials;
    }

    void _Initialize()
    {
        if (m_initialized)
            return;

        m_meshFilter = GetComponent<MeshFilter>();
        m_meshRenderer = GetComponent<MeshRenderer>();
        m_planet = GetComponent<PlanetController>();
    }

    public void GenerateMesh()
    {
        _Initialize();

        m_vertices = new List<Vector3>();
        m_indices = new List<List<int>>();
        m_mesh = new Mesh();

        /*float angleStep = Mathf.PI * 2.0f / discSubdivisions;
        m_vertices.Add(Vector3.zero);
        for (int i = 0; i < discSubdivisions; ++i)
        {
            float angle = angleStep * i;
            Vector3 v = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0.0f) * radius;
            m_vertices.Add(v);

            m_indices.Add(0);
            m_indices.Add(((i + 1) % discSubdivisions) + 1);
            m_indices.Add(i + 1);
        }

        m_mesh = new Mesh();
        m_mesh.SetVertices(m_vertices);
        m_mesh.SetIndices(m_indices, MeshTopology.Triangles, 0);*/

        float segmentStep = Mathf.PI * 2.0f / m_planet.segments;
        float angleStep = Mathf.PI * 2.0f / discSubdivisions;
        int segmentSubdivisionsCount = Mathf.CeilToInt((float)discSubdivisions / m_planet.segments);

        float originDistance = (separationWidth * .5f) / Mathf.Sin(segmentStep * .5f);
        float offsetAngle = Mathf.Atan(separationWidth * .5f / m_planet.radius);

        for (int i = 0; i < m_planet.segments; ++i)
        {
            m_indices.Add(new List<int>());

            float startAngle = segmentStep * i;
            float endAngle = startAngle + segmentStep;
            Vector3 startDirection = new Vector3(Mathf.Cos(startAngle), Mathf.Sin(startAngle), 0.0f);
            Vector3 endDirection = new Vector3(Mathf.Cos(endAngle), Mathf.Sin(endAngle), 0.0f);
            Vector3 middleDirection = (startDirection + endDirection).normalized;
            Vector3 origin = middleDirection * originDistance;

            int baseIndex = m_vertices.Count;

            m_indices[i].Add(baseIndex);
            m_indices[i].Add(baseIndex + 2);
            m_indices[i].Add(baseIndex + 1);

            m_vertices.Add(origin);
            m_vertices.Add(new Vector3(Mathf.Cos(startAngle + offsetAngle), Mathf.Sin(startAngle + offsetAngle), 0.0f) * m_planet.radius);
            for (int j = 0; j < segmentSubdivisionsCount; ++j)
            {
                float a = j * angleStep;
                if (a <= offsetAngle || a >= (segmentStep - offsetAngle))
                {
                    continue;
                }

                m_vertices.Add(new Vector3(Mathf.Cos(startAngle + a), Mathf.Sin(startAngle + a), 0.0f) * m_planet.radius);

                m_indices[i].Add(baseIndex);
                m_indices[i].Add(m_vertices.Count);
                m_indices[i].Add(m_vertices.Count - 1);
            }
            m_vertices.Add(new Vector3(Mathf.Cos(endAngle - offsetAngle), Mathf.Sin(endAngle - offsetAngle), 0.0f) * m_planet.radius);
        }

        m_mesh.SetVertices(m_vertices);
        m_mesh.subMeshCount = m_indices.Count;
        for (int i = 0; i < m_indices.Count; ++i)
        {
            m_mesh.SetTriangles(m_indices[i], i);
        }

        m_meshFilter.mesh = m_mesh;

        Material[] materials = new Material[m_indices.Count];
        for (int i = 0; i < materials.Length; ++i)
        {
            materials[i] = idleMaterial;
        }
        m_meshRenderer.sharedMaterials = materials;

    }

    MeshFilter m_meshFilter;
    MeshRenderer m_meshRenderer;
    PlanetController m_planet;
    Mesh m_mesh;
    List<Vector3> m_vertices;
    List<List<int>> m_indices;

    bool m_initialized = false;
}

[CustomEditor(typeof(PlanetMeshController))]
public class PlanetMeshControllerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlanetMeshController o = (PlanetMeshController)target;
        if (GUILayout.Button("Refresh Mesh"))
        {
            o.GenerateMesh();
        }
    }
}