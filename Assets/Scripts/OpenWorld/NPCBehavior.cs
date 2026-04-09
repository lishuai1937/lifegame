using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NPC AI Behavior - NPCs live their own life in the grid world
/// 
/// DESIGN:
/// - Each NPC has a daily routine (3 waypoints they cycle through)
/// - Occasionally deviates from routine (random wander, visit a shop, sit on bench)
/// - When player enters NPC's awareness radius (~5m), NPC may approach
/// - "Approach" NPCs walk toward player, trigger dialogue once, then resume routine
/// - NPCs don't stalk/follow the player, one interaction per encounter
/// - Different routines based on NPC role:
///   - Student: home -> school -> playground
///   - Worker: home -> office -> restaurant
///   - Elder: home -> park -> market
///   - Child: home -> playground -> friend's house
/// </summary>
[RequireComponent(typeof(NPC))]
public class NPCBehavior : MonoBehaviour
{
    [Header("Routine")]
    public List<Vector3> RoutinePoints = new List<Vector3>(); // 3 waypoints
    public float MoveSpeed = 2f;
    public float WaitTimeAtPoint = 5f;      // seconds to stay at each point
    public float DeviationChance = 0.15f;   // 15% chance to wander off routine

    [Header("Player Interaction")]
    public float AwarenessRadius = 6f;      // detect player within this range
    public float ApproachRadius = 2f;       // stop and talk at this distance
    public bool WillApproachPlayer = false; // set by SocialSystem
    public bool HasApproachedThisVisit = false; // only approach once

    [Header("State")]
    public NPCAIState CurrentState = NPCAIState.Idle;

    private int currentPointIndex = 0;
    private float waitTimer = 0f;
    private float deviationTimer = 0f;
    private Vector3 deviationTarget;
    private Transform playerTransform;
    private NPC npcComponent;

    void Start()
    {
        npcComponent = GetComponent<NPC>();

        // Find player
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // Generate routine if not set
        if (RoutinePoints.Count == 0)
            GenerateRoutine();

        // Start at first routine point
        if (RoutinePoints.Count > 0)
            transform.position = RoutinePoints[0];

        CurrentState = NPCAIState.Walking;
    }

    void Update()
    {
        // Check for player proximity
        if (!HasApproachedThisVisit && WillApproachPlayer && playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist < AwarenessRadius && CurrentState != NPCAIState.Approaching)
            {
                CurrentState = NPCAIState.Approaching;
            }
        }

        switch (CurrentState)
        {
            case NPCAIState.Walking:
                DoWalking();
                break;
            case NPCAIState.Waiting:
                DoWaiting();
                break;
            case NPCAIState.Deviating:
                DoDeviating();
                break;
            case NPCAIState.Approaching:
                DoApproaching();
                break;
            case NPCAIState.Idle:
                break;
        }
    }

    void DoWalking()
    {
        if (RoutinePoints.Count == 0) return;

        Vector3 target = RoutinePoints[currentPointIndex];
        Vector3 dir = (target - transform.position);
        dir.y = 0;

        if (dir.magnitude < 0.5f)
        {
            // Arrived at waypoint
            waitTimer = WaitTimeAtPoint + Random.Range(-2f, 3f); // slight variation
            CurrentState = NPCAIState.Waiting;
            return;
        }

        // Move toward waypoint
        transform.position += dir.normalized * MoveSpeed * Time.deltaTime;

        // Face movement direction
        if (dir.magnitude > 0.1f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir.normalized), 5f * Time.deltaTime);
    }

    void DoWaiting()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0)
        {
            // Chance to deviate from routine
            if (Random.value < DeviationChance)
            {
                // Wander to a random nearby point
                deviationTarget = RoutinePoints[currentPointIndex] +
                    new Vector3(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));
                deviationTimer = Random.Range(3f, 8f);
                CurrentState = NPCAIState.Deviating;
            }
            else
            {
                // Next routine point
                currentPointIndex = (currentPointIndex + 1) % RoutinePoints.Count;
                CurrentState = NPCAIState.Walking;
            }
        }
    }

    void DoDeviating()
    {
        // Walk to deviation point
        Vector3 dir = (deviationTarget - transform.position);
        dir.y = 0;

        if (dir.magnitude < 0.5f || deviationTimer <= 0)
        {
            // Done deviating, resume routine
            currentPointIndex = (currentPointIndex + 1) % RoutinePoints.Count;
            CurrentState = NPCAIState.Walking;
            return;
        }

        deviationTimer -= Time.deltaTime;
        transform.position += dir.normalized * MoveSpeed * 0.7f * Time.deltaTime;

        if (dir.magnitude > 0.1f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir.normalized), 5f * Time.deltaTime);
    }

    void DoApproaching()
    {
        if (playerTransform == null) { CurrentState = NPCAIState.Walking; return; }

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        // Player walked away, give up
        if (dist > AwarenessRadius * 1.5f)
        {
            CurrentState = NPCAIState.Walking;
            return;
        }

        // Close enough, trigger dialogue
        if (dist < ApproachRadius)
        {
            HasApproachedThisVisit = true;
            CurrentState = NPCAIState.Idle;

            // Face player
            Vector3 lookDir = (playerTransform.position - transform.position);
            lookDir.y = 0;
            if (lookDir.magnitude > 0.1f)
                transform.rotation = Quaternion.LookRotation(lookDir);

            // Force trigger interaction
            if (npcComponent != null)
                npcComponent.Interact();

            // After dialogue, resume routine after a delay
            Invoke(nameof(ResumeRoutine), 3f);
            return;
        }

        // Walk toward player
        Vector3 dir = (playerTransform.position - transform.position);
        dir.y = 0;
        transform.position += dir.normalized * MoveSpeed * 1.2f * Time.deltaTime;

        if (dir.magnitude > 0.1f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir.normalized), 5f * Time.deltaTime);
    }

    void ResumeRoutine()
    {
        CurrentState = NPCAIState.Walking;
    }

    /// <summary>
    /// Generate a 3-point routine based on NPC role
    /// Points are relative to NPC's spawn position
    /// </summary>
    void GenerateRoutine()
    {
        Vector3 home = transform.position;
        var r = new System.Random(npcComponent != null ? npcComponent.NpcName.GetHashCode() : 0);

        Vector3 point2, point3;

        NPCRole role = npcComponent != null ? npcComponent.Role : NPCRole.Stranger;

        switch (role)
        {
            case NPCRole.Classmate:
            case NPCRole.Authority:
                // Home -> School area -> Playground
                point2 = home + new Vector3(r.Next(-10, 10), 0, r.Next(5, 15));
                point3 = home + new Vector3(r.Next(-15, 15), 0, r.Next(-5, 5));
                break;
            case NPCRole.Colleague:
                // Home -> Office -> Restaurant
                point2 = home + new Vector3(r.Next(5, 20), 0, r.Next(-5, 5));
                point3 = home + new Vector3(r.Next(-5, 10), 0, r.Next(-10, -3));
                break;
            case NPCRole.Elder:
                // Home -> Park -> Market (short distances)
                point2 = home + new Vector3(r.Next(-5, 5), 0, r.Next(3, 8));
                point3 = home + new Vector3(r.Next(-8, 0), 0, r.Next(-3, 3));
                break;
            case NPCRole.Child:
                // Home -> Playground -> Friend's area (energetic, wider range)
                point2 = home + new Vector3(r.Next(-12, 12), 0, r.Next(-12, 12));
                point3 = home + new Vector3(r.Next(-15, 15), 0, r.Next(-15, 15));
                break;
            case NPCRole.Family:
                // Stays close to home
                point2 = home + new Vector3(r.Next(-5, 5), 0, r.Next(-5, 5));
                point3 = home + new Vector3(r.Next(-3, 3), 0, r.Next(-3, 3));
                break;
            default:
                // Random 3 points
                point2 = home + new Vector3(r.Next(-15, 15), 0, r.Next(-15, 15));
                point3 = home + new Vector3(r.Next(-15, 15), 0, r.Next(-15, 15));
                break;
        }

        RoutinePoints.Add(home);
        RoutinePoints.Add(point2);
        RoutinePoints.Add(point3);
    }
}

public enum NPCAIState
{
    Idle,           // standing still
    Walking,        // moving to next routine point
    Waiting,        // at a routine point, hanging out
    Deviating,      // wandered off routine temporarily
    Approaching     // walking toward player to initiate contact
}