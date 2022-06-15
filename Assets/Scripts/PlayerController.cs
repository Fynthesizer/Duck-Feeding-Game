using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using Gyroscope = UnityEngine.InputSystem.Gyroscope;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using TouchState = UnityEngine.InputSystem.LowLevel.TouchState;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private int startingFood = 10;
    public int availableFood;
    [SerializeField] private float throwForce = 100;
    [SerializeField] private GameObject duckFood;

    [SerializeField] private Camera cam;

    [SerializeField] private InputActions controls;
    private InputAction touch;
    private InputAction gyro;
    private InputAction attitude;
    private Quaternion attitudeOffset;

    [SerializeField] private float swipeVelocityDamping = 1f;
    [SerializeField] private float throwVelocityThreshold = 1000f;
    [SerializeField] private float throwVelocityDivisor = 100f;
    private bool touching = false;
    private Vector2 swipeStartPos;
    private Vector2 swipeVelocity;
    private Vector2 swipePos;
    private bool canThrow = true;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        controls = new InputActions();
    }

    private void OnEnable()
    {
        if (Gyroscope.current != null) InputSystem.EnableDevice(Gyroscope.current);
        if (AttitudeSensor.current != null) InputSystem.EnableDevice(AttitudeSensor.current);

        touch = controls.Player.Touch;
        touch.Enable();
        touch.performed += OnTouch;
        gyro = controls.Player.Rotation;
        gyro.Enable();
        attitude = controls.Player.Attitude;
        attitude.Enable();
    }

    private void OnDisable()
    {
        touch.Disable();
        gyro.Disable();
    }

    void Start()
    {
        availableFood = startingFood;
        attitudeOffset = Quaternion.Euler(90f, 0f, 0f);
    }

    void Update()
    {
        Look();

        if (touching)
        {
            swipePos = Vector2.SmoothDamp(swipePos, touch.ReadValue<TouchState>().position, ref swipeVelocity, swipeVelocityDamping);
        }
    }

    private void Look()
    {
        //Vector3 gyroValue = gyro.ReadValue<Vector3>();
        //Vector3 deviceRotation = new Vector3(-gyroValue.x, -gyroValue.y, gyroValue.z);
        //transform.Rotate(deviceRotation, Space.Self);

        Quaternion deviceOrientation = attitude.ReadValue<Quaternion>();
        transform.rotation = attitudeOffset * Utilities.GyroToUnity(deviceOrientation);
    }

    private void OnTouch(InputAction.CallbackContext callback)
    {
        var state = callback.ReadValue<TouchState>();
        var phase = state.phase;
        switch (phase)
        {
            case TouchPhase.Began:
                swipeStartPos = state.position;
                swipePos = state.position;
                //print("began");
                touching = true;
                //ThrowFood(transform.forward,throwForce);
                break;
            case TouchPhase.Moved:
                break;
            case TouchPhase.Ended:
                ReadSwipe(swipeVelocity);
                touching = false;
                break;
            default:
                break;
        }
    }

    private void ReadSwipe(Vector2 velocity)
    {
        if (velocity.magnitude > throwVelocityThreshold)
        {
            Vector2 swipeDirection = velocity.normalized;
            float angle = Utilities.Map(swipeDirection.x, -1, 1, Mathf.PI / 4, -Mathf.PI / 4);
            Vector3 throwDirection = Utilities.RotateVector(transform.forward, angle);
            throwDirection.y = 0.1f;
            ThrowFood(throwDirection, velocity.magnitude / throwVelocityDivisor);
        }
    }

    private void ThrowFood(Vector3 direction, float force)
    {
        if (canThrow && availableFood > 0) {
            StartCoroutine(ThrowCooldown());
            availableFood--;
            Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            spawnPos += transform.forward;
            //spawnPos.x += Utilities.Map(swipeStartPos.x, 0, Screen.width, -1f, 1f);
            GameObject food = Instantiate(duckFood, spawnPos, Quaternion.identity);
            Rigidbody foodRb = food.GetComponent<Rigidbody>();
            foodRb.AddForce(direction * force);
        }
        if (availableFood == 0) GameEnd();
    }

    IEnumerator ThrowCooldown()
    {
        canThrow = false;
        yield return new WaitForSeconds(0.5f);
        canThrow = true;
    }


    private void GameEnd()
    {
        GameObject[] ducks = GameObject.FindGameObjectsWithTag("Duck");
        int fedCount = 0;
        foreach(GameObject duckObject in ducks)
        {
            Duck duck = duckObject.GetComponent<Duck>();
            if (duck.foodConsumed > 0) fedCount += 1;
        }
        float fedPercent = (float)fedCount / (float)ducks.Length;
        print($"Fed {fedCount} of {ducks.Length} ducks");
        float score = fedPercent * 5f;
        print($"Score: {score.ToString("0.0")} / 5");
    }
}
