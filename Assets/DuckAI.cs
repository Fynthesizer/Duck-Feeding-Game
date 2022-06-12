using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckAI : MonoBehaviour
{
    [SerializeField] private Transform model;
    private Rigidbody rb;

    private Vector3 targetLocation;
    private Vector3 targetDirection;
    private GameObject targetFood;
    private float targetDistance;
    private Coroutine waitTimer;

    public int foodConsumed = 0;
    public State state = State.Idle;

    [SerializeField] private float speed = 1;
    [SerializeField] private float turnSpeed = 5;
    [SerializeField] private float minIdleTime = 1;
    [SerializeField] private float maxIdleTime = 8;
    [SerializeField] private float eatTime = 1;
    [SerializeField] private float foodDetectionRadius = 10;
    [SerializeField] private float wanderRadius = 5;
    
    public enum State
    {
        Idle,
        Wandering,
        Pursuit,
        Eating
    }

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        targetLocation = transform.position;
        waitTimer = StartCoroutine(Wait());
    }

    void Update()
    {
        Movement();
    }

    void Movement()
    {
        if(state == State.Wandering || state == State.Pursuit) {
            targetDirection = (targetLocation - transform.position).normalized;
            targetDirection = new Vector3(targetDirection.x, 0, targetDirection.z); //Flatten the direction so that it is only on one plane
            rb.AddForce(targetDirection * speed);
            rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(targetDirection,Vector3.up), Time.deltaTime * turnSpeed);
            targetDistance = Vector3.Distance(transform.position, targetLocation);
            if (targetDistance < 0.1) SetState(State.Idle);
        }
    }

    void SetState(State _state)
    {
        state = _state;
        switch (_state){
            case State.Idle:
                waitTimer = StartCoroutine(Wait());
                break;
            case State.Wandering:
                SetTargetLocation(FindNearPosition(wanderRadius));
                break;
            case State.Eating:
                waitTimer = StartCoroutine(Eat());
                break;
            default:
                break;
        }
    }

    void SetTargetLocation(Vector3 newTarget)
    {
        StopCoroutine(waitTimer);
        targetLocation = newTarget;
    }

    void ReleaseTarget()
    {
        SetState(State.Idle);
        targetLocation = transform.position;
        targetFood = null;
    }

    IEnumerator Wait()
    {
        float waitTime = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(waitTime);
        SetState(State.Wandering);
    }

    IEnumerator Eat()
    {
        yield return new WaitForSeconds(eatTime);
        SetState(State.Idle);
        CheckSurroundings();
    }

    private Vector3 FindNearPosition(float maxDistance)
    {
        Vector3 newTarget;
        while (true) { 
            Vector2 targetOffset = Random.insideUnitCircle * maxDistance;
            newTarget = transform.position + new Vector3(targetOffset.x,0,targetOffset.y);
            if (TargetIsOnLake(newTarget)) break;
        }
        return newTarget;
    }

    private bool TargetIsOnLake(Vector3 target)
    {
        RaycastHit hit;
        Vector3 origin = new Vector3(target.x, 10, target.z);
        if (Physics.Raycast(origin, Vector3.down, out hit, 10))
        {
            return hit.collider.gameObject.layer == 4 ? true : false;
        }
        else return false;
    }

    public void SetTargetObject(GameObject target)
    {
        targetFood = target;
        SetTargetLocation(target.transform.position);
        SetState(State.Pursuit);
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("DuckFood") && other.transform.parent.gameObject == targetFood) //On reaching food
        {
            targetFood.SetActive(false);
            Destroy(other.transform.parent.gameObject);
            targetFood = null;
            foodConsumed++;
            SetState(State.Eating);
        }
    }

    public void CheckSurroundings()
    {
        //Look for nearby food
        DuckFood[] allFood = FindObjectsOfType<DuckFood>();
        DuckFood closestFood = null;
        float closestDistance = Mathf.Infinity;
        foreach (DuckFood food in allFood)
        {
            float distance = Vector3.Distance(transform.position, food.transform.position);
            if(distance < closestDistance)
            {
                closestFood = food;
                closestDistance = distance;
            }
            if (distance < foodDetectionRadius && (targetFood == null || distance < targetDistance)) SetTargetObject(food.gameObject);
        }
        if (closestFood != null && closestDistance < foodDetectionRadius) SetTargetObject(closestFood.gameObject);

        //Check if targeted food still exists
        else if (targetFood != null && !targetFood.activeSelf)
        {
            ReleaseTarget();
        }
    }

    public static void UpdateSurroundings()
    {
        DuckAI[] ducks = FindObjectsOfType<DuckAI>();
        foreach(DuckAI duck in ducks)
        {
            if(duck.state != State.Eating) duck.CheckSurroundings();
        }
    }
}
