using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

[Serializable]
public struct ResourceStorage
{
    public ResourceType type;
    public uint maxStorage;
    [HideInInspector] public uint stored;
}

public class PlanetController : MonoBehaviour
{
    public int segments = 3;
    public float radius = 2.0f;
    public ResourceStorage[] planetResources;

    [Header("Internal")]
    public PlanetMeshController planetMeshController;

    public StructureController[] structures { get; private set; }
    public Dictionary<ResourceType, ResourceStorage> resourceStorage { get; private set; }

    void Awake()
    {
        planetMeshController = GetComponentInChildren<PlanetMeshController>();
        structures = new StructureController[segments];
        resourceStorage = new Dictionary<ResourceType, ResourceStorage>();
    }

    void Start()
    {
        for (int i = 0; i < planetResources.Length; ++i)
        {
            AddResourceStorage(planetResources[i].type, planetResources[i].maxStorage);
            AddResource(planetResources[i].type, planetResources[i].maxStorage);
        }

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

    public void OnCycle()
    {
        foreach (StructureController structure in structures)
        {
            if (structure == null)
                continue;

            structure.OnCycle();
        }
    }

    public void SetSlotStructure(int _slot, StructureController _structure)
    {
        Assert.IsFalse(_slot < 0 || _slot >= segments);

        if (structures[_slot])
        {
            structures[_slot].BeforeRemovedFromPlanet();
            Destroy(structures[_slot].gameObject);
        }

        structures[_slot] = _structure;

        if (structures[_slot])
        {
            structures[_slot].transform.SetParent(planetMeshController.slotSockets[_slot]);
            structures[_slot].transform.localPosition = Vector3.zero;
            structures[_slot].transform.localRotation = Quaternion.identity;
            structures[_slot].transform.localScale = Vector3.one;
            _structure.planet = this;
            structures[_slot].AfterAddedToPlanet();
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
            float baseAngle = planetMeshController.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
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

    public void AddResourceStorage(ResourceType _type, uint _amount)
    {
        if (_amount == 0)
            return;

        ResourceStorage storage;
        if (resourceStorage.TryGetValue(_type, out storage))
        {
            Assert.IsTrue(storage.type == _type);

            storage.maxStorage += _amount;
            resourceStorage[_type] = storage;
        }
        else
        {
            storage.type = _type;
            storage.maxStorage = _amount;
            storage.stored = 0;
            resourceStorage[_type] = storage;
        }
    }

    public void RemoveResourceStorage(ResourceType _type, uint _amount)
    {
        if (_amount == 0)
            return;

        ResourceStorage storage;
        if (resourceStorage.TryGetValue(_type, out storage))
        {
            Assert.IsTrue(storage.type == _type);
            Assert.IsTrue(storage.maxStorage >= _amount);

            storage.maxStorage -= _amount;
            storage.stored = Math.Min(storage.maxStorage, storage.stored);
            if (storage.maxStorage > 0)
            {
                resourceStorage[_type] = storage;
            }
            else
            {
                resourceStorage.Remove(_type);
            }
        }
        else
        {
            Assert.IsTrue(false);
        }
    }

    public void AddResource(ResourceType _type, uint _amount)
    {
        if (_amount == 0)
            return;

        ResourceStorage storage;
        if (resourceStorage.TryGetValue(_type, out storage))
        {
            storage.stored = Math.Min(storage.stored + _amount, storage.maxStorage);
            resourceStorage[_type] = storage;
        }
    }

    public void RemoveResource(ResourceType _type, uint _amount)
    {
        if (_amount == 0)
            return;

        ResourceStorage storage;
        if (resourceStorage.TryGetValue(_type, out storage))
        {
            Assert.IsTrue(storage.stored >= _amount);

            storage.stored -= _amount;
            resourceStorage[_type] = storage;
        }
    }

    public ResourceStorage GetResourceStorage(ResourceType _type)
    {
        ResourceStorage storage;
        if (!resourceStorage.TryGetValue(_type, out storage))
        {
            storage.type = _type;
            storage.maxStorage = 0;
            storage.stored = 0;
        }
        return storage;
    }
}

[CustomEditor(typeof(PlanetController))]
public class PlanetControllerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlanetController o = (PlanetController)target;
        if (GUILayout.Button("Generate Mesh"))
        {
            o.planetMeshController.GenerateMesh();
        }
    }
}