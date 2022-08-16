using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroEventHandler : MonoBehaviour
{
    [SerializeField] private AudioSource SwordAudioSource;
    [SerializeField] private AudioClip SwordAudioClip;
    
    private PlayerController _mainScript;
    // Start is called before the first frame update
    void Start()
    {
        _mainScript = GetComponentInParent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDoDamage()
    {
        _mainScript.DoDamageFromAnimation();
    }

    public void OnAttackFinished()
    {
        _mainScript.OnAttackFinished();
    }

    public void OnSlashPlayMusic()
    {
        SwordAudioSource.PlayOneShot(SwordAudioClip);
    }
}
