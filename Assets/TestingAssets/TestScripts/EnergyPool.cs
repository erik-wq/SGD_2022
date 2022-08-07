using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyPool : MonoBehaviour
{
    private HopeAI _hopeAI;
    private void Awake()
    {
        _hopeAI = GetComponent<HopeAI>();
    }
    public void AddEnergy(float ammount)
    {
        _hopeAI.AddEnergy(ammount);
    }
}
