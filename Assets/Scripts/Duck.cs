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

    [SerializeField] private float speed = 1;
    [SerializeField] private float turnSpeed = 5;
    [SerializeField] private float minIdleTime = 1;
    [SerializeField] private float maxIdleTime = 8;
    [SerializeField] private float eatTime = 1;
    [SerializeField] private float foodDetectionRadius = 10;
    [SerializeField] private float wanderRadius = 5;
    [SerializeField] private bool avoidDucks = true;

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
    }

    void Update()
    {
        Movement();
    }

    void Movement()
    {
        if(state == State.Wandering || state == State.Pursuit) {
            float moveSpeed = state == State.Pursuit ? speed * 2 : speed;
            Vector3 moveForce = new Vector3();
            targetDirection = (targetLocation - transform.position).normalized;
            targetDirection = new Vector3(targetDirection.x, 0, targetDirection.z); //Flatten the direction so that it is only on one plane
            moveForce += targetDirection;

            if (avoidDucks) { 
                var nearDucks = Duck.DucksInVicinity(transform.position, 2);
                foreach(Duck duck in nearDucks)
                {
                    float distanceToDuck = Vector3.Distance(duck.transform.position, transform.position);
                    Vector3 directionToDuck = (duck.transform.position - transform.position).normalized;
                    moveForce -= directionToDuck * (2 - distanceToDuck);
                }
            }

            moveForce = moveForce.normalized * moveSpeed;

            rb.AddForce(moveForce);
            rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(targetDirection, Vector3.up), Time.deltaTime * turnSpeed);
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
        CheckSurroundings();
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
        while (true) { 
            Vector2 targetOffset = Random.insideUnitCircle * maxDistance;
            newTarget = transform.position + new Vector3(targetOffset.x,0,targetOffset.y);
            if (GameManager.Instance.PositionIsOnLake(newTarget)) break;
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

    public void CheckSurroundings()
    {
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
        else if (targetFood != null && !targetFood.activeSelf)
        {
            ReleaseTarget();
        }
    }

    public static void UpdateSurroundings()
    {
        Duck[] ducks = FindObjectsOfType<Duck>();
        foreach(Duck duck in ducks)
        {
            if(duck.state != State.Eating) duck.CheckSurroundings();
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
