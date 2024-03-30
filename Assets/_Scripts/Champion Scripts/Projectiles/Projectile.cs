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
                                        NetworkPrefabRef projectilePrefab, Transform SpawnPoint, float speed, float damage, float lifeTime)
    {
        var Projectile = Runner.Spawn(projectilePrefab, SpawnPoint.position, Quaternion.identity);

        Vector2 velocity;
        if (isFacingLeft) velocity = new Vector2(-1 * speed, 0);
        else velocity = new Vector2(1 * speed, 0);

        Projectile.GetComponent<Projectile>().SetUp(owner, velocity, damage, lifeTime);
    }

    public static void SpawnProjectileDiagonal(NetworkRunner Runner, Champion owner, bool isFacingLeft,
                                        NetworkPrefabRef projectilePrefab, Transform SpawnPoint, float speed, float damage, float lifeTime)
    {
        NetworkObject Projectile;

        if (isFacingLeft) Projectile = Runner.Spawn(projectilePrefab, SpawnPoint.position, Quaternion.Euler(0, 0, 45));
        else Projectile = Runner.Spawn(projectilePrefab, SpawnPoint.position, Quaternion.Euler(0, 0, -45));

        Vector2 velocity;
        if (isFacingLeft) velocity = new Vector2(Mathf.Cos(-135 * Mathf.Deg2Rad), Mathf.Sin(-135 * Mathf.Deg2Rad));
        else velocity = new Vector2(Mathf.Cos(-45 * Mathf.Deg2Rad), Mathf.Sin(-45 * Mathf.Deg2Rad));
        velocity = velocity.normalized * speed;

        Projectile.GetComponent<Projectile>().SetUp(owner, velocity, damage, lifeTime);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Static Stuck Rotation Range
    static Vector2 championStuckRotationRange = new Vector2(-30, 30);
    static Vector2 environmentStuckRotationRange = new Vector2(-5, 5);
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Projectile Variables
    [SerializeField][Networked] private Champion owner { get; set; }
    [SerializeField][Networked] private NetworkObject StuckTarget { get; set; }
    [SerializeField] private BoxCollider2D HitBox;
    [SerializeField] private SpriteRenderer SpriteRenderer;

    [SerializeField][Networked] private bool flying { get; set; }
    [SerializeField][Networked] private bool isFacingLeft { get; set; }
    [SerializeField][Networked] private Vector2 velocity { get; set; }
    [SerializeField] private float damage;

    [SerializeField][Networked] private float stuckYRotation { get; set; }
    [SerializeField][Networked] private float stuckZRotation { get; set; }
    [SerializeField][Networked] private Vector3 stuckLocalPosition { get; set; }

    [SerializeField][Networked] private float lifeTime { get; set; }
    [SerializeField][Networked] private TickTimer remainingLifeTime { get; set; }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    public override void Spawned()
    {
        HitBox = GetComponent<BoxCollider2D>();
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void SetUp(Champion owner, Vector2 velocity, float damage, float lifeTime)
    {
        flying = true;
        this.owner = owner;
        this.velocity = velocity;
        this.isFacingLeft = owner.isFacingLeftNetworked;
        this.damage = damage;
        this.lifeTime = lifeTime;
        remainingLifeTime = TickTimer.CreateFromSeconds(Runner, lifeTime);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Stuck Logic
    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_StuckOntoChampion(Champion enemy)
    {
        if (enemy == null) return;

        flying = false;
        remainingLifeTime = TickTimer.CreateFromSeconds(Runner, lifeTime);
        StuckTarget = enemy.GetComponent<NetworkObject>();
        gameObject.transform.parent = StuckTarget.GetComponent<Champion>().AttackBoxesParent;

        stuckYRotation = enemy.isFacingLeftNetworked ? 180 : 0;
        stuckZRotation = transform.rotation.eulerAngles.z + Random.Range(championStuckRotationRange.x, championStuckRotationRange.y);
        stuckLocalPosition = gameObject.transform.localPosition;
    }

    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_StuckOntoEnvironment(NetworkObject networkObject)
    {
        if (networkObject == null) return;

        flying = false;
        remainingLifeTime = TickTimer.CreateFromSeconds(Runner, lifeTime);
        StuckTarget = networkObject;
        gameObject.transform.parent = networkObject.transform;

        stuckYRotation = 0;
        stuckZRotation = transform.rotation.eulerAngles.z + Random.Range(environmentStuckRotationRange.x, environmentStuckRotationRange.y);
        stuckLocalPosition = gameObject.transform.localPosition;
    }

    public void FixStuck()
    {
        //Stopped flying but not properly stuck
        if (!flying && gameObject.transform.parent == null)
        {
            //Stuck to a champion
            if (StuckTarget.GetComponent<Champion>() != null)
                gameObject.transform.parent = StuckTarget.GetComponent<Champion>().AttackBoxesParent;

            //Stuck to a champion
            if (StuckTarget.GetComponent<Environment>() != null)
                gameObject.transform.parent = StuckTarget.transform;
        }

        //Move to stuck position
        if (!flying)
        {
            gameObject.GetComponent<NetworkTransform>().enabled = false;
            gameObject.transform.localPosition = stuckLocalPosition;
            gameObject.transform.localRotation = Quaternion.Euler(0, stuckYRotation, stuckZRotation);
            gameObject.GetComponent<NetworkTransform>().InterpolationTarget.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------

    public override void FixedUpdateNetwork()
    {
        if (remainingLifeTime.ExpiredOrNotRunning(Runner))
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

                //Server checks and handles collision
                Collider2D[] colliders = Physics2D.OverlapBoxAll(HitBox.bounds.center, HitBox.bounds.size, HitBox.transform.eulerAngles.z, LayerMask.GetMask("Champion"));
                foreach (Collider2D collider in colliders)
                {
                    Champion enemy = collider.GetComponent<Champion>();
                    if (enemy != null && enemy != owner && enemy.healthNetworked > 0 && enemy.statusNetworked != Champion.Status.ROLL)
                    {
                        enemy.TakeDamageNetworked(damage, isFacingLeft);
                        RPC_StuckOntoChampion(enemy);
                        break;
                    }
                }

                if (flying)
                {
                    //Server checks and handles collision
                    colliders = Physics2D.OverlapBoxAll(HitBox.bounds.center, HitBox.bounds.size, HitBox.transform.eulerAngles.z, LayerMask.GetMask("Ground"));
                    if (colliders.Length > 0)
                    {
                        RPC_StuckOntoEnvironment(colliders[0].GetComponentInParent<NetworkObject>());
                    }
                }
            }
        }
        FixStuck();
    }

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
