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
    [Header("Enemy variables")]
    public float speed;
    #endregion

    #region Private
    private Rigidbody2D _rb;
    private Vector2 _direction;
    private EnemySpawn _spawner;
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
        _rb = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        //for (int i =0; i< 20; i++)
        //{
        //    print(MathUtility.NormalRNG(0, 180 / innerModifier) + " /"+innerModifier);
        //}
        //for (int i = 0; i < 20; i++)
        //{
        //    print(MathUtility.NormalRNG(0, 180 / midModifier)+ " /"+ midModifier);
        //}
        //for (int i = 0; i < 20; i++)
        //{
        //    print(MathUtility.NormalRNG(0, 180/outerModifier)+ " /"+outerModifier);
        //}
        //for (int i = 0; i < 20; i++)
        //{
        //    print(MathUtility.NormalRNG(0, 180 / farModifier) + " /"+farModifier);
        //}
        StartCoroutine(Moving());
    }
    private void FixedUpdate()
    {
        _rb.velocity = _direction * speed;
    }
    void ChangeDirention()
    {
        Vector2 dir = _spawner.transform.position - transform.position;
        dir = dir.normalized;
        float angle;

        if (!_spawner.InsideBounds(transform.position))
        {
            angle = MathUtility.NormalRNG(0, 180/farModifier);
            _direction = MathUtility.RotateVector(dir, angle);
            return;
        }
        if (Vector2.Distance(transform.position, _spawner.transform.position) < _spawner.GetInnerRadius() / 2) 
        {
            angle = MathUtility.NormalRNG(0, 180 / innerModifier);
            _direction = MathUtility.RotateVector(dir, angle);
        }
        else if(Vector2.Distance(transform.position, _spawner.transform.position) <= _spawner.GetInnerRadius()){
            angle = MathUtility.NormalRNG(0, 180 / midModifier);
            _direction = MathUtility.RotateVector(dir, angle);
        }
        else
        {
            angle = MathUtility.NormalRNG(0, 180 / outerModifier);
            _direction = MathUtility.RotateVector(dir, angle);
        }
    }
    IEnumerator Moving()
    {
        yield return new WaitForSeconds(0.2f);
        while (true)
        {
            if (_spawner != null)
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
        this._spawner = spawner;
    }
}
