// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using System.Collections;

public class CameraAndCarMovement : MonoBehaviour {

	
	[Tooltip("Transform that contains main camera")]
	public Transform cameraTransform;
	[Tooltip("Transform that contains WheelJoint2Ds")]
	public Transform carTransform;
	private WheelJoint2D frontWheel;
	private WheelJoint2D backWheel;
	private Camera carCamera;

	//Zoom settings
	[Tooltip("Max value of orthographic size in camera.")]
	public float maxZoom = 12;
	[Tooltip("Min value of orthographic size in camera.")]
	public float minZoom = 1;
	[Tooltip("Multiplier for mouse scroll value")]
	public float zoomSpeed = 200;
	
	[Tooltip("Camera moves on Y-axis when zooming")]
	public float minZoomCameraY = 1;
	[Tooltip("Camera moves on Y-axis when zooming")]
	public float maxZoomCameraY = 9;

    void Start()
    {
        WheelJoint2D[] wheel = carTransform.GetComponents<WheelJoint2D>();
		if (wheel.Length < 2) {
			Debug.LogError("There are less than 2 wheels!");			
		}
        frontWheel = wheel[0];
        backWheel = wheel[1];
		carCamera = cameraTransform.GetComponent<Camera> ();
        JointMotor2D motor = frontWheel.motor;
        motor.motorSpeed = -1000;
        frontWheel.motor = backWheel.motor = motor;
    }

	// Update is called once per frame
	void Update () {

        

		//Zoom wiht mouse
		float scroll = -Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomSpeed;
		carCamera.orthographicSize = Mathf.Clamp (carCamera.orthographicSize + scroll, minZoom, maxZoom);

		//Use motors
        float axis = Input.GetAxisRaw("Horizontal");
        if(Input.GetMouseButton(0) == false) { 
            if (axis == 0)
            {
                frontWheel.useMotor = backWheel.useMotor = false;
                return;
            }
        }
        frontWheel.useMotor = backWheel.useMotor = true;
        if (axis > 0)
        {
            JointMotor2D motor = frontWheel.motor;
            motor.motorSpeed = -1500;
            frontWheel.motor = backWheel.motor = motor;
        }
        else
        {
            JointMotor2D motor = frontWheel.motor;
            motor.motorSpeed = 1500;
            frontWheel.motor = backWheel.motor = motor;
        }
        if (Input.GetMouseButton(0))
        {
            JointMotor2D motor = frontWheel.motor;
            motor.motorSpeed = -1500;
            frontWheel.motor = backWheel.motor = motor;
        }
    }

    //Camera Update
    void LateUpdate()
    {
        Vector3 pos = cameraTransform.position;
		pos.x = carTransform.position.x;
		pos.y = carTransform.position.y +  Mathf.Lerp(minZoomCameraY, maxZoomCameraY, (carCamera.orthographicSize - minZoom) / (maxZoom - minZoom));
        cameraTransform.position = pos;

        if (pos.y < -40)
        {
#if UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            Application.LoadLevel(0);
#else
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
#endif
        }

    }
}
