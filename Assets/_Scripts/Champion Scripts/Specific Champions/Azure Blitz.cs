using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AzureBlitz : Champion
{
    [Header("Azure Blitz Variables")]
    [SerializeField] private float dashSpeed = 30;

    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.ATTACK2) enemy.AddVelocity(new Vector2(0, crowdControlStrength));
        else if (statusNetworked == Status.SPECIAL_ATTACK) enemy.AddVelocity(new Vector2(direction * crowdControlStrength / 2, crowdControlStrength));
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------
    protected override void UpdatePosition()
    {
        base.UpdatePosition();
        if (statusNetworked == Status.AIR_ATTACK &&
            ChampionAnimationController.GetNormalizedTime() >= 2.0f / 8.0f &&
            ChampionAnimationController.GetNormalizedTime() <= 5.0f / 8.0f)
        {
            float xChange = dashSpeed * Time.fixedDeltaTime;

            if (isFacingLeftNetworked) xChange *= -1;
            Rigid.position = new Vector2(Rigid.position.x + xChange, Rigid.position.y);
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
