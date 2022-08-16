using Assets.Scripts.Utils;
using Assets.TestingAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLogic : MonoBehaviour
{
    #region Serialized
    [SerializeField] public float ProjectileSpeed = 45;
    [SerializeField] public float ProjectileDamage = 10;
    [SerializeField] public float MaxRange = 150;
    #endregion

    #region Private
    private Vector2 _direction;
    private float _lengthTraveled = 0;
    private float _rotationOffset = -90;
    #endregion

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

    }

    public void SetDirection(Vector2 direction)
    {
        this._direction = direction;
        var lookAt = (Vector2)this.transform.position + direction;
        this.transform.rotation = Quaternion.Euler(0, 0, MathUtility.FullAngle(Vector2.up, direction) + _rotationOffset);
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector2 possition = _direction * ProjectileSpeed * Time.fixedDeltaTime;
        this.transform.position += new Vector3(possition.x, possition.y, 0);
        _lengthTraveled += possition.magnitude;

        if (_lengthTraveled > MaxRange)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
       if (collision.tag == "Hope" || collision.tag == "Player")
        {
            IEnemy iEnemy = collision.gameObject.GetComponent<IEnemy>();
            if (iEnemy == null)
            {
                iEnemy = collision.gameObject.GetComponentInParent<IEnemy>();
            }

            if (iEnemy == null)
            {
                iEnemy = collision.transform.parent.GetComponentInChildren<IEnemy>();
            }

            iEnemy.TakeDamage(ProjectileDamage);
            Destroy(this.gameObject);
        }
    }
}
