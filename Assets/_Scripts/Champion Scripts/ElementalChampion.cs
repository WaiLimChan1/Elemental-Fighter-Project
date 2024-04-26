using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalChampion : Champion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Transform Variables & Functions
    private float CostToStayInElementalForm = 100;

    //Default to Elemental
    protected override void HostSetUpTransformChampion(float healthRatio, float TransformHealthGainAmount, float manaRatio, float TransformManaGainAmount, float ultimateMeter, float ultimateMeterCost)
    {
        base.HostSetUpTransformChampion(healthRatio, TransformHealthGainAmount, manaRatio, TransformManaGainAmount, ultimateMeter, ultimateMeterCost);
        setUltimateMeterNetworked(ultimateMeter - ultimateMeterCost, false);

        //Add a buffer for Ultimate Meter so that the Elemental doesn't automatically transform back
        setUltimateMeterNetworked(ultimateMeterNetworked + CostToStayInElementalForm, false);

        statusNetworked = Status.UNIQUE1;
    }

    //Default to Elemental
    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    protected override void RPC_ClientSetUpTransformChampion(bool isFacingLeft)
    {
        this.isFacingLeft = isFacingLeft;
        status = Status.UNIQUE1;
    }

    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    protected void RPC_BeginTransformAnimation()
    {
        if (Runner.LocalPlayer == Object.InputAuthority) status = Status.BEGIN_DEATH;
        statusNetworked = Status.BEGIN_DEATH;
        ChampionAnimationController.ChangeAnimation((int)statusNetworked);
        ChampionAnimationController.RestartAnimation();
    }

    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    protected void RPC_ClientTriggerBeginTransformAnimation()
    {
        RPC_BeginTransformAnimation();
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Attack Variables & Attack Functions
    //Status.UNIQUE1 : Transform

    public override void SetAttack_ChampionUI(AllAttacks_ChampionUI ChampionUI)
    {
        base.SetAttack_ChampionUI(ChampionUI);
        ChampionUI.Roll.gameObject.SetActive(false);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    //Status.UNIQUE1 : Transform

    protected override bool SingleAnimationStatus()
    {
        return (base.SingleAnimationStatus() ||
            status == Status.UNIQUE1);
    }

    protected override bool UnstoppableStatusNetworked()
    {
        return (base.UnstoppableStatusNetworked() || statusNetworked == Status.UNIQUE1 ||
            statusNetworked == Status.BEGIN_DEATH || statusNetworked == Status.FINISHED_DEATH);
    }

    protected override void EndStatus()
    {
        base.EndStatus();

        Status lastStatus = (Status)ChampionAnimationController.GetAnimatorStatus();
        if (lastStatus == Status.BEGIN_DEATH)
        {
            if (ChampionAnimationController.AnimationFinished()) status = Status.FINISHED_DEATH;
            else status = Status.BEGIN_DEATH;
        }
        else if (lastStatus == Status.FINISHED_DEATH) status = Status.FINISHED_DEATH;
    }

    protected override void TransformTakeInput() 
    {
        if (Input.GetKeyDown(KeyCode.E)) RPC_ClientTriggerBeginTransformAnimation();
    }


    protected override void CancelTakeInput()
    {
        base.CancelTakeInput();
        if (status == Status.ROLL) status = Status.RUN; //Elemental Champions do not have Roll.
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override int GetAttackBoxIndex()
    {
        if (statusNetworked == Status.AIR_ATTACK) return 0;
        else if (statusNetworked == Status.ATTACK1) return 1;
        else if (statusNetworked == Status.ATTACK2) return 2;
        else if (statusNetworked == Status.ATTACK3) return 3;
        else if (statusNetworked == Status.SPECIAL_ATTACK) return 4;
        else if (statusNetworked == Status.UNIQUE1) return 5;
        else return -1;
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Logic
    protected void ApplyUltimateMeterCost()
    {
        if (!Runner.IsServer) return;
        if (GameManager.CanUseGameManager() && GameManager.Instance.StopResourceRegenAndDecay) return;

        if (statusNetworked != Status.BEGIN_DEATH && statusNetworked != Status.FINISHED_DEATH)
            setUltimateMeterNetworked(ultimateMeterNetworked - CostToStayInElementalForm * Runner.DeltaTime);
    }

    protected void RanOutOfUltimateMeter()
    {
        if (!Runner.IsServer) return;
        if (ultimateMeterNetworked <= 0)
        {
            if (!UnstoppableStatusNetworked()) 
                RPC_BeginTransformAnimation();
        }
    }

    protected override void ApplyEffects()
    {
        base.ApplyEffects();
        ApplyUltimateMeterCost();
        RanOutOfUltimateMeter();
    }

    protected override void UpdateResourceBarVisuals()
    {
        ResourceBar.UpdateResourceBarVisuals(healthNetworked, maxHealth, manaNetworked, maxMana, TakeHitRecoveryPercentage, ultimateMeterNetworked, ultimateMeterCost, false);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
