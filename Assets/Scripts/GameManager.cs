using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum ResourceType
{
    Undefined,
    Soil,
    Minerals,
    People,
    Food,
    Metal,
}

public class GameManager : MonoBehaviour
{
    public float cycleDuration = 2.0f;
    public float cycleTimeRatio = 1.0f;

    public float totalGameTime { get; private set; }

    [Header("Internal")]
    public StructureController cityPrefab;
    public StructureController farmPrefab;

    void Awake()
    {
        m_planets = new List<PlanetController>();
    }

    void Start()
    {
        totalGameTime = 0.0f;
        m_cycleTimer = cycleDuration;
    }

    void Update()
    {
        // INPUT
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool hoveringPlanet = false;
        foreach (PlanetController planet in m_planets)
        {
            int slot = planet.GetSlotAtWorldPosition(mouseWorldPosition);
            if (slot >= 0)
            {
                hoveringPlanet = true;
                SetCurrentlyHoveredPlanet(planet, slot);
                break;
            }
        }
        if (!hoveringPlanet)
        {
            SetCurrentlyHoveredPlanet(null);
        }

        if (m_currentlyHoveredPlanet)
        {
            if (m_currentlyHoveredPlanet.structures[m_currentlyHoveredSlot] == null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    StructureController structure = Instantiate(cityPrefab);
                    m_currentlyHoveredPlanet.SetSlotStructure(m_currentlyHoveredSlot, structure);
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    StructureController structure = Instantiate(farmPrefab);
                    m_currentlyHoveredPlanet.SetSlotStructure(m_currentlyHoveredSlot, structure);
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(2))
                {
                    m_currentlyHoveredPlanet.SetSlotStructure(m_currentlyHoveredSlot, null);
                }
            }
        }

        // CYCLES
        if (cycleDuration > 0.0f)
        {
            m_cycleTimer -= Time.deltaTime * cycleTimeRatio;
            totalGameTime += (Time.deltaTime * cycleTimeRatio) / cycleDuration;
            while (m_cycleTimer <= 0.0f)
            {
                m_cycleTimer += cycleDuration;

                foreach (PlanetController planet in m_planets)
                {
                    planet.OnCycle();
                }
            }
        }
    }

    public void RegisterPlanet(PlanetController _planet)
    {
        Assert.IsNull(m_planets.Find(planet => planet == _planet));

        m_planets.Add(_planet);
    }

    public void UnregisterPlanet(PlanetController _planet)
    {
        Assert.IsNotNull(m_planets.Find(planet => planet == _planet));

        m_planets.Remove(_planet);
    }

    void SetCurrentlyHoveredPlanet(PlanetController _planet, int _slot = -1)
    {
        if (_planet == m_currentlyHoveredPlanet && _slot == m_currentlyHoveredSlot)
            return;

        if (m_currentlyHoveredPlanet)
        {
            if (_planet != m_currentlyHoveredPlanet)
            {
                m_currentlyHoveredPlanet.planetMeshController.ClearHovers();
            }
            else if (_slot != m_currentlyHoveredSlot)
            {
                m_currentlyHoveredPlanet.planetMeshController.SetSlotHovered(m_currentlyHoveredSlot, false);
            }
        }

        m_currentlyHoveredPlanet = _planet;
        m_currentlyHoveredSlot = _slot;

        if (m_currentlyHoveredPlanet)
        {
            m_currentlyHoveredPlanet.planetMeshController.SetSlotHovered(m_currentlyHoveredSlot, true);
        }
    }

    List<PlanetController> m_planets;
    PlanetController m_currentlyHoveredPlanet;
    int m_currentlyHoveredSlot = -1;
    float m_cycleTimer = 0.0f;

    public static GameManager Get()
    {
        if (s_singleton == null)
        {
            s_singleton = FindObjectOfType<GameManager>();
        }
        return s_singleton;
    }
    static GameManager s_singleton;
}
