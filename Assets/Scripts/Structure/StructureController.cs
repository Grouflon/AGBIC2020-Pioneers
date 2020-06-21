using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureController : MonoBehaviour
{
    [HideInInspector] public PlanetController planet;

    public virtual void OnCycle() { }
    public virtual void AfterAddedToPlanet() { }
    public virtual void BeforeRemovedFromPlanet() { }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
