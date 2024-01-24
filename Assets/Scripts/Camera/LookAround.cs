using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * ### LookAround
 * -------
 * Simple class for player mouse directed camera
 */
public class LookAround : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    //private float xRotation = 0f;
    /**
     * #### Start
     * Unity engine event called at start of runtime
     * Locks cursor to window
     */
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    /**
     * #### Update
     * Unity engine event called every frame
     * Handles user I/O
     */
    void Update()
    {
        /*float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //X Rotation
        playerBody.Rotate(Vector3.up * mouseX);

        //Y Rotation
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);*/
    }
}
