using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Utils;

public class ParticleSpawn : MonoBehaviour
{
    private List<Particle> spawnedParticles;
    private EnergyPool _hope;

    public int numberOfParticles;
    public float energyAmmount;
    public float hopeCheckRadius;
    public float forcePower;
    public Particle particlePrefab;
    public float timeToDespawn;

    private void Awake()
    {
        spawnedParticles = new List<Particle>();
        StartCoroutine(Spawn());
    }
    private void FixedUpdate()
    {
            CheckHope();
    }
    private void CheckHope()
    {
        Collider2D[] hope = Physics2D.OverlapCircleAll(transform.position, hopeCheckRadius, LayerMask.GetMask("Hope"));
        if (hope.Length == 0) return;
        _hope = hope[0].GetComponent<EnergyPool>();
        foreach (var x in spawnedParticles)
        {
            if (x != null)
            {
                x.GoToHope(hope[0].transform, this);
            }
        }
    }
    IEnumerator Spawn()
    {
        for (int i = 0; i < numberOfParticles; i++)
        {
            var particl = Instantiate(particlePrefab);
            spawnedParticles.Add(particl);
            particl.transform.position = transform.position;
            particl.transform.SetParent(transform);
            particl.Init(energyAmmount / numberOfParticles);
            Rigidbody2D rb = particl.GetComponent<Rigidbody2D>();
            Vector2 dir = MathUtility.RotateVector(Vector2.up, Random.Range(-50f, 50f)) * forcePower;
            rb.gravityScale = 1;
            rb.AddForce(dir, ForceMode2D.Impulse);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.45f));
        }
        StartCoroutine(Despawn());
    }
    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(timeToDespawn);
        while (spawnedParticles.Count > 0)
        {
            Destroy(spawnedParticles[spawnedParticles.Count - 1].gameObject);
            spawnedParticles.RemoveAt(spawnedParticles.Count-1);
            if(spawnedParticles.Count > 0)
            {
            yield return new WaitForSeconds(Random.Range(0.2f,0.3f));
            }
        }
        Destroy(transform.parent.gameObject);
    }
    public void ParticleDestroied(float energy,Particle part)
    {
        _hope.AddEnergy(energy);
        spawnedParticles.Remove(part);
        if (spawnedParticles.Count <= 0)
        {
            _hope.GetComponent<HopeAI>().Collect();
            StopAllCoroutines();
            Destroy(transform.parent.transform.gameObject);
        }
    }
}
