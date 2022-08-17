using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thunder : MonoBehaviour
{
    private LIghtningArea LA;
    private Animator anim;
    public GameObject indic;
    public float waitTime;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        anim.enabled = false;
        StartCoroutine(StrikeWait());
    }
    public void Init(LIghtningArea area)
    {
        this.LA = area;
    }
    private void OnThunder()
    {
        Debug.Log(LA);
        if(LA != null)
        {
            LA.CheckDamage(transform.position);
        }
    }
    private void OnThunderDestroy()
    {
        Destroy(this.gameObject);
    }
    IEnumerator StrikeWait()
    {
        yield return new WaitForSeconds(waitTime);
        indic.SetActive(false);
        anim.enabled = true;
    }
}
