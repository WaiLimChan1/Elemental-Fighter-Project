using Fusion;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MetalBladekeeper : Champion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Metal Bladekeeper Variables")]
    [SerializeField] private float SpecialHopForce = 18; 

    [Header("Dagger Variables")]
    [SerializeField] private NetworkPrefabRef DaggerPrefab;
    [SerializeField] private Transform DaggerSpawnSpot;
    [SerializeField] private float DaggerSpeed = 30;
    [SerializeField] private float DaggerLifeTime = 5;

    [SerializeField] private Attack daggerAttack;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Attack Variables & Attack Functions
    //Status.UNIQUE1 : Throw
    //Status.UNIQUE2 : Air_Throw

    public override void SetAttack_ChampionUI(ChampionUI ChampionUI)
    {
        base.SetAttack_ChampionUI(ChampionUI);
        ChampionUI.SetAttack_ChampionUI(ChampionUI.UniqueB, daggerAttack, "Q");
    }

    protected override Attack getAttack(Status status)
    {
        if (status == Status.UNIQUE1 || status == Status.UNIQUE2) return daggerAttack;
        return base.getAttack(status);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    //Status.UNIQUE1 : Throw
    //Status.UNIQUE2 : Air_Throw

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
            if (Input.GetKeyDown(KeyCode.Q) && canUseAttack(Status.UNIQUE1))
            {
                status = Status.UNIQUE1;
            }
        }

        if (inAir && InAirInterruptableStatus())
        {
            if (Input.GetKeyDown(KeyCode.Q) && canUseAttack(Status.UNIQUE2))
            {
                status = Status.UNIQUE2;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack logic
    public override void DealDamageToVictim(Champion enemy, float damage)
    {
        if (statusNetworked == Status.SPECIAL_ATTACK) enemy.TakeDamageNetworked(this, damage, isFacingLeftNetworked, AttackType.BlockByFacingAttacker, transform.position);
        else enemy.TakeDamageNetworked(this, damage, isFacingLeftNetworked, AttackType.BlockByFacingAttack);
    }

    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.SPECIAL_ATTACK)
        {
            BoxCollider2D crowdControlBox = Attacks[4].hitBox;
            Vector2 center = AttackBoxesParent.TransformPoint(crowdControlBox.offset);

            if (enemy.transform.position.x < center.x) enemy.AddVelocity(new Vector2(-1 * crowdControlStrength, crowdControlStrength));
            else enemy.AddVelocity(new Vector2(crowdControlStrength, crowdControlStrength));
        }
    }

    public override void AnimationTriggerProjectileSpawn() 
    {
        if (!Runner.IsServer) return;

        if (statusNetworked == Status.UNIQUE1)
            Projectile.SpawnProjectileHorizontal(Runner, this, isFacingLeftNetworked, DaggerPrefab, DaggerSpawnSpot, DaggerSpeed, daggerAttack.damage, daggerAttack.crowdControlStrength, DaggerLifeTime);
        else if (statusNetworked == Status.UNIQUE2)
            Projectile.SpawnProjectileDiagonal(Runner, this, isFacingLeftNetworked, DaggerPrefab, DaggerSpawnSpot, DaggerSpeed, daggerAttack.damage, daggerAttack.crowdControlStrength, DaggerLifeTime);
    }

    public override void AnimationTriggerMobility()
    {
        if (statusNetworked == Status.UNIQUE2)
        {
            Rigid.velocity = new Vector3(Rigid.velocity.x, SpecialHopForce, 0);
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}