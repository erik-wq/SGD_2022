using Assets.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]

public class EnemySpawn : MonoBehaviour
{
    #region Public
    [Header("Enemy settings")]
    [Tooltip("Total number of enemies that spawn in the area of the spawn")] 
    public int numberOfEnemies;
    [Tooltip("GameObjec(prefab) of enemy that will spawn in spawn area")] 
    public WanderingAI enemy;
    [Header("Enemy spawn settings")]
    [Tooltip("Collider used to get maximum radius")]
    public CircleCollider2D max;
    [Tooltip("Changes how clase to center of circle collider enemies will spaw. Higher number more in center")]
    [Range(3.5f,4.5f)]
    public float deviations;
    [Tooltip("It change how close to each other will enemies spawn to each other")]
    [Range(1.7f,2)]
    public float radius;
    [Header("Optional paremeters (if used need both of them)")]
    [Tooltip("Optional colider. Defines outer area outside normal polygon collider that is used to spawn enemies. In this area enemies can move but can't spawn")]
    public PolygonCollider2D outerPolygon;
    public LineRenderer outerLineRenderer;
    #endregion
    #region Private
    private LineRenderer LR;
    private PolygonCollider2D spawnShape;
    // spawn colider points
    private List<Vector2> _points;
    private Vector2 _topLeft;
    private Vector2 _bottomRight;
    // optional colider points
    private List<Vector2> _extraPoints;
    private Vector2 _extraTopLeft;
    private Vector2 _extraBottomRight;

    private bool _extraBounds;

    private float _max;
    #endregion
    #region Serialized
    //layer mask setting
    [Header("Physics Setings")]
    [SerializeField]
    [Tooltip("LayerMask of spawned enemies")]
    private LayerMask enemyMask;
    [Header("Enemy parent")]
    [Tooltip("GameObject in hierarchi. All enemies are children of this GameObject.")]
    [SerializeField] 
    private Transform enemyParent;
    #endregion

    #region Setup
    private void Awake()
    {
        _extraBounds = false;
        _max = max.radius;
        LR = GetComponent<LineRenderer>();
        spawnShape = GetComponent<PolygonCollider2D>();
        CreatePoints();
    }
    private void CreatePoints()
    {
        UpdateLineRenderer();
        _points = new List<Vector2>();
        int x = LR.positionCount;
        for (int i = 0; i < x; i++)
        {
            _points.Add(LR.GetPosition(i));
        }
        _points.Add(LR.GetPosition(0));
        FindExtremes();
    }
    private void UpdateLineRenderer()
    {
        Vector2[] points = spawnShape.points;
        LR.positionCount = points.Length + 1;
        Vector3[] line = new Vector3[points.Length + 1];
        for (int i = 0; i < points.Length; i++)
        {
            line[i] = transform.TransformPoint((Vector3)points[i]);
        }
        line[line.Length - 1] = transform.TransformPoint((Vector3)points[0]);
        LR.SetPositions(line);
        if(outerLineRenderer != null && outerPolygon != null)
        {
            _extraBounds = true;
            UpadateOuterLineRender();
        }
    }
    private void UpadateOuterLineRender()
    {
        Vector2[] points = outerPolygon.points;
        outerLineRenderer.positionCount = points.Length + 1;
        Vector3[] line = new Vector3[points.Length + 1];
        for (int i = 0; i < points.Length; i++)
        {
            line[i] = transform.TransformPoint((Vector3)points[i]);
        }
        line[line.Length - 1] = transform.TransformPoint((Vector3)points[0]);
        outerLineRenderer.SetPositions(line);
        ExtraPoints();
    }
    private void ExtraPoints()
    {
        _extraPoints = new List<Vector2>();
        int x = outerLineRenderer.positionCount;
        for (int i = 0; i < x; i++)
        {
            _extraPoints.Add(outerLineRenderer.GetPosition(i));
        }
        _extraPoints.Add(outerLineRenderer.GetPosition(0));
        FindExtraExtremes();
    }
    private void FindExtraExtremes()
    {
        _extraTopLeft = _extraPoints[0];
        _extraBottomRight = _extraPoints[0];
        foreach (Vector2 x in _extraPoints)
        {
            if (_extraTopLeft.x > x.x)
            {
                _extraTopLeft.x = x.x;
            }
            if (_extraTopLeft.y < x.y)
            {
                _extraTopLeft.y = x.y;
            }
            if (_extraBottomRight.x < x.x)
            {
                _extraBottomRight.x = x.x;
            }
            if (_extraBottomRight.y > x.y)
            {
                _extraBottomRight.y = x.y;
            }
        }
    }
    private void FindExtremes()
    {
        _topLeft = _points[0];
        _bottomRight = _points[0];
        foreach (Vector2 x in _points)
        {
            if (_topLeft.x > x.x)
            {
                _topLeft.x = x.x;
            }
            if (_topLeft.y < x.y)
            {
                _topLeft.y = x.y;
            }
            if (_bottomRight.x < x.x)
            {
                _bottomRight.x = x.x;
            }
            if (_bottomRight.y > x.y)
            {
                _bottomRight.y = x.y;
            }
        }
        StartCoroutine(Spawning());
    }
    #endregion
    #region PositionCheck
    private bool IsInside(Vector2 position)
    {
        int n = _points.Count;
        int count = 0;
        for (int i = 0; i < n - 1; i++)
        {
            if (CheckAxies(position, _points[i], _points[i + 1]))
            {
                count++;
            }
        }
        return count % 2 == 0 ? false : true;
    }
    private bool CheckAxies(Vector2 pos, Vector2 first, Vector2 second)
    {
        if (pos.y < first.y != pos.y < second.y &&
            pos.x < (second.x - first.x) * (pos.y - first.y) / (second.y - first.y) + first.x)
        {
            return true;
        }
        return false;
    }
    bool CheckPosition(Vector2 pos)
    {
        if (Physics2D.OverlapCircle(pos, 1.7f, enemyMask)){
            return false;
        }
        return true;
    }
    private bool IsInsideOuter(Vector2 position)
    {
        int n = _extraPoints.Count;
        int count = 0;
        for (int i = 0; i < n - 1; i++)
        {
            if (CheckOuterAxies(position, _extraPoints[i], _extraPoints[i + 1]))
            {
                count++;
            }
        }
        return count % 2 == 0 ? false : true;
    }
    private bool CheckOuterAxies(Vector2 pos, Vector2 first, Vector2 second)
    {
        if (pos.y < first.y != pos.y < second.y &&
            pos.x < (second.x - first.x) * (pos.y - first.y) / (second.y - first.y) + first.x)
        {
            return true;
        }
        return false;
    }
    #endregion
    #region Spawning
    IEnumerator Spawning()
    {
        outerPolygon.enabled = false;
        spawnShape.enabled = false;
        max.enabled = false;
        Vector2 pos;
        int x = 0;
        while (x < numberOfEnemies)
        {
            float randx = MathUtility.NormalRNG(0, _max / deviations);
            float randy = MathUtility.NormalRNG(0, _max / deviations);
            pos = new Vector2(transform.position.x + randx,transform.position.y + randy);
            if (IsInside(pos) && CheckPosition(pos))
            {
                Spawn(pos);
                x++;
            }
            yield return new WaitForSeconds(0.02f);
        }
    }
    private void Spawn(Vector2 pos)
    {
        var en = Instantiate(enemy);
        en.Init(this);
        en.transform.position = pos;
        en.transform.SetParent(enemyParent);
    }
    #endregion
    #region EnemyMethods
    public bool InsideBounds(Vector2 position)
    {
        if (IsInside(position))
        {
            return true;
        }
        if (_extraBounds)
        {
            if (IsInsideOuter(position))
            {
                return true;
            }
        }
        return false;
    }
    public float GetInnerRadius()
    {
        return _max;
    }
    //public float GetOuterRadius()
    //{
    //    if(outerPolygon != null && outerLineRenderer != null)
    //    {
    //        return  
    //    }
    //    return 0;
    //}
    #endregion
}
