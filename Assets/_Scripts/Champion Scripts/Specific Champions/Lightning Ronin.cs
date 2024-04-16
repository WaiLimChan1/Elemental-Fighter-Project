using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningRonin : Champion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Lightning Ronin Dash Variables")]
    [SerializeField] private Attack lightningDashAttack;
    [SerializeField] private float dashSpeed = 25;
    [SerializeField] private float lightningDashSpeed = 30;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Attack Variables & Attack Functions
    //Status.UNIQUE1 : Dash
    //Status.UNIQUE2 : Lightning Dash

    public override void SetAttack_ChampionUI(ChampionUI ChampionUI)
    {
        base.SetAttack_ChampionUI(ChampionUI);
        ChampionUI.SetAttack_ChampionUI(ChampionUI.UniqueB, lightningDashAttack, "A/D + Q");
    }

    protected override Attack getAttack(Status status)
    {
        if (status == Status.UNIQUE1 || status == Status.UNIQUE2) return lightningDashAttack;
        return base.getAttack(status);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    //Status.UNIQUE1 : Dash
    //Status.UNIQUE2 : Lightning Dash

    protected override bool SingleAnimationStatus()
    {
        return (base.SingleAnimationStatus() ||
            status == Status.UNIQUE1 || status == Status.UNIQUE2);
    }

    protected override bool UnstoppableStatusNetworked()
    {
        return (base.UnstoppableStatusNetworked() || statusNetworked == Status.UNIQUE2);
    }

    protected override bool MobilityStatus(Status status)
    {
        return (base.MobilityStatus(status) ||
            status == Status.UNIQUE1 || status == Status.UNIQUE2);
    }

    protected override void OnGroundTakeInput()
    {
        base.OnGroundTakeInput();
        //if (Input.GetKeyDown(KeyCode.Q) && canUseAttack(Status.UNIQUE1)) status = Status.UNIQUE1; //Dash
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            if (Input.GetKeyDown(KeyCode.Q) && CanUseAttack(Status.UNIQUE2)) status = Status.UNIQUE2; //Lightning Dash
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void DealDamageToVictim(Champion enemy, float damage)
    {
        if (statusNetworked == Status.SPECIAL_ATTACK) enemy.TakeDamageNetworked(this, damage, isFacingLeftNetworked, AttackType.BlockByFacingAttacker, transform.position);
        else enemy.TakeDamageNetworked(this, damage, isFacingLeftNetworked, AttackType.BlockByFacingAttack);
    }

    public override int GetAttackBoxIndex()
    {
        if (statusNetworked == Status.AIR_ATTACK) return 0;
        else if (statusNetworked == Status.ATTACK1) return 1;
        else if (statusNetworked == Status.ATTACK2) return 2;
        else if (statusNetworked == Status.ATTACK3) return 3;
        else if (statusNetworked == Status.SPECIAL_ATTACK) return 4;
        else if (statusNetworked == Status.UNIQUE2) return -2;
        else return -1;
    }

    public override void GetAttackBoxAndDamage(ref BoxCollider2D attackBox, ref float damage, int index)
    {
        if (statusNetworked == Status.UNIQUE2)
        {
            attackBox = lightningDashAttack.hitBox;
            damage = getCalculatedDamage(lightningDashAttack);
        }
        else base.GetAttackBoxAndDamage(ref attackBox, ref damage, index);
    }

    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.ATTACK3) enemy.AddVelocity(new Vector2(direction * crowdControlStrength / 2, crowdControlStrength));
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Logic
    private void LightningDashMovementLogic()
    {
        if ((statusNetworked == Status.UNIQUE1 || statusNetworked == Status.UNIQUE2) &&
            ChampionAnimationController.GetNormalizedTime() <= 8.0f / 11.0f)
        {
            float xChange = 0;

            if (statusNetworked == Status.UNIQUE1) xChange = dashSpeed * mobilityModifier * Runner.DeltaTime;
            else if (statusNetworked == Status.UNIQUE2) xChange = lightningDashSpeed * mobilityModifier * Runner.DeltaTime;

            if (isFacingLeftNetworked) xChange *= -1;

            Rigid.position = new Vector2(Rigid.position.x + xChange, Rigid.position.y);
        }
    }

    protected override void UpdatePosition()
    {
        base.UpdatePosition();
        LightningDashMovementLogic();
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
