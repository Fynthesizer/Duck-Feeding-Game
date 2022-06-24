using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class Duck : MonoBehaviour
{
    [SerializeField] private Transform model;
    [SerializeField] private Animator animator;
    private Rigidbody rb;
    private CapsuleCollider collider;

    private Vector3 targetLocation;
    private Vector3 targetDirection;
    private Vector3 moveDirection;
    private GameObject targetFood;
    private float targetDistance;
    private Coroutine waitTimer;

    [Header("Global Variables")]
    [SerializeField] private float pursuitSpeedMultiplier = 1.5f;
    [SerializeField] private float turnSpeed = 5;
    [SerializeField] private float minIdleTime = 1;
    [SerializeField] private float maxIdleTime = 8;
    [SerializeField] private float minQuackInterval = 2;
    [SerializeField] private float maxQuackInterval = 3;
    [SerializeField] private float eatTime = 1;
    [SerializeField] private float wanderRadius = 5;
    [SerializeField] private float satiationPeriod = 1;

    [Header("Duck Data")]
    [SerializeField] public DuckData duckData;

    [Header("Duck State")]
    public int foodConsumed = 0;
    public bool hungry;
    public State state = State.Idle;

    [Header("Obstacle Avoidance")]
    [SerializeField] private bool avoidObstacles = true;
    [SerializeField] private float avoidDistance = 1f;
    [SerializeField] private LayerMask wanderAvoidLayers;
    [SerializeField] private LayerMask pursuitAvoidLayers;

    [Header("Quack")]
    [SerializeField] private AudioClip[] quackClips;
    private AudioSource quackSource;

    [Header("Glow")]
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
        collider = gameObject.GetComponent<CapsuleCollider>();
        material = model.GetComponent<SkinnedMeshRenderer>().material;
        quackSource = gameObject.GetComponent<AudioSource>();
        targetLocation = transform.position;
        Vector2 initialLookDirection = Random.insideUnitCircle.normalized;
        moveDirection = new Vector3(initialLookDirection.x, 0f, initialLookDirection.y);
        rb.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        waitTimer = StartCoroutine(Wait());
        StartCoroutine(QuackCoroutine());
        StartCoroutine(SwimCoroutine());
        hungry = CheckIfHungry();
    }

    private bool CheckIfHungry()
    {
        DateTime lastFed;
        if(!DateTime.TryParse(duckData.lastFedTime, out lastFed)) return true; //If last fed time can't be parsed, return true
        TimeSpan elapsedTime = DateTime.Now.Subtract(lastFed);
        return elapsedTime.Hours >= satiationPeriod;
    }

    public void SetData(DuckData data)
    {
        duckData = data;
        gameObject.name = duckData.duckName;
    }

    void Update()
    {
        //Movement();
        rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(moveDirection, Vector3.up), Time.deltaTime * turnSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(targetLocation, 0.1f);
        Debug.DrawLine(transform.position, targetLocation, Color.blue);
    }

    IEnumerator SwimCoroutine()
    {
        while (true)
        {
            if (state == State.Wandering || state == State.Pursuit)
            {
                float moveSpeed = duckData.speed;
                if (state == State.Pursuit) moveSpeed *= pursuitSpeedMultiplier;

                targetDirection = (targetLocation - transform.position).normalized;
                targetDirection = new Vector3(targetDirection.x, 0, targetDirection.z); //Flatten the direction so that it is only on one plane
                targetDistance = Vector3.Distance(transform.position, targetLocation);
                moveDirection = targetDirection;

                if (avoidObstacles)
                {
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
                
                if (targetDistance < collider.radius) SetState(State.Idle);

            }

            //rb.AddTorque(Vector3.up * turnSpeed * 5f);
            //
            

            yield return new WaitForSeconds(0.1f);
        }
    }

    void Movement()
    {
        if(state == State.Wandering || state == State.Pursuit) {
            targetDirection = (targetLocation - transform.position).normalized;
            targetDirection = new Vector3(targetDirection.x, 0, targetDirection.z); //Flatten the direction so that it is only on one plane
            moveDirection = targetDirection;

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

            rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(moveDirection, Vector3.up), Time.deltaTime * turnSpeed);
            targetDistance = Vector3.Distance(transform.position, targetLocation);
            if (targetDistance < collider.radius) SetState(State.Idle);
        }
    }

    private bool PathIsClear(Vector3 direction, float distance)
    {
        LayerMask layerMask = state == State.Pursuit ? pursuitAvoidLayers : wanderAvoidLayers;
        Ray ray = new Ray(transform.position, direction);
        if (!Physics.SphereCast(ray, collider.radius, distance, layerMask)) return true;
        else return false;
    }

    void SetState(State _state)
    {
        state = _state;
        switch (_state){
            case State.Idle:
                waitTimer = StartCoroutine(Wait());
                targetLocation = transform.position;
                targetFood = null;
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
            material.SetColor("_Emissive_Color", new Color(glowAmount,glowAmount,glowAmount,1));
            yield return null;
        }
    }

    IEnumerator QuackCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minQuackInterval, maxQuackInterval));
            Quack();
        }
    }

    private void Quack()
    {
        AudioClip quackClip = quackClips[Random.Range(0, quackClips.Length)];
        quackSource.PlayOneShot(quackClip);
        animator.SetTrigger("Quack");
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
            attempts++;
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
        if(hungry) StartCoroutine(FlashCoroutine());
        animator.SetTrigger("Eat");
        Destroy(food);
        duckData.lastFedTime = DateTime.Now.ToString();
        hungry = false;
        targetFood = null;
        foodConsumed++;
        SetState(State.Eating);
    }

    IEnumerator CheckSurroundings()
    {
        yield return new WaitForSeconds(duckData.reactionTime);
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
        if (closestFood != null && closestDistance < duckData.awarenessRadius) SetTargetObject(closestFood.gameObject);

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
