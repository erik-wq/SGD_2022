using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAgro : MonoBehaviour
{
    public float agroRange;
    private Collider2D[] _cols;
    [SerializeField] LayerMask mask;
    private void FixedUpdate()
    {
        _cols = Physics2D.OverlapCircleAll(transform.position, agroRange, mask);
        if(_cols.Length > 0)
        {
            foreach(Collider2D x in _cols)
            {
                WanderingAI ai = x.GetComponent<WanderingAI>();
                ai.ChasePlayer(transform,agroRange);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Color color = Color.yellow;
        color.a = 0.2f;
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, agroRange);
    }
}
