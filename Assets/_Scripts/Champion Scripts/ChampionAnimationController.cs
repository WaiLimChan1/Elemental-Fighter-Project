using UnityEngine;


public class ChampionAnimationController : AnimationController
{
    private Champion Champion;

    public new void Awake()
    {
        base.Awake();
        Champion = GetComponentInParent<Champion>();
    }

    private void AnimationTriggerAttack() { Champion.AnimationTriggerAttack(); }
    private void AnimationTriggerCrowdControl() { Champion.AnimationTriggerCrowdControl(); }
    private void AnimationTriggerAbilitySpawn() { Champion.AnimationTriggerAbilitySpawn(); }
    private void AnimationTriggerProjectileSpawn() { Champion.AnimationTriggerProjectileSpawn(); }
    private void AnimationTriggerMobility() { Champion.AnimationTriggerMobility(); }
    private void AnimationTriggerTransform() { Champion.AnimationTriggerTransform(); }
}
