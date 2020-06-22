using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ResourceQuantity
{
    public ResourceType type;
    public uint quantity;
}

[Serializable]
public struct ProductionRule
{
    public string name;
    public ResourceQuantity[] input;
    public ResourceQuantity[] output;
    public bool animates;
    [HideInInspector] public bool isRunning;
}

public class ResourceStructureController : StructureController
{
    public ProductionRule[] rules;
    public ResourceQuantity[] storage;

    public override void OnCycle()
    {
        base.OnCycle();

        if (rules.Length > 0)
        {
            for (int i = 0; i < rules.Length; ++i)
            {
                // OUTPUT
                if (rules[i].isRunning)
                {
                    bool canOutput = true;
                    foreach (ResourceQuantity resource in rules[i].output)
                    {
                        ResourceStorage storage = planet.GetResourceStorage(resource.type);
                        if ((storage.maxStorage - storage.stored) < resource.quantity)
                        {
                            canOutput = false;
                            break;
                        }
                    }

                    if (canOutput)
                    {
                        foreach (ResourceQuantity resource in rules[i].output)
                        {
                            planet.AddResource(resource.type, resource.quantity);
                        }
                        rules[i].isRunning = false;
                    }
                }

                // INPUT
                if (!rules[i].isRunning)
                {
                    bool canInput = true;
                    foreach (ResourceQuantity resource in rules[i].input)
                    {
                        ResourceStorage storage = planet.GetResourceStorage(resource.type);
                        if (storage.stored < resource.quantity)
                        {
                            canInput = false;
                            break;
                        }
                    }

                    if (canInput)
                    {
                        foreach (ResourceQuantity resource in rules[i].input)
                        {
                            planet.RemoveResource(resource.type, resource.quantity);
                        }
                        rules[i].isRunning = true;
                    }
                }
            }
        }
    }

    public override void AfterAddedToPlanet()
    {
        base.AfterAddedToPlanet();

        foreach (ResourceQuantity resource in storage)
        {
            planet.AddResourceStorage(resource.type, resource.quantity);
        }
    }

    public override void BeforeRemovedFromPlanet()
    {
        foreach (ResourceQuantity resource in storage)
        {
            planet.RemoveResourceStorage(resource.type, resource.quantity);
        }

        base.BeforeRemovedFromPlanet();
    }

    void Awake()
    {
        
    }

    void Start()
    {
        
    }

    void Update()
    {
        bool shouldAnimate = false;
        foreach (ProductionRule rule in rules)
        {
            if (rule.animates && rule.isRunning)
            {
                shouldAnimate = true;
                break;
            }
        }

        if (shouldAnimate)
        {
            Vector3 scale = transform.localScale;
            scale.y = 1.0f + Mathf.Sin(GameManager.Get().totalCycles * (Mathf.PI * 2.0f) * 2.0f) * 0.2f;
            transform.localScale = scale;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.y = 1.0f;
            transform.localScale = scale;
        }
    }
}
