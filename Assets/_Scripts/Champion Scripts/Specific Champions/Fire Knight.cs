using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireKnight : Champion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void DealDamageToVictim(Champion enemy, float damage, float numOfAttacks)
    {
        if (statusNetworked == Status.ATTACK2) enemy.TakeDamageNetworked(this, damage, numOfAttacks, isFacingLeftNetworked, AttackType.BlockByFacingAttacker, transform.position);
        else enemy.TakeDamageNetworked(this, damage, numOfAttacks, isFacingLeftNetworked, AttackType.BlockByFacingAttack);
    }

    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.ATTACK3) enemy.AddVelocity(new Vector2(direction * crowdControlStrength * 1 / 2, crowdControlStrength));
        else if (statusNetworked == Status.SPECIAL_ATTACK) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, 0));

    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
