using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalBladekeeper : Champion
{
    [Header("Metal Bladekeeper Variables")]
    [SerializeField] private float SpecialHopForce = 18; 

    [Header("Dagger Variables")]
    [SerializeField] private NetworkPrefabRef DaggerPrefab;
    [SerializeField] private Transform DaggerSpawnSpot;
    [SerializeField] private float DaggerSpeed = 30;
    [SerializeField] private float DaggerDamage = 10;
    [SerializeField] private float DaggerLifeTime = 5;


    //---------------------------------------------------------------------------------------------------------------------------------------------
    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.SPECIAL_ATTACK)
        {
            BoxCollider2D crowdControlBox = AttackBoxes[4];
            Vector2 center = AttackBoxesParent.TransformPoint(crowdControlBox.offset);

            if (enemy.transform.position.x < center.x) enemy.AddVelocity(new Vector2(-1 * crowdControlStrength, crowdControlStrength));
            else enemy.AddVelocity(new Vector2(crowdControlStrength, crowdControlStrength));
        }
    }

    public override void AnimationTriggerProjectileSpawn() 
    {
        if (!Runner.IsServer) return;

        if (statusNetworked == Status.UNIQUE1)
        {
            var Dagger = Runner.Spawn(DaggerPrefab, DaggerSpawnSpot.position, Quaternion.identity);

            Vector2 velocity;
            if (isFacingLeftNetworked) velocity = new Vector2(-1 * DaggerSpeed, 0);
            else velocity = new Vector2(1 * DaggerSpeed, 0);

            Dagger.GetComponent<Dagger>().SetUp(this, velocity, DaggerDamage, DaggerLifeTime);
        }
        else if (statusNetworked == Status.UNIQUE2)
        {
            NetworkObject Dagger;

            if (isFacingLeftNetworked) Dagger = Runner.Spawn(DaggerPrefab, DaggerSpawnSpot.position, Quaternion.Euler(0,0,45));
            else Dagger = Runner.Spawn(DaggerPrefab, DaggerSpawnSpot.position, Quaternion.Euler(0, 0, -45));

            Vector2 velocity;
            if (isFacingLeftNetworked) velocity = new Vector2(Mathf.Cos(-135 * Mathf.Deg2Rad), Mathf.Sin(-135 * Mathf.Deg2Rad));
            else velocity = new Vector2(Mathf.Cos(-45 * Mathf.Deg2Rad), Mathf.Sin(-45 * Mathf.Deg2Rad));
            velocity = velocity.normalized * DaggerSpeed;

            Dagger.GetComponent<Dagger>().SetUp(this, velocity, DaggerDamage, DaggerLifeTime);
        }
    }

    public override void AnimationTriggerMobility()
    {
        if (statusNetworked == Status.UNIQUE2)
        {
            Rigid.velocity = new Vector3(Rigid.velocity.x, SpecialHopForce, 0);
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
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
            if (Input.GetKeyDown(KeyCode.Q))
            {
                status = Status.UNIQUE1;
            }
        }

        if (inAir && InAirInterruptableStatus())
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                status = Status.UNIQUE2;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}