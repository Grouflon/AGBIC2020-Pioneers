﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class PlanetMeshController : MonoBehaviour
{
    public int segments = 3;
    public int discSubdivisions = 64;
    public float radius = 20.0f;
    public float separationWidth = 1.0f;

    void Awake()
    {
        GenerateMesh();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateMesh()
    {
        m_meshFilter = GetComponent<MeshFilter>();
        m_meshRenderer = GetComponent<MeshRenderer>();

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

        float segmentStep = Mathf.PI * 2.0f / segments;
        float angleStep = Mathf.PI * 2.0f / discSubdivisions;
        int segmentSubdivisionsCount = Mathf.CeilToInt((float)discSubdivisions / segments);

        float originDistance = (separationWidth * .5f) / Mathf.Sin(segmentStep * .5f);
        float offsetAngle = Mathf.Atan(separationWidth * .5f / radius);

        for (int i = 0; i < segments; ++i)
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
            m_vertices.Add(new Vector3(Mathf.Cos(startAngle + offsetAngle), Mathf.Sin(startAngle + offsetAngle), 0.0f) * radius);
            for (int j = 0; j < segmentSubdivisionsCount; ++j)
            {
                float a = j * angleStep;
                if (a <= offsetAngle || a >= (segmentStep - offsetAngle))
                {
                    continue;
                }

                m_vertices.Add(new Vector3(Mathf.Cos(startAngle + a), Mathf.Sin(startAngle + a), 0.0f) * radius);

                m_indices[i].Add(baseIndex);
                m_indices[i].Add(m_vertices.Count);
                m_indices[i].Add(m_vertices.Count - 1);
            }
            m_vertices.Add(new Vector3(Mathf.Cos(endAngle - offsetAngle), Mathf.Sin(endAngle - offsetAngle), 0.0f) * radius);
        }

        m_mesh.SetVertices(m_vertices);
        m_mesh.subMeshCount = m_indices.Count;
        for (int i = 0; i < m_indices.Count; ++i)
        {
            m_mesh.SetTriangles(m_indices[i], i);
        }

        m_meshFilter.mesh = m_mesh;
        m_meshRenderer.materials = new Material[m_indices.Count];
    }

    MeshFilter m_meshFilter;
    MeshRenderer m_meshRenderer;
    Mesh m_mesh;
    List<Vector3> m_vertices;
    List<List<int>> m_indices;
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