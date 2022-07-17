using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using TouchState = UnityEngine.InputSystem.LowLevel.TouchState;

public class EditController : MonoBehaviour
{
    private InputActions input;
    private InputAction touch;

    private Vector3 targetPosition;
    private bool targetValid;

    private GameObject blueprint;

    public DecorationItem activeDecoration;
    [SerializeField] private Transform decorationsGroup;
    [SerializeField] private LayerMask layerMask;

    private void Awake()
    {
        input = GameManager.Instance.Input;
    }

    private void OnEnable()
    {
        input.Player.Touch.performed += OnTouch;
    }

    private void OnDisable()
    {
        input.Player.Touch.performed -= OnTouch;
    }

    private void OnTouch(InputAction.CallbackContext callback)
    {
        var phase = callback.ReadValue<TouchState>().phase;

        if (phase == TouchPhase.Began)
        {
            /*
            if (FindTargetPosition(activeDecoration.placementSurfaces, out targetPosition)) 
                Instantiate(activeDecoration.objectPrefab, targetPosition, Quaternion.identity, decorationsGroup);
            */

            if (targetValid) Instantiate(activeDecoration.objectPrefab, targetPosition, Quaternion.identity, decorationsGroup);
        }
    }

    private void SetActiveDecoration(DecorationItem newDecoration)
    {
        activeDecoration = newDecoration;
        if (blueprint != null) Destroy(blueprint);
        blueprint = Instantiate(activeDecoration.blueprintPrefab, targetPosition, Quaternion.identity, transform);
    }


    private void Start()
    {
        SetActiveDecoration(activeDecoration);
    }

    void Update()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit, 50f, layerMask);

        targetPosition = hit.point;
        targetValid = GetTargetValidity(hit);

        if (blueprint != null) blueprint.transform.position = targetPosition;
    }

    private bool GetTargetValidity(RaycastHit hitInfo)
    {
        int layer = hitInfo.collider.gameObject.layer;
        if ((activeDecoration.placementSurfaces.HasFlag(DecorationItem.PlacementSurfaces.Water) && layer == 4) || 
            (activeDecoration.placementSurfaces.HasFlag(DecorationItem.PlacementSurfaces.Land) && layer == 6)) return true;
        else return false;
    }
}
