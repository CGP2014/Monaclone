using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PatrolBehaviour))] [RequireComponent(typeof(InvestigateBehaviour))] [RequireComponent(typeof(ChaseBehaviour))] [RequireComponent(typeof(UnconsciousBehaviour))]
[RequireComponent(typeof(Sight))] 

// ABOUT //
/*
 *  The AI Controller drives and determines what behaviour the entity should be in.
 *  There are four states:
 *  - Patrolling
 *  - Investigating
 *  - Chasing
 *  - Unconscious
 *
 *  Each state is driven by its own component, which works independently from the AIController.
 *
 *  Patrolling - This behaviour takes input nodes, and follows them one by one.
 *  pretty simple stuff.
 * 
 *  Investigating - If this behaviour is called, it will take whatever is in the positionToInvestigate
 *  value and will then investigate it, they first go to the position, then choose two random rotations
 *  to look in, if they do not change state in that time, they fail the investigation and return to patrolling.
 *
 *  Chasing - When chasing, the entity will constantly follow the player.
 *
 *  Unconscious - The entity is still, not doing anything.
 *
 *  In this controller, a timer called the detection meter, starts at a certain value, if the timer ticks down
 *  below a certain threshold set by the investigateStartTime value, it will switch to the investigate behaviour.
 *  other things such as a throwable can force a switch to investigation from Patrolling. If the player is within
 *  the entity's view for long enough, Chase mode activates. at that point, the player will have to stay out of the
 *  entity's view so the detection timer ticks back up back to investigation, if an investigation fails, they will
 *  return to patrolling again.
 * 
*/

public class AIController : MonoBehaviour
{
    [Header("Runtime Values")]
    // The state that the AI is currently in.
    public AIState aiState;
    // Has a player been detected at all?
    public bool playerDetected;

    
    [Header("Sight Settings")]
    //How long does the player have to be within this AI's view to make them investigate what they saw
    public float investigateStartTime;
    
    //How long does the player have to be within this AI's view to make them chase.
    public float chaseStartTime; 
    
    // This time ticks up and down depending on player detection.
    private float _detectionMeter;
    

    [Header("AI Settings")] 
    // How fast can the AI move?
    public float movementSpeed;
    
    // How fast can the AI rotate?
    public float angularSpeed;
    
    // This keeps track of the latest investigation position the AI has been assigned.
    [HideInInspector]public Vector3 positionToInvestigate;
    
    
    // COMPONENTS
    private PatrolBehaviour _patrolBehaviour;
    private InvestigateBehaviour _investigateBehaviour;
    private ChaseBehaviour _chaseBehaviour;
    private UnconsciousBehaviour _unconsciousBehaviour;
    private Sight _sight;
    private NavMeshAgent _agent;
    
    public enum AIState
    {
        Patrolling,
        Investigating,
        Chasing,
        Unconscious
    }

    private void Awake()
    {
        // Assign our required systems.
        AssignAgentValues();
        _patrolBehaviour = GetComponent<PatrolBehaviour>();
        _investigateBehaviour = GetComponent<InvestigateBehaviour>();
        _chaseBehaviour = GetComponent<ChaseBehaviour>();
        _unconsciousBehaviour = GetComponent<UnconsciousBehaviour>();
        _sight = GetComponent<Sight>();
        _sight.seenTag.AddListener(PlayerDetected);
        _detectionMeter = chaseStartTime; //Set our internal timer to the max, which is the chase start time. Once this ticks down. the AI will chase the player.
    }
    private void Start()
    {
        UpdateAIState(AIState.Patrolling);
    }

    private void Update()
    {
        playerDetected = false;
    }

    private void FixedUpdate()
    {
        UpdateFieldOfViewColour();
        
        DetectionLogic();
    }
    private void AssignAgentValues()
    {
        _agent = GetComponentInChildren<NavMeshAgent>();
        _agent.speed = movementSpeed;
        _agent.angularSpeed = angularSpeed;
    }
    public void UpdateAIState(AIState stateToUpdateTo)
    {
        Debug.Log("AIState: " + stateToUpdateTo);
        if (aiState == AIState.Unconscious) return;
        aiState = stateToUpdateTo;
        switch (aiState)
        {
            case AIState.Patrolling:
                StopAICoroutines();
                _patrolBehaviour.StartPatrolling();
                break;
            case AIState.Investigating:
                StopAICoroutines();
                _investigateBehaviour.GoInvestigatePosition(positionToInvestigate);
                break;
            case AIState.Chasing:
                StopAICoroutines();
                _chaseBehaviour.StartChasing();
                break;
            case AIState.Unconscious:
                StopAICoroutines();
                _unconsciousBehaviour.LoseConciousness();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    // Updates the FOV cone colour depending on whats going on.
    private void UpdateFieldOfViewColour()
    {
        if(playerDetected)
            _sight.SetFieldOfViewColour(Color.yellow);
        else
            _sight.SetFieldOfViewColour(Color.green);
        if (aiState == AIState.Investigating)
            _sight.SetFieldOfViewColour(Color.yellow);
        if (aiState == AIState.Chasing)
            _sight.SetFieldOfViewColour(Color.red);
        if(aiState == AIState.Unconscious)
            _sight.SetFieldOfViewColour(Color.gray);
    }

    private void PlayerDetected(string tag)
    {
        Debug.Log("Tag seen: " + tag);
        if (tag == "Player")
        {
            playerDetected = true;
        }
    }
    // Check for player presence and if so. Tick our countdown down.
    private void DetectionLogic()
    {
        if (aiState == AIState.Unconscious) return;
        // If we detect the player, tick down the detection timer.
        if (playerDetected) 
        {
            _detectionMeter -= Time.fixedDeltaTime;
        }
        // If we dont find them. bring it back up.
        else
        {
            _detectionMeter += Time.fixedDeltaTime;
            
            if (_detectionMeter > chaseStartTime)
                _detectionMeter = chaseStartTime;
        }
        // If the player was in the AI's Field of view for long enough. The AI will decide to investigate whatever is going on.
        if (chaseStartTime - investigateStartTime > _detectionMeter && _detectionMeter > 0 && aiState != AIState.Investigating)
        {
            positionToInvestigate = GameManager.Instance.GetPlayerPosition();
            UpdateAIState(AIState.Investigating);
        }
        // The player has been found! Chase State Should alert people around them and have them chase too!
        if (_detectionMeter <= 0 && aiState != AIState.Chasing)
        {
            UpdateAIState(AIState.Chasing);
        }
    }
    private void StopAICoroutines()
    {
        _patrolBehaviour.StopAllCoroutines();
        _investigateBehaviour.StopAllCoroutines();
        _chaseBehaviour.StopAllCoroutines();
    }
}
