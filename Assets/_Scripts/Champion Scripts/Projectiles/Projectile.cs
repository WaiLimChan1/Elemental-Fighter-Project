using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Static Projectile Spawn Functions
    public static void SpawnProjectileHorizontal(NetworkRunner Runner, Champion owner, bool isFacingLeft,
                                        NetworkPrefabRef projectilePrefab, Transform SpawnPoint, float speed, float damage, float numOfAttacks, float ccStrength, float lifeTime)
    {
        var Projectile = Runner.Spawn(projectilePrefab, SpawnPoint.position, Quaternion.identity);

        Vector2 velocity;
        if (isFacingLeft) velocity = new Vector2(-1 * speed, 0);
        else velocity = new Vector2(1 * speed, 0);

        Projectile.GetComponent<Projectile>().SetUp(owner, velocity, damage, numOfAttacks, ccStrength, lifeTime);
    }

    public static void SpawnProjectileDiagonal(NetworkRunner Runner, Champion owner, bool isFacingLeft,
                                        NetworkPrefabRef projectilePrefab, Transform SpawnPoint, float speed, float damage, float numOfAttacks, float ccStrength, float lifeTime)
    {
        NetworkObject Projectile;

        if (isFacingLeft) Projectile = Runner.Spawn(projectilePrefab, SpawnPoint.position, Quaternion.Euler(0, 0, 45));
        else Projectile = Runner.Spawn(projectilePrefab, SpawnPoint.position, Quaternion.Euler(0, 0, -45));

        Vector2 velocity;
        if (isFacingLeft) velocity = new Vector2(Mathf.Cos(-135 * Mathf.Deg2Rad), Mathf.Sin(-135 * Mathf.Deg2Rad));
        else velocity = new Vector2(Mathf.Cos(-45 * Mathf.Deg2Rad), Mathf.Sin(-45 * Mathf.Deg2Rad));
        velocity = velocity.normalized * speed;

        Projectile.GetComponent<Projectile>().SetUp(owner, velocity, damage, numOfAttacks, ccStrength, lifeTime);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Projectile Variables
    [SerializeField][Networked] protected Champion owner { get; set; }
    [SerializeField] protected BoxCollider2D HitBox;
    [SerializeField] protected SpriteRenderer SpriteRenderer;
    [SerializeField] protected Animator Animator;

    [SerializeField][Networked] protected bool flying { get; set; }
    [SerializeField][Networked] protected bool isFacingLeft { get; set; }
    [SerializeField][Networked] protected Vector2 velocity { get; set; }
    [SerializeField] protected float damage;
    [SerializeField][Networked] protected float numOfAttacks { get; set; }
    [SerializeField] protected float crowdControlStrength;

    [SerializeField][Networked] protected float lifeTime { get; set; }
    [SerializeField][Networked] protected TickTimer remainingLifeTime { get; set; }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Projectile Initialization
    public override void Spawned()
    {
        HitBox = GetComponent<BoxCollider2D>();
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Animator = GetComponentInChildren<Animator>();
    }

    public void SetUp(Champion owner, Vector2 velocity, float damage, float numOfAttacks, float ccStrength, float lifeTime)
    {
        flying = true;
        this.owner = owner;
        this.velocity = velocity;
        this.isFacingLeft = owner.isFacingLeftNetworked;
        this.damage = damage;
        this.numOfAttacks = numOfAttacks;
        this.crowdControlStrength = ccStrength;
        this.lifeTime = lifeTime;
        remainingLifeTime = TickTimer.CreateFromSeconds(Runner, lifeTime);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Projectile Logic
    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    public virtual void RPC_HitChampion(Champion enemy) 
    {
        flying = false;
        enemy.TakeDamageNetworked(owner, damage, numOfAttacks, isFacingLeft);
        ApplyCrowdControl(enemy, crowdControlStrength);
    }

    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    public virtual void RPC_HitEnvironment(NetworkObject collided) 
    { 
        flying = false; 
    }

    public virtual void ApplyCrowdControl(Champion enemy, float CrowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeft) direction *= -1;

        enemy.AddVelocity(new Vector2(direction * CrowdControlStrength, CrowdControlStrength / 2));
    }

    public virtual bool ShouldDespawn() { return remainingLifeTime.ExpiredOrNotRunning(Runner);  }

    public override void FixedUpdateNetwork()
    {
        if (ShouldDespawn())
        {
            Runner.Despawn(this.Object);
            return;
        }

        if (Runner.IsServer)
        {
            if (flying)
            {
                //Server updates the position
                transform.position = new Vector3(transform.position.x + velocity.x * Runner.DeltaTime, transform.position.y + velocity.y * Runner.DeltaTime, 0);

                //Server checks and handles collision with champions
                Collider2D[] colliders = Physics2D.OverlapBoxAll(HitBox.bounds.center, HitBox.bounds.size, HitBox.transform.eulerAngles.z, LayerMask.GetMask("Champion"));
                foreach (Collider2D collider in colliders)
                {
                    Champion enemy = collider.GetComponent<Champion>();
                    if (enemy != null && owner != null 
                        && enemy.CanBeAttacked(owner) && enemy.statusNetworked != Champion.Status.ROLL)
                    {
                        RPC_HitChampion(enemy);
                        break;
                    }
                }

                if (flying)
                {
                    //Server checks and handles collision with environment
                    colliders = Physics2D.OverlapBoxAll(HitBox.bounds.center, HitBox.bounds.size, HitBox.transform.eulerAngles.z, LayerMask.GetMask("Ground"));
                    if (colliders.Length > 0)
                    {
                        RPC_HitEnvironment(colliders[0].GetComponentInParent<NetworkObject>());
                    }
                }
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------

    public override void Render()
    {
        if (isFacingLeft) SpriteRenderer.flipX = true;

        //Fad away
        if (!remainingLifeTime.ExpiredOrNotRunning(Runner))
        {
            float alpha = (remainingLifeTime.RemainingTime(Runner) ?? 0.0f) / lifeTime;
            SpriteRenderer.color = new Color(SpriteRenderer.color.r, SpriteRenderer.color.g, SpriteRenderer.color.b, alpha);
        }
    }
}
