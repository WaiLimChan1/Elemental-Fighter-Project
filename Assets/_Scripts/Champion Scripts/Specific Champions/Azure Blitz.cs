using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AzureBlitz : Champion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Azure Blitz Variables")]
    [SerializeField] private float airAttackDashSpeed = 30;

    private const float airAttackDashStartTime = 2.0f / 8.0f;
    private const float airAttackDashEndTime = 6.0f / 8.0f;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.AIR_ATTACK) enemy.SetVelocity(new Vector2(direction * crowdControlStrength, crowdControlStrength));
        else if (statusNetworked == Status.ATTACK2) enemy.AddVelocity(new Vector2(0, crowdControlStrength));
        else if (statusNetworked == Status.SPECIAL_ATTACK) enemy.AddVelocity(new Vector2(direction * crowdControlStrength / 2, crowdControlStrength));
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Logic
    protected override void UpdatePosition()
    {
        base.UpdatePosition();
        if (statusNetworked == Status.AIR_ATTACK &&
            ChampionAnimationController.GetNormalizedTime() >= airAttackDashStartTime &&
            ChampionAnimationController.GetNormalizedTime() < airAttackDashEndTime)
        {
            float xChange = airAttackDashSpeed * Time.fixedDeltaTime;
            if (isFacingLeftNetworked) xChange *= -1;

            Rigid.position = new Vector2(Rigid.position.x + xChange, Rigid.position.y);
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}