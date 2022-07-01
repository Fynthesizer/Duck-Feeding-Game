using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class Duck : MonoBehaviour
{
    [SerializeField] private Transform model;
    public Animator animator;
    private Rigidbody rb;
    private CapsuleCollider collider;

    private Vector3 targetLocation;
    private Vector3 targetDirection;
    private Vector3 moveDirection;
    private float targetDistance;

    [Header("Global Variables")]
    public DuckGlobals globalVars;

    [Header("Duck Data")]
    //[SerializeField] public DuckData duckData;
    public string duckName;
    public DuckBreed breed;
    public Gender gender;
    public float speed;
    public float weight;
    public float reactionTime;
    public float awarenessRadius;

    [Header("Duck State")]
    public DateTime lastFedTime;
    public float satiety = 0f;
    //public State state = State.Idle;
    public DuckState state;

    public bool Hungry { get => satiety <= 0f; }

    public float tickInterval = 60f;
    public float tickTimer = 0f;

    /*
    [Header("Obstacle Avoidance")]
    [SerializeField] private bool avoidObstacles = true;
    [SerializeField] private float avoidDistance = 1f;
    [SerializeField] private LayerMask wanderAvoidLayers;
    [SerializeField] private LayerMask pursuitAvoidLayers;

    public LayerMask WanderAvoidLayers => wanderAvoidLayers;
    public LayerMask PursuitAvoidLayers => pursuitAvoidLayers;
    */

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

    public enum Gender
    {
        Male,
        Female,
        Unisex
    }

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        collider = gameObject.GetComponent<CapsuleCollider>();
        quackSource = gameObject.GetComponent<AudioSource>();
    }

    void Start()
    {
        SetState(new IdleState(this));
        //material = model.GetComponent<SkinnedMeshRenderer>().material;
        //breed = GameManager.Instance.duckInfoDatabase.breeds.Find(x => x.breedName.Equals(duckData.breed));
        //material = new Material(breed.material);
        
        targetLocation = transform.position;
        Vector2 initialLookDirection = Random.insideUnitCircle.normalized;
        moveDirection = new Vector3(initialLookDirection.x, 0f, initialLookDirection.y);
        rb.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        //waitTimer = StartCoroutine(Wait());
        StartCoroutine(QuackCoroutine());
        StartCoroutine(SwimCoroutine());
        StartCoroutine(CheckSurroundings());
        
    }

    private void OnEnable()
    {
        //GameManager.OnGameTick += OnGameTick;
    }

    private void OnDisable()
    {
        //GameManager.OnGameTick -= OnGameTick;
    }

    private void OnGameTick()
    {
        satiety -= 1f / (globalVars.satiationPeriod * 3600);
        satiety = Mathf.Clamp(satiety, 0f, 1f);
    }

    private bool CheckIfHungry()
    {
        TimeSpan elapsedTime = DateTime.Now.Subtract(lastFedTime);
        return elapsedTime.Hours >= globalVars.satiationPeriod;
    }

    public void LoadData(DuckData data)
    {
        duckName = data.duckName;
        gameObject.name = duckName;
        breed = GameManager.Instance.duckInfoDatabase.breeds.Find(x => x.breedName.Equals(data.breed));
        material = new Material(breed.material);
        model.GetComponent<SkinnedMeshRenderer>().material = material;
        gender = data.gender;
        speed = data.speed;
        weight = data.weight;
        reactionTime = data.reactionTime;
        awarenessRadius = data.awarenessRadius;
        tickTimer = data.tickTimer;
        DateTime.TryParse(data.lastFedTime, out lastFedTime);
        //hungry = CheckIfHungry();
        satiety = data.satiety;
    }

    public DuckData PackageData()
    {
        DuckData data = new DuckData();
        data.duckName = duckName;
        data.breed = breed.breedName;
        data.gender = gender;
        data.speed = speed;
        data.weight = weight;
        data.reactionTime = reactionTime;
        data.awarenessRadius = awarenessRadius;
        data.lastFedTime = lastFedTime.ToString();
        data.satiety = satiety;
        data.tickTimer = tickTimer;
        return data;
    }

    void Update()
    {
        state.Update();
        rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(moveDirection, Vector3.up), Time.deltaTime * globalVars.turnSpeed);

        //UpdateSatiety(Time.deltaTime);
        UpdateTicks(Time.deltaTime);
    }

    public void UpdateTicks(float deltaTime)
    {
        if (Hungry) return;
            
        tickTimer += deltaTime;

        if (tickTimer > tickInterval) { 
            int tickCount = Mathf.FloorToInt(tickTimer / tickInterval);
            tickTimer -= tickInterval * tickCount;

            for (int i = 0; i < tickCount; i++)
            {
                Tick();
                if (Hungry)
                {
                    tickTimer = 0f;
                    break;
                }
            }
        }
    }

    void Tick()
    {
        satiety -= 1 / globalVars.satiationPeriod;
        satiety = Mathf.Clamp(satiety, 0f, 1f);
        GameManager.Instance.AddCurrency(1);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(targetLocation, 0.1f);
        Debug.DrawLine(transform.position, targetLocation, Color.blue);
    }

    public void Swim(Vector3 targetPosition, float moveSpeed, LayerMask avoidLayers)
    {
        targetDirection = (targetPosition - transform.position).normalized;
        targetDirection = new Vector3(targetDirection.x, 0, targetDirection.z); //Flatten the direction so that it is only on one plane
        targetDistance = Vector3.Distance(transform.position, targetPosition);
        moveDirection = targetDirection;

        float directionRotation = 0;
        //Rotate move direction until a clear path is found
        int direction = 1;
        int attempt = 1;
        while (true)
        {
            if (PathIsClear(moveDirection, globalVars.avoidDistance, avoidLayers)) break; //If the path ahead is clear, continue
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
                SetState(new IdleState(this));
                break;
            }
        }

        Vector3 moveForce = moveDirection * moveSpeed;
        rb.AddForce(moveForce);

        if (targetDistance < collider.radius) SetState(new IdleState(this));
    }

    private IEnumerator SwimCoroutine()
    {
        while (true)
        {
            state.Swim();
            yield return new WaitForSeconds(0.1f);
        }
    }

    public bool PathIsClear(Vector3 direction, float distance, LayerMask avoidLayers)
    {
        //LayerMask layerMask = state == State.Pursuit ? pursuitAvoidLayers : wanderAvoidLayers;
        Ray ray = new Ray(transform.position + collider.center, direction);
        if (!Physics.SphereCast(ray, collider.radius, distance, avoidLayers)) return true;
        else return false;
    }

    public void SetState(DuckState _state)
    {
        if(state != null) StartCoroutine(state.Exit());
        state = _state;
        StartCoroutine(state.Enter());
    }

    public void PickWanderTarget()
    {
        Vector3 newTarget;
        int attempts = 0;
        while (true)
        {
            Vector2 targetOffset = Random.insideUnitCircle * globalVars.wanderRadius;
            newTarget = transform.position + new Vector3(targetOffset.x, 0, targetOffset.y);
            Vector3 newTargetDirection = newTarget - transform.position;
            float newTargetDistance = Vector3.Distance(transform.position, newTarget);
            attempts++;
            if (GameManager.Instance.PositionIsOnLake(newTarget) && PathIsClear(newTargetDirection, newTargetDistance, globalVars.wanderAvoidLayers)) break;
            else if (attempts > 50)
            {
                newTarget = transform.position;
                break;
            }
        }

        SetTargetLocation(newTarget);
    }

    void SetTargetLocation(Vector3 newTarget)
    {
        targetLocation = newTarget;
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
            yield return new WaitForSeconds(Random.Range(globalVars.minQuackInterval, globalVars.maxQuackInterval));
            Quack();
        }
    }

    private void Quack()
    {
        AudioClip quackClip = quackClips[Random.Range(0, quackClips.Length)];
        quackSource.PlayOneShot(quackClip);
        animator.SetTrigger("Quack");
    }

    public void SetTargetObject(GameObject target)
    {
        SetTargetLocation(target.transform.position);
        SetState(new PursuitState(this, target));
    }

    public void Eat(GameObject food)
    {
        food.SetActive(false);
        if (Hungry)
        {
            StartCoroutine(FlashCoroutine());
            GameManager.Instance.AddCurrency(1);
        }
        animator.SetTrigger("Eat");
        Destroy(food);
        lastFedTime = DateTime.Now;
        //hungry = false;
        satiety = 1f;
        SetState(new EatState(this));
    }

    IEnumerator CheckSurroundings()
    {
        while (true) { 
            yield return new WaitForSeconds(reactionTime);
            //Look for nearby food
            GameObject[] allFood = GameObject.FindGameObjectsWithTag("DuckFood");
            GameObject closestFood = null;
            float closestDistance = Mathf.Infinity;
            foreach (GameObject food in allFood)
            {
                float distance = Vector3.Distance(transform.position, food.transform.position);
                if(distance < closestDistance && food.GetComponent<DuckFood>().inWater)
                {
                    closestFood = food;
                    closestDistance = distance;
                }
                //if (distance < foodDetectionRadius && (targetFood == null || distance < targetDistance)) SetTargetObject(food.gameObject);
            }
            if (closestFood == null) state.UpdateNearestFood(null);
            else if (closestDistance < awarenessRadius) state.UpdateNearestFood(closestFood);
        }
    }
}
