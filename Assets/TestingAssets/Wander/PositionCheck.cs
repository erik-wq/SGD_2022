using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]

public class PositionCheck : MonoBehaviour
{
    protected LineRenderer LR;
    protected PolygonCollider2D spawnShape;
    // spawn colider points
    protected List<Vector2> _points;
    protected Vector2 _topLeft;
    protected Vector2 _bottomRight;
    // optional colider points
    protected List<Vector2> _extraPoints;
    protected Vector2 _extraTopLeft;
    protected Vector2 _extraBottomRight;

    protected bool _extraBounds;

    protected float _max;
    [Header("Optional paremeters (if used need both of them)")]
    [Tooltip("Optional colider. Defines outer area outside normal polygon collider that is used to spawn enemies. In this area enemies can move but can't spawn")]

    public PolygonCollider2D outerPolygon;
    public LineRenderer outerLineRenderer;


    private void Awake()
    {
        _extraBounds = false;
        LR = GetComponent<LineRenderer>();
        spawnShape = GetComponent<PolygonCollider2D>();

        _max = Vector3.Distance(spawnShape.bounds.min, transform.position);
        CreatePoints();
    }
    protected virtual void CreatePoints()
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
        if (outerLineRenderer != null && outerPolygon != null)
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
    }


    public bool IsInside(Vector2 position)
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
    protected bool IsInsideOuter(Vector2 position)
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
    protected bool CheckOuterAxies(Vector2 pos, Vector2 first, Vector2 second)
    {
        if (pos.y < first.y != pos.y < second.y &&
            pos.x < (second.x - first.x) * (pos.y - first.y) / (second.y - first.y) + first.x)
        {
            return true;
        }
        return false;
    }
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
    
    #endregion

}
