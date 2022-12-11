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

    private List<Quaternion> _filterSamples = new List<Quaternion>();
    private int _filterQueueCapacity = 30;


   


    // Update is called once per frame
    void Update()
    {
        ApplyGyroRotation();
        ApplyCalibration();

        Quaternion filteredRotation = LowPassFilter(_rawGyroRotation.rotation);

        Debug.Log(SystemInfo.supportsGyroscope);
        if (active) transform.rotation = Quaternion.Slerp(transform.rotation, filteredRotation, smoothing);
    }

    private Quaternion LowPassFilter(Quaternion input)
    {
        _filterSamples.Add(input);
        if (_filterSamples.Count > _filterQueueCapacity) _filterSamples.RemoveAt(0);

        Quaternion filteredRotation = new Quaternion();

        for(int i = 0; i < _filterSamples.Count; i++)
        {
            if (i == 0) filteredRotation = _filterSamples[i];
            else filteredRotation = Quaternion.Slerp(_filterSamples[i - 1], _filterSamples[i], 0.5f);
        }

        return filteredRotation;
    }

    private void Awake()
    {
        controls = GameManager.Instance.Input;
    }

    private void Start()
    {
        StartCoroutine(InitialiseGyroscope());
    }

    private void OnEnable()
    {
        
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
        attitude.Disable();
        if (Gyroscope.current != null) InputSystem.DisableDevice(Gyroscope.current);
        if (AttitudeSensor.current != null) InputSystem.DisableDevice(AttitudeSensor.current);
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
        if (Gyroscope.current != null) InputSystem.EnableDevice(Gyroscope.current);
        if (AttitudeSensor.current != null) InputSystem.EnableDevice(AttitudeSensor.current);
        gyro = controls.Player.Rotation;
        gyro.Enable();
        attitude = controls.Player.Attitude;
        attitude.Enable();
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
