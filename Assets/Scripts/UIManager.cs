using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Button[] timeMultiplierButtons;
    public TextMeshProUGUI cyclesDisplayText;

    public void OnTimeMultiplierButtonClicked(int _button)
    {
        GameManager.Get().cycleTimeMultiplier = _button;
        for (int i = 0; i < timeMultiplierButtons.Length; ++i)
        {
            timeMultiplierButtons[i].interactable = i != _button;
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        cyclesDisplayText.text = string.Format("Cycles: {0:0.0}", GameManager.Get().totalCycles);
    }
}
