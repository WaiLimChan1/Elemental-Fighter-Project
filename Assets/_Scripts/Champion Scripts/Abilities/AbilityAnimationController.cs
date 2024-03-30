using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityAnimationController : AnimationController
{
    private Ability Ability;

    public new void Awake()
    {
        base.Awake();
        Ability = GetComponentInParent<Ability>();
    }

    private void AnimationTriggerAttack() { Ability.AnimationTriggerAttack(); }
    private void AnimationTriggerCrowdControl() { Ability.AnimationTriggerCrowdControl(); }
    private void AnimationTriggerDespawn() { Ability.AnimationTriggerDespawn(); }
}
