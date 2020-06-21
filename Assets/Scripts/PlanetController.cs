using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(PlanetMeshController))]

public class PlanetController : MonoBehaviour
{
    public int segments = 3;
    public float radius = 2.0f;

    public PlanetMeshController planetMeshController { get; private set; }

    void Awake()
    {
        planetMeshController = GetComponent<PlanetMeshController>();
        m_structures = new StructureController[segments];
    }

    void Start()
    {
        GameManager.Get().RegisterPlanet(this);
    }

    void OnDestroy()
    {
        // can't control destruction order ...
        if (GameManager.Get())
            GameManager.Get().UnregisterPlanet(this);
    }

    void Update()
    {
        
    }

    public void SetSlotStructure(int _slot, StructureController _structure)
    {
        Assert.IsFalse(_slot < 0 || _slot >= segments);

        if (m_structures[_slot])
        {
            Destroy(m_structures[_slot].gameObject);
        }

        m_structures[_slot] = _structure;

        if (m_structures[_slot])
        {
            m_structures[_slot].transform.SetParent(planetMeshController.slotSockets[_slot]);
            m_structures[_slot].transform.localPosition = Vector3.zero;
            m_structures[_slot].transform.localRotation = Quaternion.identity;
            m_structures[_slot].transform.localScale = Vector3.one;
        }
    }

    public int GetSlotAtWorldPosition(Vector3 _worldPosition)
    {
        _worldPosition.z = 0.0f;
        Vector3 planetPosition = transform.position;
        planetPosition.z = 0.0f;

        if (Vector3.Distance(_worldPosition, planetPosition) <= radius)
        {
            float segmentStep = Mathf.PI * 2.0f / segments;
            float baseAngle = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            Vector3 planetToMouse = _worldPosition - planetPosition;

            for (int i = 0; i < segments; ++i)
            {
                Vector3 beginDirection = new Vector3(Mathf.Cos(segmentStep * i + baseAngle), Mathf.Sin(segmentStep * i + baseAngle), 0.0f);
                float angle = Vector3.SignedAngle(beginDirection, planetToMouse, Vector3.forward) * Mathf.Deg2Rad;

                if (angle >= 0.0f && angle < segmentStep)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    StructureController[] m_structures;
}
