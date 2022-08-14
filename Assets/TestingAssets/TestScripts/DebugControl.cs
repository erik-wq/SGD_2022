using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugControl : MonoBehaviour
{
    #region Serialized
    [SerializeField] private TMP_Text HeroSpeedText;
    [SerializeField] private TMP_Text ShadeSpeedText;
    [SerializeField] private TMP_Text Energy;
    [SerializeField] private TMP_Text ChangeTargetText;
    [SerializeField] private TMP_Text LooseAggroTest;
    [SerializeField] private TMP_Text AggroTarget;
    #endregion

    #region Serialized
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetHeroSpeed(float _speed)
    {
        //HeroSpeedText.text = "Hero speed: " + _speed;
    }

    public void SetShadeSpeed(float _speed)
    {
        //ShadeSpeedText.text = "Shade speed: " + _speed;
    }

    public void SetEnergy(float energy)
    {
        //Energy.text = "Player energy: " + energy;
    }

    public void ShowSwitchTargetCounter(float time)
    {
        //ChangeTargetText.text = "Change Aggro: " + time;
    }

    public void ShowLooseTargetTimer(float time)
    {
        //LooseAggroTest.text = "Loose aggro: " + time;
    }

    public void ShowTarget(string aggro)
    {
        //AggroTarget.text = "Aggro target: " + aggro;
    }
}
