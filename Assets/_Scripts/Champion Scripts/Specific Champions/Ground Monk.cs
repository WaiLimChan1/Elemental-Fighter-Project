using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundMonk : Champion
{
    [Header("Ground Monk Variables")]
    [SerializeField] protected BoxCollider2D SpecialAttackCrowdControlBox;



    //---------------------------------------------------------------------------------------------------------------------------------------------
    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.ATTACK1) enemy.AddVelocity(new Vector2(0, crowdControlStrength));
        else if (statusNetworked == Status.ATTACK3) enemy.AddVelocity(new Vector2(direction * crowdControlStrength * 1 / 5, crowdControlStrength));
        else if (statusNetworked == Status.SPECIAL_ATTACK)
        {
            BoxCollider2D crowdControlBox = SpecialAttackCrowdControlBox;
            Vector2 center = AttackBoxesParent.TransformPoint(crowdControlBox.offset);

            if (enemy.transform.position.x < center.x) enemy.AddVelocity(new Vector2(crowdControlStrength, 0));
            else enemy.AddVelocity(new Vector2(-1 * crowdControlStrength, 0));
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
            else return;

            BoxCollider2D crowdControlBox;

            if (statusNetworked == Status.SPECIAL_ATTACK) crowdControlBox = SpecialAttackCrowdControlBox;
            else crowdControlBox = AttackBoxes[index];

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



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status.UNIQUE1 : Begin_Meditation
    //Status.UNIQUE2 : Meditation
    protected override bool InterruptableStatus()
    {
        return (base.InterruptableStatus() || 
            status == Status.UNIQUE1 || status == Status.UNIQUE2);
    }

    protected override void TakeInput()
    {
        base.TakeInput();

        if (dead)
        {
            return;
        }

        if (!inAir && InterruptableStatus())
        {
            if (Input.GetKey(KeyCode.Q))
            {
                //Begin_Meditation, then Begin_Meditation into Meditation, then if already Meditation, continue Meditation
                Status lastStatus = (Status)ChampionAnimationController.GetAnimatorStatus();
                if (lastStatus != Status.UNIQUE1 && lastStatus != Status.UNIQUE2) status = Status.UNIQUE1;
                else if (lastStatus == Status.UNIQUE1)
                {
                    if (ChampionAnimationController.AnimationFinished()) status = Status.UNIQUE2;
                    else status = Status.UNIQUE1;
                }
                else if (lastStatus == Status.UNIQUE2) status = Status.UNIQUE2;

            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
