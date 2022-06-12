using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float throwForce = 100;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject duckFood;

    public InputActions controls;
    private InputAction touch;

    private bool touching = false;
    private Vector2 swipeStartPos;
    private bool canThrow = true;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        controls = new InputActions();
    }

    private void OnEnable()
    {
        touch = controls.Player.Touch;
        touch.Enable();
        touch.performed += Touch_performed;
    }

    IEnumerator ThrowCooldown()
    {
        canThrow = false;
        yield return new WaitForSeconds(0.5f);
        canThrow = true;
    }

    private void Touch_performed(InputAction.CallbackContext callback)
    {
        var state = callback.ReadValue<UnityEngine.InputSystem.LowLevel.TouchState>();
        var phase = state.phase;
        print(callback);
        switch (phase)
        {
            case TouchPhase.Began:
                swipeStartPos = state.position;
                //print("began");
                touching = true;
                //ThrowFood(transform.forward,throwForce);
                break;
            case TouchPhase.Moved:
                //print("moved to " + state.position);
                break;
            case TouchPhase.Ended:
                ReadSwipe(state.position - swipeStartPos);
                touching = false;
                break;
            default:
                break;
        }
    }

    private void ReadSwipe(Vector2 delta)
    {
        if(delta.magnitude > 1)
        {
            Vector2 swipeDirection = delta.normalized;
            float angle = Map(swipeDirection.x, -1, 1, Mathf.PI / 4, -Mathf.PI / 4);
            Vector3 throwDirection = RotateVector(transform.forward, angle);
            throwDirection.y = 0.1f;
            ThrowFood(throwDirection, delta.magnitude / 5);
        }
    }

    float Map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    Vector3 RotateVector(Vector3 original, float angle)
    {
        Vector3 newVector = new Vector3(
            original.x * Mathf.Cos(angle) - original.z * Mathf.Sin(angle),
            original.y,
            original.x * Mathf.Sin(angle) + original.z * Mathf.Cos(angle)
            );
        return newVector;
    }

    private void OnDisable()
    {
        touch.Disable();
    }

    private void Tap(InputAction.CallbackContext context)
    {
        Debug.Log("tap");
    }

    void Start()
    {
        
    }

    void Update()
    {
    }

    private void ThrowFood(Vector3 direction, float force)
    {
        if (canThrow) {
            StartCoroutine(ThrowCooldown());
            GameObject food = Instantiate(duckFood, transform.position, Quaternion.identity);
            Rigidbody foodRb = food.GetComponent<Rigidbody>();
            foodRb.AddForce(direction * force);
        }
    }

    public void OnFire()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit))
        {
            Vector3 throwDirection = (hit.point - transform.position).normalized * throwForce;
            GameObject food = Instantiate(duckFood, transform.position, Quaternion.identity);
            Rigidbody foodRb = food.GetComponent<Rigidbody>();
            foodRb.AddForce(throwDirection);
        }
        
    }

    public void OnDrag()
    {
        
    }
}
