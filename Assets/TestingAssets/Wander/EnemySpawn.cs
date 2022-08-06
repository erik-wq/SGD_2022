using Assets.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : PositionCheck
{
    #region Public
    [Header("Enemy settings")]
    [Tooltip("Total number of enemies that spawn in the area of the spawn")] 
    public int numberOfEnemies;
    [Tooltip("GameObjec(prefab) of enemy that will spawn in spawn area")] 
    public WanderingAI enemy;
    [Header("Enemy spawn settings")]
    [Tooltip("Changes how clase to center of circle collider enemies will spaw. Higher number more in center")]
    [Range(3.5f,4.5f)]
    public float density;
    [Tooltip("It change how close to each other will enemies spawn to each other")]
    [Range(1.7f,2)]
    public float radius;
    #endregion
    #region Serialized
    //layer mask setting
    [Header("Enemy parent")]
    [Tooltip("GameObject in hierarchi. All enemies are children of this GameObject.")]
    [SerializeField] 
    private Transform enemyParent;
    [Header("Physics Setings")]
    [SerializeField]
    [Tooltip("LayerMask of spawned enemies")]
    protected LayerMask enemyMask;

    #endregion

    #region Spawning
    protected override void CreatePoints()
    {
        base.CreatePoints();
        StartCoroutine(Spawning());
    }
    IEnumerator Spawning()
    {
        outerPolygon.enabled = false;
        spawnShape.enabled = false;
        Vector2 pos;
        int x = 0;
        while (x < numberOfEnemies)
        {
            float randx = MathUtility.NormalRNG(0, _max / density);
            float randy = MathUtility.NormalRNG(0, _max / density);
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
    protected bool CheckPosition(Vector2 pos)
    {
        if (Physics2D.OverlapCircle(pos, 1.7f, enemyMask))
        {
            return false;
        }
        return true;
    }
    #endregion
}
