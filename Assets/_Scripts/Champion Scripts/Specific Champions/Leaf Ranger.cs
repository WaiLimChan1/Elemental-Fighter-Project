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

    [Header("Attack3 Variables")]
    [SerializeField] private NetworkPrefabRef AbilityPrefab;
    [SerializeField] float Attack3YOffSet = 4.2f;

    [SerializeField][Networked] private Champion Attack3Target { get; set; }
    [SerializeField] private float Attack3Range = 20f;
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
                if (Input.GetKeyDown(KeyCode.Q)) status = Status.UNIQUE1;
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

        if (statusNetworked == Status.ATTACK1) enemy.SetVelocity(new Vector2(direction * crowdControlStrength, 0));
        else if (statusNetworked == Status.SPECIAL_ATTACK) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, crowdControlStrength/2));
    }

    public override void AnimationTriggerAbilitySpawn()
    {
        if (!Runner.IsServer) return;

        if (statusNetworked == Status.ATTACK3)
        {
            Vector2 SpawnPoint = new Vector2(transform.position.x, transform.position.y);

            if (Attack3Target == null) Attack3Target = MainGameUtils.FindClosestEnemyCircle(this, transform.position, Attack3Range);
            if (Attack3Target != null)
            {
                SpawnPoint = new Vector2(Attack3Target.transform.position.x, Attack3Target.transform.position.y);
            }

            SpawnPoint = new Vector2(SpawnPoint.x, transform.position.y + Attack3YOffSet);
            Ability.SpawnAbility(Runner, this, isFacingLeftNetworked, AbilityPrefab, SpawnPoint, Attacks[3].damage, Ability.AbilityStatus.Leaf_Ranger_ATK3);

        }
    }

    public override void AnimationTriggerProjectileSpawn()
    {
        if (!Runner.IsServer) return;

        if (statusNetworked == Status.ATTACK2)
            Projectile.SpawnProjectileHorizontal(Runner, this, isFacingLeftNetworked, ArrowPrefab, ArrowSpawnSpot, ArrowSpeed, Attacks[2].damage, Attacks[2].crowdControlStrength, ArrowLifeTime);
        else if (statusNetworked == Status.AIR_ATTACK)
            Projectile.SpawnProjectileDiagonal(Runner, this, isFacingLeftNetworked, ArrowPrefab, ArrowAirSpawnSpot, ArrowSpeed, Attacks[0].damage, Attacks[0].crowdControlStrength, ArrowLifeTime);
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

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        Attack3Target = MainGameUtils.FindClosestEnemyCircle(this, transform.position, Attack3Range);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, Attack3Range);
        MainGameUtils.OnDrawGizmos_TeleportTarget(this, Attack3Target, 1);
    }
}
