using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalChampion : Champion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status.UNIQUE1 : Transform

    protected override bool SingleAnimationStatus()
    {
        return (base.SingleAnimationStatus() ||
            status == Status.UNIQUE1);
    }


    protected override void TakeInput()
    {
        base.TakeInput();

        if (dead)
        {
            return;
        }

        //Elemental Champions do not have Roll.
        if (status == Status.ROLL) status = Status.RUN;

        if (!inAir && InterruptableStatus())
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                status = Status.UNIQUE1;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    public override void AnimationTriggerAttack()
    {
        //if (Runner.IsServer)
        {
            int index = 0;
            if (statusNetworked == Status.AIR_ATTACK) index = 0;
            else if (statusNetworked == Status.ATTACK1) index = 1;
            else if (statusNetworked == Status.ATTACK2) index = 2;
            else if (statusNetworked == Status.ATTACK3) index = 3;
            else if (statusNetworked == Status.SPECIAL_ATTACK) index = 4;
            else if (statusNetworked == Status.UNIQUE1) index = 5;
            else return;

            BoxCollider2D attackBox = AttackBoxes[index];
            float damage = AttackDamages[index];

            Collider2D[] colliders = Physics2D.OverlapBoxAll(attackBox.bounds.center, attackBox.bounds.size, 0, LayerMask.GetMask("Champion"));
            foreach (Collider2D collider in colliders)
            {
                Champion enemy = collider.GetComponent<Champion>();
                if (enemy != null && enemy != this && enemy.healthNetworked > 0)
                    DealDamageToVictim(enemy, damage);
            }
        }
    }

    public override void AnimationTriggerCrowdControl()
    {
        //if (Runner.IsServer)
        {
            int index = 0;
            if (statusNetworked == Status.AIR_ATTACK) index = 0;
            else if (statusNetworked == Status.ATTACK1) index = 1;
            else if (statusNetworked == Status.ATTACK2) index = 2;
            else if (statusNetworked == Status.ATTACK3) index = 3;
            else if (statusNetworked == Status.SPECIAL_ATTACK) index = 4;
            else if (statusNetworked == Status.UNIQUE1) index = 5;
            else return;

            BoxCollider2D crowdControlBox = AttackBoxes[index];
            float crowdControlStrength = CrowdControlStrength[index];

            Collider2D[] colliders = Physics2D.OverlapBoxAll(crowdControlBox.bounds.center, crowdControlBox.bounds.size, 0, LayerMask.GetMask("Champion"));
            foreach (Collider2D collider in colliders)
            {
                Champion enemy = collider.GetComponent<Champion>();
                if (enemy != null && enemy != this && enemy.healthNetworked > 0)
                    ApplyCrowdControl(enemy, crowdControlStrength);
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
