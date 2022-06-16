using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using Gyroscope = UnityEngine.InputSystem.Gyroscope;

public class GyroscopeControls : MonoBehaviour
{
    [SerializeField] private InputActions controls;
    [SerializeField] private float smoothing = 0.5f;
    [SerializeField] private bool active = false;

    private InputAction gyro;
    private InputAction attitude;
    private float _initialYAngle = 0f;
    private float _appliedGyroYAngle = 0f;
    private float _calibrationYAngle = 0f;
    private Transform _rawGyroRotation;

    // Update is called once per frame
    void Update()
    {
        ApplyGyroRotation();
        ApplyCalibration();
        if (active) transform.rotation = Quaternion.Slerp(transform.rotation, _rawGyroRotation.rotation, smoothing);
    }

    private void OnEnable()
    {
        controls = gameObject.GetComponent<PlayerController>().controls;
        StartCoroutine(InitialiseGyroscope());

        if (Gyroscope.current != null) InputSystem.EnableDevice(Gyroscope.current);
        if (AttitudeSensor.current != null) InputSystem.EnableDevice(AttitudeSensor.current);
        gyro = controls.Player.Rotation;
        gyro.Enable();
        attitude = controls.Player.Attitude;
        attitude.Enable();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void OnApplicationFocus(bool focus)
    {
        if (!focus) InputSystem.DisableDevice(AttitudeSensor.current);
        else InputSystem.EnableDevice(AttitudeSensor.current);
    }
#endif

    private void OnDisable()
    {
        gyro.Disable();
        InputSystem.DisableDevice(Gyroscope.current);
        InputSystem.DisableDevice(AttitudeSensor.current);
    }

    private IEnumerator InitialiseGyroscope()
    {
        _initialYAngle = transform.eulerAngles.y;
        _rawGyroRotation = new GameObject("GyroRaw").transform;
        _rawGyroRotation.position = transform.position;
        _rawGyroRotation.rotation = transform.rotation;
        yield return new WaitForSeconds(0.5f);
        _calibrationYAngle = _appliedGyroYAngle - _initialYAngle;
        active = true;
    }

    private void ApplyGyroRotation()
    {
        _rawGyroRotation.rotation = attitude.ReadValue<Quaternion>();
        _rawGyroRotation.Rotate(0f, 0f, 180f, Space.Self);
        _rawGyroRotation.Rotate(90f, 180f, 0f, Space.World); 
        _appliedGyroYAngle = _rawGyroRotation.eulerAngles.y; 
    }

    private void ApplyCalibration()
    {
        _rawGyroRotation.Rotate(0f, -_calibrationYAngle, 0f, Space.World); 
    }
}
