using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterElemental : ElementalChampion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    protected override void CancelTakeInput()
    {
        base.CancelTakeInput();
        if (status == Status.BEGIN_DEFEND || status == Status.DEFEND) status = Status.IDLE; //Water Elemental does not have Defend
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void DealDamageToVictim(Champion enemy, float damage)
    {
        if (statusNetworked == Status.SPECIAL_ATTACK || statusNetworked == Status.UNIQUE1) 
            enemy.TakeDamageNetworked(this, damage, isFacingLeftNetworked, AttackType.BlockByFacingAttacker, transform.position);
        else enemy.TakeDamageNetworked(this, damage, isFacingLeftNetworked, AttackType.BlockByFacingAttack);
    }

    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.AIR_ATTACK) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, 0));
        else if (statusNetworked == Status.ATTACK1) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, 0));
        else if (statusNetworked == Status.ATTACK2) enemy.AddVelocity(new Vector2(direction * crowdControlStrength / 2, crowdControlStrength));
        else if (statusNetworked == Status.ATTACK3) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, crowdControlStrength));
        else if (statusNetworked == Status.UNIQUE1)
        {
            if (enemy.transform.position.x < transform.position.x) enemy.AddVelocity(new Vector2(-1 * crowdControlStrength, crowdControlStrength));
            else enemy.AddVelocity(new Vector2(crowdControlStrength, crowdControlStrength));
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}