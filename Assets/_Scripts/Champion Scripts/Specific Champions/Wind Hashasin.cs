using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class WindHashasin : Champion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Special Attack Variables")]
    [SerializeField][Networked] private Champion specialAttackTarget { get; set; }
    [SerializeField] private float specialAttackTeleportRange = 20f;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    protected override bool UnstoppableStatusNetworked()
    {
        return (statusNetworked == Status.BEGIN_DEFEND || statusNetworked == Status.DEFEND);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.ATTACK3) enemy.AddVelocity(new Vector2(direction * crowdControlStrength / 2, crowdControlStrength));
    }

    public override void AnimationTriggerMobility()
    {
        if (statusNetworked == Status.SPECIAL_ATTACK)
        {
            if (specialAttackTarget == null) specialAttackTarget = MainGameUtils.FindClosestEnemyCircle(this, transform.position, specialAttackTeleportRange);
            if (specialAttackTarget != null) transform.position = specialAttackTarget.transform.position;
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
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position, specialAttackTeleportRange);
        MainGameUtils.OnDrawGizmos_TeleportTarget(this, specialAttackTarget);
    }
}
