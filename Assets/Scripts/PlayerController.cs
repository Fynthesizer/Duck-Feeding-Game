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
    private bool canThrow = true;

    private AudioSource audioSource;
    [SerializeField] private AudioClip throwSound;

    private void Awake()
    {
        controls = GameManager.Instance.Input;
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

#if UNITY_ANDROID && !UNITY_EDITOR
        gyroControls.enabled = true;
#endif
    }

    private void Update()
    {
        if (touching)
        {
            swipePos = Vector2.SmoothDamp(swipePos, controls.Player.Touch.ReadValue<TouchState>().position, ref swipeVelocity, swipeVelocityDamping);
        }
    }

    private void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Feeding) gyroControls.enabled = true;
        else gyroControls.enabled = false;
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
                touching = true;
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
            Vector3 throwDirection = Quaternion.AngleAxis(angle, transform.up) * transform.forward;
            throwDirection = Quaternion.AngleAxis(-Mathf.PI * throwArchMultiplier, transform.right) * throwDirection;
            //throwDirection.y = 0.1f;
            ThrowFood(throwDirection, velocity.magnitude / throwVelocityDivisor);
        }
    }

    private void ThrowFood(Vector3 direction, float force)
    {
        if (canThrow && (GameManager.Instance.food > 0 || infiniteFood)) {
            StartCoroutine(ThrowCooldown());
            if (!infiniteFood) GameManager.Instance.Food--;
            Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            spawnPos += transform.forward;
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
