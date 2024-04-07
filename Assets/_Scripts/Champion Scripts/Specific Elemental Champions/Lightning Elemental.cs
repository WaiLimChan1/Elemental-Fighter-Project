using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningElemental : ElementalChampion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Lightning Elemental Variables")]
    [SerializeField] private float attack1TeleportRange = 15f;
    [SerializeField][Networked] private Champion attack1Target { get; set; }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void DealDamageToVictim(Champion enemy, float damage)
    {
        if (statusNetworked == Status.AIR_ATTACK || statusNetworked == Status.SPECIAL_ATTACK || statusNetworked == Status.UNIQUE1)
            enemy.TakeDamageNetworked(damage, isFacingLeftNetworked, AttackType.BlockByFacingAttacker, transform.position);
        else enemy.TakeDamageNetworked(damage, isFacingLeftNetworked, AttackType.BlockByFacingAttack);
    }

    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.AIR_ATTACK)
        {
            BoxCollider2D crowdControlBox = Attacks[0].hitBox;
            Vector2 center = AttackBoxesParent.TransformPoint(crowdControlBox.offset);

            if (enemy.transform.position.x < center.x) enemy.SetVelocity(new Vector2(crowdControlStrength, crowdControlStrength));
            else if (enemy.transform.position.x > center.x) enemy.SetVelocity(new Vector2(-1 * crowdControlStrength, crowdControlStrength));
        }
        else if (statusNetworked == Status.ATTACK1) enemy.AddVelocity(new Vector2(direction * crowdControlStrength / 2, crowdControlStrength));
        else if (statusNetworked == Status.ATTACK2) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, 0));
        else if (statusNetworked == Status.ATTACK3)
        {
            BoxCollider2D crowdControlBox = Attacks[3].hitBox;
            Vector2 center = AttackBoxesParent.TransformPoint(crowdControlBox.offset);
            Vector2 changeVector = new Vector2(center.x - enemy.transform.position.x, center.y - enemy.transform.position.y).normalized;

            enemy.AddVelocity(changeVector * crowdControlStrength);
        }
    }

    public override void AnimationTriggerMobility()
    {
        if (statusNetworked == Status.ATTACK1)
        {
            if (attack1Target == null) attack1Target = MainGameUtils.FindClosestEnemyCircle(this, transform.position, attack1TeleportRange);
            if (attack1Target != null)
            {
                Vector3 linkPoint = new Vector3(Attacks[1].hitBox.offset.x, Collider.offset.y);
                Vector3 changeVector = attack1Target.transform.TransformPoint(attack1Target.Collider.offset) - AttackBoxesParent.TransformPoint(linkPoint);
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
        attack1Target = MainGameUtils.FindClosestEnemyCircle(this, transform.position, attack1TeleportRange);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attack1TeleportRange);
        MainGameUtils.OnDrawGizmos_TeleportTarget(this, attack1Target, 1);
    }
}