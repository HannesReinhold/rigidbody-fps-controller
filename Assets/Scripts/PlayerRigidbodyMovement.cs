using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRigidbodyMovement : MonoBehaviour
{
    [Header("Camera Control")]
    public float MouseSensitivity = 100f;

    private float RotationX = 0f;
    private float RotationY = 0f;

    [Header("Movement Speed")]
    public float WalkSpeed = 12f;
    public float SprintSpeed = 18f;
    public float CrouchingSpeed = 5f;
    public float AirMultiplier = 0.5f;

    private float CurrentMaxSpeed = 0f;
    private float CurrentSpeed = 0f;

    [Header("Height")]
    public float StandingHeight = 1f;
    public float CrouchingHeight = 0.5f;

    private Vector3 OriginalScale;

    [Header("Jumping")]
    public float JumpHeight = 3f;
    public bool AutoJump = false;
    private Vector3 Normal;

    [Header("Ground Check")]
    public LayerMask GroundMask;
    public float MaxSlopeAngle = 45f;

    [Header("References")]
    public Rigidbody rigidbody;
    public Camera PlayerCamera;
    public Transform PlayerBody;


    private bool IsMoving;
    private bool IsGrounded;
    private bool IsSprinting;
    private bool IsCrouching;
    private bool CanJump;
    private bool IsJumping;

    private float InputX = 0f;
    private float InputZ = 0f;



    private void Start()
    {
        CurrentMaxSpeed = WalkSpeed;
        OriginalScale = transform.localScale;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        UpdateControls();
        UpdateMaxSpeed();
        Look();

    }


    void FixedUpdate()
    {
        UpdateMovement();
    }


    void UpdateControls()
    {
        InputX = Input.GetAxisRaw("Horizontal");
        InputZ = Input.GetAxisRaw("Vertical");

        IsSprinting = Input.GetKey(KeyCode.LeftShift) && InputZ == 1 && !IsCrouching;
        IsCrouching = Input.GetKey(KeyCode.LeftControl);
        IsMoving = InputX != 0 || InputZ != 0;

        if (Input.GetKeyDown(KeyCode.Space)) CanJump = true;
        if (Input.GetKeyUp(KeyCode.Space)) CanJump = false;
    }


    void UpdateMaxSpeed()
    {
        if (IsSprinting)
        {
            IsCrouching = false;
            CurrentMaxSpeed = SprintSpeed;

        }
        else if (IsCrouching)
        {
            IsSprinting = false;
            CurrentMaxSpeed = CrouchingSpeed;

            SetPlayerHeightTo(CrouchingHeight);

        }
        else
        {
            CurrentMaxSpeed = WalkSpeed;

            SetPlayerHeightTo(StandingHeight);

        }
    }


    void UpdateMovement()
    {
        Vector3 move = transform.right * InputX + transform.forward * InputZ;
        move.Normalize();

        if (IsGrounded)
        {
            if (IsMoving)
            {
                rigidbody.AddForce(move * 500 * Time.deltaTime * CurrentMaxSpeed);
                rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, CurrentMaxSpeed);
            }
            else
            {
                rigidbody.AddForce(-rigidbody.velocity * 500 * Time.deltaTime);
            }
        }
        else
        {
            rigidbody.AddForce(move * 50 * CurrentMaxSpeed * AirMultiplier * Time.deltaTime);
        }


        if (IsGrounded && CanJump && !IsJumping) Jump();
    }


    void Jump()
    {
        Vector3 velocity = rigidbody.velocity;
        velocity.y = 0;
        rigidbody.velocity = velocity;

        IsGrounded = false;
        IsJumping = true;
        if (!AutoJump) CanJump = false;

        rigidbody.AddForce(Normal * JumpHeight * 1.5f, ForceMode.Impulse);
        rigidbody.AddForce(Vector3.up * JumpHeight * 0.5f, ForceMode.Impulse);

        Invoke("ResetJump", 0.4f);
    }

    void ResetJump()
    {
        IsJumping = false;
    }


    void SetPlayerHeightTo(float height)
    {
        Vector3 scale = OriginalScale;
        scale.y = height;
        transform.localScale = scale;
    }


    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.fixedDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.fixedDeltaTime;

        RotationX -= mouseY;
        RotationX = Mathf.Clamp(RotationX, -90f, 90f);

        RotationY += mouseX;

        PlayerCamera.transform.rotation = Quaternion.Euler(RotationX, RotationY, 0f);

        Quaternion DesiredRotation = Quaternion.Euler(0f, RotationY, 0f);
        PlayerBody.localRotation = Quaternion.Slerp(PlayerBody.rotation, DesiredRotation, Time.deltaTime * 30);
    }


    void OnCollisionStay(Collision collision)
    {
        int layer = collision.gameObject.layer;

        if (GroundMask != (GroundMask | (1 << layer))) return;

        for(int i=0; i<collision.contactCount; i++)
        {
            Vector3 normal = collision.contacts[i].normal;

            if (CheckIfWalkable(normal))
            {
                IsGrounded = true;
                Normal = normal;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        int layer = collision.gameObject.layer;
        if (GroundMask == (GroundMask | (1 << layer))) IsGrounded = false ;
    }

    bool CheckIfWalkable(Vector3 n)
    {
        float angle = Vector3.Angle(Vector3.up, n);
        return angle <= MaxSlopeAngle;
    }

}
