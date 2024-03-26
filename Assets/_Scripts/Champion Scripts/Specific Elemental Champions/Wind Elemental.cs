using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindElemental : ElementalChampion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Wind Elemental Variables")]
    [SerializeField] private float SpecialAttackRange = 25f;
    [SerializeField] private BoxCollider2D SpecialAttackPartII;
    [SerializeField] private float SpecialAttackPartIIDamageMultiplier = 5f;
    private const float SpecialPartIICutOffTime = 15.0f / 22.0f;

    [SerializeField] private float BlinkRange = 20f;

    [SerializeField][Networked] private Champion championTarget { get; set; }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    protected override bool SingleAnimationStatus()
    {
        return (base.SingleAnimationStatus() ||
            status == Status.UNIQUE2);
    }

    protected override void TakeInput()
    {
        base.TakeInput();

        if (dead)
        {
            return;
        }

        //Water Elemental does not have Defend
        if (status == Status.BEGIN_DEFEND || status == Status.DEFEND) status = Status.IDLE;

        if (!inAir && InterruptableStatus())
        {
            if (Input.GetKey(KeyCode.Q)) status = Status.UNIQUE2;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void DealDamageToVictim(Champion enemy, float damage)
    {
        if (statusNetworked == Status.AIR_ATTACK || statusNetworked == Status.UNIQUE1)
            enemy.TakeDamageNetworked(damage, isFacingLeftNetworked, AttackType.BlockByFacingAttacker, transform.position);
        else enemy.TakeDamageNetworked(damage, isFacingLeftNetworked, AttackType.BlockByFacingAttack);
    }

    public override void GetAttackBoxAndDamage(ref BoxCollider2D attackBox, ref float damage, int index)
    {
        if (statusNetworked == Status.SPECIAL_ATTACK && ChampionAnimationController.GetNormalizedTime() >= SpecialPartIICutOffTime)
        {
            attackBox = SpecialAttackPartII;
            damage = AttackDamages[index] * SpecialAttackPartIIDamageMultiplier;
        }
        else
        {
            attackBox = AttackBoxes[index];
            damage = AttackDamages[index];
        }
    }

    public override void GetControlBoxAndStrength(ref BoxCollider2D crowdControlBox, ref float crowdControlStrength, int index)
    {
        if (statusNetworked == Status.SPECIAL_ATTACK && ChampionAnimationController.GetNormalizedTime() >= SpecialPartIICutOffTime)
        {
            crowdControlBox = SpecialAttackPartII;
            crowdControlStrength = CrowdControlStrength[index];
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

        if (statusNetworked == Status.ATTACK1) enemy.AddVelocity(new Vector2(0, crowdControlStrength));
        else if (statusNetworked == Status.ATTACK3) enemy.AddVelocity(new Vector2(direction * crowdControlStrength / 2, crowdControlStrength));
        else if (statusNetworked == Status.SPECIAL_ATTACK)
        {
            enemy.AddVelocity(new Vector2(direction * crowdControlStrength, 0));
            Debug.Log(ChampionAnimationController.GetNormalizedTime());
        }
        else if (statusNetworked == Status.UNIQUE1)
        {
            if (enemy.transform.position.x < transform.position.x) enemy.AddVelocity(new Vector2(-1 * crowdControlStrength, crowdControlStrength));
            else enemy.AddVelocity(new Vector2(crowdControlStrength, crowdControlStrength));
        }
    }

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
                Vector3 linkPoint = new Vector3(AttackBoxes[4].offset.x, Collider.offset.y);
                Vector3 changeVector = championTarget.transform.TransformPoint(championTarget.Collider.offset) - AttackBoxesParent.TransformPoint(linkPoint);
                transform.position = transform.position + changeVector;
            }
        }
        else if (statusNetworked == Status.UNIQUE2)
        {
            championTarget = null;
            if (championTarget == null)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, BlinkRange, LayerMask.GetMask("Champion"));
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
                transform.position = championTarget.transform.TransformPoint(championTarget.Collider.offset);
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, SpecialAttackRange);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, BlinkRange);
    }
}