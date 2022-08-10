using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    private ParticleSpawn _spawner;
    private Rigidbody2D _rb;
    private bool _falling;
    private Transform _hope;
    private float _energy;


    public float timeToStopFalling;
    public float speedToHope;
    public float distanceToHope;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _falling = false;
    }
    private void FixedUpdate()
    {
        if(_rb.velocity.y <= 0.15f && !_falling)
        {
            StartCoroutine(DropTimer());
        }
        if (_hope != null)
        {
            StopAllCoroutines();
            _rb.velocity = MoveVector();
            if (Physics2D.OverlapCircle(transform.position,distanceToHope,LayerMask.GetMask("Hope")))
            {
                _spawner.ParticleDestroied(_energy,this);
            }
        }
    }
    IEnumerator DropTimer()
    {
        _falling = true;
        _rb.gravityScale = 1.7f;
        yield return new WaitForSeconds(timeToStopFalling);
        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0;
    }
    public void GoToHope(Transform hope,ParticleSpawn spawn)
    {
        StopAllCoroutines();
        _falling = true;
        _hope = hope;
        _spawner = spawn;
    }
    private Vector2 MoveVector()
    {
        return (_hope.position - transform.position).normalized * speedToHope;
    }
    public void Init(float energy)
    {
        _energy = energy;
    }
}
