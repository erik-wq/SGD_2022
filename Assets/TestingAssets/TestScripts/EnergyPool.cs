using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyPool : MonoBehaviour
{
    public float energyLimit;
    public float startingEnergy;
    private float _energy;
    private void Awake()
    {
        _energy = startingEnergy;
    }
    public void AddEnergy(float ammount)
    {
        _energy += ammount;
        Mathf.Clamp(_energy, 0, energyLimit);
    }
}
