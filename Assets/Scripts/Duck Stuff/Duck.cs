using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using System;
using Pathfinding;
using Random = UnityEngine.Random;

public class Duck : MonoBehaviour
{
    [SerializeField] private bool alive;

    [SerializeField] private Transform model;
    [SerializeField] private Transform neck;
    public Transform head;
    public Transform mouth;
    public ChainIKConstraint headIK;
    public TwistChainConstraint lookConstraint;
    public Transform labelAnchor;
    public Animator animator;
    public SkinnedMeshRenderer mesh;
    private Rigidbody rb;
    private CapsuleCollider collider;
    private Material material;

    private Seeker seeker;
    public Path path;
    private int currentWaypoint = 0;

    [Header("Animation")]
    public float billOpenness = 0f;
    private float _billOpenness = 0f;
    public float dipAmount = 0f;
    private float _dipAmount = 0f;

    private Vector3 lookDirection;
    public Vector3 targetLookDirection;
    private Vector3 moveDirection;
    public GameObject nearestFood;

    [Header("Global Variables")]
    public DuckGlobals globalVars;

    [Header("Duck Data")]
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
    //public DuckState state;
    public DuckStateMachine stateMachine;
    public DuckState state { get { return stateMachine.GetState(stateMachine.currentState); } }
    //public DuckStateID initialState;

    public bool Hungry { get => satiety <= 0f; }

    public float tickInterval = 60f;
    public float tickTimer = 0f;

    [Header("Quack")]
    [SerializeField] private AudioClip[] quackClips;
    private AudioSource quackSource;

    /*
    [Header("Glow")]
    [SerializeField] private float glowFadeSpeed = 0.05f;
    private float glowAmount = 0.0f;
    */

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
        mesh = model.GetComponent<SkinnedMeshRenderer>();
        seeker = gameObject.GetComponent<Seeker>();
    }

    void Start()
    {
        Vector2 initialLookDirection = Random.insideUnitCircle.normalized;
        moveDirection = new Vector3(initialLookDirection.x, 0f, initialLookDirection.y);
        targetLookDirection = -moveDirection;
        lookDirection = -moveDirection;
        rb.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        stateMachine = new DuckStateMachine(this);
        stateMachine.ChangeState(PickInitialState());

        StartCoroutine(QuackCoroutine());
        StartCoroutine(CheckForFood());
    }

    DuckStateID PickInitialState()
    {
        float randomValue = Random.value;
        if (randomValue < 0.45f) return DuckStateID.Idle;
        else if (randomValue < 0.9f) return DuckStateID.Wander;
        else return DuckStateID.Sleep;
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
        satiety = data.satiety;
    }

    public DuckData Data
    {
        get { 
            return new DuckData()
            {
                duckName = duckName,
                breed = breed.breedName,
                gender = gender,
                speed = speed,
                weight = weight,
                reactionTime = reactionTime,
                awarenessRadius = awarenessRadius,
                lastFedTime = lastFedTime.ToString(),
                satiety = satiety,
                tickTimer = tickTimer
            };
        }
    }

    private void Update()
    {
        if (alive) stateMachine.Update();

        rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(moveDirection, Vector3.up), Time.deltaTime * 5f);

        UpdateTicks(Time.deltaTime);
    }

    private void LateUpdate()
    {
        ApplyAnimation();
    }

    public void SetTarget(Vector3 newTarget)
    {
        seeker.StartPath(transform.position, newTarget, OnPathComplete);
    }

    public void OnPathComplete(Path p)
    {
        Debug.Log("A path was calculated. Did it fail with an error? " + p.error);

        if (!p.error)
        {
            path = p;
            // Reset the waypoint counter so that we start to move towards the first point in the path
            currentWaypoint = 0;
        }
    }

    private void ApplyAnimation()
    {
        LookAnimation();

        float IKTargetWeight = state.animationDriver == AnimationDriver.IK ? 1f : 0f;
        float twistTargetWeight = state.animationDriver == AnimationDriver.Twist ? 1f : 0f;
        float animatorTargetWeight = state.animationDriver == AnimationDriver.Animator ? 1f : 0f;

        headIK.weight = Mathf.Lerp(headIK.weight, IKTargetWeight * _dipAmount, Time.deltaTime * globalVars.animationBlendSpeed);
        lookConstraint.weight = Mathf.Lerp(lookConstraint.weight, twistTargetWeight, Time.deltaTime * globalVars.animationBlendSpeed);
        animator.SetLayerWeight(0, Mathf.Lerp(animator.GetLayerWeight(0), animatorTargetWeight, Time.deltaTime * globalVars.animationBlendSpeed));
        _billOpenness = Mathf.Lerp(_billOpenness, billOpenness, Time.deltaTime * globalVars.animationBlendSpeed);
        _dipAmount = Mathf.Lerp(_dipAmount, dipAmount, Time.deltaTime * globalVars.animationBlendSpeed);
        mesh.SetBlendShapeWeight(0, mesh.GetBlendShapeWeight(0) + _billOpenness * 100f); //Add on to weight already set by animator, to avoid overriding animation
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
        GameManager.Instance.Currency++;
    }


    public void Swim(float moveSpeed, bool avoidDucks = true)
    {
        if (path == null) return;
        
        Vector3 targetMoveDirection;

        float distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
        if (distanceToWaypoint < 0.2f)
        {
            if (currentWaypoint + 1 < path.vectorPath.Count) currentWaypoint++;
        }
        if (currentWaypoint + 1 < path.vectorPath.Count)
        {
            //If the duck is already closer to the next waypoint, skip the current one
            float distanceToNextWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint + 1]);
            float distanceBetweenWaypoints = Vector3.Distance(path.vectorPath[currentWaypoint], path.vectorPath[currentWaypoint + 1]);
            if (distanceToNextWaypoint < distanceBetweenWaypoints) currentWaypoint++;
        }
        targetMoveDirection = (path.vectorPath[currentWaypoint] - transform.position).normalized;

        //If another duck is in the way, turn to avoid them
        if (avoidDucks && !PathIsClear(targetMoveDirection, 1f)) targetMoveDirection = Quaternion.AngleAxis(90f, Vector3.up) * targetMoveDirection;

        moveDirection = Vector3.Slerp(moveDirection, targetMoveDirection, globalVars.turnSpeed);

        float targetAlignment = Vector3.Dot(moveDirection, targetMoveDirection);

        Vector3 moveForce = moveDirection * moveSpeed;
        if (targetAlignment > 0.8f) rb.AddForce(moveForce);
    }

    public bool PathIsClear(Vector3 direction, float distance)
    {
        Ray ray = new Ray(transform.position + collider.center, direction);

        if (!Physics.SphereCast(ray, collider.radius, distance - collider.radius, (1 << 7))) return true;
        else return false;
    }

    private void LookAnimation()
    {
        lookDirection = Vector3.Slerp(lookDirection, targetLookDirection, Time.deltaTime * globalVars.headTurnSpeed);
        //twistC = Quaternion.LookRotation(lookDirection);
        //localPosition = -lookDirection;
        lookConstraint.data.tipTarget.transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    /*
    IEnumerator FlashCoroutine()
    {
        glowAmount = 1f;
        while (glowAmount > 0f)
        {
            glowAmount -= glowFadeSpeed;
            material.SetColor("_Emissive_Color", new Color(glowAmount,glowAmount,glowAmount,1));
            yield return null;
        }
    }
    */

    IEnumerator QuackCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(globalVars.minQuackInterval / state.QuackFrequency, globalVars.maxQuackInterval / state.QuackFrequency));
            if (state.allowQuack) Quack();
        }
    }

    private void Quack()
    {
        AudioClip quackClip = quackClips[Random.Range(0, quackClips.Length)];
        quackSource.PlayOneShot(quackClip);
        animator.SetTrigger("Quack");
    }

    public void Eat(GameObject food)
    {
        food.SetActive(false);
        Destroy(food);
        lastFedTime = DateTime.Now;
        satiety = 1f;
        //stateMachine.ChangeState(DuckStateID.Eat);
    }

    IEnumerator CheckForFood()
    {
        while (true) { 
            yield return new WaitForSeconds(reactionTime);

            GameObject[] allFood = GameObject.FindGameObjectsWithTag("DuckFood");
            if (allFood.Length == 0)
            {
                UpdateNearestFood(null);
                continue;
            }

            GameObject nearestFound = null;
            float nearestDistance = Mathf.Infinity;

            foreach (GameObject food in allFood)
            {
                float distance = Vector3.Distance(transform.position, food.transform.position);
                if(distance < nearestDistance && food.GetComponent<DuckFood>().inWater)
                {
                    nearestFound = food;
                    nearestDistance = distance;
                }
            }
            if (nearestDistance < awarenessRadius) UpdateNearestFood(nearestFound);
            else UpdateNearestFood(null);

            //if (closestFood == null) state.UpdateNearestFood(null);
            //else if (closestDistance < awarenessRadius) state.UpdateNearestFood(closestFood);
        }
    }

    private void UpdateNearestFood(GameObject food)
    {
        nearestFood = food;
        state.UpdateNearestFood(food);
    }
}
