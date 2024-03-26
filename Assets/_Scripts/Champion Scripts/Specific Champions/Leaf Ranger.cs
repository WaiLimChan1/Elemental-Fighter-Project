using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafRanger : Champion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Leaf Ranger Variables")]
    [SerializeField] protected float slideMoveSpeed = 20;

    [Header("Arrow Variables")]
    [SerializeField] private NetworkPrefabRef ArrowPrefab;
    [SerializeField] private Transform ArrowSpawnSpot;
    [SerializeField] private Transform ArrowAirSpawnSpot;
    [SerializeField] private float ArrowSpeed = 30;
    [SerializeField] private float ArrowLifeTime = 5;
    //---------------------------------------------------------------------------------------------------------------------------------------------
    
    
    
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    //Status.UNIQUE1 : Slide
    protected override bool SingleAnimationStatus()
    {
        return (base.SingleAnimationStatus() ||
            status == Status.UNIQUE1);
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
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                if (Input.GetKey(KeyCode.Q)) status = Status.UNIQUE1;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override int GetAttackBoxIndex()
    {
        if (statusNetworked == Status.ATTACK1) return 1;
        else if (statusNetworked == Status.SPECIAL_ATTACK) return 4;
        else return -1;
    }

    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.SPECIAL_ATTACK) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, 0));
    }

    public override void AnimationTriggerProjectileSpawn()
    {
        if (!Runner.IsServer) return;

        if (statusNetworked == Status.ATTACK2)
            Projectile.SpawnProjectileHorizontal(Runner, this, isFacingLeftNetworked, ArrowPrefab, ArrowSpawnSpot, ArrowSpeed, AttackDamages[2], ArrowLifeTime);
        else if (statusNetworked == Status.AIR_ATTACK)
            Projectile.SpawnProjectileDiagonal(Runner, this, isFacingLeftNetworked, ArrowPrefab, ArrowAirSpawnSpot, ArrowSpeed, AttackDamages[0], ArrowLifeTime);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Logic
    protected override void UpdatePosition()
    {
        base.UpdatePosition();
        if (statusNetworked == Status.UNIQUE1)
        {
            float xChange = slideMoveSpeed * Time.fixedDeltaTime;
            if (isFacingLeftNetworked) xChange *= -1;
            Rigid.position = new Vector2(Rigid.position.x + xChange, Rigid.position.y);
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
