using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeavyControl : MonoBehaviour
{
	private DefaultInputActions defaultInput;

	private float mouseX, mouseY; //  Stores the value of mouse movement

	private float xRotation; //  Stores the rotation of the x-axis

	public float Sensitivity; //  Mouse sensitivity

	[SerializeField] GameObject pov;

    private void Awake()
    {
		defaultInput = new DefaultInputActions();
    }
    void Update()
    {
		//Gets the value of the mouse movement
		mouseX = Input.GetAxis("Mouse X") * Sensitivity * Time.deltaTime;
		mouseY = Input.GetAxis("Mouse Y") * Sensitivity * Time.deltaTime;
		//Controls the rotation of the X axis
		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -70, 70);
		//Controls the rotation of the Y axis
		pov.transform.Rotate(Vector3.up * mouseX);
		transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
	}
}
