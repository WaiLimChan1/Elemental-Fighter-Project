using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireElemental : ElementalChampion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Fire Elemental Variables")]
    [SerializeField] private float attack3DashForce = 15;
    [SerializeField] private float attack3DashSpeed = 30;

    private const float attack3DashStartTime = 1.0f / 10.0f;
    private const float attack3DashEndTime = 6.0f / 10.0f;

    [Header("Fire Ball Variables")]
    [SerializeField] private NetworkPrefabRef FireBallPrefab;
    [SerializeField] private Transform FireBallSpawnPoint;
    [SerializeField] private float FireBallSpeed = 45;
    [SerializeField] private float FireBallLifeTime = 5;

    [SerializeField] private Attack fireBallAttack;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Attack Variables & Attack Functions
    //Status.UNIQUE2 : Throw_Fire_Ball

    public override void SetAttack_ChampionUI(ChampionUI ChampionUI)
    {
        base.SetAttack_ChampionUI(ChampionUI);
        ChampionUI.SetAttack_ChampionUI(ChampionUI.UniqueB, fireBallAttack, "Q");
    }

    protected override Attack getAttack(Status status)
    {
        if (status == Status.UNIQUE2) return fireBallAttack;
        return base.getAttack(status);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    //Status.UNIQUE2 : Throw_Fire_Ball

    protected override bool SingleAnimationStatus()
    {
        return (base.SingleAnimationStatus() ||
            status == Status.UNIQUE2);
    }

    protected override bool UnstoppableStatusNetworked()
    {
        return (base.UnstoppableStatusNetworked() || statusNetworked == Status.ATTACK3);
    }

    protected override bool MobilityStatus(Status status)
    {
        return (base.MobilityStatus(status) || status == Status.ATTACK3);
    }

    protected override bool AttackSpeedStatus(Status status)
    {
        return (base.AttackSpeedStatus(status) || status == Status.SPECIAL_ATTACK || status == Status.UNIQUE2);
    }

    protected override void OnGroundTakeInput()
    {
        base.OnGroundTakeInput();
        if (Input.GetKeyDown(KeyCode.Q) && CanUseAttack(Status.UNIQUE2)) status = Status.UNIQUE2; //Throw Fire Ball
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void DealDamageToVictim(Champion enemy, float damage)
    {
        if (statusNetworked == Status.UNIQUE1) 
            enemy.TakeDamageNetworked(this, damage, isFacingLeftNetworked, AttackType.BlockByFacingAttacker, transform.position);
        else enemy.TakeDamageNetworked(this, damage, isFacingLeftNetworked, AttackType.BlockByFacingAttack);
    }

    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.AIR_ATTACK) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, crowdControlStrength));
        else if (statusNetworked == Status.ATTACK1) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, 0));
        else if (statusNetworked == Status.ATTACK2) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, 0));
        else if (statusNetworked == Status.ATTACK3) enemy.SetVelocity(new Vector2(direction * crowdControlStrength, crowdControlStrength / 2));
        else if (statusNetworked == Status.SPECIAL_ATTACK) enemy.AddVelocity(new Vector2(direction * crowdControlStrength / 2, crowdControlStrength));
        else if (statusNetworked == Status.UNIQUE1) enemy.SetVelocity(new Vector2(0, crowdControlStrength));
    }

    public override void AnimationTriggerMobility()
    {
        if (statusNetworked == Status.ATTACK3)
        {
            float direction = 1;
            if (isFacingLeftNetworked) direction *= -1;

            Rigid.velocity = new Vector3(direction * attack3DashForce * mobilityModifier, 0, 0);
        }
    }

    public override void AnimationTriggerProjectileSpawn()
    {
        if (!Runner.IsServer) return;

        if (statusNetworked == Status.UNIQUE2)
            Projectile.SpawnProjectileHorizontal(Runner, this, isFacingLeftNetworked, FireBallPrefab, FireBallSpawnPoint, FireBallSpeed, getCalculatedDamage(fireBallAttack), fireBallAttack.crowdControlStrength, FireBallLifeTime);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Logic
    private void Attack3DashMovementLogic()
    {
        if (statusNetworked == Status.ATTACK3 &&
            ChampionAnimationController.GetNormalizedTime() >= attack3DashStartTime &&
            ChampionAnimationController.GetNormalizedTime() < attack3DashEndTime)
        {
            float xChange = attack3DashSpeed * mobilityModifier * Runner.DeltaTime;
            if (isFacingLeftNetworked) xChange *= -1;

            Rigid.position = new Vector2(Rigid.position.x + xChange, Rigid.position.y);
        }
    }

    protected override void UpdatePosition()
    {
        base.UpdatePosition();
        Attack3DashMovementLogic();
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
