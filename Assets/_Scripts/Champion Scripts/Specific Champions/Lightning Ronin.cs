using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningRonin : Champion
{
    [Header("Lightning Ronin Dash Variables")]
    [SerializeField] private float dashSpeed = 25;
    [SerializeField] private float lightningDashSpeed = 30;

    [SerializeField] private BoxCollider2D lightningDashAttackBox;
    [SerializeField] private float lightningDashDamage = 20;

    //---------------------------------------------------------------------------------------------------------------------------------------------
    public override void DealDamageToVictim(Champion enemy, float damage)
    {
        if (statusNetworked == Status.SPECIAL_ATTACK) enemy.TakeDamageNetworked(damage, isFacingLeftNetworked, AttackType.BlockByFacingAttacker, transform.position);
        else enemy.TakeDamageNetworked(damage, isFacingLeftNetworked, AttackType.BlockByFacingAttack);
    }

    public override void AnimationTriggerAttack()
    {
        base.AnimationTriggerAttack();
        if (statusNetworked == Status.UNIQUE2)
        {
            BoxCollider2D attackBox = lightningDashAttackBox;
            float damage = lightningDashDamage;

            Collider2D[] colliders = Physics2D.OverlapBoxAll(attackBox.bounds.center, attackBox.bounds.size, 0, LayerMask.GetMask("Champion"));
            foreach (Collider2D collider in colliders)
            {
                Champion enemy = collider.GetComponent<Champion>();
                if (enemy != null && enemy != this && enemy.healthNetworked > 0)
                    DealDamageToVictim(enemy, damage);
            }
        }
    }

    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.ATTACK3) enemy.AddVelocity(new Vector2(direction * crowdControlStrength / 2, crowdControlStrength));
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------


    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status.UNIQUE1 : Dash
    //Status.UNIQUE2 : Lightning Dash

    protected override bool SingleAnimationStatus()
    {
        return (base.SingleAnimationStatus() ||
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
            if (Input.GetKey(KeyCode.Q)) status = Status.UNIQUE1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                if (Input.GetKey(KeyCode.Q)) status = Status.UNIQUE2;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------

    //---------------------------------------------------------------------------------------------------------------------------------------------
    protected override void UpdatePosition()
    {
        base.UpdatePosition();
        if ((statusNetworked == Status.UNIQUE1 || statusNetworked == Status.UNIQUE2) &&
            ChampionAnimationController.GetNormalizedTime() <= 8.0f/11.0f)
        {
            float xChange = 0;

            if (statusNetworked == Status.UNIQUE1)
                xChange = dashSpeed * Time.fixedDeltaTime;
            else if (statusNetworked == Status.UNIQUE2)
                xChange = lightningDashSpeed * Time.fixedDeltaTime;

            if (isFacingLeftNetworked) xChange *= -1;
            Rigid.position = new Vector2(Rigid.position.x + xChange, Rigid.position.y);
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
