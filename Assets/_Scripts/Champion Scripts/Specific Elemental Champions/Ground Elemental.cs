using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundElemental : ElementalChampion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Ground Elemental Variables")]
    [SerializeField] protected BoxCollider2D TransformPullCrowdControlBox;
    [SerializeField] protected float TransformPullStrength = 2.0f;
    private const float TransformPullCutOffTime = 18.0f / 28.0f;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    protected override bool UnstoppableStatusNetworked()
    {
        return base.UnstoppableStatusNetworked() ||
            statusNetworked == Status.AIR_ATTACK || statusNetworked == Status.ATTACK1 || 
            statusNetworked == Status.ATTACK2 || statusNetworked == Status.ATTACK3;
    }

    protected override void TakeInput()
    {
        base.TakeInput();

        if (dead)
        {
            return;
        }

        //Ground Elemental does not have Defend
        if (status == Status.BEGIN_DEFEND || status == Status.DEFEND) status = Status.IDLE;
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void DealDamageToVictim(Champion enemy, float damage)
    {
        if (statusNetworked == Status.SPECIAL_ATTACK || statusNetworked == Status.UNIQUE1) 
            enemy.TakeDamageNetworked(damage, isFacingLeftNetworked, AttackType.BlockByFacingAttacker, transform.position);
        else enemy.TakeDamageNetworked(damage, isFacingLeftNetworked, AttackType.BlockByFacingAttack);
    }

    public override void GetControlBoxAndStrength(ref BoxCollider2D crowdControlBox, ref float crowdControlStrength, int index)
    {
        if (statusNetworked == Status.UNIQUE1 && ChampionAnimationController.GetNormalizedTime() < TransformPullCutOffTime)
        {
            crowdControlBox = TransformPullCrowdControlBox;
            crowdControlStrength = TransformPullStrength;
        }
        else
        {
            crowdControlBox = AttackBoxes[index];
            crowdControlStrength = CrowdControlStrength[index];
        }
    }

    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.AIR_ATTACK) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, crowdControlStrength / 2));
        else if (statusNetworked == Status.ATTACK1) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, crowdControlStrength / 2));
        else if (statusNetworked == Status.ATTACK2) enemy.AddVelocity(new Vector2(direction * crowdControlStrength / 2, crowdControlStrength));
        else if (statusNetworked == Status.ATTACK3) enemy.SetVelocity(new Vector2(0, crowdControlStrength));
        else if (statusNetworked == Status.SPECIAL_ATTACK)
        {
            BoxCollider2D crowdControlBox = AttackBoxes[4];
            Vector2 center = AttackBoxesParent.TransformPoint(crowdControlBox.offset);
            Vector2 changeVector = new Vector2(center.x - enemy.transform.position.x, center.y - enemy.transform.position.y).normalized;

            enemy.SetVelocity(changeVector * crowdControlStrength);
        }    
        else if (statusNetworked == Status.UNIQUE1)
        {
            if (ChampionAnimationController.GetNormalizedTime() < TransformPullCutOffTime) //Pulling
            {
                BoxCollider2D crowdControlBox = TransformPullCrowdControlBox;
                Vector2 center = AttackBoxesParent.TransformPoint(crowdControlBox.offset);
                Vector2 changeVector = new Vector2(center.x - enemy.transform.position.x, center.y - enemy.transform.position.y).normalized;

                enemy.SetVelocity(changeVector * crowdControlStrength);
            }
            else //Explosion
            {
                BoxCollider2D crowdControlBox = AttackBoxes[5];
                Vector2 center = AttackBoxesParent.TransformPoint(crowdControlBox.offset);

                if (enemy.transform.position.x < transform.position.x) enemy.AddVelocity(new Vector2(-1 * crowdControlStrength, crowdControlStrength));
                else enemy.AddVelocity(new Vector2(crowdControlStrength, crowdControlStrength));
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}