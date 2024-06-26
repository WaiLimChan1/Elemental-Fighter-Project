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
    //Status Logic
    protected override bool UnstoppableStatusNetworked()
    {
        return base.UnstoppableStatusNetworked() ||
            statusNetworked == Status.AIR_ATTACK || statusNetworked == Status.ATTACK1 || 
            statusNetworked == Status.ATTACK2 || statusNetworked == Status.ATTACK3;
    }

    protected override bool AttackSpeedStatus(Status status)
    {
        return (status == Status.AIR_ATTACK || status == Status.ATTACK1 || status == Status.ATTACK2 || status == Status.ATTACK3 || status == Status.SPECIAL_ATTACK);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void DealDamageToVictim(Champion enemy, float damage, float numOfAttacks)
    {
        if (statusNetworked == Status.AIR_ATTACK) 
            enemy.TakeDamageNetworked(this, damage, numOfAttacks, isFacingLeftNetworked, AttackType.BlockByFacingAttacker, transform.position);
        else enemy.TakeDamageNetworked(this, damage, numOfAttacks, isFacingLeftNetworked, AttackType.BlockByFacingAttack);
    }

    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.AIR_ATTACK) enemy.SetVelocity(new Vector2(0, crowdControlStrength));
        else if (statusNetworked == Status.ATTACK1) enemy.AddVelocity(new Vector2(direction * crowdControlStrength / 2, crowdControlStrength));
        else if (statusNetworked == Status.ATTACK2) enemy.AddVelocity(new Vector2(direction * crowdControlStrength / 2, crowdControlStrength));
        else if (statusNetworked == Status.ATTACK3)
        {
            BoxCollider2D SpecialAttackBox = Attacks[4].hitBox;
            Vector2 center = AttackBoxesParent.TransformPoint(SpecialAttackBox.offset);

            float distance = Mathf.Abs(enemy.transform.position.x - center.x);
            float xCrowdControlStrength = Mathf.Lerp(0, crowdControlStrength, distance / SpecialAttackBox.size.x);

            if (enemy.transform.position.x < center.x) enemy.AddVelocity(new Vector2(xCrowdControlStrength, crowdControlStrength));
            else enemy.AddVelocity(new Vector2(-1 * xCrowdControlStrength, crowdControlStrength));
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
        else crowdControlBox = Attacks[index].hitBox;
        crowdControlStrength = Attacks[index].crowdControlStrength;
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
