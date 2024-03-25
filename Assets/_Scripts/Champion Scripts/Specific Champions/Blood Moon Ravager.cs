using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodMoonRavager : Champion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Blood Moon Ravager Variables")]
    [SerializeField] private float SpecialAttackRange = 20f;
    [SerializeField][Networked] private Champion championTarget { get; set; }
    //---------------------------------------------------------------------------------------------------------------------------------------------
    
    
    
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    //Status.UNIQUE1 : Howl
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

        if (!inAir && InterruptableStatus())
        {
            if (Input.GetKey(KeyCode.Q)) status = Status.UNIQUE1;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void AnimationTriggerMobility()
    {
        if (statusNetworked == Status.SPECIAL_ATTACK)
        {
            championTarget = null;
            if (championTarget == null)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, SpecialAttackRange, LayerMask.GetMask("Champion"));
                foreach (Collider2D collider in colliders)
                {
                    Champion enemy = collider.GetComponent<Champion>();
                    if (enemy != null && enemy != this && enemy.healthNetworked > 0)
                    {
                        championTarget = enemy;
                        break;
                    }
                }
            }

            if (championTarget != null)
            {
                Vector3 changeVector = championTarget.transform.TransformPoint(championTarget.Collider.offset) - AttackBoxesParent.TransformPoint(AttackBoxes[4].offset);
                transform.position = transform.position + changeVector;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, SpecialAttackRange);
    }
}
