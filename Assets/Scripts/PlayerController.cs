using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float walkSpeed = 2;
    [SerializeField] float walkMaxSpeed = 3;
    [SerializeField] float runSpeed = 4;
    [SerializeField] float runMaxSpeed = 5;

    [SerializeField] float jump = 3;
    [SerializeField] float gravity = 3;
    [SerializeField] float fallMaxSpeed = 2;

    [SerializeField] float drag = 2;
    [SerializeField] float mass = 1;

    [SerializeField] float groundRayDist = 1;
    [SerializeField] LayerMask groundLayerMask = 0;

    CharacterController character;
    Vector3 currentVelocity;

    private void Start()
    {
        character = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Moving();
        Gravity();
    }

    private void FixedUpdate()
    {
        // apply drag
        currentVelocity -= currentVelocity * drag * Time.fixedDeltaTime;

        if (currentVelocity.magnitude <= 0.1f)
            currentVelocity = Vector3.zero;

        // cap horizontal velocity
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        float v = horizontalVelocity.magnitude;

        if (v > CurrentMaxHorizontalVelocity())
        {
            Vector3 newVel = horizontalVelocity.normalized * CurrentMaxHorizontalVelocity();
            currentVelocity = new Vector3(newVel.x, currentVelocity.y, newVel.z);
        }

        // cap vertical velocity
        currentVelocity.y = Mathf.Clamp(currentVelocity.y, -fallMaxSpeed, fallMaxSpeed);

        // move character
        character.Move(transform.TransformVector(currentVelocity) * Time.fixedDeltaTime);
    }

    //==============================================================
    #region Input
    //--------------------------------------------------------------

    Vector2 directionInput;
    bool runInput;

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (!context.canceled)
            directionInput = context.ReadValue<Vector2>().normalized;
        else
            directionInput = Vector2.zero;
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.performed)
            Jump();
    }

    public void OnRunInput(InputAction.CallbackContext context)
    {
        if (context.started)
            runInput = true;

        if (context.canceled)
            runInput = false;
    }

    public void OnInteractInput(InputAction.CallbackContext context)
    {
        if (context.performed)
            Interact();
    }

    #endregion
    //==============================================================

    //==============================================================
    #region Movement
    //--------------------------------------------------------------

    void Moving()
    {
        Vector3 movement = new Vector3(directionInput.x, 0, directionInput.y).normalized;
        Vector3 acceleration = movement * CurrentHorizontalAcceleration() / mass;
        currentVelocity += acceleration;
    }

    void Interact()
    {
        Debug.Log("Hello");
    }

    void Gravity()
    {
        if (!IsGrounded())
        {
            Vector3 acceleration = Vector3.down * gravity / mass;
            currentVelocity += acceleration;
        }
    }

    void Jump()
    {
        if (IsGrounded())
        {
            currentVelocity.y = 0; // reset y vel first because game jumps aren't realistic lol

            Vector3 acceleration = Vector3.up * jump / mass;
            currentVelocity += acceleration;
        }
    }

    float CurrentHorizontalAcceleration()
    {
        float acceleration;

        if (runInput)
            acceleration = runSpeed;
        else
            acceleration = walkSpeed;

        return acceleration;
    }

    float CurrentMaxHorizontalVelocity()
    {
        float velocity;

        if (runInput)
            velocity = runMaxSpeed;
        else
            velocity = walkMaxSpeed;

        return velocity;
    }

    #endregion
    //==============================================================

    public bool IsGrounded()
    {
        if (Physics.Raycast(transform.position, -transform.up, groundRayDist, groundLayerMask))
            return true;
        else
            return false;
    }

    public void ResetVelocity()
    {
        currentVelocity = Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + -transform.up * groundRayDist);
    }
}