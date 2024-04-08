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
    //Status Logic
    //Status.UNIQUE2 : Throw_Fire_Ball
    protected override Attack getAttack(Status status)
    {
        if (status == Status.UNIQUE2) return fireBallAttack;
        return base.getAttack(status);
    }

    protected override bool SingleAnimationStatus()
    {
        return (base.SingleAnimationStatus() ||
            status == Status.UNIQUE2);
    }

    protected override bool UnstoppableStatusNetworked()
    {
        return (base.UnstoppableStatusNetworked() || statusNetworked == Status.ATTACK3);
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
            if (Input.GetKeyDown(KeyCode.Q) && canUseAttack(Status.UNIQUE2))
            {
                status = Status.UNIQUE2;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void DealDamageToVictim(Champion enemy, float damage)
    {
        if (statusNetworked == Status.UNIQUE1) 
            enemy.TakeDamageNetworked(damage, isFacingLeftNetworked, AttackType.BlockByFacingAttacker, transform.position);
        else enemy.TakeDamageNetworked(damage, isFacingLeftNetworked, AttackType.BlockByFacingAttack);
    }

    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.AIR_ATTACK) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, crowdControlStrength));
        else if (statusNetworked == Status.ATTACK1) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, 0));
        else if (statusNetworked == Status.ATTACK2) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, 0));
        else if (statusNetworked == Status.ATTACK3) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, crowdControlStrength / 2));
        else if (statusNetworked == Status.SPECIAL_ATTACK) enemy.AddVelocity(new Vector2(direction * crowdControlStrength / 2, crowdControlStrength));
        else if (statusNetworked == Status.UNIQUE1) enemy.SetVelocity(new Vector2(0, crowdControlStrength));
    }

    public override void AnimationTriggerMobility()
    {
        if (statusNetworked == Status.ATTACK3)
        {
            float direction = 1;
            if (isFacingLeftNetworked) direction *= -1;

            Rigid.velocity = new Vector3(direction * attack3DashForce, 0, 0);
        }
    }

    public override void AnimationTriggerProjectileSpawn()
    {
        if (!Runner.IsServer) return;

        if (statusNetworked == Status.UNIQUE2)
            Projectile.SpawnProjectileHorizontal(Runner, this, isFacingLeftNetworked, FireBallPrefab, FireBallSpawnPoint, FireBallSpeed, fireBallAttack.damage, fireBallAttack.crowdControlStrength, FireBallLifeTime);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Logic
    protected override void UpdatePosition()
    {
        base.UpdatePosition();
        if (statusNetworked == Status.ATTACK3 &&
            ChampionAnimationController.GetNormalizedTime() >= attack3DashStartTime &&
            ChampionAnimationController.GetNormalizedTime() < attack3DashEndTime)
        {
            float xChange = attack3DashSpeed * Time.fixedDeltaTime;
            if (isFacingLeftNetworked) xChange *= -1;

            Rigid.position = new Vector2(Rigid.position.x + xChange, Rigid.position.y);
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
