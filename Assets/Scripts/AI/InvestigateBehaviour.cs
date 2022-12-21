using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class InvestigateBehaviour : MonoBehaviour
{
    // How long should the AI investigate a location for before going back to other tasks?
    public float lookAroundTime;
    
    private AIController _aiController;
    private NavMeshAgent _agent;
    private bool _lookAround;
    private Quaternion _randomRot;
   
    private void Awake()
    {
        // Get the required components to get this Script running.
        _aiController = GetComponent<AIController>();
        _agent = GetComponentInChildren<NavMeshAgent>();
        
        if(!_aiController)
            Debug.LogError(gameObject + " cannot find a AIController! Make sure there is one on it.");
        
        if(!_agent)
            Debug.LogError(gameObject + " cannot find a NavMeshAgent! Make sure there is one on it.");
    }
    
    private void Update()
    {
        if(_lookAround) 
            _agent.transform.rotation = Quaternion.RotateTowards(_agent.transform.rotation, _randomRot, Time.deltaTime * _aiController.angularSpeed);
    }

    public void GoInvestigatePosition(Vector3 posToInvestigate) // This just allows you to call the coroutine from other scripts
    {
        StartCoroutine(nameof(InvestigatePosition), posToInvestigate);
    }
    private IEnumerator InvestigatePosition(Vector3 posToInvestigate)
    {
        UpdateAgentPosition(_agent.transform.position);
        //TODO: Include little animation for the AI to perform to visualise that they have gone into investigation mode
        yield return new WaitForSeconds(.2f);
        UpdateAgentPosition(posToInvestigate);
        
        yield return new WaitForSeconds(0.1f); // Fixes an issue where the wait until condition was immediately satisfied. TODO: Find a better way to prevent this issue
        
        yield return new WaitUntil(() => _agent.remainingDistance < 0.1f);
        _lookAround = true;
        _randomRot = (Random.rotation);
        yield return new WaitForSeconds(lookAroundTime / 2);
        _randomRot = (Random.rotation);
        yield return new WaitForSeconds(lookAroundTime / 2);
        _lookAround = false;
        ReturnToPatrolState();
        StopAllCoroutines(); 
    }

    private void ReturnToPatrolState()
    {
        if(_aiController)
            _aiController.UpdateAIState(AIController.AIState.Patrolling);
    }

    private void UpdateAgentPosition(Vector3 pos) // Just checks for the NavMeshAgent component before setting a position to prevent errors.
    {
        if (_agent)
            _agent.SetDestination(pos);
    }
}
