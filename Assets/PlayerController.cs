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

    private void Awake()
    {
        controls = new InputActions();
    }

    private void OnEnable()
    {
        touch = controls.Player.Touch;
        touch.Enable();
        touch.performed += Touch_performed;
    }

    private void Touch_performed(InputAction.CallbackContext callback)
    {
        var state = callback.ReadValue<UnityEngine.InputSystem.LowLevel.TouchState>();
        var phase = state.phase;
        switch (phase)
        {
            case TouchPhase.Began:
                print("began");
                ThrowFood(transform.forward,throwForce);
                break;
            case TouchPhase.Moved:
                print("moved to " + state.position);
                break;
            case TouchPhase.Ended:
                print("ended");
                break;
            default:
                break;
        }
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
        GameObject food = Instantiate(duckFood, transform.position, Quaternion.identity);
        Rigidbody foodRb = food.GetComponent<Rigidbody>();
        foodRb.AddForce(direction * force);
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
