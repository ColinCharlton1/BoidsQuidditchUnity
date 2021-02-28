using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public enum Team { G, S };

    [Header("Trait Viewer")]
    public Team myTeam;
    public float weight = 75;
    public float maxV = 18;
    public float aggressiveness = 22;
    public float maxExhaust = 65;

    [Header("Linked Scriptables")]
    public EnvironmentScriptable environmentData;
    public PlayerConstraints cons;

    private Rigidbody rb;
    private Rigidbody snitchRB;
    private SphereCollider myCollider;
    private GameObject snitch;
    private bool resting;
    private float exhaustion;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        snitch = GameObject.FindWithTag("GoldenSnitch");
        snitchRB = snitch.GetComponent<Rigidbody>();
        myCollider = GetComponent<SphereCollider>();
        resting = false;
        exhaustion = 0;
        //SetTraits();
    }

    //private void SetTraits()
    //{
    //    if (myTeam.Equals(Team.G))
    //    {

    //    }
    //    else
    //    {

    //    }
    //}
    
    // Start is called before the first frame update
    void Start()
    {
    }

    //Update is called once per frame
    void Update()
    {
        transform.forward = rb.velocity.normalized;
    }

    private void FixedUpdate()
    {
        if (resting)
        {
            exhaustion -= maxExhaust / 10.0f * Time.deltaTime;
            //I know you don't need to multiply by Time.deltaTime, but it lets you calculate the force needed to reduce velocity by 1/4.
            if (rb.velocity.magnitude > 1) rb.AddForce((rb.velocity * 0.25f * Time.deltaTime));
            if (exhaustion < maxExhaust * 2) resting = false;
        }
        else
        {
            if (exhaustion > maxExhaust) return;
            Vector3 newForce = Vector3.zero;
            newForce += GetBarrierAdjustment() * cons.barrierAvoidStrength;
            newForce += GetEnvironmentConstraints() * cons.environmentConstraintStrength;
            newForce += GetSnitchAdjustment() * aggressiveness;
            newForce += GetPlayerAdjustment() * cons.playerAvoidStrength;
            Vector3 velocity = rb.velocity;
            Vector3 potentialNewV = velocity + (newForce * Time.deltaTime) / rb.mass;
            if (potentialNewV.magnitude > maxV)
            {
                potentialNewV = (potentialNewV.normalized * maxV);
                newForce = (potentialNewV - velocity) * rb.mass / Time.deltaTime;
            }
            rb.AddForce(newForce);
            exhaustion += Time.deltaTime;
            if (exhaustion > maxExhaust * 0.9f) resting = true;
        }
    }

    private Vector3 GetBarrierAdjustment()
    {
        
        Vector3 forceAdjustment = Vector3.zero;
        RaycastHit[] barrierhits = Physics.SphereCastAll(origin: transform.position,
                                                  radius: cons.barrierAvoidDistance,
                                                  direction: transform.forward,
                                                  maxDistance: cons.barrierAvoidDistance,
                                                  layerMask: LayerMask.GetMask("Barrier"));
        foreach (RaycastHit hit in barrierhits)
        {
            //float distanceFactor = cons.barrierAvoidStrength / Mathf.Max(hit.distance, 1f); (transform.position - hit.point).normalized * distanceFactor;
            forceAdjustment -= (rb.position - hit.rigidbody.position).normalized;
            Debug.Log("barrier hit at: " + (rb.position - hit.rigidbody.position).normalized);
            Debug.DrawLine(rb.position, rb.position + forceAdjustment * cons.barrierAvoidStrength, Color.blue, 0.1f);
        }
        return forceAdjustment;
    }

    private Vector3 GetPlayerAdjustment()
    {
        Vector3 forceAdjustment = Vector3.zero;
        RaycastHit[] playerhits = Physics.SphereCastAll(origin: transform.position,
                                                  radius: cons.playerAvoidDistance,
                                                  direction: transform.forward,
                                                  maxDistance: 0.0f,
                                                  layerMask: LayerMask.GetMask("Player"));
        foreach (RaycastHit hit in playerhits)
        {
            if (hit.collider != myCollider) {
                //float distanceFactor = cons.collisionAvoidStrength / Mathf.Max(hit.distance * hit.distance, 0.5f);
                forceAdjustment += (rb.position - hit.rigidbody.position).normalized;
                Debug.DrawLine(rb.position, rb.position + forceAdjustment * cons.playerAvoidStrength, Color.green, 0.1f);
            }
        }
        if (forceAdjustment != Vector3.zero) forceAdjustment = IncreaseMinVectorVal(forceAdjustment);
        return forceAdjustment;
    }

    private Vector3 IncreaseMinVectorVal(Vector3 vec)
    {
        float absx = Mathf.Abs(vec.x);
        float absy = Mathf.Abs(vec.y);
        float absz = Mathf.Abs(vec.z);
        if (absx < absy && absx < absz)
        {
            vec.x += UnityEngine.Random.Range(0.5f, 2f) * Mathf.Sign(vec.x);
        }
        else if (absy < absx && absy < absz)
        {
            vec.y += UnityEngine.Random.Range(0.5f, 2f) * Mathf.Sign(vec.y);
        }
        else
        {
            vec.z += UnityEngine.Random.Range(0.5f, 2f) * Mathf.Sign(vec.z);
        }
        return vec;
    }

    private Vector3 GetEnvironmentConstraints()
    {
        float heightMaxDist = transform.position.y - environmentData.maxHeight;
        float xMaxDist = rb.position.x - environmentData.xMax;
        float xMinDist = rb.position.x - environmentData.xMin;
        float zMaxDist = rb.position.z - environmentData.zMax;
        float zMinDist = rb.position.z - environmentData.zMin;
        Vector3 forceAdjustment = Vector3.zero;
        if (heightMaxDist > -cons.environmentConstraintDistance) forceAdjustment.y = 1 / Mathf.Min(heightMaxDist, -1f);
        if (xMaxDist > -cons.environmentConstraintDistance) forceAdjustment.x = 1 / Mathf.Min(xMaxDist, -1f);
        if (xMinDist < cons.environmentConstraintDistance) forceAdjustment.x = 1 / Mathf.Max(xMinDist, 1f);
        if (zMaxDist > -cons.environmentConstraintDistance) forceAdjustment.z = 1 / Mathf.Min(zMaxDist, -1f);
        if (zMinDist < cons.environmentConstraintDistance) forceAdjustment.z = 1 / Mathf.Max(zMinDist, 1f);
        Debug.DrawLine(rb.position, rb.position + forceAdjustment * cons.environmentConstraintStrength, Color.red, 0.1f);
        return forceAdjustment;
    }

    private Vector3 GetSnitchAdjustment()
    {
        Vector3 desiredMove = (snitchRB.position - rb.position).normalized;
        //float distanceFactor = rb.velocity.magnitude * collisionAvoidStrength / Mathf.Min(desiredMove.magnitude, 0.1f); // Have players speed up/slow down based on distance?
        Debug.DrawLine(rb.position, rb.position + desiredMove * aggressiveness, Color.cyan, 0.1f);
        return desiredMove;
    }
}