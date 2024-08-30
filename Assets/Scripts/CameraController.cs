using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // Serialize Variables
    [SerializeField] private GameObject objectToFollow;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float verticalRotationLimit = 85f;
    [SerializeField] private GameObject head;

    // Local Variables
    private Transform transformObjectToFollow, transformCamera;
    private Vector3 offset;
    private float fixYOffset, distance;
    private float verticalRotation = 0f;
    private float yawRotation = 0f; // To store yaw rotation

    // Perspective
    private enum Perspective
    {
        FirstPerson,
        ThirdPerson
    }
    private Perspective _perspective;

    // Start function
    void Start()
    {
        _perspective = Perspective.ThirdPerson; // Set perspective to 3rd person
        head.SetActive(true); // Set head to true -> 3rd person
        InitCamera(); // Initialize camera
    }
    
    // Update function
    void Update()
    {
        // Check perspective and adjust the cameras position and rotation
        if (_perspective == Perspective.ThirdPerson)
        {
            transformCamera.position = transformObjectToFollow.position + offset;
            transformCamera.LookAt(new Vector3(transformObjectToFollow.position.x,
                transformObjectToFollow.position.y + 1.6f, transformObjectToFollow.position.z));
        }
        else
        {
            // Update the camera's position in first-person mode to always follow the player's head
            transformCamera.position = transformObjectToFollow.position + Vector3.up * 1f; // Adjust this for head height
        }

        // Check if perspective changes
        if (Input.GetKeyDown(KeyCode.V))
        {
            ChangePerspective();
        }
    }

    // Initialize Camera function
    void InitCamera()
    {
        transformObjectToFollow = objectToFollow.GetComponent<Transform>();
        transformCamera = gameObject.GetComponent<Transform>();
        SetInitialCameraPosition();
        SetOffset();
    }

    // Set initial camera position funciton
    void SetInitialCameraPosition()
    {
        // Check perspective
        if (_perspective == Perspective.ThirdPerson)
        {
            // Set camera to initial third-person position -> in relation to the player
            transformCamera.position = transformObjectToFollow.position - transformObjectToFollow.forward * 3.5f + Vector3.up * 2f;
            transformCamera.rotation = Quaternion.Euler(17.58f, transformCamera.eulerAngles.y, transformCamera.eulerAngles.z);
        }
        else
        {
            // Set camera to initial first-person position -> in relation to the player
            transformCamera.position = transformObjectToFollow.position + Vector3.up * 1f; // Assuming head height for first-person
            transformCamera.rotation = Quaternion.Euler(0f, transformObjectToFollow.eulerAngles.y, 0f);
        }
        
        distance = Vector3.Distance(transformCamera.position, transformObjectToFollow.position);
        fixYOffset = transformCamera.position.y - transformObjectToFollow.position.y;
    }

    // Calculate and set the offset if the perspective is set to 3rd person
    void SetOffset()
    {
        if (_perspective == Perspective.ThirdPerson)
        {
            offset = transformCamera.position - transformObjectToFollow.position;
            offset.Normalize();
            offset *= distance;
            offset.y = fixYOffset;
        }
    }

    // Change perspective function
    void ChangePerspective()
    {
        // Check perspective and change accordingly
        // Also (de-)activate head to avoid interfering in 1st person mode
        if (_perspective == Perspective.ThirdPerson)
        {
            _perspective = Perspective.FirstPerson;
            head.SetActive(false);
        }
        else
        {
            _perspective = Perspective.ThirdPerson;
            head.SetActive(true);
        }

        SetInitialCameraPosition();
        SetOffset();
    }

    // On camera rotation function
    void OnCameraRotation(InputValue inputValue)
    {
        Vector2 input = inputValue.Get<Vector2>();

        // Check the perspective
        if (_perspective == Perspective.FirstPerson)
        {
            verticalRotation -= input.y * mouseSensitivity * Time.deltaTime; // Adjust vertical rotation
            verticalRotation = Mathf.Clamp(verticalRotation, -verticalRotationLimit, verticalRotationLimit); // Limit vertical rotation
            yawRotation += input.x * mouseSensitivity * Time.deltaTime; // Adjust horizontal rotation
            transformCamera.rotation = Quaternion.Euler(verticalRotation, yawRotation, 0f); // Apply the calculated rotations to the camera transform
        }
        else
        {
            transform.RotateAround(objectToFollow.transform.position, Vector3.up, input.x); // Rotate the camera around the object to follow based on the input
            objectToFollow.transform.Rotate(Vector3.up, input.x); // Rotate the object to follow based on the input
            SetOffset();
        }
    }

}
