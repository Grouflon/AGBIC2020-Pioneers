using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class PlanetMeshController : MonoBehaviour
{
    public int discSubdivisions = 64;
    public float separationWidth = 1.0f;
    public Material idleMaterial;
    public Material hoverMaterial;

    public PlanetController planet { get; private set; }
    public Transform[] slotSockets { get; private set; }

    static string s_slotsParentName = "_slots";

    void Awake()
    {
        planet = GetComponent<PlanetController>();
        m_meshRenderer = GetComponent<MeshRenderer>();

        slotSockets = new Transform[planet.segments];
        Transform slotsParent = transform.Find(s_slotsParentName);
        Assert.IsNotNull(slotsParent);
        for (int i = 0; i < planet.segments; ++i)
        {
            slotSockets[i] = slotsParent.GetChild(i);
        }
    }

    void Start()
    {
    }

    public void SetSlotHovered(int _slot, bool _hovered)
    {
        if (_slot < 0 || _slot >= planet.segments)
            return;

        Material[] materials = m_meshRenderer.sharedMaterials;
        materials[_slot] = _hovered ? hoverMaterial : idleMaterial;
        m_meshRenderer.sharedMaterials = materials;
    }

    public void ClearHovers()
    {
        Material[] materials = m_meshRenderer.sharedMaterials;
        for (int i = 0; i < materials.Length; ++i)
        {
            materials[i] = idleMaterial;
        }
        m_meshRenderer.sharedMaterials = materials;
    }

    void Update()
    {

    }

    public void GenerateMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        PlanetController p = GetComponent<PlanetController>();

        Transform slotsParent = transform.Find(s_slotsParentName);
        if (slotsParent != null)
        {
            DestroyImmediate(slotsParent.gameObject);
        }
        slotsParent = new GameObject().transform;
        slotsParent.name = s_slotsParentName;
        slotsParent.transform.SetParent(transform);
        slotsParent.transform.localPosition = new Vector3(0.0f, 0.0f, 2.0f);
        slotsParent.transform.localRotation = Quaternion.identity;
        slotSockets = new Transform[p.segments];

        List<Vector3> vertices = new List<Vector3>();
        List<List<int>> indices = new List<List<int>>();
        Mesh mesh = new Mesh();

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

        float segmentStep = Mathf.PI * 2.0f / p.segments;
        float angleStep = Mathf.PI * 2.0f / discSubdivisions;
        int segmentSubdivisionsCount = Mathf.CeilToInt((float)discSubdivisions / p.segments);

        float originDistance = (separationWidth * .5f) / Mathf.Sin(segmentStep * .5f);
        float offsetAngle = Mathf.Atan(separationWidth * .5f / p.radius);

        for (int i = 0; i < p.segments; ++i)
        {
            indices.Add(new List<int>());

            float startAngle = segmentStep * i;
            float endAngle = startAngle + segmentStep;
            Vector3 startDirection = new Vector3(Mathf.Cos(startAngle), Mathf.Sin(startAngle), 0.0f);
            Vector3 endDirection = new Vector3(Mathf.Cos(endAngle), Mathf.Sin(endAngle), 0.0f);
            Vector3 middleDirection = (startDirection + endDirection).normalized;
            Vector3 origin = middleDirection * originDistance;

            int baseIndex = vertices.Count;

            indices[i].Add(baseIndex);
            indices[i].Add(baseIndex + 2);
            indices[i].Add(baseIndex + 1);

            vertices.Add(origin);
            vertices.Add(new Vector3(Mathf.Cos(startAngle + offsetAngle), Mathf.Sin(startAngle + offsetAngle), 0.0f) * p.radius);
            for (int j = 0; j < segmentSubdivisionsCount; ++j)
            {
                float a = j * angleStep;
                if (a <= offsetAngle || a >= (segmentStep - offsetAngle))
                {
                    continue;
                }

                vertices.Add(new Vector3(Mathf.Cos(startAngle + a), Mathf.Sin(startAngle + a), 0.0f) * p.radius);

                indices[i].Add(baseIndex);
                indices[i].Add(vertices.Count);
                indices[i].Add(vertices.Count - 1);
            }
            vertices.Add(new Vector3(Mathf.Cos(endAngle - offsetAngle), Mathf.Sin(endAngle - offsetAngle), 0.0f) * p.radius);

            // Slot
            GameObject slot = new GameObject();
            slot.transform.SetParent(slotsParent);
            slot.name = "slot " + i;
            slot.transform.localPosition = middleDirection * p.radius;
            slot.transform.localRotation = Quaternion.LookRotation(Vector3.forward, middleDirection);
            slotSockets[i] = slot.transform;
        }

        mesh.SetVertices(vertices);
        mesh.subMeshCount = indices.Count;
        for (int i = 0; i < indices.Count; ++i)
        {
            mesh.SetTriangles(indices[i], i);
        }

        meshFilter.mesh = mesh;

        Material[] materials = new Material[indices.Count];
        for (int i = 0; i < materials.Length; ++i)
        {
            materials[i] = idleMaterial;
        }
        meshRenderer.sharedMaterials = materials;

    }

    MeshRenderer m_meshRenderer;
}

[CustomEditor(typeof(PlanetMeshController))]
public class PlanetMeshControllerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlanetMeshController o = (PlanetMeshController)target;
        if (GUILayout.Button("Generate Mesh"))
        {
            o.GenerateMesh();
        }
    }
}