using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundElemental : ElementalChampion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Ground Monk Variables")]
    [SerializeField] protected BoxCollider2D TransformPullCrowdControlBox;
    [SerializeField] protected float TransformPullStrength = 2.0f;
    private const float PullCutOffTime = 18.0f / 28.0f;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void DealDamageToVictim(Champion enemy, float damage)
    {
        if (statusNetworked == Status.SPECIAL_ATTACK || statusNetworked == Status.UNIQUE1) enemy.TakeDamageNetworked(damage, isFacingLeftNetworked, AttackType.BlockByFacingAttacker, transform.position);
        else enemy.TakeDamageNetworked(damage, isFacingLeftNetworked, AttackType.BlockByFacingAttack);
    }

    public override void GetControlBoxAndStrength(ref BoxCollider2D crowdControlBox, ref float crowdControlStrength, int index)
    {
        if (statusNetworked == Status.UNIQUE1 && ChampionAnimationController.GetNormalizedTime() < PullCutOffTime)
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

        if (statusNetworked == Status.ATTACK3) enemy.AddVelocity(new Vector2(0, crowdControlStrength));
        else if (statusNetworked == Status.SPECIAL_ATTACK)
        {
            BoxCollider2D crowdControlBox = AttackBoxes[4];
            Vector2 center = AttackBoxesParent.TransformPoint(crowdControlBox.offset);
            Vector2 changeVector = new Vector2(center.x - enemy.transform.position.x, center.y - enemy.transform.position.y).normalized;

            enemy.AddVelocity(changeVector * crowdControlStrength);
        }    
        else if (statusNetworked == Status.UNIQUE1)
        {
            if (ChampionAnimationController.GetNormalizedTime() < PullCutOffTime) //Pulling
            {
                BoxCollider2D crowdControlBox = TransformPullCrowdControlBox;
                Vector2 center = AttackBoxesParent.TransformPoint(crowdControlBox.offset);
                Vector2 changeVector = new Vector2(center.x - enemy.transform.position.x, center.y - enemy.transform.position.y).normalized;

                enemy.AddVelocity(changeVector * crowdControlStrength);
            }
            else //Explosion
            {
                BoxCollider2D crowdControlBox = AttackBoxes[5];
                Vector2 center = AttackBoxesParent.TransformPoint(crowdControlBox.offset);
                Vector2 changeVector = new Vector2(enemy.transform.position.x - center.x, enemy.transform.position.y - center.y).normalized;
                enemy.AddVelocity(changeVector * crowdControlStrength);
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}