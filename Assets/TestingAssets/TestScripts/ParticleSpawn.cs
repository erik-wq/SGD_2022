using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawn : MonoBehaviour
{
    private ParticleSystem system;
    private CircleCollider2D radius;
    private bool canColect = false;
    private ParticleSystem.Particle[] m_Particles;
    public float ammount;
    private void Awake()
    {
        radius = GetComponent<CircleCollider2D>();
        system = GetComponent<ParticleSystem>();
        m_Particles = new ParticleSystem.Particle[system.main.maxParticles];
    }
    private void FixedUpdate()
    {
        if (system.time >= 0.35f)
        {
            canColect = true;
        }
        if (system.GetParticles(m_Particles) <= 0 && canColect)
        {
            Destroy(transform.parent.gameObject);
        }
        if(canColect)
        {
            CheckHope();
        }
    }
    private void CheckHope()
    {
        Collider2D[] hope = Physics2D.OverlapCircleAll(transform.position, radius.radius, LayerMask.GetMask("Hope"));
        if (hope.Length == 0) return;
        hope[0].GetComponent<EnergyPool>().AddEnergy(ammount);
        hope[0].GetComponent<HopeAi>().Collect();
        //print("colected");
        Destroy(transform.parent.gameObject);
    }
}
