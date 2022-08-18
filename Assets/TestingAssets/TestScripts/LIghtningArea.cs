using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Interfaces;
using Assets.TestingAssets;

public class LIghtningArea : MonoBehaviour
{
    private CircleCollider2D area;
    private bool active;
    private Vector2 previousPos;

    public float intesity = 1;
    public float damage = 15;
    [Range(0.5f, 2)]
    public float minDistance = 0.5f;
    public Thunder thunder;
    public float damageRadius = 1.5f;
    public float nockback = 20;
    public LayerMask mask;

    private void Awake()
    {
        active = true;
        area = GetComponent<CircleCollider2D>();
        StartCoroutine(Striking());
    }

    IEnumerator Striking()
    {
        previousPos = Position();
        while (true)
        {
            if (active)
            {
                yield return new WaitForSeconds(intesity);
                Vector2 newPos = new Vector2();
                do
                {
                    newPos = Position();
                    yield return new WaitForSeconds(0.01f);
                } while (!CheckPosition(newPos));
                SpawnThunder(newPos);
                previousPos = newPos;
                active = false;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    private Vector2 Position()
    {
        float x = transform.position.x + Random.Range(-area.radius, area.radius);
        float y = transform.position.y + Random.Range(-area.radius, area.radius);
    return new Vector2(x,y);
    }
    private bool CheckPosition(Vector2 pos)
    {
        if (Vector2.Distance(previousPos, pos) > minDistance && Vector2.Distance(transform.position, pos) <= area.radius)
        {
            return true;
        }
        return false;
    }
    public void SpawnThunder(Vector2 pos)
    {
        var obj = Instantiate(thunder);
        obj.transform.parent = this.transform;
        obj.Init(this);
        obj.transform.position = pos;
    }
    public void CheckDamage(Vector2 pos)
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(pos, damageRadius, mask);
        Debug.Log(cols.Length);
        if(cols.Length != 0)
        {
            foreach(var x in cols)
            {
                Vector2 nockbackVector = x.transform.position - transform.position.normalized;
                x.GetComponent<IEnemy>().TakeDamage(damage,nockback ,nockbackVector);
            }
        }
        active = true;
    }
}
