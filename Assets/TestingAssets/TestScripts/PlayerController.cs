using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Serialized
    [SerializeField] private float MovementSpeed = 4f;
    [SerializeField] private float CollisionOffset = 0.05f;
    [SerializeField] private ContactFilter2D CollisionsFilter;
    [SerializeField] private AstarPath Pathfinder;
    [SerializeField] private BasicFollow HopesFollow;
    #endregion

    #region Private
    private Vector2 _movementInput = Vector2.zero;
    private Rigidbody2D _rigidBody2D;
    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        HopesFollow.Target = this.transform;
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Move(_movementInput);    
    }

    #region Public Functions
    public void TakeHit(int damage, Vector2 position, float attackKnockbackPower)
    {
        Vector2 normal = (_rigidBody2D.position - position).normalized;
        _rigidBody2D.AddForce(normal * attackKnockbackPower);
    }
    #endregion

    #region Inputs registering
    private void OnMove(InputValue input)
    {
        _movementInput = input.Get<Vector2>();
    }
    #endregion

    #region Inputs handeling
    private void Move(Vector2 input)
    {
        if (input == Vector2.zero)
            return;
        
        List<RaycastHit2D> foundCollisions = new List<RaycastHit2D>();
        var freeDirection = MovementUtility.CastAndAdjust(_rigidBody2D,
                            input,
                            CollisionsFilter,
                            foundCollisions,
                            MovementSpeed * Time.fixedDeltaTime + CollisionOffset
                          );

        if (freeDirection != Vector2.zero)
        {
            _rigidBody2D.MovePosition(_rigidBody2D.position + freeDirection * MovementSpeed * Time.fixedDeltaTime);
        }
    }
    #endregion
}
