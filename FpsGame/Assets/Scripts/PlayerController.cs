using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Effects;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform playerCamera = null;                       // Mouse looking
    [SerializeField] GameObject standCamera = null;
    [SerializeField] GameObject crouchCamera = null;

    [SerializeField] KeyCode runKey;                                      // Player movement
    [SerializeField] KeyCode crouchKey;                                   // Player crouch input

    [SerializeField] float mouseSesitivity;                               // Mouse looking
    [SerializeField] float walkSpeed, runSpeed;                           // Player movement
    [SerializeField] float runBuildUpSpeed;                               // Player movement
    [SerializeField] float gravity = -13f;                                // Player movement
    [SerializeField] float jumpMultiplier;                                // Player jump input
    [SerializeField] float smoothCrouchingCamera = 0.3f;                  // Player crouch input
    [SerializeField] [Range(0.0f, 0.5f)] float moveSmoothTime = 0.3f;     // Player looking
    [SerializeField] [Range(0.0f, 0.5f)] float mouseSmoothTime = 0.03f;   // Mouse movement

    [SerializeField] bool lockCursor = true;                              // Mouse movement

    [SerializeField] AnimationCurve jumpFallOff;                          // Player jump input

    [SerializeField] GameObject standingCollider;                         // Player crouch input
    [SerializeField] GameObject crouchingCollider;                        // Player crouch input

    private float movementSpeed;
    private float velocityY;
    private float cameraPitch = 0.0f;
    private float crouchHeight = 0.5f;
    private float standHeight;
    private Vector2 currentDir = Vector2.zero;                            // Player movement
    private Vector2 currentDirVelocity = Vector2.zero;                    // Player movement
    private Vector2 currentMouseDelta = Vector2.zero;                     // Mouse movement
    private Vector2 currentMouseDeltaVelocity = Vector2.zero;             // Mouse movement

    private bool runCheck = false;
    private bool isCrouchingCamera = false;                               // Player crouch input
    private bool isCrouching = true;                                      // Player crouch input
    private bool crouchingCameraCheck = false;

    CharacterController controller = null;

    void Start()
    {
        {
            controller = GetComponent<CharacterController>();
            if (lockCursor)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            standHeight = controller.height;
            crouchingCollider.SetActive(false);
        }
    }

    void Update()
    {
        UpdateMouseLook();
        UpdateMovement();
    }

    void UpdateMouseLook()
    {
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);

        cameraPitch -= currentMouseDelta.y * mouseSesitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -85f, 85f);
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;

        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSesitivity);
    }

    void UpdateMovement()
    {
        Vector2 targetDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        targetDir.Normalize();

        currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, moveSmoothTime);

        if (controller.isGrounded)
        {
            velocityY = 0.0f;
        }

        velocityY += gravity * Time.deltaTime;

        Vector3 velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * movementSpeed + Vector3.up * velocityY;

        controller.Move(velocity * Time.deltaTime);

        setMovementSpeed();
        JumpInput();
        crouchMovement();

    }

    private void setMovementSpeed()
    {
        if(Input.GetKey(runKey) && !runCheck)
        {
            movementSpeed = Mathf.Lerp(movementSpeed, runSpeed, Time.deltaTime * runBuildUpSpeed);
        }
        else
        {
            movementSpeed = Mathf.Lerp(movementSpeed, walkSpeed, Time.deltaTime * runBuildUpSpeed);
        }
    }

    private void JumpInput()
    {
        if(Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            StartCoroutine(JumpEvent());
        }
    }

    private IEnumerator JumpEvent()
    {
        controller.slopeLimit = 90.0f;
        float timeInAir = 0.0f;
        do
        {
            float jumpForce = jumpFallOff.Evaluate(timeInAir);
            controller.Move(Vector3.up * jumpForce * jumpMultiplier * Time.deltaTime);
            timeInAir += Time.deltaTime;
            yield return null;
        } while (!controller.isGrounded && controller.collisionFlags != CollisionFlags.Above);
        controller.slopeLimit = 45.0f;
    }

    private void crouchMovement()
    {
        Vector3 currentCamera = playerCamera.position;
        Vector3 standingOffset = standCamera.transform.position;
        Vector3 crouchingOffset = crouchCamera.transform.position;
        Vector3 standingCamera = new Vector3(standingOffset.x, standingOffset.y, standingOffset.z);
        Vector3 crouchingCamera = new Vector3(currentCamera.x, crouchingOffset.y, currentCamera.z);

        if (Input.GetKeyDown(crouchKey))
        {
            if (isCrouching && !runCheck)
            {
                isCrouching = false;
                runCheck = true;
                controller.height = crouchHeight;
                controller.center = new Vector3(0, 0.5f, 0);
                standingCollider.SetActive(false);
                crouchingCollider.SetActive(true);

                playerCamera.transform.position = Vector3.SmoothDamp(currentCamera, crouchingCamera, ref currentCamera, smoothCrouchingCamera * Time.deltaTime);
            }
        }
        else if(Input.GetKeyUp(crouchKey))
        {
            if (!isCrouching && runCheck)
            {
                isCrouching = true;
                runCheck = false;
                controller.height = standHeight;
                controller.center = new Vector3(0, 1.0f, 0);
                standingCollider.SetActive(true);
                crouchingCollider.SetActive(false);

                playerCamera.transform.position = Vector3.SmoothDamp(currentCamera, standingCamera, ref currentCamera, smoothCrouchingCamera * Time.deltaTime);
            }

        }
    }
}
