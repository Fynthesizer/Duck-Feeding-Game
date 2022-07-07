using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using System;
using Random = UnityEngine.Random;

public class Duck : MonoBehaviour
{
    [SerializeField] private bool alive;

    [SerializeField] private Transform model;
    [SerializeField] private Transform neck;
    public Transform head;
    public ChainIKConstraint headIK;
    public MultiRotationConstraint lookConstraint;
    public Transform labelAnchor;
    public Animator animator;
    public SkinnedMeshRenderer mesh;
    private Rigidbody rb;
    private CapsuleCollider collider;
    private Material material;

    [Header("Animation")]
    public float headIKWeight = 0f;
    public float neckRotationWeight = 0f;
    public float animatorWeight = 0f;
    public float billOpenness = 0f;
    private float _billOpenness = 0f;

    private Vector3 targetLocation;
    private Vector3 targetDirection;
    private Vector3 lookDirection;
    public Vector3 targetLookDirection;
    private Vector3 moveDirection;
    private float targetDistance;
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
    }

    void Start()
    {
        targetLocation = transform.position;
        Vector2 initialLookDirection = Random.insideUnitCircle.normalized;
        moveDirection = new Vector3(initialLookDirection.x, 0f, initialLookDirection.y);
        targetLookDirection = -moveDirection;
        lookDirection = -moveDirection;
        rb.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        stateMachine = new DuckStateMachine(this);
        DuckStateID initialState = Random.value > 0.5f ? DuckStateID.Idle : DuckStateID.Wander;
        stateMachine.ChangeState(initialState);

        StartCoroutine(QuackCoroutine());
        StartCoroutine(CheckForFood());
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
        if (alive) stateMachine.Update();

        rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(moveDirection, Vector3.up), Time.deltaTime * globalVars.turnSpeed);

        UpdateTicks(Time.deltaTime);
        
    }

    private void InterpolateAnimation()
    {
        headIK.weight = Mathf.Lerp(headIK.weight, state.HeadIkWeight, Time.deltaTime * 5f);
        lookConstraint.weight = Mathf.Lerp(lookConstraint.weight, state.NeckRotationWeight, Time.deltaTime * 5f);
        animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), state.AnimatorWeight, Time.deltaTime * 5f));
        _billOpenness = Mathf.Lerp(_billOpenness, state.BillOpenness, Time.deltaTime * 5f);
        mesh.SetBlendShapeWeight(0, _billOpenness * 100f);
    }

    private void LateUpdate()
    {
        if (state.allowLook) LookAnimation();
        InterpolateAnimation();
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
        Vector3 targetMoveDirection;

        targetDirection = (targetPosition - transform.position).normalized;
        targetDirection = new Vector3(targetDirection.x, 0, targetDirection.z); //Flatten the direction so that it is only on one plane
        targetDistance = Vector3.Distance(transform.position, targetPosition);
        targetMoveDirection = targetDirection;

        float directionRotation = 0;
        //Rotate move direction until a clear path is found
        int direction = 1;
        int attempt = 1;
        while (true)
        {
            if (PathIsClear(targetMoveDirection, Mathf.Min(globalVars.avoidDistance, targetDistance), avoidLayers)) break; //If the path ahead is clear, continue
            else if (directionRotation < Mathf.PI * 2) //Otherwise try a different path
            {
                directionRotation += Mathf.PI / 16f;
                targetMoveDirection = Quaternion.AngleAxis(directionRotation * Mathf.Rad2Deg, Vector3.up) * targetDirection;//Utilities.RotateVector(targetDirection, directionRotation);
                continue;
            }
            else //If there are no clear paths, set state to idle
            {
                stateMachine.ChangeState(DuckStateID.Idle);
                break;
            }
        }

        moveDirection = Vector3.Lerp(moveDirection, targetMoveDirection, 0.25f);

        Vector3 moveForce = moveDirection * moveSpeed;
        rb.AddForce(moveForce);
    }

    public bool PathIsClear(Vector3 direction, float distance, LayerMask avoidLayers)
    {
        Ray ray = new Ray(transform.position + collider.center, direction);
        Debug.DrawRay(transform.position, direction, Color.red, 0.5f);
        if (!Physics.SphereCast(ray, collider.radius, distance, avoidLayers)) return true;
        else return false;
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

    private void LookAnimation()
    {
        lookDirection = Vector3.Slerp(lookDirection, targetLookDirection, Time.deltaTime * globalVars.headTurnSpeed);
        //neck.rotation = Quaternion.LookRotation(lookDirection);
        //localPosition = -lookDirection;
        lookConstraint.data.sourceObjects[0].transform.rotation = Quaternion.LookRotation(lookDirection);
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
            yield return new WaitForSeconds(Random.Range(globalVars.minQuackInterval, globalVars.maxQuackInterval));
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
