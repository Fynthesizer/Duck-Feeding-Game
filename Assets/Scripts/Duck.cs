using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Duck : MonoBehaviour
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

    [SerializeField] private bool debug = true;

    [SerializeField] private float speed = 1;
    [SerializeField] private float reactionTime = 0.1f;
    [SerializeField] private float pursuitSpeedMultiplier = 1.5f;
    [SerializeField] private float turnSpeed = 5;
    [SerializeField] private float minIdleTime = 1;
    [SerializeField] private float maxIdleTime = 8;
    [SerializeField] private float eatTime = 1;
    [SerializeField] private float foodDetectionRadius = 10;
    [SerializeField] private float wanderRadius = 5;

    [Header("Obstacle Avoidance")]
    [SerializeField] private bool avoidObstacles = true;
    [SerializeField] private float avoidDistance = 1f;
    [SerializeField] private LayerMask wanderAvoidLayers;
    [SerializeField] private LayerMask pursuitAvoidLayers;

    [SerializeField] private AudioClip[] quackClips;
    private AudioSource quackSource;

    [SerializeField] private float glowFadeSpeed = 0.05f;
    private Material material;
    private float glowAmount = 0.0f;

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
        material = model.GetComponent<MeshRenderer>().material;
        quackSource = gameObject.GetComponent<AudioSource>();
        targetLocation = transform.position;
        waitTimer = StartCoroutine(Wait());
        StartCoroutine(QuackCoroutine());
        speed = Random.Range(0.75f, 1.25f);
        reactionTime = Random.Range(0f, 1f);
    }

    void Update()
    {
        Movement();
    }

    private void OnDrawGizmos()
    {
        if (debug)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(targetLocation, 0.1f);
            Debug.DrawLine(transform.position, targetLocation, Color.blue);
        }
    }

    void Movement()
    {
        if(state == State.Wandering || state == State.Pursuit) {
            float moveSpeed = speed;
            if (state == State.Pursuit) moveSpeed *= pursuitSpeedMultiplier;
            targetDirection = (targetLocation - transform.position).normalized;
            targetDirection = new Vector3(targetDirection.x, 0, targetDirection.z); //Flatten the direction so that it is only on one plane
            Vector3 moveDirection = targetDirection;

            if (avoidObstacles) {
                float directionRotation = 0;
                //Rotate move direction until a clear path is found
                int direction = 1;
                int attempt = 1;
                while (true)
                {
                    if (PathIsClear(moveDirection, avoidDistance)) break; //If the path ahead is clear, continue
                    else if (directionRotation < Mathf.PI * 2) //Otherwise try a different path
                    {
                        directionRotation = Mathf.PI / 16f * attempt;
                        directionRotation *= direction;
                        attempt++;
                        //direction *= -1;
                        moveDirection = Utilities.RotateVector(targetDirection, directionRotation);
                        continue;
                    }
                    else //If there are no clear paths, set state to idle
                    {
                        SetState(State.Idle);
                        break;
                    }
                }
            }
            Vector3 moveForce = moveDirection * moveSpeed;

            rb.AddForce(moveForce);
            rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(moveDirection, Vector3.up), Time.deltaTime * turnSpeed);
            targetDistance = Vector3.Distance(transform.position, targetLocation);
            if (targetDistance < 0.1) SetState(State.Idle);
        }
    }

    private bool PathIsClear(Vector3 direction, float distance)
    {
        LayerMask layerMask = state == State.Pursuit ? pursuitAvoidLayers : wanderAvoidLayers;
        Ray ray = new Ray(transform.position, direction);
        if (!Physics.SphereCast(ray,0.1f,distance, layerMask)) return true;
        else return false;
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
                waitTimer = StartCoroutine(Digest());
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

    IEnumerator Digest()
    {
        yield return new WaitForSeconds(eatTime);
        SetState(State.Idle);
        StartCoroutine(CheckSurroundings());
    }

    IEnumerator FlashCoroutine()
    {
        glowAmount = 1f;
        while (glowAmount > 0f)
        {
            glowAmount -= glowFadeSpeed;
            //material.SetFloat("Glow", glowAmount);
            material.SetColor("Emissive_Color", new Color(glowAmount,glowAmount,glowAmount,1));
            yield return null;
        }
    }

    IEnumerator QuackCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 40f));
            AudioClip quackClip = quackClips[Random.Range(0, quackClips.Length)];
            quackSource.PlayOneShot(quackClip);
        }
    }

    private Vector3 FindNearPosition(float maxDistance)
    {
        Vector3 newTarget;
        int attempts = 0;
        while (true) { 
            Vector2 targetOffset = Random.insideUnitCircle * maxDistance;
            newTarget = transform.position + new Vector3(targetOffset.x,0,targetOffset.y);
            Vector3 newTargetDirection = newTarget - transform.position;
            float newTargetDistance = Vector3.Distance(transform.position, newTarget);
            if (GameManager.Instance.PositionIsOnLake(newTarget) && PathIsClear(newTargetDirection,newTargetDistance)) break;
            else if (attempts > 50)
            {
                newTarget = transform.position;
                break;
            }
        }
        return newTarget;
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
            Eat(targetFood);
        }
    }

    private void Eat(GameObject food)
    {
        food.SetActive(false);
        StartCoroutine(FlashCoroutine());
        Destroy(food);
        targetFood = null;
        foodConsumed++;
        SetState(State.Eating);
    }

    IEnumerator CheckSurroundings()
    {
        yield return new WaitForSeconds(reactionTime);
        //Look for nearby food
        DuckFood[] allFood = FindObjectsOfType<DuckFood>();
        DuckFood closestFood = null;
        float closestDistance = Mathf.Infinity;
        foreach (DuckFood food in allFood)
        {
            float distance = Vector3.Distance(transform.position, food.transform.position);
            if(distance < closestDistance && food.inWater)
            {
                closestFood = food;
                closestDistance = distance;
            }
            //if (distance < foodDetectionRadius && (targetFood == null || distance < targetDistance)) SetTargetObject(food.gameObject);
        }
        if (closestFood != null && closestDistance < foodDetectionRadius) SetTargetObject(closestFood.gameObject);
        //Check if targeted food still exists
        /*
        else if (targetFood != null && !targetFood.activeSelf)
        {
            ReleaseTarget();
        }
        */

        if (state == State.Pursuit && targetFood == null) SetState(State.Idle);
    }

    public static void UpdateSurroundings()
    {
        Duck[] ducks = FindObjectsOfType<Duck>();
        foreach(Duck duck in ducks)
        {
            if(duck.state != State.Eating) duck.StartCoroutine(duck.CheckSurroundings());
        }
    }

    public static List<Duck> DucksInVicinity(Vector3 origin, float radius)
    {
        Duck[] allDucks = FindObjectsOfType<Duck>();
        var nearDucks = new List<Duck>();
        foreach(Duck duck in allDucks)
        {
            if (Vector3.Distance(duck.transform.position, origin) < radius) nearDucks.Add(duck);
        }
        return nearDucks;
    }
}
