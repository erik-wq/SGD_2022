using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Utils;

public class WanderingAI : MonoBehaviour
{
    #region Public
    [Header("Walk timers variables")]
    public float minWalkTime;
    public float maxWalkTime;
    [Header("Stay timers variables")]
    public float minStayTime;
    public float maxStayTime;
    [Header("Collision timers variables")]
    public float minCollTime;
    public float maxCollTime;
    [Header("Enemy variables")]
    public float speed;
    public BasicFollow folow;
    #endregion

    #region Private
    private Rigidbody2D _rb;
    private Vector2 _direction;
    public PositionCheck spawn;
    private bool _stop;
    #endregion
    #region Serialized
    [Header("Angle modifiers.\nHow much angle will go in direction of midle spawn point\nHigher value higher chance for midle direction")]
    [SerializeField]
    [Tooltip("Half of radius of spawn circle")]
    [Range(1,1.9f)]
    private float innerModifier;
    [SerializeField]
    [Tooltip("Full radius of spawn circle")]
    [Range(1.75f,4f)]
    private float midModifier;
    [SerializeField]
    [Tooltip("If is outer polygon region")]
    [Range(3.25f,8f)]
    private float outerModifier;
    [SerializeField]
    [Tooltip("if is out outer polygon region")]
    [Range(6f,10)]
    private float farModifier;

    #endregion

    private void Awake()
    {
        _stop = false;
        _rb = GetComponent<Rigidbody2D>();
        _rb.velocity = Vector2.zero;
    }
    private void Start()
    {
        folow.enabled = false;
        StartCoroutine(Moving());
    }
    private void FixedUpdate()
    {
        if (!_stop)
        {
            _rb.velocity = _direction * speed;
        }else
        {
            _rb.velocity = Vector2.zero;
        }
    }
    void ChangeDirention()
    {
        Vector2 dir = spawn.transform.position - transform.position;
        float angle;

        if (!spawn.InsideBounds(transform.position))
        {
            angle = MathUtility.NormalRNG(0, 180/farModifier);
            _direction = MathUtility.RotateVector(dir, angle).normalized;
            return;
        }
        if (Vector2.Distance(transform.position, spawn.transform.position) < spawn.GetInnerRadius() / 2) 
        {
            angle = MathUtility.NormalRNG(0, 180 / innerModifier);
            _direction = MathUtility.RotateVector(dir, angle).normalized;
        }
        else if(Vector2.Distance(transform.position, spawn.transform.position) <= spawn.GetInnerRadius()){
            angle = MathUtility.NormalRNG(0, 180 / midModifier);
            _direction = MathUtility.RotateVector(dir, angle).normalized;
        }
        else
        {
            angle = MathUtility.NormalRNG(0, 180 / outerModifier);
            _direction = MathUtility.RotateVector(dir, angle).normalized;
        }
    }
    IEnumerator Moving()
    {
        yield return new WaitForSeconds(0.2f);
        while (true)
        {
            if (spawn != null)
            {
                if(Random.value <= 0.5)
                {
                    _direction = Vector2.zero;
                    yield return new WaitForSeconds(Random.Range(minStayTime,maxStayTime));
                }
                ChangeDirention();
                yield return new WaitForSeconds(Random.Range(minWalkTime, maxWalkTime));
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    public void Init(EnemySpawn spawner)
    {
        this.spawn = spawner;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 dir;
        float angle;
        ContactPoint2D[] points = collision.contacts;
        if (collision.gameObject.tag == "Zombie")
        {
            _stop = true;
            StopAllCoroutines();
            dir = -(points[0].point - (Vector2)transform.position).normalized;
            angle = MathUtility.NormalRNG(0, 180 / 5);
            _direction = MathUtility.RotateVector(dir, angle);
            StartCoroutine(CollisionTimer());
            return;
        }
        foreach (ContactPoint2D x in points)
        {
            Debug.Log("collision" + this.gameObject.name + " other " + collision.gameObject.name);
            Debug.Log(x.point);
        }
        StopAllCoroutines();
        _stop = true;
        dir = -(points[0].point - (Vector2)transform.position).normalized;
        angle = MathUtility.NormalRNG(0, 180 / 5);
        _direction = MathUtility.RotateVector(dir, angle);
        StartCoroutine(CollisionTimer());
    }
    IEnumerator CollisionTimer()
    {
        yield return new WaitForSeconds(Random.Range(minCollTime, maxCollTime));
        _stop = false;
        yield return new WaitForSeconds(Random.Range(minWalkTime, maxWalkTime));
        StartCoroutine(Moving());
    }
    public void ChasePlayer(Transform player, float range)
    {
        StopAllCoroutines();
        _stop = true;
        _rb.velocity = Vector2.zero;
        folow.enabled = true;
        folow.SetTarget(player);
        StartCoroutine(Returning(player, range));
    }
    IEnumerator Returning(Transform player, float range)
    {
        while (Vector2.Distance(transform.position,player.position) < range)
        {
            yield return new WaitForSeconds(0.02f);
        }
        folow.SetTarget(spawn.transform);
        bool inside = false;
        while (!inside)
        {
            yield return new WaitForSeconds(0.2f);
            if (spawn.InsideBounds(transform.position))
            {
                inside = true;
            }
        }
        folow.SetTarget(null);
        folow.enabled = false;
        _stop = false;
        StartCoroutine(Moving());
    }

}
