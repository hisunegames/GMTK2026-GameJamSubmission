using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleManual : MonoBehaviour
{
    [SerializeField] private GameObject manualPanel;

    // Start is called before the first frame update
    void Start()
    {
        manualPanel.SetActive(false);
    }

    public void ToggleManualPanel()
    {
        if (manualPanel != null)
        {
            manualPanel.SetActive(!manualPanel.activeSelf);
        }
    }
}
