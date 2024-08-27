using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    
    [SerializeField] private GameObject objectToFollow;

    private Transform transformObjectToFollow, transformCamera;

    private Vector3 offset;

    private float fixYOffset, distance;
    
    
    // Start is called before the first frame update
    void Start()
    {
        transformObjectToFollow = objectToFollow.GetComponent<Transform>();
        transformCamera = gameObject.GetComponent<Transform>();
        fixYOffset = transformCamera.position.y - transformObjectToFollow.position.y;
        distance = Vector3.Distance(transformCamera.position, transformObjectToFollow.position);
        SetOffset();
    }

    public void SetOffset()
    {
        offset = transformCamera.position - transformObjectToFollow.position;
        offset.Normalize();
        offset *= distance;
        offset.y = fixYOffset;
    }

    // Update is called once per frame
    void Update()
    {
        transformCamera.position = transformObjectToFollow.position + offset;
        transformCamera.LookAt(new Vector3(transformObjectToFollow.position.x, transformObjectToFollow.position.y + 1.6f, transformObjectToFollow.position.z));
    }

    void OnCameraRotation(InputValue inputValue)
    {
        Vector2 input = inputValue.Get<Vector2>();
        transform.RotateAround(objectToFollow.transform.position, new Vector3(0, 1, 0), input.x);
        objectToFollow.transform.Rotate(Vector3.up, input.x);
        SetOffset();
    }
}
