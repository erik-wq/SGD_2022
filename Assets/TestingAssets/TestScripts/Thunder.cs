using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thunder : MonoBehaviour
{
    private LightningArea LA;
    private Animator anim;
    public GameObject indic;
    public float waitTime;
    public event Action OnThunderEnds;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        anim.enabled = false;
        StartCoroutine(StrikeWait());
    }
    public void Init(LightningArea area)
    {
        this.LA = area;
    }
    private void OnThunder()
    {
        if (LA != null)
        {
            LA.CheckDamage(transform.position);
        }
    }

    private void OnThunderDestroy()
    {
        if(OnThunderEnds != null)
        {
            OnThunderEnds();
        }

        Destroy(this.gameObject);
    }

    IEnumerator StrikeWait()
    {
        yield return new WaitForSeconds(waitTime);
        indic.SetActive(false);
        anim.enabled = true;
    }
}
