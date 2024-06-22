using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    [Header("Wheels collider")]
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider backLeftWheelCollider;
    public WheelCollider backRightWheelCollider;

    [Header("Wheels Transform")]
    public Transform frontLeftWheelTransform;
    public Transform frontRightWheelTransform;
    public Transform backLeftWheelTransform;
    public Transform backRightWheelTransform;

    [Header("Car Engine")]
    public float accelerationForce = 300f;
    public float breakingForce = 3000f;
    private float presentBreakForce = 0f;
    private float presentAcceleration = 0f;

    [Header("Car Steering")]
    public float wheelsTorque = 35f;
    private float presentTurnAngle = 0f;

    public Transform cameraTransform;
    public GameObject car;
    public GameObject virtualCamera;
    private Coroutine fadeOutCoroutine;
    private bool isMoving = false;
    private bool startFadeOut = false;
    public float fadeOutDuration = 5.0f;
    public Text restarting;

    private int countOfJumpsLeft = 3;

    public void Start()
    {
        Application.targetFrameRate = -1;
        Application.targetFrameRate = 60;
    }
    private void FixedUpdate()
    {
        MoveCar();
        CarSteering();
        ApplyBreaks();
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z) && countOfJumpsLeft >=0)
        {
            car.GetComponent<Rigidbody>().velocity = Vector3.zero;
            car.GetComponent<Rigidbody>().AddForce(0, 200000, 0);
            car.transform.rotation = new Quaternion(0, 0, 0, 0);
            countOfJumpsLeft--;
        }
        else if(countOfJumpsLeft == 0)
        {
            StartCoroutine(ResetVariableAfterDelay(5.0f));
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if(Input.GetKeyDown(KeyCode.W))
        {
            car.GetComponent<AudioSource>().Play();
        }
        else if(Input.GetKeyUp(KeyCode.W))
        {
            car.GetComponent<AudioSource>().Stop();
        }
        if (Input.GetKeyDown (KeyCode.R))
        {
            restart();
        }

    }
    public void restart()
    {
        restarting.enabled = true;
        car.GetComponent<Transform>().position = new Vector3(45.0f, 1.0f, 75.0f);
        car.GetComponent<Rigidbody>().velocity = Vector3.zero;
        car.transform.rotation = Quaternion.identity;
        virtualCamera.GetComponent<CinemachineVirtualCamera>().enabled = false;
        StartCoroutine(turnOnCinemachine());
    }
    IEnumerator ResetVariableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        countOfJumpsLeft = 3;
    }
    IEnumerator turnOnCinemachine()
    {
        yield return new WaitForSeconds(1f);
        virtualCamera.GetComponent<CinemachineVirtualCamera>().enabled = true;
        virtualCamera.transform.position = new Vector3(45, 8, 75);
        virtualCamera.transform.rotation = new Quaternion(85, 0, 0, 0);
        restarting.enabled = false;
    }
    public void MoveCar()
    {
        bool flag = true;
        presentAcceleration = accelerationForce * SimpleInput.GetAxis("Vertical");
        frontLeftWheelCollider.motorTorque = presentAcceleration;
        frontRightWheelCollider.motorTorque = presentAcceleration;
        backLeftWheelCollider.motorTorque = presentAcceleration;
        backRightWheelCollider.motorTorque = presentAcceleration;
        
        
        isMoving = Mathf.Abs(presentAcceleration) > 0.1f;
        if (!isMoving && startFadeOut && car.GetComponent<AudioSource>().isPlaying)
        {
            startFadeOut = false;
            if (fadeOutCoroutine == null)
            {
                fadeOutCoroutine = StartCoroutine(FadeOutAudio());
            }
        }
        if (isMoving && !car.GetComponent<AudioSource>().isPlaying)
        {
            car.GetComponent<AudioSource>().Play();
        }
        startFadeOut = !isMoving;
    }
    private IEnumerator FadeOutAudio()
    {
        AudioSource audioSource = car.GetComponent<AudioSource>();
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeOutDuration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
        fadeOutCoroutine = null;
    }

    public void CarSteering()
    {
        
       presentTurnAngle = wheelsTorque * SimpleInput.GetAxis("Horizontal");
       
        frontLeftWheelCollider.steerAngle = presentTurnAngle;
        frontRightWheelCollider.steerAngle = presentTurnAngle;
      
        SteeringWheels(frontLeftWheelCollider, frontLeftWheelTransform);
        SteeringWheels(frontRightWheelCollider, frontRightWheelTransform);
        SteeringWheels(backLeftWheelCollider, backLeftWheelTransform);
        SteeringWheels(backRightWheelCollider, backRightWheelTransform);
    }

    void SteeringWheels(WheelCollider WC, Transform WT)
    {
        Vector3 position;
        Quaternion rotation;

        WC.GetWorldPose(out position, out rotation);

        WT.position = position;
        WT.rotation = rotation;
    }

    public void ApplyBreaks()
    {
        if (Input.GetKey(KeyCode.Space))
            presentBreakForce = breakingForce;

        else
            presentBreakForce = 0f;


        frontLeftWheelCollider.brakeTorque = presentBreakForce;
        frontRightWheelCollider.brakeTorque = presentBreakForce;
        backLeftWheelCollider.brakeTorque = presentBreakForce;
        backRightWheelCollider.brakeTorque = presentBreakForce;
    }
    public void jump()
    {
        if (countOfJumpsLeft >= 0)
        {
            car.GetComponent<Rigidbody>().velocity = Vector3.zero;
            car.GetComponent<Rigidbody>().AddForce(0, 200000, 0);
            car.transform.rotation = new Quaternion(0, 0, 0, 0);
            countOfJumpsLeft--;
        }
        else if (countOfJumpsLeft == 0)
        {
            StartCoroutine(ResetVariableAfterDelay(5.0f));
        }
    }
    public void level2Change()
    {
        SceneManager.LoadScene(1);
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name == car.name)
        {
            Debug.Log("car collided");
        }
    }
}
