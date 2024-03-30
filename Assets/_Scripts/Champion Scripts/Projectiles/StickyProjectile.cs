using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyProjectile : Projectile
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Static Stuck Rotation Range
    static Vector2 championStuckRotationRange = new Vector2(-30, 30);
    static Vector2 environmentStuckRotationRange = new Vector2(-5, 5);
    //---------------------------------------------------------------------------------------------------------------------------------------------




    //---------------------------------------------------------------------------------------------------------------------------------------------
    //StickyProjectile Variables
    [SerializeField][Networked] protected NetworkObject StuckTarget { get; set; }

    [SerializeField][Networked] protected float stuckYRotation { get; set; }
    [SerializeField][Networked] protected float stuckZRotation { get; set; }
    [SerializeField][Networked] protected Vector3 stuckLocalPosition { get; set; }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Stuck Logic
    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_StuckOntoChampion(Champion enemy)
    {
        if (enemy == null) return;

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



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Projectile Logic
    public override void HitChampion(Champion enemy)
    {
        base.HitChampion(enemy);
        RPC_StuckOntoChampion(enemy);
    }

    public override void HitEnvironment(NetworkObject collided)
    {
        base.HitEnvironment(collided);
        RPC_StuckOntoEnvironment(collided);
    }

    public override void FixedUpdateNetwork()
    {
        if (ShouldDespawn())
        {
            Runner.Despawn(this.Object);
            return;
        }

        base.FixedUpdateNetwork();

        FixStuck();
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
