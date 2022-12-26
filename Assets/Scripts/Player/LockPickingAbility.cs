using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class LockPickingAbility : Ability
{
    [Header("Lock Picking Stats")] 
    // These stats determine how long it takes to open a locked door.
    // There are 3 for the 3 possible ability states.
    public float positivePickTime = -2f;
    public float neutralPickTime = 0f;
    public float negativePickTime = 2f;
    [Space]
    private bool _unlockingDoor;
    [SerializeField]private Lock _lockImLookingAt;
    [SerializeField]private LayerMask environmentMask;

    public override void Awake()
    {
        base.Awake();
        reputation = PlayerPrefs.GetInt("PickReputation");
    }

    public override void Update()
    {
        // Set lock to null. If we are looking at one, it will update this value.
        _lockImLookingAt = null;
        RaycastHit2D hit = Physics2D.Raycast(GameManager.Instance.GetPlayerTransform().position, GameManager.Instance.GetPlayerTransform().up, useRange, environmentMask);
        Debug.DrawRay(GameManager.Instance.GetPlayerTransform().position, GameManager.Instance.GetPlayerTransform().up);
        if (!hit.collider) return;
        if (!hit.collider.TryGetComponent<Lock>(out var l)) return;
        
        if (l.locked)
            _lockImLookingAt = l;
        
        base.Update();
    }

    public override void UseAbility()
    {
        if (charges < 1) return;
        if (_unlockingDoor) return;
        Lock toUnlock = (_lockImLookingAt);

        // Once we have checked for locked doors. if we found a valid door. We start unlocking it.
        if (!toUnlock) return;
        
        switch (abilityLevel)
        {
            case AbilityLevel.Positive:
                StartCoroutine(Unlock(toUnlock, toUnlock.unlockTime + positivePickTime));
                break;
            case AbilityLevel.Neutral:
                StartCoroutine(Unlock(toUnlock, toUnlock.unlockTime + neutralPickTime));
                break;
            case AbilityLevel.Negative:
                StartCoroutine(Unlock(toUnlock, toUnlock.unlockTime + negativePickTime));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    // This coroutine checks if the player is still in range of the door while picking it.
    // If they still are when the time is up, they successfully unlock the door.
    private IEnumerator Unlock(Lock @lock, float unlockTime)
    {
        //Debug.Log("Unlocking Door...");
        StartCoroutine(Wait(unlockTime));
        while (_unlockingDoor)
        {
            if (Vector2.Distance(GameManager.Instance.GetPlayerPosition(), @lock.transform.position) > useRange)
            {
                //Debug.Log("Player got too far from the door. Canceling this action.");
                _unlockingDoor = false;
                StopAllCoroutines();
            }
            yield return new WaitForEndOfFrame();
        }

        _unlockingDoor = false;
        // We successfully unlocked the door. Lose a pick, and tell the door its open.
        //Debug.Log("Door Successfully opened!");
        @lock.Unlock();
        charges--;
    }
    // Wait while the door is being unlocked.
    private IEnumerator Wait(float unlockTime)
    {
        // Prevents unlock time going into the negatives. Probably would break stuff.
        if (unlockTime < 0)
            unlockTime = 0;
        
        _unlockingDoor = true;
        yield return new WaitForSeconds(unlockTime);
        _unlockingDoor = false;
    }
}
