using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalChampion : Champion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Transform Variables & Functions

    //Default to Elemental
    protected override void HostSetUpTransformChampion(float healthRatio, float TransformHealthGainAmount, float manaRatio, float TransformManaGainAmount, float ultimateMeter)
    {
        CalculateStats();
        setHealthNetworked(maxHealth * healthRatio + maxHealth * TransformHealthGainAmount);
        setManaNetworked(maxMana * manaRatio + maxMana * TransformManaGainAmount);
        setUltimateMeterNetworked(ultimateMeter);
    }

    //Default to Elemental
    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    protected override void RPC_ClientSetUpTransformChampion(bool isFacingLeft)
    {
        this.isFacingLeft = isFacingLeft;
        status = Status.UNIQUE1;
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Attack Variables & Attack Functions
    //Status.UNIQUE1 : Transform

    public override void SetAttack_ChampionUI(ChampionUI ChampionUI)
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
        return (base.UnstoppableStatusNetworked() || statusNetworked == Status.UNIQUE1);
    }

    protected override void TransformTakeInput() {}

    protected override void OnGroundTakeInput()
    {
        base.OnGroundTakeInput();
        //if (Input.GetKeyDown(KeyCode.E)) status = Status.UNIQUE1; //Transform
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
}
