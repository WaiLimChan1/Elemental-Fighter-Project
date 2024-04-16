using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPriestess : Champion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Water Priestess Mediate Variables")]
    [SerializeField] protected Attack meditate;
    [SerializeField] protected float meditateHealthRegen = 15.0f;
    [SerializeField] protected float mediateManaRegen = 5.0f;

    [Header("Water Priestess Water Slide Variables")]
    [SerializeField] private Attack waterSlideAttack;
    [SerializeField] private float waterSlideSpeed = 25;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Attack Variables & Attack Functions
    //Status.UNIQUE1 : Begin_Meditation
    //Status.UNIQUE2 : Meditation
    //Status.UNIQUE3 : Water_Slide

    public override void SetAttack_ChampionUI(ChampionUI ChampionUI)
    {
        base.SetAttack_ChampionUI(ChampionUI);
        ChampionUI.SetAttack_ChampionUI(ChampionUI.UniqueA, meditate, "Hold Q");
        ChampionUI.SetAttack_ChampionUI(ChampionUI.UniqueB, waterSlideAttack, "A/D + Q");
    }

    protected override Attack getAttack(Status status)
    {
        if (status == Status.UNIQUE2) return meditate;
        else if (status == Status.UNIQUE3) return waterSlideAttack;
        return base.getAttack(status);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    //Status.UNIQUE1 : Begin_Meditation
    //Status.UNIQUE2 : Meditation
    //Status.UNIQUE3 : Water_Slide

    protected override bool LoopingAnimationStatus(Status status)
    {
        return base.LoopingAnimationStatus(status) || status == Status.UNIQUE2;
    }

    protected override bool SingleAnimationStatus()
    {
        return (base.SingleAnimationStatus() ||
            status == Status.UNIQUE3);
    }

    protected override bool InterruptableStatus()
    {
        return (base.InterruptableStatus() ||
            status == Status.UNIQUE1 || status == Status.UNIQUE2);
    }

    protected override bool UnstoppableStatusNetworked()
    {
        return (base.UnstoppableStatusNetworked() || statusNetworked == Status.UNIQUE3);
    }

    protected override bool MobilityStatus(Status status)
    {
        return (base.MobilityStatus(status) || status == Status.UNIQUE3);
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
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            if (Input.GetKeyDown(KeyCode.Q) && CanUseAttack(Status.UNIQUE3)) status = Status.UNIQUE3; //Water Slide
        }
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
        else if (statusNetworked == Status.UNIQUE3) return -2;
        else return -1;
    }

    public override void GetAttackBoxAndDamage(ref BoxCollider2D attackBox, ref float damage, int index)
    {
        if (statusNetworked == Status.UNIQUE3)
        {
            attackBox = waterSlideAttack.hitBox;
            damage = getCalculatedDamage(waterSlideAttack);
        }
        else base.GetAttackBoxAndDamage(ref attackBox, ref damage, index);
    }

    public override void GetControlBoxAndStrength(ref BoxCollider2D crowdControlBox, ref float crowdControlStrength, int index)
    {
        if (statusNetworked == Status.UNIQUE3)
        {
            crowdControlBox = waterSlideAttack.hitBox;
            crowdControlStrength = waterSlideAttack.crowdControlStrength;
        }
        else
        {
            crowdControlBox = Attacks[index].hitBox;
            crowdControlStrength = Attacks[index].crowdControlStrength;
        }
    }

    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.UNIQUE3) enemy.SetVelocity(new Vector2(direction * crowdControlStrength, crowdControlStrength));
        else if (statusNetworked == Status.ATTACK2) enemy.AddVelocity(new Vector2(0, crowdControlStrength));
        else if (statusNetworked == Status.ATTACK3) enemy.AddVelocity(new Vector2(direction * crowdControlStrength * 1 / 5, crowdControlStrength));
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Logic
    private void WaterSlideMovementLogic()
    {
        if (statusNetworked == Status.UNIQUE3)
        {
            float xChange = waterSlideSpeed * mobilityModifier * Runner.DeltaTime;
            if (isFacingLeftNetworked) xChange *= -1;
            Rigid.position = new Vector2(Rigid.position.x + xChange, Rigid.position.y);
        }
    }

    protected override void UpdatePosition()
    {
        base.UpdatePosition();
        WaterSlideMovementLogic();
    }

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
