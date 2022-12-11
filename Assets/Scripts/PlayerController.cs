using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using TouchState = UnityEngine.InputSystem.LowLevel.TouchState;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private bool infiniteFood;
    //public int availableFood;
    [SerializeField] private GameObject duckFood;

    private GyroscopeControls gyroControls;

    [Header("Controls")]
    private InputActions controls;
    private InputAction touch;

    [SerializeField] private float swipeVelocityDamping = 1f;
    [SerializeField] private float throwVelocityThreshold = 1000f;
    [SerializeField] private float throwVelocityDivisor = 100f;
    [SerializeField] private float throwAngleMultiplier = 20f;
    [SerializeField] private float throwArchMultiplier = 5f;
    private bool touching = false;
    private Vector2 swipeStartPos;
    private Vector2 swipeVelocity;
    private Vector2 swipePos;
    private Vector2 smoothedSwipePos;
    private Vector2 lastSwipePos;
    private bool canThrow = true;

    [SerializeField] private float screenActionThresholdPercentage;
    private float screenActionThreshold;

    [SerializeField] private Transform camera;
    [SerializeField] private float cameraRotationSpeed = 0.01f;
    private float xRotation = 0;
    private float yRotation = 0;
    private float minXRotation = -45;
    private float maxXRotation = 45;

    private AudioSource audioSource;
    [SerializeField] private AudioClip throwSound;

    private void Awake()
    {
        controls = GameManager.Instance.Input;
        yRotation = transform.eulerAngles.y;
        xRotation = camera.transform.eulerAngles.x;
    }


    private void OnEnable()
    {
        controls.Player.Touch.performed += OnTouch;
        
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        controls.Player.Touch.performed -= OnTouch;

        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    private void Start()
    {
        //availableFood = GameManager.Instance.duckCount;
        GameManager.UIManager.UpdateFoodCount();
        gyroControls = gameObject.GetComponent<GyroscopeControls>();
        audioSource = GetComponent<AudioSource>();

        screenActionThreshold = Screen.height * screenActionThresholdPercentage;

#if UNITY_ANDROID && !UNITY_EDITOR
        //gyroControls.enabled = true;
#endif
    }

    private void Update()
    {
        if (touching)
        {
            swipePos = controls.Player.Touch.ReadValue<TouchState>().position;
            smoothedSwipePos = Vector2.SmoothDamp(smoothedSwipePos, controls.Player.Touch.ReadValue<TouchState>().position, ref swipeVelocity, swipeVelocityDamping);
        }
    }

    private void UpdateRotation()
    {
        float xDistance = swipePos.x - lastSwipePos.x;
        float yDistance = swipePos.y - lastSwipePos.y;
        xRotation += yDistance * cameraRotationSpeed;
        yRotation += -xDistance * cameraRotationSpeed;
        xRotation = Mathf.Clamp(xRotation, minXRotation, maxXRotation);
        transform.eulerAngles = new Vector3(0, yRotation, 0);
        camera.transform.localEulerAngles = new Vector3(xRotation, 0, 0);
    }

    private void OnGameStateChanged(GameState newState)
    {
        //if (newState == GameState.Feeding) gyroControls.enabled = true;
        //else gyroControls.enabled = false;
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
                smoothedSwipePos = swipePos;
                lastSwipePos = state.position;
                touching = true;
                break;
            case TouchPhase.Moved:
                if (swipeStartPos.y > screenActionThreshold) UpdateRotation();
                lastSwipePos = swipePos;
                break;
            case TouchPhase.Ended:
                if (swipeStartPos.y < screenActionThreshold) ReadSwipe(swipeVelocity);
                touching = false;
                break;
            default:
                break;
        }
    }

    /*
    public void AddFood(int amount)
    {
        availableFood += amount;
        availableFood = Mathf.Clamp(availableFood, 0, GameManager.Instance.gameData.raft.Count);
        GameManager.UIManager.UpdateFoodCount();
    }
    */

    private void ReadSwipe(Vector2 velocity)
    {
        if (velocity.magnitude > throwVelocityThreshold && velocity.y > 0)
        {
            Vector2 swipeDirection = velocity.normalized;
            float angle = Utilities.Map(swipeDirection.x, -1, 1, -Mathf.PI, Mathf.PI) * throwAngleMultiplier;
            Vector3 throwDirection = Quaternion.AngleAxis(angle, camera.transform.up) * camera.transform.forward;
            throwDirection = Quaternion.AngleAxis(-Mathf.PI * throwArchMultiplier, camera.transform.right) * throwDirection;
            //throwDirection.y = 0.1f;
            ThrowFood(throwDirection, velocity.magnitude / throwVelocityDivisor);
        }
    }

    private void ThrowFood(Vector3 direction, float force)
    {
        if (canThrow && (GameManager.Instance.food > 0 || infiniteFood)) {
            StartCoroutine(ThrowCooldown());
            if (!infiniteFood) GameManager.Instance.Food--;
            Vector3 spawnPos = new Vector3(camera.transform.position.x, camera.transform.position.y, camera.transform.position.z);
            spawnPos += camera.transform.forward;
            //spawnPos.x += Utilities.Map(swipeStartPos.x, 0, Screen.width, -1f, 1f);
            audioSource.PlayOneShot(throwSound);
            GameObject food = Instantiate(duckFood, spawnPos, Quaternion.identity);
            Rigidbody foodRb = food.GetComponent<Rigidbody>();
            foodRb.AddForce(direction * force);
        }
    }

    IEnumerator ThrowCooldown()
    {
        canThrow = false;
        yield return new WaitForSeconds(0.5f);
        canThrow = true;
    }
}
