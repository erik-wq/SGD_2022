using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Serialized
    [SerializeField] private float MovementSpeedForward = 6f;
    [SerializeField] private float MovementSpeedForwardSideway = 8f;
    [SerializeField] private float MovementSpeedBackwards = 4f;
    [SerializeField] private float MovementSpeedBackwardsSideway = 3f;


    [SerializeField] private float CollisionOffset = 0.05f;
    [SerializeField] private ContactFilter2D CollisionsFilter;
    [SerializeField] private AstarPath Pathfinder;
    [SerializeField] private BasicFollow HopesFollow;
    [SerializeField] private Transform MouseIndicatorTransform;
    [SerializeField] private Camera MainCamera;
    [SerializeField] private CameraManagement CameraManagement;

    [SerializeField] private AnimationCurve SprintCurve;
    [SerializeField] private float SprintLenght = 3;

    /// <summary>
    /// Speed added to normal movement speed;
    /// </summary>
    [SerializeField] private float SprintSpeed = 5;

    #endregion

    #region Private
    private Vector2 _movementInput = Vector2.zero;
    private Rigidbody2D _rigidBody2D;
    private float _mouseIndicatorOffset = 2;
    //private PlayerControls _playerControls;
    private float _sprintStart;
    private bool _sprintInProgress = false;
    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        //_playerControls = new PlayerControls();
        HopesFollow.Target = this.transform;
    }

    //void Awake()
    //{
    //    _playerControls = new PlayerControls();
    //    _playerControls.Player.Sprint.performed += (arg) => StartSprint();
    //}

    // Update is called once per frame
    private void Update()
    {
        RegisterButtons();
    }

    private void FixedUpdate()
    {
        if (_movementInput != Vector2.zero)
        {
            Move(_movementInput);
        }
        else
        {
            CameraManagement.SetMoving(false);
        }

        MoveIndicator();
    }

    private void OnEnable()
    {
        //_playerControls.Disable();
    }

    private void OnDisable()
    {
        //_playerControls.Enable();
    }

    private void RegisterButtons()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartSprint();
        }
    }

    private void StartSprint()
    {
        _sprintStart = Time.time;
        _sprintInProgress = true;
    }

    private void MoveIndicator()
    {
        Vector2 newPossition = (Vector2)this.transform.position + (_mouseIndicatorOffset * GetFaceDirection());
        MouseIndicatorTransform.position = new Vector3(newPossition.x, newPossition.y, MouseIndicatorTransform.position.z);
    }

    private Vector2 GetFaceDirection()
    {
        Ray ray = MainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector2 direction = ((Vector2)ray.direction).normalized;
        return direction;

        //Vector3 mousePos = Mouse.current.position.ReadValue();
        //mousePos.z = MainCamera.nearClipPlane;
        //var worldPosition = MainCamera.ScreenToWorldPoint(mousePos);
        //return ((Vector2)this.transform.position - (Vector2)worldPosition).normalized;
    }

    private float GetMovementSpeed(Vector2 input)
    {
        float dotProduct = Vector2.Dot(input, GetFacingDirectionRounded());
        if (dotProduct == -1)
        {
            return MovementSpeedBackwards;
        }
        else if (dotProduct < -1)
        {
            return MovementSpeedBackwardsSideway;
        }
        else
        {
            if (input.x == 0 || input.y == 0)
            {
                return MovementSpeedForward;
            }
            else
            {
                return MovementSpeedForwardSideway;
            }
        }
    }

    public Vector2 GetFacingDirectionRounded()
    {
        var direction = GetFaceDirection();
        return new Vector2(Mathf.RoundToInt(direction.x), Mathf.RoundToInt(direction.y));
    }

    public float GetMovementSpeedWithSprint(Vector2 input)
    {
        var basicSpeed = GetMovementSpeed(input);
        var pointAtSprint = (Time.time - _sprintStart) / SprintLenght;
        var value = SprintCurve.Evaluate(pointAtSprint);

        if ((_sprintStart + SprintLenght) < Time.time)
        {
            _sprintInProgress = false;
        }

        return basicSpeed + value * SprintSpeed;

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

        float movementSpeed = 0;

        if (_sprintInProgress)
        {
            movementSpeed = GetMovementSpeedWithSprint(input);
        }
        else
        {
            movementSpeed = GetMovementSpeed(input);
        }

        CameraManagement.SetPlayersSpeed(movementSpeed);

        List<RaycastHit2D> foundCollisions = new List<RaycastHit2D>();
        var freeDirection = MovementUtility.CastAndAdjust(_rigidBody2D,
                            input,
                            CollisionsFilter,
                            foundCollisions,
                            movementSpeed * Time.fixedDeltaTime + CollisionOffset
                          );

        if (freeDirection != Vector2.zero)
        {
            _rigidBody2D.MovePosition(_rigidBody2D.position + freeDirection * movementSpeed * Time.fixedDeltaTime);
        }
    }
    #endregion
}
