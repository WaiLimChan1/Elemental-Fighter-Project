using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalElemental : ElementalChampion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Crystal Elemental Variables")]
    [SerializeField] protected BoxCollider2D Attack3CrowdControlBox;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void DealDamageToVictim(Champion enemy, float damage)
    {
        if (statusNetworked == Status.AIR_ATTACK || statusNetworked == Status.ATTACK3 || statusNetworked == Status.UNIQUE1) 
            enemy.TakeDamageNetworked(damage, isFacingLeftNetworked, AttackType.BlockByFacingAttacker, transform.position);
        else enemy.TakeDamageNetworked(damage, isFacingLeftNetworked, AttackType.BlockByFacingAttack);
    }

    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.ATTACK3)
        {
            BoxCollider2D SpecialAttackBox = AttackBoxes[4];
            Vector2 center = AttackBoxesParent.TransformPoint(SpecialAttackBox.offset);

            if (enemy.transform.position.x < center.x) enemy.AddVelocity(new Vector2(crowdControlStrength / 2, crowdControlStrength));
            else enemy.AddVelocity(new Vector2(-1 * crowdControlStrength / 2, crowdControlStrength));
        }
        else if (statusNetworked == Status.SPECIAL_ATTACK) enemy.AddVelocity(new Vector2(0, crowdControlStrength));
        else if (statusNetworked == Status.UNIQUE1)
        {
            if (enemy.transform.position.x < transform.position.x) enemy.AddVelocity(new Vector2(-1 * crowdControlStrength, crowdControlStrength));
            else enemy.AddVelocity(new Vector2(crowdControlStrength, crowdControlStrength));
        }
    }

    public override void GetControlBoxAndStrength(ref BoxCollider2D crowdControlBox, ref float crowdControlStrength, int index)
    {
        if (statusNetworked == Status.ATTACK3) crowdControlBox = Attack3CrowdControlBox;
        else crowdControlBox = AttackBoxes[index];
        crowdControlStrength = CrowdControlStrength[index];
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
