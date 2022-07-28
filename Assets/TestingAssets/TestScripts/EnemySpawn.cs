using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class EnemySpawn : MonoBehaviour
{
    public int numberOfEnemies;
    public SpriteRenderer enemy;
    public SpriteRenderer test;

    private LineRenderer LR;
    private PolygonCollider2D spawnShape;
    private List<Vector2> _points;
    private Vector2 _topLeft;
    private Vector2 _bottomRight;

    #region Setup
    private void Awake()
    {
        LR = GetComponent<LineRenderer>();
        spawnShape = GetComponent<PolygonCollider2D>();
        CreatePoints();
    }
    private void CreatePoints()
    {
        UpdateLineRenderer();
        _points = new List<Vector2>();
        int x = LR.positionCount;
        for (int i =0; i<x;i++)
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
        Vector3[] line = new Vector3[points.Length+1];
        for(int i = 0; i < points.Length; i++)
        {
            line[i] = transform.TransformPoint((Vector3)points[i]);
        }
        line[line.Length - 1] = transform.TransformPoint((Vector3)points[0]);
        LR.SetPositions(line);
    }
    private void FindExtremes()
    {
        _topLeft = _points[0];
        _bottomRight = _points[0];
        foreach(Vector2 x in _points)
        {
            if(_topLeft.x > x.x)
            {
                _topLeft.x = x.x;
            }
            if(_topLeft.y < x.y)
            {
                _topLeft.y = x.y;
            }
            if(_bottomRight.x < x.x)
            {
                _bottomRight.x = x.x;
            }
            if(_bottomRight.y > x.y)
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
        for (int i = 0; i < n-1;i++)
        {
            if (CheckAxies(position, _points[i], _points[i + 1]))
            {
                count++;
            }
        }
        return count % 2 == 0 ? false : true;
    }
    private bool CheckAxies(Vector2 pos,Vector2 first, Vector2 second)
    {
        if(pos.y < first.y != pos.y < second.y &&
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
        Vector2 pos;
        int x = 0;
        while(x < numberOfEnemies)
        {
            pos = new Vector2(Random.Range(_topLeft.x, _bottomRight.x), Random.Range(_bottomRight.y, _topLeft.y));
            var point = Instantiate(enemy);
            point.transform.position = pos;
            point.transform.SetParent(transform);
            if (IsInside(pos))
            {
                point.color = Color.red;
            }
            else
            {
                point.color = Color.cyan;
            }
            x++;
            yield return new WaitForSeconds(0.05f);
        }
    }
    #endregion
    private void FixedUpdate()
    {
        if (IsInside(test.transform.position))
        {
            test.color = Color.red;
        }else
        {
            test.color = Color.cyan;
        }
    }
}
