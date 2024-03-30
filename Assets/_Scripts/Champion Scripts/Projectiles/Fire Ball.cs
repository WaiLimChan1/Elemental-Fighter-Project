using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : Projectile
{
    public override void HitChampion(Champion enemy) 
    {
        base.HitChampion(enemy);
        Animator.SetTrigger("Hit");
    }
    public override void HitEnvironment(NetworkObject collided) 
    {
        base.HitEnvironment(collided);
        Animator.SetTrigger("Hit");
    }

    public override bool ShouldDespawn() { return base.ShouldDespawn() || 
            (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f && !flying); }
}
