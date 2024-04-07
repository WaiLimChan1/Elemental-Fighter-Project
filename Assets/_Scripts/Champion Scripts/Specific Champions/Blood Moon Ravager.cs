using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodMoonRavager : Champion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Blood Moon Ravager Variables")]
    [SerializeField][Networked] private Champion specialAttackTarget { get; set; }
    [SerializeField] private float specialAttackTeleportRange = 20f;

    [SerializeField] private float howlManaCost = 10f;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    //Status.UNIQUE1 : Howl
    protected override float getManaCost(Status status)
    {
        float manaCost = base.getManaCost(status);
        if (status == Status.UNIQUE1) manaCost = howlManaCost;
        return manaCost;
    }

    protected override bool SingleAnimationStatus()
    {
        return (base.SingleAnimationStatus() ||
            status == Status.UNIQUE1);
    }

    protected override bool UnstoppableStatusNetworked()
    {
        return (statusNetworked == Status.BEGIN_DEFEND || statusNetworked == Status.DEFEND);
    }

    protected override void TakeInput()
    {
        base.TakeInput();

        if (dead)
        {
            return;
        }

        if (!inAir && InterruptableStatus() && manaNetworked >= getManaCost(Status.UNIQUE1))
        {
            if (Input.GetKeyDown(KeyCode.Q)) status = Status.UNIQUE1;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void AnimationTriggerMobility()
    {
        if (statusNetworked == Status.SPECIAL_ATTACK)
        {
            if (specialAttackTarget == null) specialAttackTarget = MainGameUtils.FindClosestEnemyCircle(this, transform.position, specialAttackTeleportRange);
            if (specialAttackTarget != null)
            {
                Vector3 changeVector = specialAttackTarget.transform.TransformPoint(specialAttackTarget.Collider.offset) - AttackBoxesParent.TransformPoint(Attacks[4].hitBox.offset);
                transform.position = transform.position + changeVector;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Logic
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        specialAttackTarget = MainGameUtils.FindClosestEnemyCircle(this, transform.position, specialAttackTeleportRange);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, specialAttackTeleportRange);
        MainGameUtils.OnDrawGizmos_TeleportTarget(this, specialAttackTarget);
    }
}
