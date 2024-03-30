using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : Projectile
{
    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    public override void RPC_HitChampion(Champion enemy) 
    {
        base.RPC_HitChampion(enemy);
        Animator.SetTrigger("Hit");
    }

    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    public override void RPC_HitEnvironment(NetworkObject collided) 
    {
        base.RPC_HitEnvironment(collided);
        Animator.SetTrigger("Hit");
    }

    public override bool ShouldDespawn() { return base.ShouldDespawn() || 
            (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f && !flying); }
}
