using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class snitch_behaviour : MonoBehaviour
{
    [Header("Speed Controls")]
    public float minSpeed = 5.0f;
    public float maxSpeed = 10.0f;

    [Header("Random Params")]
    public float randomChangeStrength = 1.0f;
    public float changeRandomInterval = 0.1f;
    public float randomChangeMax = 1.0f;

    [Header("Barrier Avoidance")]
    public EnvironmentScriptable environmentData;
    public float barrierAvoidDistance = 5.0f;
    public float barrierAvoidStrength = 0.5f;


    private Rigidbody rb;
    private Vector3 currentRandom;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f),
                                    UnityEngine.Random.Range(-5.0f, 5.0f),
                                    UnityEngine.Random.Range(-5.0f, 5.0f))
                                    .normalized * minSpeed;
        InvokeRepeating("ChangeRandomForce", 0.0f, changeRandomInterval);
    }

    //Update is called once per frame
    void Update()
    {
        transform.forward = rb.velocity.normalized;
    }

    private Vector3 GetBarrierAdjustment()
    {
        Vector3 forceAdjustment = Vector3.zero;
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, barrierAvoidDistance, transform.forward, out hit, barrierAvoidDistance, LayerMask.GetMask("Barrier")))
        {
            float distanceFactor = rb.velocity.magnitude * barrierAvoidStrength / Mathf.Min(hit.distance, 0.1f);
            forceAdjustment = (transform.position - hit.point).normalized * distanceFactor;
            Debug.DrawLine(transform.position, hit.point, Color.red, 1.0f);
        }
        Debug.Log("barrier adjustment of: " + forceAdjustment);
        return forceAdjustment;
    }

    private Vector3 GetEnvironmentConstraints()
    {
        float heightMaxDist = transform.position.y - environmentData.maxHeight;
        float xMaxDist = transform.position.x - environmentData.xMax;
        float xMinDist = transform.position.x - environmentData.xMin;
        float zMaxDist = transform.position.z - environmentData.zMax;
        float zMinDist = transform.position.z - environmentData.zMin;
        float distanceFactor = barrierAvoidStrength * rb.velocity.magnitude;
        Vector3 forceAdjustment = Vector3.zero;
        if (heightMaxDist > -barrierAvoidDistance) forceAdjustment.y = distanceFactor / Mathf.Min(heightMaxDist, -0.1f);
        if (xMaxDist > -barrierAvoidDistance) forceAdjustment.x = distanceFactor / Mathf.Min(xMaxDist, -0.1f);
        if (xMinDist < barrierAvoidDistance) forceAdjustment.x = distanceFactor / Mathf.Max(xMinDist, 0.1f);
        if (zMaxDist > -barrierAvoidDistance) forceAdjustment.z = distanceFactor / Mathf.Min(zMaxDist, -0.1f);
        if (zMinDist < barrierAvoidDistance) forceAdjustment.z = distanceFactor / Mathf.Max(zMinDist, 0.1f);
        return forceAdjustment;
    }

    private void FixedUpdate()
    {
        Vector3 newForce = currentRandom;
        newForce += GetBarrierAdjustment();
        newForce += GetEnvironmentConstraints();
        rb.AddForce(newForce);
        Vector3 velocity = rb.velocity;
        Debug.Log("velocity after force addition: " + velocity);
        
        if (velocity.magnitude > maxSpeed)
        {
            rb.AddForce(((velocity.normalized * maxSpeed) - velocity));
        }
        else if (velocity.magnitude < minSpeed)
        {
            rb.AddForce(((velocity.normalized * minSpeed) - velocity));
        }
        
        Debug.Log("velocity after corrections: " + rb.velocity);
        Debug.Log("###############################");
    }

    private void ChangeRandomForce()
    {
        Vector3 result = new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f),
                                     UnityEngine.Random.Range(-1.0f, 1.0f),
                                     UnityEngine.Random.Range(-1.0f, 1.0f));
        result = (currentRandom / randomChangeStrength + (result.normalized * randomChangeMax)).normalized * randomChangeStrength;
        currentRandom = result;
    }
}

