using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafRanger : Champion
{
    [Header("Leaf Ranger Variables")]
    [SerializeField] protected float slideMoveSpeed = 20;

    [Header("Arrow Variables")]
    [SerializeField] private NetworkPrefabRef ArrowPrefab;
    [SerializeField] private Transform ArrowSpawnSpot;
    [SerializeField] private Transform ArrowAirSpawnSpot;
    [SerializeField] private float ArrowSpeed = 30;
    [SerializeField] private float ArrowDamage = 10;
    [SerializeField] private float ArrowLifeTime = 5;


    //---------------------------------------------------------------------------------------------------------------------------------------------
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
        {
            var Arrow = Runner.Spawn(ArrowPrefab, ArrowSpawnSpot.position, Quaternion.identity);

            Vector2 velocity;
            if (isFacingLeftNetworked) velocity = new Vector2(-1 * ArrowSpeed, 0);
            else velocity = new Vector2(1 * ArrowSpeed, 0);

            Arrow.GetComponent<Arrow>().SetUp(this, velocity, ArrowDamage, ArrowLifeTime);
        }
        else if (statusNetworked == Status.AIR_ATTACK)
        {
            NetworkObject Arrow;

            if (isFacingLeftNetworked) Arrow = Runner.Spawn(ArrowPrefab, ArrowAirSpawnSpot.position, Quaternion.Euler(0, 0, 45));
            else Arrow = Runner.Spawn(ArrowPrefab, ArrowAirSpawnSpot.position, Quaternion.Euler(0, 0, -45));

            Vector2 velocity;
            if (isFacingLeftNetworked) velocity = new Vector2(Mathf.Cos(-135 * Mathf.Deg2Rad), Mathf.Sin(-135 * Mathf.Deg2Rad));
            else velocity = new Vector2(Mathf.Cos(-45 * Mathf.Deg2Rad), Mathf.Sin(-45 * Mathf.Deg2Rad));
            velocity = velocity.normalized * ArrowSpeed;

            Arrow.GetComponent<Arrow>().SetUp(this, velocity, ArrowDamage, ArrowLifeTime);
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
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
