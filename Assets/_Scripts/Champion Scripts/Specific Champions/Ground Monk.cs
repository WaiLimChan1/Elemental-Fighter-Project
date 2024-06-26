using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundMonk : Champion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Ground Monk Variables")]
    [SerializeField] protected BoxCollider2D SpecialAttackCrowdControlBox;

    [SerializeField] protected Attack meditate;
    [SerializeField] protected float meditateHealthRegen = 15.0f;
    [SerializeField] protected float mediateManaRegen = 5.0f;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Attack Variables & Attack Functions
    //Status.UNIQUE1 : Begin_Meditation
    //Status.UNIQUE2 : Meditation

    public override void SetAttack_ChampionUI(AllAttacks_ChampionUI ChampionUI)
    {
        base.SetAttack_ChampionUI(ChampionUI);
        ChampionUI.SetAttack_ChampionUI(ChampionUI.UniqueB, meditate, "Hold Q");
    }

    protected override Attack getAttack(Status status)
    {
        if (status == Status.UNIQUE2) return meditate;
        return base.getAttack(status);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    //Status.UNIQUE1 : Begin_Meditation
    //Status.UNIQUE2 : Meditation
    protected override bool LoopingAnimationStatus(Status status)
    {
        return base.LoopingAnimationStatus(status) || status == Status.UNIQUE2;
    }

    protected override bool InterruptableStatus()
    {
        return (base.InterruptableStatus() ||
            status == Status.UNIQUE1 || status == Status.UNIQUE2);
    }

    protected override void OnGroundTakeInput()
    {
        base.OnGroundTakeInput();
        if (Input.GetKey(KeyCode.Q))
        {
            //Begin_Meditation, then Begin_Meditation into Meditation, then if already Meditation, continue Meditation
            Status lastStatus = (Status)ChampionAnimationController.GetAnimatorStatus();
            if (lastStatus != Status.UNIQUE1 && lastStatus != Status.UNIQUE2 && CanUseAttack(Status.UNIQUE2)) status = Status.UNIQUE1;
            else if (lastStatus == Status.UNIQUE1)
            {
                if (ChampionAnimationController.AnimationFinished()) status = Status.UNIQUE2;
                else status = Status.UNIQUE1;
            }
            else if (lastStatus == Status.UNIQUE2) status = Status.UNIQUE2;

        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
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

            if (enemy.transform.position.x < center.x) enemy.SetVelocity(new Vector2(crowdControlStrength, 0));
            else enemy.SetVelocity(new Vector2(-1 * crowdControlStrength, 0));
        }
    }

    public override void GetControlBoxAndStrength(ref BoxCollider2D crowdControlBox, ref float crowdControlStrength, int index)
    {
        if (statusNetworked == Status.SPECIAL_ATTACK) crowdControlBox = SpecialAttackCrowdControlBox;
        else crowdControlBox = Attacks[index].hitBox;
        crowdControlStrength = Attacks[index].crowdControlStrength;
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Logic
    private void ApplyMeditationEffects()
    {
        if (!Runner.IsServer) return;

        if (statusNetworked == Status.UNIQUE2) //Meditation Health & Mana Regen
        {
            if (healthNetworked > 0)
            {
                setHealthNetworked(healthNetworked + meditateHealthRegen * Runner.DeltaTime);
                setManaNetworked(manaNetworked + mediateManaRegen * Runner.DeltaTime);
            }
        }
    }

    protected override void ApplyEffects()
    {
        base.ApplyEffects();
        ApplyMeditationEffects();
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
