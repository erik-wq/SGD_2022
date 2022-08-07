using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private BaseScreen[] _screens;
    private void Awake()
    {
        GameData.menuManager = this;
        _screens = GetComponentsInChildren<BaseScreen>(true);
    }

    public void Show<T>()
    {
        foreach(BaseScreen x in _screens)
        {
            if(x.GetType() == typeof(T))
            {
                x.Show();
                return;
        }
            }
    }
    public void Hide<T>()
    {
        foreach (BaseScreen x in _screens)
        {
            if (x.GetType() == typeof(T))
            {
                x.Hide();
                return;
            }
        }
    }
}
