using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject objectToFollow;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float verticalRotationLimit = 85f;
    [SerializeField] private GameObject head;

    private Transform transformObjectToFollow, transformCamera;

    private Vector3 offset;

    private float fixYOffset, distance;
    private float verticalRotation = 0f;
    private float yawRotation = 0f; // To store yaw rotation

    private enum Perspective
    {
        FirstPerson,
        ThirdPerson
    }

    private Perspective _perspective;

    void Start()
    {
        _perspective = Perspective.ThirdPerson;
        head.SetActive(true);
        InitCamera();
    }

    void InitCamera()
    {
        transformObjectToFollow = objectToFollow.GetComponent<Transform>();
        transformCamera = gameObject.GetComponent<Transform>();
        SetInitialCameraPosition();
        SetOffset();
    }

    void SetInitialCameraPosition()
    {
        if (_perspective == Perspective.ThirdPerson)
        {
            // Set camera to initial third-person position
            transformCamera.position = transformObjectToFollow.position - transformObjectToFollow.forward * 3.5f + Vector3.up * 2f;
            transformCamera.rotation = Quaternion.Euler(17.58f, transformCamera.eulerAngles.y, transformCamera.eulerAngles.z);
        }
        else
        {
            // Set camera to initial first-person position
            transformCamera.position = transformObjectToFollow.position + Vector3.up * 1f; // Assuming head height for first-person
            transformCamera.rotation = Quaternion.Euler(0f, transformObjectToFollow.eulerAngles.y, 0f);
        }

        distance = Vector3.Distance(transformCamera.position, transformObjectToFollow.position);
        fixYOffset = transformCamera.position.y - transformObjectToFollow.position.y;
    }

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

    void Update()
    {
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

        if (Input.GetKeyDown(KeyCode.V))
        {
            ChangePerspective();
        }
    }

    void ChangePerspective()
    {
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
        SetOffset();  // Recalculate offset after changing perspective
    }

    void OnCameraRotation(InputValue inputValue)
    {
        Vector2 input = inputValue.Get<Vector2>();

        if (_perspective == Perspective.FirstPerson)
        {
            verticalRotation -= input.y * mouseSensitivity * Time.deltaTime;
            verticalRotation = Mathf.Clamp(verticalRotation, -verticalRotationLimit, verticalRotationLimit);

            yawRotation += input.x * mouseSensitivity * Time.deltaTime;

            transformCamera.rotation = Quaternion.Euler(verticalRotation, yawRotation, 0f);
        }
        else
        {
            transform.RotateAround(objectToFollow.transform.position, Vector3.up, input.x);
            objectToFollow.transform.Rotate(Vector3.up, input.x);
            SetOffset();
        }
    }
}
