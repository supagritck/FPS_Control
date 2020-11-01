using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraScript : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public float clampAngle = 90f;

    private float rotationYaxis = 0f; //rotation around X-axis
    private float rotationXaxis = 0f; //rotation around Y-axis

    // Start is called before the first frame update
    void Start()
    {
        Vector3 rotation = transform.localRotation.eulerAngles;
        Cursor.lockState = CursorLockMode.Locked;
        rotationYaxis = rotation.y;
        rotationXaxis = rotation.x;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

        rotationYaxis += mouseX * mouseSensitivity * Time.deltaTime;
        rotationXaxis += mouseY * mouseSensitivity * Time.deltaTime;

        rotationXaxis = Mathf.Clamp(rotationXaxis, -clampAngle, clampAngle);

        transform.rotation = Quaternion.Euler(rotationXaxis, rotationYaxis, 0f);
    }
}
