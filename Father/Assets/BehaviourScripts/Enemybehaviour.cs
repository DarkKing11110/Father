using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyBehaviour : MonoBehaviour
{
    public List<PatrolPath> patrolRoutes; // Liste von Patrouillenrouten
    public Transform player; // Referenz zum Spieler
    public float chaseRange = 10f; // Sichtweite, um den Spieler zu verfolgen
    public float hearingRange = 15f; // Hörreichweite
    public float attackRange = 2f; // Reichweite, um den Spieler anzugreifen
    public float fleeHealthThreshold = 20f; // Gesundheitsschwelle zum Wegrennen
    public float fleeDistance = 10f; // Distanz zum Weglaufen
    public int maxHealth = 100; // Maximale Gesundheit
    public float routeSwitchInterval = 30f; // Intervall für Routenwechsel
    public int currentHealth; // Aktuelle Gesundheit des Gegners

    [Header("Versteck-Logik")]
    public LayerMask hidingLayerMask; // Layer für Verstecke
    public float lostInterestTime = 5f; // Zeit, bis das Interesse verloren geht

    private int currentPatrolIndex;
    private NavMeshAgent agent;
    private Animator animator;
    private bool isFleeing;
    private Transform[] patrolPoints;
    private PatrolPath currentPatrolRoute;
    private float routeSwitchTimer;

    private float hidingTimer = 0f;
    private bool playerIsHidden = false;

    private enum State { Patrolling, Chasing, Attacking, Fleeing }
    private State currentState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth; // Setze die aktuelle Gesundheit auf die maximale Gesundheit

        if (patrolRoutes != null && patrolRoutes.Count > 0)
        {
            SwitchToRandomPatrolRoute();
        }

        currentState = State.Patrolling;
        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Spieler hören, wenn er rennt
        if (IsPlayerRunning() && distanceToPlayer <= hearingRange)
        {
            currentState = State.Chasing;
        }

        // Versteck-Logik: Prüfen, ob Spieler versteckt ist
        if (currentState == State.Chasing || currentState == State.Attacking)
        {
            if (IsPlayerHidden())
            {
                if (!playerIsHidden)
                {
                    playerIsHidden = true;
                    hidingTimer = 0f;
                }
                else
                {
                    hidingTimer += Time.deltaTime;
                    if (hidingTimer >= lostInterestTime)
                    {
                        currentState = State.Patrolling;
                        GoToNextPatrolPoint();
                        playerIsHidden = false;
                        hidingTimer = 0f;
                        return;
                    }
                }
            }
            else
            {
                playerIsHidden = false;
                hidingTimer = 0f;
            }
        }

        switch (currentState)
        {
            case State.Patrolling:
                HandlePatrollingState(distanceToPlayer);
                break;

            case State.Chasing:
                HandleChasingState(distanceToPlayer);
                break;

            case State.Attacking:
                HandleAttackingState(distanceToPlayer);
                break;

            case State.Fleeing:
                HandleFleeingState();
                break;
        }

        // Routenwechsel-Logik
        routeSwitchTimer += Time.deltaTime;
        if (routeSwitchTimer >= routeSwitchInterval)
        {
            SwitchToRandomPatrolRoute();
            routeSwitchTimer = 0f;
        }
    }

    private void HandlePatrollingState(float distanceToPlayer)
    {
        if (currentHealth <= fleeHealthThreshold && distanceToPlayer <= chaseRange)
        {
            currentState = State.Fleeing;
            FleeFromPlayer();
        }
        else if (distanceToPlayer <= attackRange)
        {
            currentState = State.Attacking;
            AttackPlayer();
        }
        else if (distanceToPlayer <= chaseRange && CanSeePlayer())
        {
            currentState = State.Chasing;
            ChasePlayer();
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPatrolPoint();
        }
    }

    private void HandleChasingState(float distanceToPlayer)
    {
        if (currentHealth <= fleeHealthThreshold)
        {
            currentState = State.Fleeing;
            FleeFromPlayer();
        }
        else if (distanceToPlayer <= attackRange)
        {
            currentState = State.Attacking;
            AttackPlayer();
        }
        else if (!CanSeePlayer())
        {
            currentState = State.Patrolling;
            GoToNextPatrolPoint();
        }
        else
        {
            ChasePlayer();
        }
    }

    private void HandleAttackingState(float distanceToPlayer)
    {
        if (currentHealth <= fleeHealthThreshold)
        {
            currentState = State.Fleeing;
            FleeFromPlayer();
        }
        else if (distanceToPlayer > attackRange || !CanSeePlayer())
        {
            currentState = State.Chasing;
            ChasePlayer();
        }
    }

    private void HandleFleeingState()
    {
        if (!isFleeing)
        {
            currentState = State.Patrolling;
            GoToNextPatrolPoint();
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Debug.Log("Gehe zu PatrolPoint: " + patrolPoints[currentPatrolIndex].name);
        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        animator.SetBool("isWalking", true);
    }

    void ChasePlayer()
    {
        agent.destination = player.position;
        animator.SetBool("isWalking", true);
        animator.SetBool("isAttacking", false);
    }

    void AttackPlayer()
    {
        agent.ResetPath();
        animator.SetBool("isAttacking", true);
        animator.SetBool("isWalking", false);
    }

    void FleeFromPlayer()
    {
        isFleeing = true;
        Vector3 fleeDirection = (transform.position - player.position).normalized;
        Vector3 fleePosition = transform.position + fleeDirection * fleeDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleePosition, out hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.destination = hit.position;
        }
        else
        {
            fleeDirection = Quaternion.Euler(0, 90, 0) * fleeDirection;
            fleePosition = transform.position + fleeDirection * fleeDistance;
            if (NavMesh.SamplePosition(fleePosition, out hit, fleeDistance, NavMesh.AllAreas))
            {
                agent.destination = hit.position;
            }
        }

        animator.SetBool("isWalking", true);
        animator.SetBool("isAttacking", false);

        Invoke("StopFleeing", 5f);
    }

    void StopFleeing()
    {
        isFleeing = false;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        animator.SetTrigger("takeDamage");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        animator.SetTrigger("isDead");
        agent.isStopped = true;
        Destroy(gameObject, 2f);
    }

    private bool IsPlayerRunning()
    {
        //Überprüfen, ob der Spieler rennt
        PlayerBehaviour playerMovement = player.GetComponent<PlayerBehaviour>();
        return playerMovement != null && playerMovement.isRunning;
    }

    private bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, chaseRange))
        {
            return hit.transform == player;
        }
        return false;
    }

    // Prüft, ob der Spieler unter einem Versteck-Objekt ist
    private bool IsPlayerHidden()
    {
        // Raycast von oben nach unten auf den Spieler, prüft auf hidingLayerMask
        Ray ray = new Ray(player.position + Vector3.up * 0.5f, Vector3.up);
        float maxHideCheck = 5f;
        return Physics.Raycast(ray, maxHideCheck, hidingLayerMask);
    }

    private void SwitchToRandomPatrolRoute()
    {
        if (patrolRoutes == null || patrolRoutes.Count == 0)
            return;

        // Wenn es nur eine Route gibt, einfach diese nehmen
        if (patrolRoutes.Count == 1)
        {
            currentPatrolRoute = patrolRoutes[0];
        }
        else
        {
            // Wähle eine andere Route als die aktuelle
            int currentIndex = patrolRoutes.IndexOf(currentPatrolRoute);
            int newIndex = currentIndex;
            int safety = 0; // Schutz vor Endlosschleife

            while (newIndex == currentIndex && safety < 10)
            {
                newIndex = Random.Range(0, patrolRoutes.Count);
                safety++;
            }
            currentPatrolRoute = patrolRoutes[newIndex];
        }

        patrolPoints = currentPatrolRoute.patrolPoints.ToArray();
        currentPatrolIndex = 0;
        GoToNextPatrolPoint();
    }

}
