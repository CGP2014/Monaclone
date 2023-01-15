using UnityEngine;
using UnityEngine.InputSystem;

public class KnockoutAbility : Ability
{
    public float throwRangePositive;
    public float throwRangeNeutral;
    public float throwRangeNegative;

    public GameObject throwablePrefab;

    public override void UseAbility(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (Charges < 1) return;
        Transform playerPos = GameManager.Instance.GetPlayerTransform();
        Throwable throwable = Instantiate(throwablePrefab, playerPos.position, Quaternion.identity).GetComponent<Throwable>();
        var throwForce = 0f;
        switch (AbilityLevel)
        {
            case AbilityLevel.Positive:
                throwForce = throwRangePositive;
                break;
            case AbilityLevel.Neutral:
                throwForce = throwRangeNeutral;
                break;
            case AbilityLevel.Negative:
                throwForce = throwRangeNegative;
                break;
        }
        throwable.ThrowMe(playerPos.up, throwForce);
        Charges--;
    }
}
