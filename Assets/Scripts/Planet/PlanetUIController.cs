using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlanetUIController : MonoBehaviour
{
    [Header("Internal")]
    public TextMeshPro planetText;

    public PlanetController planet { get; private set; }

    void Awake()
    {
        planet = GetComponentInParent<PlanetController>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        planetText.text = "";
        int i = 0;
        foreach (KeyValuePair<ResourceType, ResourceStorage> pair in planet.resourceStorage)
        {
            if (i != 0) planetText.text += "\n";
            planetText.text += string.Format("{0}: {1}/{2}", pair.Key.ToString(), pair.Value.stored, pair.Value.maxStorage);

            ++i;
        }
    }
}
